using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data.MissionData;
using EnhancedBattleTest.Patch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.SinglePlayer
{
    public class BattleStarter
    {
        private enum MemberState
        {
            Original,
            Leader,
            Prisoner
        }
        private static readonly Dictionary<Hero, KeyValuePair<PartyBase, MemberState>> _originalParties = new Dictionary<Hero, KeyValuePair<PartyBase, MemberState>>();
        private static readonly Dictionary<Hero, Settlement> _originalSettlements = new Dictionary<Hero, Settlement>();
        private static readonly Dictionary<Hero, int> _originalHitPoints = new Dictionary<Hero, int>();
        private static readonly Dictionary<Banner, String> _bannerSave = new Dictionary<Banner, String>();
        private static PartyBase _playerParty;
        private static PartyBase _enemyParty;
        private static List<PartyBase> _playerSideParties;
        private static List<PartyBase> _enemySideParties;
        private static MobileParty _oldMainParty;
        private static BasicCharacterObject _oldPlayerCharacter;
        public static bool IsEnhancedBattleTestBattle = false;
        private static MapEvent _mapEvent;
        private static SiegeEvent _siegeEvent;

        private static bool ValidateNoDuplicateHero(BattleConfig config)
        {
            var troops =
                config.PlayerSideConfig.Teams.SelectMany(team => team.Generals.Troops.Concat(team.Troops.Troops))
                    .Concat(config.EnemySideConfig.Teams.SelectMany(team =>
                        team.Generals.Troops.Concat(team.Troops.Troops)));
            var heroes = troops.Select(troop => troop.Character.CharacterObject as CharacterObject)
                .Where(character => character != null)
                .Select(character => character.HeroObject).Where(hero => hero != null).ToList();
            if (heroes.GroupBy(hero => hero).Any(group => group.Count() > 1))
            {
                Utility.DisplayMessage("Hero should not be duplicated.");
                return false;
            }

            return true;
        }

        private static Hero FindAHeroNotInConfig(BattleConfig config)
        {
            var troops =
                config.PlayerSideConfig.Teams.SelectMany(team => team.Generals.Troops.Concat(team.Troops.Troops))
                    .Concat(config.EnemySideConfig.Teams.SelectMany(team =>
                        team.Generals.Troops.Concat(team.Troops.Troops)));
            var heroes = troops.Select(troop => troop.Character.CharacterObject as CharacterObject)
                .Where(character => character != null)
                .Select(character => character.HeroObject).Where(hero => hero != null).ToList();
            if (heroes.GroupBy(hero => hero).Any(group => group.Count() > 1))
            {
                Utility.DisplayMessage("Hero should not be duplicated.");
                return null;
            }
            return Campaign.Current.AliveHeroes.FirstOrDefault(hero => !heroes.Contains(hero) && hero.CharacterObject != null);
        }

        public static void Start(BattleConfig config, string mapId)
        {
            try
            {
                var rec = CreateMissionInitializerRecord(config, mapId);
                
                _oldPlayerCharacter = Game.Current.PlayerTroop;
                // First team and general troop always contains at least one item.
                var newPlayerCharacter =
                    config.PlayerSideConfig.Teams[0].Generals.Troops[0].Character.CharacterObject as CharacterObject;
                // Player Character must be hero, or PartyGroupAgentOrigin.IsUnderPlayersCommand would throw.
                if (newPlayerCharacter?.IsHero ?? false)
                {
                    if (!ValidateNoDuplicateHero(config))
                        return;
                    Game.Current.PlayerTroop =
                        config.PlayerSideConfig.Teams[0].Generals.Troops[0].Character.CharacterObject;
                }
                else
                {
                    // However if player team first character is not hero, we still need to set PlayerTroop,
                    // because the player character may be in the enemy team, in which case PartyGroupAgentOrigin.IsUnderPlayersCommand
                    // may think that the enemy soldier is under player command.
                    var hero = FindAHeroNotInConfig(config);
                    if (hero == null)
                        return;
                    Game.Current.PlayerTroop = hero.CharacterObject;
                }

                _oldMainParty = Campaign.Current.MainParty;

                _playerParty = CreateParty(config.PlayerSideConfig.Teams[0], true, 0);
                _playerSideParties = new List<PartyBase>() { _playerParty };
                _enemyParty = CreateParty(config.EnemySideConfig.Teams[0], false, 0);
                _enemySideParties = new List<PartyBase>() { _enemyParty };
                typeof(Campaign).GetProperty("MainParty").SetValue(Campaign.Current, _playerParty.MobileParty);
                IsEnhancedBattleTestBattle = true;

                Mission mission = null;
                switch (config.BattleTypeConfig.BattleType)
                {
                    case BattleType.Village:
                    case BattleType.Field:
                        PlayerEncounter.RestartPlayerEncounter(
                            config.BattleTypeConfig.PlayerSide == BattleSideEnum.Defender ? _playerParty : _enemyParty,
                            config.BattleTypeConfig.PlayerSide == BattleSideEnum.Attacker ? _playerParty : _enemyParty);

                        PlayerEncounter.StartBattle();
                        for (int i = 1; i < config.PlayerSideConfig.Teams.Count; ++i)
                        {
                            _playerSideParties.Add(CreateParty(config.PlayerSideConfig.Teams[i], true, i));
                            _playerSideParties[i].MobileParty.MapEventSide = _playerParty.MapEventSide;
                        }

                        for (int i = 1; i < config.EnemySideConfig.Teams.Count; ++i)
                        {
                            _enemySideParties.Add(CreateParty(config.EnemySideConfig.Teams[i], false, i));
                            _enemySideParties[i].MobileParty.MapEventSide = _enemyParty.MapEventSide;
                        } 

                        mission = CampaignMission.OpenBattleMission(rec) as Mission;
                        break;
                    case BattleType.Siege:
                        var (attackerParty, defenderParty) =
                            config.BattleTypeConfig.PlayerSide == BattleSideEnum.Attacker
                                ? (_playerParty, _enemyParty)
                                : (_enemyParty, _playerParty);
                        var (attackerParties, defenderParties) =
                            config.BattleTypeConfig.PlayerSide == BattleSideEnum.Attacker
                                ? (_playerSideParties, _enemySideParties)
                                : (_enemySideParties, _playerSideParties);

                        var settlement = new Settlement
                        {
                            Town = new Town(),
                            Culture = Campaign.Current.ObjectManager.GetObject<CultureObject>("empire")
                        };
                        // avoid Town.Owner == null
                        typeof(Town).GetProperty(nameof(Town.Owner)).SetValue(settlement.Town, settlement.Party);
                        // avoid Settlement.Town == null
                        typeof(Settlement).GetProperty(nameof(Settlement.SettlementComponent))
                            .SetValue(settlement, settlement.Town);

                        _siegeEvent =
                            Campaign.Current.SiegeEventManager.StartSiegeEvent(settlement, attackerParty.MobileParty);
                        // avoid Settlement.OwnerClan == null
                        // set this after siege event created to avoid relation impact, and also avoid crash when owner clan is neutral clan.
                        typeof(Town).GetField("_ownerClan", BindingFlags.Instance | BindingFlags.NonPublic)
                            .SetValue(settlement.Town, GetSettlementClan(config));

                        PlayerEncounter.RestartPlayerEncounter(settlement.Party, attackerParty, true);
                        if (config.MapConfig.IsSallyOutSelected)
                        {
                            PlayerEncounter.Current.ForceSallyOut = true;
                        }
                        PlayerEncounter.StartBattle();
                        defenderParty.MapEventSide = attackerParty.MapEvent.GetMapEventSide(BattleSideEnum.Defender);

                        for (int i = 1; i < config.PlayerSideConfig.Teams.Count; ++i)
                        {
                            _playerSideParties.Add(CreateParty(config.PlayerSideConfig.Teams[i], true, i));
                            _playerSideParties[i].MobileParty.MapEventSide = _playerParty.MapEventSide;
                        }

                        for (int i = 1; i < config.EnemySideConfig.Teams.Count; ++i)
                        {
                            _enemySideParties.Add(CreateParty(config.EnemySideConfig.Teams[i], false, i));
                            _enemySideParties[i].MobileParty.MapEventSide = _enemyParty.MapEventSide;
                        }

                        foreach (var partyBase in attackerParties)
                        {
                            partyBase.MobileParty.BesiegerCamp = _siegeEvent.BesiegerCamp;
                        }

                        foreach (var partyBase in defenderParties)
                        {
                            partyBase.MobileParty.CurrentSettlement = settlement;
                        }

                        var siegeWeaponsCountOfAttackers = GetSiegeWeapons(
                            config.SiegeMachineConfig.AttackerMeleeMachines.Concat(config.SiegeMachineConfig
                                .AttackerRangedMachines), BattleSideEnum.Attacker);
                        var siegeWeaponsCountOfDefenders = GetSiegeWeapons(config.SiegeMachineConfig.DefenderMachines,
                            BattleSideEnum.Defender);


                        // Fix for weather/time settings being ignored in SiegeMission
                        Patch_Initializer.Init(rec.AtmosphereOnCampaign, rec.TimeOfDay);

                        mission = CampaignMission.OpenSiegeMissionWithDeployment(mapId,
                            GetWallHitpointPercentages(config.MapConfig.BreachedWallCount),
                            HasAnySiegeTower(siegeWeaponsCountOfAttackers),
                            siegeWeaponsCountOfAttackers,
                            siegeWeaponsCountOfDefenders,
                            config.BattleTypeConfig.PlayerSide == BattleSideEnum.Attacker,
                            config.MapConfig.SceneLevel,
                            config.MapConfig.IsSallyOutSelected,
                            false) as Mission;
                        break;
                }
            }
            catch (Exception e)
            {
                Utility.DisplayMessage(e.ToString());
                // clean objects on exception
                BeforeMissionEnded();
                MissionEnded();
            }
            finally
            {
                // Reset Initializer behavior after Mission is created
                Patch_Initializer.Reset();
            }
        }

        // Revert main character and party slightly before MissionEnded to prevent mod conflict
        public static void BeforeMissionEnded()
        {
            RollbackPartyBanners();

            if (_oldPlayerCharacter != null)
            {
                Game.Current.PlayerTroop = _oldPlayerCharacter;
            }

            _mapEvent = MapEvent.PlayerMapEvent;

            typeof(Campaign).GetProperty("MainParty").SetValue(Campaign.Current, _oldMainParty);
        }

        public static void MissionEnded()
        {
            try
            {
                //RollbackPartyBanners();

                for (int i = _enemySideParties.Count - 1; i > 0; --i)
                {
                    (_enemySideParties[i].MobileParty.PartyComponent as EnhancedBattleTestPartyComponent).RemoveHeroes();
                    _enemySideParties[i].MapEventSide = null;
                    _enemySideParties[i].MobileParty?.RemoveParty();
                }
                for (int i = _playerSideParties.Count - 1; i > 0; --i)
                {
                    (_playerSideParties[i].MobileParty.PartyComponent as EnhancedBattleTestPartyComponent).RemoveHeroes();
                    _playerSideParties[i].MapEventSide = null;
                    _playerSideParties[i].MobileParty?.RemoveParty();
                }
                (_enemyParty?.MobileParty.PartyComponent as EnhancedBattleTestPartyComponent)?.RemoveHeroes();
                (_playerParty?.MobileParty.PartyComponent as EnhancedBattleTestPartyComponent)?.RemoveHeroes();
                RecoverHeroes();
                //if (_oldPlayerCharacter != null)
                //{
                //    Game.Current.PlayerTroop = _oldPlayerCharacter;
                //}
                _oldPlayerCharacter = null;
                ResetMapEventSide(_playerParty);
                ResetMapEventSide(_enemyParty);

                _siegeEvent?.BesiegerCamp.FinalizeSiegeEvent();
                _siegeEvent?.BesiegedSettlement.FinalizeSiegeEvent();
                List<SiegeEvent> siegeEvents = (List<SiegeEvent>)typeof(SiegeEventManager)
                    .GetField("_siegeEvents", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(Campaign.Current.SiegeEventManager);
                siegeEvents.Remove(_siegeEvent);
                _siegeEvent = null;
                _mapEvent?.FinalizeEvent();
                List<MapEvent> mapEvents = (List<MapEvent>)typeof(MapEventManager)
                    .GetField("_mapEvents", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(Campaign.Current.MapEventManager);
                mapEvents.Remove(_mapEvent);
                _mapEvent = null;
                PlayerEncounter.Finish();
                //typeof(Campaign).GetProperty("MainParty").SetValue(Campaign.Current, _oldMainParty);
                _oldMainParty = null;
                _playerParty?.MobileParty?.RemoveParty();
                _enemyParty?.MobileParty?.RemoveParty();
                _playerSideParties.Clear();
                _enemySideParties.Clear();
                _playerParty = null;
                _enemyParty = null;
                _originalHitPoints.Clear();
                _originalParties.Clear();
                _originalSettlements.Clear();
                _bannerSave.Clear();
                Campaign.Current.MainParty.Party.Visuals?.SetMapIconAsDirty();
                IsEnhancedBattleTestBattle = false;
            }
            catch (Exception e)
            {
                Utility.DisplayMessage(e.ToString());
                IsEnhancedBattleTestBattle = false;
            }
        }

        private static Clan GetSettlementClan(BattleConfig config)
        {
            var defenderPrimaryTeam = (config.BattleTypeConfig.PlayerSide == BattleSideEnum.Attacker
                ? config.EnemySideConfig
                : config.PlayerSideConfig).Teams[0];
            if (defenderPrimaryTeam.HasGeneral)
            {
                var character = defenderPrimaryTeam.Generals.Troops[0].Character.CharacterObject as CharacterObject;
                if (character?.IsHero ?? false)
                {
                    return character.HeroObject.Clan;
                }
            }

            return CampaignData.NeutralFaction;
        }

        private static void ResetMapEventSide(PartyBase partyBase)
        {
            if (partyBase != null)
                partyBase.MapEventSide = null;
        }

        private static MissionInitializerRecord CreateMissionInitializerRecord(BattleConfig config, string map)
        {
            string scene;
            MapPatchData mapPatchAtPosition = Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position2D);
            // When in field battle if overrides campaign map is not turned on, use player position to generate a map
            if (config.BattleTypeConfig.BattleType == BattleType.Field && !config.MapConfig.OverridesPlayerPosition)
            {
                scene = PlayerEncounter.GetBattleSceneForMapPatch(mapPatchAtPosition);
            }
            else
            {
                scene = map;
            }
            MissionInitializerRecord rec = new MissionInitializerRecord(scene);
            rec.TerrainType = (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace);
            rec.DamageToPlayerMultiplier = Campaign.Current.Models.DifficultyModel.GetDamageToPlayerMultiplier();
            rec.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
            rec.DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
            rec.NeedsRandomTerrain = false;
            rec.PlayingInCampaignMode = true;
            rec.RandomTerrainSeed = MBRandom.RandomInt(10000);
            rec.AtmosphereOnCampaign = AtmosphereModel.CreateAtmosphereInfoForMission(config.MapConfig.Season, config.MapConfig.TimeOfDay, true);
            rec.AtlasGroup = 2;
            if (config.BattleTypeConfig.BattleType == BattleType.Field && !config.MapConfig.OverridesPlayerPosition)
            {
                rec.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(CampaignTime.Now, MobileParty.MainParty.GetLogicalPosition());
                float num = Campaign.CurrentTime % 24f;
                if (Campaign.Current != null)
                    rec.TimeOfDay = num;
                rec.SceneHasMapPatch = true;
                rec.PatchCoordinates = mapPatchAtPosition.normalizedCoordinates;
                Vec2 direction = Campaign.Current.MainParty.Bearing;
                if (config.BattleTypeConfig.PlayerSide == BattleSideEnum.Attacker)
                {
                    direction.RotateCCW(MBMath.PI);
                }
                rec.PatchEncounterDir = direction;
            }
            else
            {
                rec.AtmosphereOnCampaign = AtmosphereModel.CreateAtmosphereInfoForMission(config.MapConfig.Season, config.MapConfig.TimeOfDay, true);
                rec.TimeOfDay = config.MapConfig.TimeOfDay;
            }

            GameTexts.SetVariable("MapName", scene);
            Utility.DisplayLocalizedText("str_ebt_current_map");

            return rec;
        }

        private static PartyBase CreateParty(TeamConfig teamConfig, bool isPlayerSide, int index = 0)
        {
            PartyBase party = MobileParty.CreateParty("EnhancedBattleTestParty",
                new EnhancedBattleTestPartyComponent(GetPartyName(isPlayerSide, index), teamConfig)).Party;            
            TryOverridePartyBanner(party, teamConfig);
            return party;
        }

        private static void TryOverridePartyBanner(PartyBase party, TeamConfig config)
        {
            if (config.CustomBanner && party.Banner != null && !_bannerSave.ContainsKey(party.Banner))
            {
                _bannerSave.Add(party.Banner, party.Banner.Serialize());
                party.Banner.Deserialize(config.BannerKey);                
            }
        }

        private static void TryRecoverPartyBanner(PartyBase party)
        {
            if (party.Banner != null && _bannerSave.TryGetValue(party.Banner, out var bannerValue))
            {
                party.Banner.Deserialize(bannerValue);
                _bannerSave.Remove(party.Banner);
            }
        }

        // Rollback party banners to their initial value
        private static void RollbackPartyBanners()
        {
            foreach (PartyBase party in _playerSideParties)
            {
                TryRecoverPartyBanner(party);
            }
            foreach (PartyBase party in _enemySideParties)
            {
                TryRecoverPartyBanner(party);
            }
        }

        public static TextObject GetPartyName(bool isPlayerSide, int index)
        {
            return GameTexts.FindText("str_ebt_party_name").SetTextVariable("PARTY_TEXT", isPlayerSide
                ? new TextObject("{=sSJSTe5p}Player Party")
                : new TextObject("{=0xC75dN6}Enemy Party")).SetTextVariable("INDEX", index);
        }

        public static void RecoverHeroes()
        {
            foreach (var originalHitPoint in _originalHitPoints)
            {
                originalHitPoint.Key.HitPoints = originalHitPoint.Value;
            }
            foreach (var pair in _originalParties)
            {
                switch (pair.Value.Value)
                {
                    case MemberState.Leader:
                        if (pair.Key.PartyBelongedTo?.Party == pair.Value.Key)
                            continue;
                        if (pair.Value.Key.MobileParty.MemberRoster.Contains(pair.Key.CharacterObject))
                            pair.Value.Key.AddElementToMemberRoster(pair.Key.CharacterObject, -1);
                        pair.Value.Key.AddElementToMemberRoster(pair.Key.CharacterObject, 1, true);
                        pair.Value.Key.MobileParty.ChangePartyLeader(pair.Key);
                        break;
                    case MemberState.Prisoner:
                        if (pair.Key.PartyBelongedToAsPrisoner == pair.Value.Key)
                            continue;
                        if (pair.Value.Key.MobileParty.PrisonRoster.Contains(pair.Key.CharacterObject))
                            pair.Value.Key.AddPrisoner(pair.Key.CharacterObject, -1);
                        pair.Value.Key.AddPrisoner(pair.Key.CharacterObject, 1);
                        break;
                    case MemberState.Original:
                        if (pair.Key.PartyBelongedTo?.Party == pair.Value.Key)
                            continue;
                        if (pair.Value.Key.MobileParty.MemberRoster.Contains(pair.Key.CharacterObject))
                            pair.Value.Key.AddElementToMemberRoster(pair.Key.CharacterObject, -1);
                        pair.Value.Key.AddElementToMemberRoster(pair.Key.CharacterObject, 1);
                        break;
                }
            }

            foreach (var pair in _originalSettlements)
            {
                pair.Key.StayingInSettlement = pair.Value;
            }
        }

        public static void RegisterHero(Hero hero)
        {
            if (!_originalHitPoints.ContainsKey(hero))
            {
                _originalHitPoints[hero] = hero.HitPoints;
                hero.HitPoints = hero.MaxHitPoints;
            }
            if (!_originalSettlements.ContainsKey(hero) && !_originalParties.ContainsKey(hero))
            {
                if (hero.StayingInSettlement != null)
                {
                    _originalSettlements[hero] = hero.StayingInSettlement;
                }
                else if (hero.PartyBelongedTo != null)
                {
                    _originalParties[hero] = new KeyValuePair<PartyBase, MemberState>(
                        hero.PartyBelongedTo.Party,
                        hero.PartyBelongedTo.LeaderHero == hero ? MemberState.Leader : MemberState.Original);
                }
                else if (hero.PartyBelongedToAsPrisoner != null)
                {
                    _originalParties[hero] = new KeyValuePair<PartyBase, MemberState>(
                        hero.PartyBelongedToAsPrisoner, MemberState.Prisoner);
                }
            }
        }

        private static float[] GetWallHitpointPercentages(int breachedWallCount)
        {
            float[] hitpointPercentages = new float[2];
            switch (breachedWallCount)
            {
                case 0:
                    hitpointPercentages[0] = 1f;
                    hitpointPercentages[1] = 1f;
                    break;
                case 1:
                    int index = MBRandom.RandomInt(2);
                    hitpointPercentages[index] = 0.0f;
                    hitpointPercentages[1 - index] = 1f;
                    break;
                default:
                    hitpointPercentages[0] = 0.0f;
                    hitpointPercentages[1] = 0.0f;
                    break;
            }
            return hitpointPercentages;
        }

        private static List<MissionSiegeWeapon> GetSiegeWeapons(IEnumerable<string> machines, BattleSideEnum side)
        {
            var result = new List<MissionSiegeWeapon>();
            foreach (var (machine, i) in machines.Select((machine, i) => (machine, i)))
            {
                SiegeEngineType siegeWeaponType = Utility.GetSiegeEngineType(machine);
                if (siegeWeaponType != null)
                {
                    var hitPoints =
                        Campaign.Current.Models.SiegeEventModel.GetSiegeEngineHitPoints(PlayerSiege.PlayerSiegeEvent,
                            siegeWeaponType, side);
                    result.Add(MissionSiegeWeapon.CreateCampaignWeapon(siegeWeaponType, i, hitPoints, hitPoints));
                }
            }

            return result;
        }

        private static bool HasAnySiegeTower(List<MissionSiegeWeapon> attackerMachines)
        {
            return attackerMachines.Exists((Predicate<MissionSiegeWeapon>)(data => data.Type == DefaultSiegeEngineTypes.SiegeTower || data.Type == DefaultSiegeEngineTypes.HeavySiegeTower));
        }
    }
}
