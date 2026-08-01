using System;
using System.Linq;
using EnhancedBattleTest.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
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
            if (!culture.Banner.IsBannerDataListEmpty())
            {
                var banner = new Banner(culture.Banner);
                uint backgroundColor = BackgroundColor(culture, isAttacker);
                uint foregroundColor = ForegroundColor(culture, isAttacker);
                if (backgroundColor != uint.MaxValue)
                    banner.ChangePrimaryColor(backgroundColor);
                if (foregroundColor != uint.MaxValue)
                    banner.ChangeIconColors(foregroundColor);
                return banner;
            }

            var randomBanner = Banner.CreateRandomBanner();
            uint fallbackBackground = BackgroundColor(culture, isAttacker);
            uint fallbackForeground = ForegroundColor(culture, isAttacker);
            randomBanner.ChangePrimaryColor(
                fallbackBackground != uint.MaxValue
                    ? fallbackBackground
                    : ClothingColor1(culture, isAttacker));
            randomBanner.ChangeIconColors(
                fallbackForeground != uint.MaxValue
                    ? fallbackForeground
                    : ClothingColor2(culture, isAttacker));
            return randomBanner;
        }

        public static Banner SPBannerFor(BasicCultureObject culture, bool isAttacker)
        {
            if (!culture.Banner.IsBannerDataListEmpty())
                return new Banner(culture.Banner, ClothingColor1(culture, isAttacker),
                    ClothingColor2(culture, isAttacker));
            else
            {
                var banner = Banner.CreateRandomBanner();
                banner.ChangePrimaryColor(ClothingColor1(culture, isAttacker));
                banner.ChangeIconColors(ClothingColor2(culture, isAttacker));
                return banner;
            }
        }

        public static BasicCultureObject GetCulture(PartyConfig config)
        {
            if (config.HasHeroes)
            {
                var character = config.Heroes.Troops.FirstOrDefault();
                if (character?.Character?.CharacterObject?.Culture != null)
                    return character.Character.CharacterObject.Culture;
            }

            foreach (var troopConfig in config.Troops.Troops)
            {
                if (troopConfig.Number > 0
                    && troopConfig.Character?.CharacterObject?.Culture != null)
                    return troopConfig.Character.CharacterObject.Culture;
            }
            foreach (var troopConfig in config.Troops.Troops)
            {
                if (troopConfig.Character?.CharacterObject?.Culture != null)
                    return troopConfig.Character.CharacterObject.Culture;
            }
            return Game.Current.ObjectManager.GetObject<BasicCultureObject>(culture => true);
        }

        public static SiegeEngineType GetSiegeEngineType(string id)
        {
            return string.IsNullOrEmpty(id) ? null : Game.Current.ObjectManager.GetObject<SiegeEngineType>(id);
        }

    }
}
