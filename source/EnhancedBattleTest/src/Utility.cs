using EnhancedBattleTest.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
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
            
            foreach (var troopConfig in config.Troops.Troops)
            {
                if (troopConfig.Number > 0)
                    return troopConfig.Character.CharacterObject.Culture;
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
                    bool isSplittableByAI = formation.IsSplittableByAI;
                    formation.PlayerOwner = mission.MainAgent;
                    formation.SetControlledByAI(isAIControlled, isSplittableByAI);
                }
            }
        }

        public static void CancelPlayerAsCommander()
        {
        }

        public static ItemModifier AverageItemModifier(ItemModifierGroup group)
        {
            /*
            public sealed class ItemModifier : MBObjectBase
            {
                private int _damage;
                private int _speed;
                private int _missileSpeed;
                private int _armor;
                private short _hitPoints;
                private short _stackCount;
                private float _mountSpeed;
                private float _maneuver;
                private float _chargeDamage;
                private float _mountHitPoints;
                public TextObject Name { get; private set; }
                public float PriceMultiplier { get; private set; }
                ...
            }
            */
            if (group == null)
                return null;
            //var result = new ItemModifier { ID = string.Empty };
            var result = MBObjectManager.Instance.CreateObject<ItemModifier>(string.Empty);
            float sum = group.NoModifierLootScore;
            foreach (var itemModifier in group.ItemModifiers)
            {
                sum += itemModifier.LootDropScore;
            }

            float damage = 0;
            float speed = 0;
            float missileSpeed = 0;
            float armor = 0;
            //float rankIndex = 0;
            float priceMultiplier = 0;
            //float weightMultiplier = 0;
            //float oldness = 0;
            //float factorOne = 0;
            //float factorTwo = 0;
            float hitPoints = 0;
            float stackCount = 0;
            float mountSpeed = 0;
            float maneuver = 0;
            float chargeDamage = 0;
            float mountHitPoints = 0;

            var bindingFlag = BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var itemModifier in group.ItemModifiers)
            {
                if (itemModifier == null)
                    continue;
                damage += itemModifier.ModifyDamage(0) * itemModifier.LootDropScore / sum;
                speed += itemModifier.ModifySpeed(0) * itemModifier.LootDropScore / sum;
                missileSpeed += itemModifier.ModifyMissileSpeed(0) * itemModifier.LootDropScore / sum;
                armor += itemModifier.ModifyArmor(0) * itemModifier.LootDropScore / sum;
                //rankIndex += itemModifier.RankIndex * itemModifier.LootDropScore / sum;
                priceMultiplier += itemModifier.PriceMultiplier * itemModifier.LootDropScore / sum;
                //weightMultiplier += itemModifier.WeightMultiplier * itemModifier.LootDropScore / sum;
                //oldness += itemModifier.Oldness * itemModifier.LootDropScore / sum;
                //factorOne += itemModifier.FactorOne * itemModifier.LootDropScore / sum;
                //factorTwo += itemModifier.FactorTwo * itemModifier.LootDropScore / sum;
                hitPoints += itemModifier.ModifyHitPoints(0) * itemModifier.LootDropScore / sum;
                stackCount += itemModifier.ModifyStackCount(0) * itemModifier.LootDropScore / sum;
                mountSpeed += itemModifier.ModifyMountSpeed(0) * itemModifier.LootDropScore / sum;
                maneuver += itemModifier.ModifyMountManeuver(0) * itemModifier.LootDropScore / sum;
                chargeDamage += itemModifier.ModifyMountCharge(0) * itemModifier.LootDropScore / sum;
                mountHitPoints += itemModifier.ModifyMountHitPoints(0) * itemModifier.LootDropScore / sum;
            }

            typeof(ItemModifier).GetField("_damage", bindingFlag)?.SetValue(result, (int)damage);
            typeof(ItemModifier).GetField("_speed", bindingFlag)?.SetValue(result, (int)speed);
            typeof(ItemModifier).GetField("_missileSpeed", bindingFlag)?.SetValue(result, (int)missileSpeed);
            typeof(ItemModifier).GetField("_armor", bindingFlag)?.SetValue(result, (int)armor);
            //typeof(ItemModifier).GetField("RankIndex", bindingFlag)?.SetValue(result, (int)rankIndex);
            typeof(ItemModifier).GetField("PriceMultiplier", bindingFlag)?.SetValue(result, priceMultiplier);
            //typeof(ItemModifier).GetField("WeightMultiplier", bindingFlag)?.SetValue(result, weightMultiplier);
            //typeof(ItemModifier).GetField("Oldness", bindingFlag)?.SetValue(result, oldness);
            //typeof(ItemModifier).GetField("FactorOne", bindingFlag)?.SetValue(result, (uint)factorOne);
            //typeof(ItemModifier).GetField("FactorTwo", bindingFlag)?.SetValue(result, (uint)factorTwo);
            typeof(ItemModifier).GetField("_hitPoints", bindingFlag)?.SetValue(result, (short)hitPoints);
            typeof(ItemModifier).GetField("_stackCount", bindingFlag)?.SetValue(result, (short)stackCount);
            typeof(ItemModifier).GetField("_mountSpeed", bindingFlag)?.SetValue(result, (int)mountSpeed);
            typeof(ItemModifier).GetField("_maneuver", bindingFlag)?.SetValue(result, (int)maneuver);
            typeof(ItemModifier).GetField("_chargeDamage", bindingFlag)?.SetValue(result, (int)chargeDamage);
            typeof(ItemModifier).GetField("_mountHitPoints", bindingFlag)?.SetValue(result, (int)mountHitPoints);
            return result;
        }

        public static List<CharacterObject> OrderHeroesByPriority(TeamConfig teamConfig)
        {
            var characters = teamConfig.Troops.Troops.Select(troopConfig => troopConfig.Character);
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

                Campaign.Current.MapEventManager.StartBattleMapEvent(attacker, defender);
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

            party.MemberRoster.Add(teamConfig.Troops.Troops.SelectMany(troopConfig =>
                    Enumerable.Repeat(
                        new FlattenedTroopRosterElement(GetCharacterObject(troopConfig.Character.CharacterObject)),
                        troopConfig.Number)));
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
    }
}
