using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnhancedBattleTest.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using Campaign = EnhancedBattleTest.GameMode.Campaign;

namespace EnhancedBattleTest
{
    public static class Utility
    {
        public static void DisplayLocalizedText(string id, string variation = null)
        {
            DisplayMessageImpl(GameTexts.FindText(id, variation).ToString());
        }
        public static void DisplayLocalizedText(string id, string variation, Color color)
        {
            DisplayMessageImpl(GameTexts.FindText(id, variation).ToString(), color);
        }
        public static void DisplayMessage(string msg)
        {
            DisplayMessageImpl(new TaleWorlds.Localization.TextObject(msg).ToString());
        }
        public static void DisplayMessage(string msg, Color color)
        {
            DisplayMessageImpl(new TaleWorlds.Localization.TextObject(msg).ToString(), color);
        }

        private static void DisplayMessageImpl(string str)
        {
            InformationManager.DisplayMessage(new InformationMessage("Enhanced Battle Test: " + str));
        }

        private static void DisplayMessageImpl(string str, Color color)
        {
            InformationManager.DisplayMessage(new InformationMessage("Enhanced Battle Test: " + str, color));
        }

        //TODO: multilplayer related
        /*
        public static List<MPPerkObject> GetAllSelectedPerks(MultiplayerClassDivisions.MPHeroClass mpHeroClass,
            int[] selectedPerks)
        {
            List<MPPerkObject> selectedPerkList = new List<MPPerkObject>();
            for (int i = 0; i < selectedPerks.Length; ++i)
            {
                var perks = mpHeroClass.GetAllAvailablePerksForListIndex(i);
                if (perks.IsEmpty())
                    continue;
                selectedPerkList.Add(perks[selectedPerks[i]]);
            }

            return selectedPerkList;
        }

        public static IEnumerable<PerkEffect> SelectRandomPerkEffectsForPerks(
            MultiplayerClassDivisions.MPHeroClass mpHeroClass,
            bool isPlayer,
            PerkType perkType,
            int[] selectedPerks)
        {
            var selectedPerkList = GetAllSelectedPerks(mpHeroClass, selectedPerks);
            return MPPerkObject.SelectRandomPerkEffectsForPerks(isPlayer, perkType, selectedPerkList);
        }
        */

        public static Equipment GetNewEquipmentsForPerks(
            MultiplayerClassDivisions.MPHeroClass heroClass,
            bool isHero,
            int firstPerk,
            int secondPerk,
            bool fixedEquipment, int seed = -1)
        {
            //TODO: multilplayer related
            throw new NotImplementedException();
            /*
            BasicCharacterObject character = isHero ? heroClass.HeroCharacter : heroClass.TroopCharacter;
            Equipment equipment = fixedEquipment
                ? character.Equipment.Clone()
                : Equipment.GetRandomEquipmentElements(character, false, false, seed);
            foreach (PerkEffect perkEffectsForPerk in SelectRandomPerkEffectsForPerks(heroClass, isHero,
                PerkType.PerkAlternativeEquipment, new[]
                {
                    firstPerk, secondPerk
                }))
                equipment[perkEffectsForPerk.NewItemIndex] = perkEffectsForPerk.NewItem.EquipmentElement;
            return equipment;
            */
        }

        public static uint ClothingColor1(BasicCultureObject culture, bool isAttacker)
        {
            return isAttacker ? culture.Color : culture.ClothAlternativeColor;
        }

        public static uint ClothingColor2(BasicCultureObject culture, bool isAttacker)
        {
            return isAttacker ? culture.Color2 : culture.ClothAlternativeColor2;
        }

        public static uint BackgroundColor(BasicCultureObject culture, bool isAttacker)
        {
            return isAttacker ? culture.BackgroundColor1 : culture.BackgroundColor2;
        }

        public static uint ForegroundColor(BasicCultureObject culture, bool isAttacker)
        {
            return isAttacker ? culture.ForegroundColor1 : culture.ForegroundColor2;
        }

        public static Banner BannerFor(BasicCultureObject culture, bool isAttacker)
        {
            if (culture.BannerKey != null)
                return new Banner(culture.BannerKey, BackgroundColor(culture, isAttacker),
                    ForegroundColor(culture, isAttacker));
            else
            {
                var banner = Banner.CreateRandomBanner();
                banner.ChangePrimaryColor(BackgroundColor(culture, isAttacker));
                banner.ChangeIconColors(ForegroundColor(culture, isAttacker));
                return banner;
            }
        }

        public static Banner SPBannerFor(BasicCultureObject culture, bool isAttacker)
        {
            if (culture.BannerKey != null)
                return new Banner(culture.BannerKey, ClothingColor1(culture, isAttacker),
                    ClothingColor2(culture, isAttacker));
            else
            {
                var banner = Banner.CreateRandomBanner();
                banner.ChangePrimaryColor(ClothingColor1(culture, isAttacker));
                banner.ChangeIconColors(ClothingColor2(culture, isAttacker));
                return banner;
            }
        }

        public static BasicCultureObject GetCulture(TeamConfig config)
        {
            if (config.HasGeneral)
            {
                var character = config.Generals.Troops.FirstOrDefault();
                if (character != null)
                    return character.Character.CharacterObject.Culture;
            }

            foreach (var troopGroupConfig in config.TroopGroups)
            {
                foreach (var troopConfig in troopGroupConfig.Troops)
                {
                    if (troopConfig.Number > 0)
                        return troopConfig.Character.CharacterObject.Culture;
                }
            }
            return Game.Current.ObjectManager.GetObject<BasicCultureObject>(culture => true);
        }

        public static SiegeEngineType GetSiegeEngineType(string id)
        {
            return string.IsNullOrEmpty(id) ? null : Game.Current.ObjectManager.GetObject<SiegeEngineType>(id);
        }

        public static void SetPlayerAsCommander(bool isSergeant)
        {
            var mission = Mission.Current;
            if (mission?.PlayerTeam == null)
                return;
            mission.PlayerTeam.PlayerOrderController.Owner = mission.MainAgent;
            foreach (var formation in mission.PlayerTeam.FormationsIncludingEmpty)
            {
                if (!isSergeant || formation.PlayerOwner != null)
                {
                    bool isAIControlled = formation.IsAIControlled;
                    formation.PlayerOwner = mission.MainAgent;
                    formation.SetControlledByAI(isAIControlled);
                }
            }
        }

        public static void CancelPlayerAsCommander()
        {
        }

        public static List<CharacterObject> OrderHeroesByPriority(TeamConfig teamConfig)
        {
            var characters = teamConfig.TroopGroups.SelectMany(troopGroupConfig =>
                troopGroupConfig.Troops.Select(troopConfig => troopConfig.Character));
            if (teamConfig.HasGeneral)
                characters = characters.Concat(teamConfig.Generals.Troops.Select(troopConfig => troopConfig.Character));
            return characters.Select(character => character.CharacterObject as CharacterObject)
                .Where(character => character != null && character.IsHero).Select(character => character.HeroObject)
                .ToList().ConvertAll(hero => hero.CharacterObject);
        }

        public static void SetMapEvents(PartyBase attacker, PartyBase defender, BattleType battleType)
        {
            try
            {
                var mapEvent = attacker.MapEvent;
                if (mapEvent != null)
                {
                    mapEvent.FinalizeEvent();
                    MBObjectManager.Instance.UnregisterObject(mapEvent);
                    var mapEvents = (List<MapEvent>)
                        typeof(MapEventManager)
                            .GetField("mapEvents", BindingFlags.Instance | BindingFlags.NonPublic)
                            ?.GetValue(Campaign.Current.MapEventManager);
                    if (mapEvents != null)
                    {
                        var index = mapEvents.FindIndex(m => m == mapEvent);
                        if (index >= 0 && index < mapEvents.Count)
                            mapEvents.RemoveAt(index);
                    }
                }
                FieldBattleEventComponent.CreateFieldBattleEvent(attacker, defender);
            }
            catch (Exception e)
            {
                DisplayMessage(e.ToString());
            }
        }

        public static void FillPartyMembers(PartyBase party, BattleSideEnum side, BasicCultureObject culture,
                TeamConfig teamConfig, bool isPlayerTeam)
        {
            party.MemberRoster.Clear();
            party.MemberRoster.Add(teamConfig.Generals.Troops.Select(troopConfig => 
                new FlattenedTroopRosterElement(GetCharacterObject(troopConfig.Character.CharacterObject),
                    teamConfig.HasGeneral ? RosterTroopState.Active : RosterTroopState.WoundedInThisBattle)).ToArray());

            party.MemberRoster.Add(teamConfig.TroopGroups.SelectMany(troopGroupConfig =>
                troopGroupConfig.Troops.SelectMany(troopConfig =>
                    Enumerable.Repeat(
                        new FlattenedTroopRosterElement(GetCharacterObject(troopConfig.Character.CharacterObject)),
                        troopConfig.Number))));
        }

        public static CharacterObject GetCharacterObject(BasicCharacterObject character)
        {
            var characterObject = character as CharacterObject;
            if (characterObject == null)
                return null;
            if (characterObject.IsHero)
                characterObject.HeroObject.HitPoints = characterObject.MaxHitPoints();
            return characterObject;
        }
        public static MissionSpawnSettings CreateSandBoxBattleWaveSpawnSettings()
        {
            return new MissionSpawnSettings(MissionSpawnSettings.InitialSpawnMethod.BattleSizeAllocating, MissionSpawnSettings.ReinforcementTimingMethod.GlobalTimer, MissionSpawnSettings.ReinforcementSpawnMethod.Wave, 3f, reinforcementWavePercentage: 0.5f, maximumReinforcementWaveCount: BannerlordConfig.GetReinforcementWaveCount());
        }
    }
}
