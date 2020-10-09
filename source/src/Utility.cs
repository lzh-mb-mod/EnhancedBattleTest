using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnhancedBattleTest.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
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

        public static Equipment GetNewEquipmentsForPerks(
            MultiplayerClassDivisions.MPHeroClass heroClass,
            bool isHero,
            int firstPerk,
            int secondPerk,
            bool fixedEquipment, int seed = -1)
        {
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
                return config.General.Character.Culture;
            else
                return config.Troops.Troops.FirstOrDefault(troopConfig => troopConfig.Number != 0)?.Character.Character
                           .Culture ?? Game.Current.ObjectManager.GetObject<BasicCultureObject>(culture => true);
        }

        public static SiegeEngineType GetSiegeEngineType(string id)
        {
            return id.IsStringNoneOrEmpty() ? null : Game.Current.ObjectManager.GetObject<SiegeEngineType>(id);
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
                    formation.IsAIControlled = isAIControlled;
                }
            }
        }

        public static void CancelPlayerAsCommander()
        {
        }

        //public static ItemModifier AverageItemModifier(ItemModifierGroup group)
        //{
        //    if (group == null)
        //        return null;
        //    var result = new ItemModifier { StringId = string.Empty };
        //    float sum = 1;
        //    foreach (var probability in group.ItemModifiersWithProbability.Values)
        //    {
        //        sum += probability.Probability;
        //    }

        //    float damage = 0;
        //    float speed = 0;
        //    float missileSpeed = 0;
        //    float armor = 0;
        //    float rankIndex = 0;
        //    float priceMultiplier = 0;
        //    float weightMultiplier = 0;
        //    float oldness = 0;
        //    float factorOne = 0;
        //    float factorTwo = 0;
        //    float hitPoints = 0;
        //    float horseSpeed = 0;
        //    float maneuver = 0;
        //    float chargeDamage = 0;

        //    var bindingFlag = BindingFlags.NonPublic | BindingFlags.Instance;
        //    foreach (var modifier in group.ItemModifiersWithProbability.Values)
        //    {
        //        if (modifier.ItemModifier == null)
        //            continue;
        //        damage += modifier.ItemModifier.Damage * modifier.Probability / sum;
        //        speed += modifier.ItemModifier.Speed * modifier.Probability / sum;
        //        missileSpeed += modifier.ItemModifier.MissileSpeed * modifier.Probability / sum;
        //        armor += modifier.ItemModifier.Armor * modifier.Probability / sum;
        //        rankIndex += modifier.ItemModifier.RankIndex * modifier.Probability / sum;
        //        priceMultiplier += modifier.ItemModifier.PriceMultiplier * modifier.Probability / sum;
        //        weightMultiplier += modifier.ItemModifier.WeightMultiplier * modifier.Probability / sum;
        //        oldness += modifier.ItemModifier.Oldness * modifier.Probability / sum;
        //        factorOne += modifier.ItemModifier.FactorOne * modifier.Probability / sum;
        //        factorTwo += modifier.ItemModifier.FactorTwo * modifier.Probability / sum;
        //        hitPoints += modifier.ItemModifier.HitPoints * modifier.Probability / sum;
        //        horseSpeed += modifier.ItemModifier.HorseSpeed * modifier.Probability / sum;
        //        maneuver += modifier.ItemModifier.Maneuver * modifier.Probability / sum;
        //        chargeDamage += modifier.ItemModifier.ChargeDamage * modifier.Probability / sum;
        //    }

        //    typeof(ItemModifier).GetField("Damage", bindingFlag)?.SetValue(result, (int)damage);
        //    typeof(ItemModifier).GetField("Speed", bindingFlag)?.SetValue(result, (int)speed);
        //    typeof(ItemModifier).GetField("MissileSpeed", bindingFlag)?.SetValue(result, (int)missileSpeed);
        //    typeof(ItemModifier).GetField("Armor", bindingFlag)?.SetValue(result, (int)armor);
        //    typeof(ItemModifier).GetField("RankIndex", bindingFlag)?.SetValue(result, (int)rankIndex);
        //    typeof(ItemModifier).GetField("PriceMultiplier", bindingFlag)?.SetValue(result, priceMultiplier);
        //    typeof(ItemModifier).GetField("WeightMultiplier", bindingFlag)?.SetValue(result, weightMultiplier);
        //    typeof(ItemModifier).GetField("Oldness", bindingFlag)?.SetValue(result, oldness);
        //    typeof(ItemModifier).GetField("FactorOne", bindingFlag)?.SetValue(result, (uint)factorOne);
        //    typeof(ItemModifier).GetField("FactorTwo", bindingFlag)?.SetValue(result, (uint)factorTwo);
        //    typeof(ItemModifier).GetField("HitPoints", bindingFlag)?.SetValue(result, (int)hitPoints);
        //    typeof(ItemModifier).GetField("HorseSpeed", bindingFlag)?.SetValue(result, (int)horseSpeed);
        //    typeof(ItemModifier).GetField("Maneuver", bindingFlag)?.SetValue(result, (int)maneuver);
        //    typeof(ItemModifier).GetField("ChargeDamage", bindingFlag)?.SetValue(result, (int)chargeDamage);
        //    return result;
        //}

        public static List<CharacterObject> OrderHeroesByPriority(TeamConfig teamConfig)
        {
            var characters = teamConfig.Troops.Troops
                .Select(troopConfig => troopConfig.Character);
            if (teamConfig.HasGeneral)
                characters = characters.Prepend(teamConfig.General);
            return characters.Select(character => character.CharacterObject as CharacterObject)
                .Where(character => character != null && character.IsHero).Select(character => character.HeroObject)
                .OrderByDescending(hero =>
                    TaleWorlds.CampaignSystem.Campaign.Current.Models.DiplomacyModel.GetCharacterSergeantScore(hero))
                .ToList().ConvertAll(hero => hero.CharacterObject);
        }

        public static void SetMapEvents(PartyBase attacker, PartyBase defender, BattleType battleType)
        {
            Campaign.Current.MapEventManager.StartBattleMapEvent(attacker, defender);
        }

        public static void FillPartyMembers(PartyBase party, BattleSideEnum side, BasicCultureObject culture,
                TeamConfig teamConfig, bool isPlayerTeam)
        {
            party.MemberRoster.Clear();
            party.MemberRoster.Add(new[]
            {
                new FlattenedTroopRosterElement(GetCharacterObject(teamConfig.General.CharacterObject),
                    teamConfig.HasGeneral ? RosterTroopState.Active : RosterTroopState.WoundedInThisBattle)
            });
            party.MemberRoster.Add(teamConfig.Troops.Troops.Select(troopConfig =>
                new FlattenedTroopRosterElement(GetCharacterObject(troopConfig.Character.CharacterObject))));
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
