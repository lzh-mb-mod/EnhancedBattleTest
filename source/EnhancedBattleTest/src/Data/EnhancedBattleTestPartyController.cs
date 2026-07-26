using EnhancedBattleTest.Config;
using EnhancedBattleTest.SinglePlayer.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.TroopSuppliers;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.Data
{
    public static class EnhancedBattleTestPartyController
    {
        public sealed class BattleContext
        {
            public MobileParty PlayerParty { get; }
            public MobileParty EnemyParty { get; }
            public IReadOnlyList<MobileParty> PlayerParties { get; }
            public IReadOnlyList<MobileParty> EnemyParties { get; }
            public IReadOnlyList<MobileParty> OwnedTemporaryParties { get; }
            public MapEvent MapEvent { get; }
            public PartyGroupTroopSupplier[] TroopSuppliers { get; }
            public MapEventSide OriginalMainPartyMapEventSide { get; }
            public CharacterObject PlayerCharacter { get; }
            public IReadOnlyList<CharacterObject> PlayerSpawnPriorityCharacters { get; }
            public IReadOnlyList<string> PlayerPriorityCharacterIds { get; }
            public IReadOnlyDictionary<Hero, int> OriginalHeroHitPoints { get; }
            public EquipmentModifierType EquipmentModifierType { get; }

            public BattleContext(
                MobileParty playerParty,
                MobileParty enemyParty,
                IReadOnlyList<MobileParty> playerParties,
                IReadOnlyList<MobileParty> enemyParties,
                IReadOnlyList<MobileParty> ownedTemporaryParties,
                MapEvent mapEvent,
                PartyGroupTroopSupplier[] troopSuppliers,
                MapEventSide originalMainPartyMapEventSide,
                CharacterObject playerCharacter,
                IReadOnlyList<CharacterObject> playerSpawnPriorityCharacters,
                IReadOnlyList<string> playerPriorityCharacterIds,
                IReadOnlyDictionary<Hero, int> originalHeroHitPoints,
                EquipmentModifierType equipmentModifierType)
            {
                PlayerParty = playerParty;
                EnemyParty = enemyParty;
                PlayerParties = playerParties;
                EnemyParties = enemyParties;
                OwnedTemporaryParties = ownedTemporaryParties;
                MapEvent = mapEvent;
                TroopSuppliers = troopSuppliers;
                OriginalMainPartyMapEventSide = originalMainPartyMapEventSide;
                PlayerCharacter = playerCharacter;
                PlayerSpawnPriorityCharacters = playerSpawnPriorityCharacters;
                PlayerPriorityCharacterIds = playerPriorityCharacterIds;
                OriginalHeroHitPoints = originalHeroHitPoints;
                EquipmentModifierType = equipmentModifierType;
            }
        }

        public static BattleContext Current { get; private set; }
        private static bool _isCleaningUp;
        private static bool _isCreatingTemporaryParty;
        private static readonly HashSet<MobileParty> TemporaryParties =
            new HashSet<MobileParty>();
        private static readonly HashSet<Hero> ParticipatingHeroes =
            new HashSet<Hero>();
        private static readonly Dictionary<MobileParty, Tuple<Banner, uint, uint>>
            TemporaryPartyAppearances =
                new Dictionary<MobileParty, Tuple<Banner, uint, uint>>();
        private static readonly Dictionary<MobileParty, bool>
            TemporaryPartyPlayerTeamMembership =
                new Dictionary<MobileParty, bool>();

        public static BattleContext Create(BattleConfig config)
        {
            Cleanup();

            MobileParty playerParty = null;
            MobileParty enemyParty = null;
            var playerParties = new List<MobileParty>();
            var enemyParties = new List<MobileParty>();
            var ownedTemporaryParties = new List<MobileParty>();
            MapEvent mapEvent = null;
            MapEventSide originalMainPartyMapEventSide =
                PartyBase.MainParty.MapEventSide;
            CharacterObject selectedPlayerCharacter =
                GetPlayerCharacter(config.PlayerTeamConfig);
            Dictionary<Hero, int> originalHeroHitPoints = null;
            try
            {
                BattleSideEnum playerSide = config.BattleTypeConfig.PlayerSide;
                bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
                BasicCharacterObject playerBannerCharacter =
                    GetPlayerBannerCharacter(
                        config,
                        selectedPlayerCharacter);
                playerParty = CreateParty(
                    new TextObject("{=sSJSTe5p}Player Party"),
                    config.PlayerTeamConfig.PrimaryParty,
                    isPlayerAttacker,
                    selectedPlayerCharacter,
                    selectedPlayerCharacter != null,
                    true,
                    playerBannerCharacter,
                    out CharacterObject playerCharacter,
                    out List<CharacterObject> playerSpawnPriorityCharacters,
                    out List<string> playerPriorityCharacterIds);
                playerParties.Add(playerParty);
                ownedTemporaryParties.Add(playerParty);
                CreateAlliedParties(
                    config.PlayerTeamConfig.AlliedParties,
                    true,
                    isPlayerAttacker,
                    playerParties,
                    ownedTemporaryParties,
                    playerSpawnPriorityCharacters,
                    config.PlayerTeamConfig.PrimaryParty.IsInArmy);

                var enemySpawnPriorityCharacters = new List<CharacterObject>();
                enemyParty = CreateParty(
                    new TextObject("{=0xC75dN6}Enemy Party"),
                    config.EnemyTeamConfig.PrimaryParty,
                    !isPlayerAttacker,
                    null,
                    false,
                    false,
                    null,
                    out _,
                    out enemySpawnPriorityCharacters,
                    out _);
                enemyParties.Add(enemyParty);
                ownedTemporaryParties.Add(enemyParty);
                CreateAlliedParties(
                    config.EnemyTeamConfig.AlliedParties,
                    false,
                    !isPlayerAttacker,
                    enemyParties,
                    ownedTemporaryParties,
                    enemySpawnPriorityCharacters,
                    false);
                originalHeroHitPoints = CaptureHeroHitPoints(ownedTemporaryParties);
                ParticipatingHeroes.UnionWith(originalHeroHitPoints.Keys);
                PrepareHeroesForBattle(originalHeroHitPoints.Keys);

                PartyBase attacker = playerSide == BattleSideEnum.Attacker
                    ? playerParty.Party
                    : enemyParty.Party;
                PartyBase defender = playerSide == BattleSideEnum.Defender
                    ? playerParty.Party
                    : enemyParty.Party;

                mapEvent = FieldBattleEventComponent
                    .CreateFieldBattleEvent(attacker, defender)
                    .MapEvent;
                AttachAlliedParties(
                    mapEvent,
                    playerSide,
                    playerParties.Skip(1));
                AttachAlliedParties(
                    mapEvent,
                    playerSide == BattleSideEnum.Attacker
                        ? BattleSideEnum.Defender
                        : BattleSideEnum.Attacker,
                    enemyParties.Skip(1));
                var suppliers = new PartyGroupTroopSupplier[2];
                FlattenedTroopRoster playerPriorityTroops =
                    CreatePriorityRoster(playerSpawnPriorityCharacters);
                FlattenedTroopRoster enemyPriorityTroops =
                    CreatePriorityRoster(enemySpawnPriorityCharacters);
                suppliers[(int)BattleSideEnum.Defender] =
                    new PartyGroupTroopSupplier(
                        mapEvent,
                        BattleSideEnum.Defender,
                        playerSide == BattleSideEnum.Defender
                            ? playerPriorityTroops
                            : enemyPriorityTroops);
                suppliers[(int)BattleSideEnum.Attacker] =
                    new PartyGroupTroopSupplier(
                        mapEvent,
                        BattleSideEnum.Attacker,
                        playerSide == BattleSideEnum.Attacker
                            ? playerPriorityTroops
                            : enemyPriorityTroops);

                PartyBase.MainParty.MapEventSide =
                    mapEvent.GetMapEventSide(playerSide);
                Current = new BattleContext(
                    playerParty,
                    enemyParty,
                    playerParties,
                    enemyParties,
                    ownedTemporaryParties,
                    mapEvent,
                    suppliers,
                    originalMainPartyMapEventSide,
                    playerCharacter,
                    playerSpawnPriorityCharacters,
                    playerPriorityCharacterIds,
                    originalHeroHitPoints,
                    config.BattleTypeConfig.EquipmentModifierType);
                EnhancedBattleTestSaveGuard.Disable();
                return Current;
            }
            catch
            {
                try
                {
                    try
                    {
                        RestoreMainPartyMapEventSide(
                            mapEvent,
                            originalMainPartyMapEventSide);
                        PrepareMapEventForCleanup(mapEvent);
                        mapEvent?.FinalizeEvent();
                    }
                    finally
                    {
                        DestroyParties(ownedTemporaryParties);
                    }
                }
                finally
                {
                    try
                    {
                        RestoreHeroHitPoints(originalHeroHitPoints);
                    }
                    finally
                    {
                        ParticipatingHeroes.Clear();
                    }
                }
                throw;
            }
        }

        public static bool IsTestParty(PartyBase party)
        {
            return party != null
                   && party.MobileParty != null
                   && TemporaryParties.Contains(party.MobileParty);
        }

        public static bool IsParticipatingHero(Hero hero)
        {
            return hero != null && ParticipatingHeroes.Contains(hero);
        }

        public static bool ShouldIgnoreHeroPartyChange(MobileParty party)
        {
            return party != null
                   && (_isCreatingTemporaryParty
                       || TemporaryParties.Contains(party));
        }

        public static bool IsPartyInPlayerTeam(PartyBase party)
        {
            return party?.MobileParty != null
                   && TemporaryPartyPlayerTeamMembership.TryGetValue(
                       party.MobileParty,
                       out bool isInPlayerTeam)
                   && isInPlayerTeam;
        }

        public static bool TryGetTemporaryPartyAppearance(
            PartyBase party,
            out Banner banner,
            out Tuple<uint, uint> colors)
        {
            if (party?.MobileParty != null
                && TemporaryPartyAppearances.TryGetValue(
                    party.MobileParty,
                    out Tuple<Banner, uint, uint> appearance))
            {
                banner = appearance.Item1;
                colors = new Tuple<uint, uint>(
                    appearance.Item2,
                    appearance.Item3);
                return true;
            }

            banner = null;
            colors = null;
            return false;
        }

        public static void Cleanup()
        {
            if (_isCleaningUp)
                return;

            BattleContext context = Current;
            if (context == null)
                return;

            _isCleaningUp = true;
            try
            {
                try
                {
                    RestoreMainPartyMapEventSide(
                        context.MapEvent,
                        context.OriginalMainPartyMapEventSide);
                    PrepareMapEventForCleanup(context.MapEvent);
                    context.MapEvent?.FinalizeEvent();
                }
                finally
                {
                    DestroyParties(context.OwnedTemporaryParties);
                }
            }
            finally
            {
                try
                {
                    RestoreHeroHitPoints(context.OriginalHeroHitPoints);
                }
                finally
                {
                    ParticipatingHeroes.Clear();
                    Current = null;
                    _isCleaningUp = false;
                }
            }
        }

        private static MobileParty CreateParty(
            TextObject name,
            PartyConfig config,
            bool isAttacker,
            CharacterObject controlledCharacter,
            bool addControlledCharacter,
            bool isInPlayerTeam,
            BasicCharacterObject preferredBannerCharacter,
            out CharacterObject playerCharacter,
            out List<CharacterObject> spawnPriorityCharacters,
            out List<string> priorityCharacterIds)
        {
            BasicCultureObject culture = Utility.GetCulture(config);
            Hero owner = GetOwner(config, culture);
            Clan clan = owner?.Clan
                        ?? Clan.All.FirstOrDefault(candidate => candidate.Culture == culture)
                        ?? Clan.PlayerClan;
            TroopRoster roster = CreateRoster(
                config,
                controlledCharacter,
                addControlledCharacter,
                out playerCharacter,
                out spawnPriorityCharacters,
                out priorityCharacterIds);
            config.ResolveAppearance(
                isAttacker,
                out Banner banner,
                out Tuple<uint, uint> colors,
                preferredBannerCharacter);
            MobileParty party = null;
            _isCreatingTemporaryParty = true;
            try
            {
                party = CustomPartyComponent.CreateQuestParty(
                    MobileParty.MainParty.Position2D,
                    0f,
                    null,
                    name,
                    clan,
                    roster,
                    TroopRoster.CreateDummyTroopRoster(),
                    owner,
                    avoidHostileActions: true);
                TemporaryParties.Add(party);
                TemporaryPartyAppearances.Add(
                    party,
                    new Tuple<Banner, uint, uint>(
                        banner,
                        colors.Item1,
                        colors.Item2));
                TemporaryPartyPlayerTeamMembership.Add(
                    party,
                    isInPlayerTeam);
                party.IsVisible = false;
                party.Ai.SetMoveModeHold();
                return party;
            }
            catch
            {
                DestroyParty(party);
                throw;
            }
            finally
            {
                _isCreatingTemporaryParty = false;
            }
        }

        private static TroopRoster CreateRoster(
            PartyConfig config,
            CharacterObject controlledCharacter,
            bool addControlledCharacter,
            out CharacterObject playerCharacter,
            out List<CharacterObject> spawnPriorityCharacters,
            out List<string> priorityCharacterIds)
        {
            TroopRoster roster = TroopRoster.CreateDummyTroopRoster();
            playerCharacter = controlledCharacter;
            spawnPriorityCharacters = new List<CharacterObject>();
            priorityCharacterIds = new List<string>();
            if (config.HasGeneral)
            {
                foreach (TroopConfig troop in config.Generals.Troops)
                {
                    if (troop.Character.CharacterObject is CharacterObject character)
                    {
                        roster.AddToCounts(character, 1);
                        if (!spawnPriorityCharacters.Contains(character))
                            spawnPriorityCharacters.Add(character);
                        priorityCharacterIds.Add(character.StringId);
                    }
                }
            }

            foreach (TroopConfig troop in config.Troops.Troops)
            {
                if (troop.Number > 0
                    && troop.Character.CharacterObject is CharacterObject character)
                {
                    roster.AddToCounts(character, troop.Number);
                    if (character.IsHero)
                        priorityCharacterIds.Add(character.StringId);
                }
            }

            if (addControlledCharacter && controlledCharacter != null)
            {
                if (!roster.Contains(controlledCharacter))
                    roster.AddToCounts(controlledCharacter, 1);
                spawnPriorityCharacters.Remove(controlledCharacter);
                spawnPriorityCharacters.Insert(0, controlledCharacter);
                if (!priorityCharacterIds.Contains(controlledCharacter.StringId))
                    priorityCharacterIds.Add(controlledCharacter.StringId);
            }

            return roster;
        }

        private static CharacterObject GetPlayerCharacter(TeamConfig config)
        {
            return config.PlayerCharacter?.CharacterObject as CharacterObject;
        }

        private static Dictionary<Hero, int> CaptureHeroHitPoints(
            IEnumerable<MobileParty> parties)
        {
            return parties
                .Where(party => party != null)
                .SelectMany(party => party.MemberRoster.GetTroopRoster())
                .Select(element => element.Character.HeroObject)
                .Where(hero => hero != null)
                .Distinct()
                .ToDictionary(hero => hero, hero => hero.HitPoints);
        }

        private static void PrepareHeroesForBattle(IEnumerable<Hero> heroes)
        {
            foreach (Hero hero in heroes)
                hero.HitPoints = hero.MaxHitPoints;
        }

        private static FlattenedTroopRoster CreatePriorityRoster(
            IEnumerable<CharacterObject> characters)
        {
            var roster = new FlattenedTroopRoster();
            foreach (CharacterObject character in characters)
                roster.Add(character, 1);
            return roster;
        }

        private static void RestoreHeroHitPoints(
            IReadOnlyDictionary<Hero, int> originalHeroHitPoints)
        {
            if (originalHeroHitPoints == null)
                return;

            foreach (KeyValuePair<Hero, int> entry in originalHeroHitPoints)
                entry.Key.HitPoints = entry.Value;
        }

        private static Hero GetOwner(PartyConfig config, BasicCultureObject culture)
        {
            Hero selectedHero = config.HasGeneral
                ? config.Generals.Troops
                    .Select(troop => (troop.Character as SPCharacterConfig)?.ActualCharacterObject?.HeroObject)
                    .FirstOrDefault(hero => hero != null)
                : null;
            return selectedHero
                   ?? Hero.AllAliveHeroes.FirstOrDefault(hero => hero.Culture == culture)
                   ?? Hero.MainHero;
        }

        private static void DestroyParty(MobileParty party)
        {
            if (party == null)
                return;

            try
            {
                if (party.IsActive)
                    DestroyPartyAction.Apply(null, party);
            }
            finally
            {
                TemporaryParties.Remove(party);
                TemporaryPartyAppearances.Remove(party);
                TemporaryPartyPlayerTeamMembership.Remove(party);
            }
        }

        private static void CreateAlliedParties(
            IEnumerable<PartyConfig> configs,
            bool isPlayerSide,
            bool isAttacker,
            ICollection<MobileParty> sideParties,
            ICollection<MobileParty> ownedTemporaryParties,
            ICollection<CharacterObject> spawnPriorityCharacters,
            bool playerPrimaryPartyIsInArmy)
        {
            if (configs == null)
                return;

            int index = 1;
            foreach (PartyConfig config in configs.Where(candidate => candidate != null))
            {
                TextObject name = GameTexts.FindText(
                    isPlayerSide
                        ? "str_ebt_player_allied_party"
                        : "str_ebt_enemy_allied_party");
                name.SetTextVariable("INDEX", index++);
                MobileParty party = CreateParty(
                    name,
                    config,
                    isAttacker,
                    null,
                    false,
                    isPlayerSide
                    && playerPrimaryPartyIsInArmy
                    && config.IsInArmy,
                    null,
                    out _,
                    out List<CharacterObject> partySpawnPriorityCharacters,
                    out _);
                sideParties.Add(party);
                ownedTemporaryParties.Add(party);
                foreach (CharacterObject character in partySpawnPriorityCharacters)
                {
                    if (!spawnPriorityCharacters.Contains(character))
                        spawnPriorityCharacters.Add(character);
                }
            }
        }

        private static void AttachAlliedParties(
            MapEvent mapEvent,
            BattleSideEnum side,
            IEnumerable<MobileParty> alliedParties)
        {
            MapEventSide mapEventSide = mapEvent.GetMapEventSide(side);
            foreach (MobileParty party in alliedParties)
                party.Party.MapEventSide = mapEventSide;
        }

        private static BasicCharacterObject GetPlayerBannerCharacter(
            BattleConfig config,
            CharacterObject playerCharacter)
        {
            if (config.BattleTypeConfig.PlayerType == PlayerType.Commander)
                return playerCharacter;

            return config.PlayerTeamConfig.PrimaryParty
                       .GetFirstGeneralCharacter(playerCharacter)
                   ?? playerCharacter;
        }

        private static void DestroyParties(
            IEnumerable<MobileParty> parties)
        {
            if (parties == null)
                return;

            Exception firstException = null;
            foreach (MobileParty party in parties.Reverse().ToList())
            {
                try
                {
                    DestroyParty(party);
                }
                catch (Exception exception)
                {
                    if (firstException == null)
                        firstException = exception;
                }
            }

            if (firstException != null)
                throw firstException;
        }

        private static void PrepareMapEventForCleanup(MapEvent mapEvent)
        {
            if (mapEvent == null)
                return;

            mapEvent.ResetBattleState();
            mapEvent.AttackerSide.Casualties = 0;
            mapEvent.DefenderSide.Casualties = 0;
        }

        private static void RestoreMainPartyMapEventSide(
            MapEvent mapEvent,
            MapEventSide originalMapEventSide)
        {
            if (PartyBase.MainParty?.MapEvent == mapEvent)
                PartyBase.MainParty.MapEventSide = originalMapEventSide;
        }

    }
}
