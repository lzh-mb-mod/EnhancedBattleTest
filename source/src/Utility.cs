using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
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
    }
}
