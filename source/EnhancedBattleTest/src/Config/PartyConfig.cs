using System;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Config
{
    public class PartyConfig
    {
        public const string DefaultBannerKey =
            "11.14.14.1536.1536.768.768.1.0.0.160.0.15.512.512.769.764.1.0.0";

        [XmlIgnore]
        private string _bannerKey = DefaultBannerKey;

        public string BannerKey
        {
            get => _bannerKey;
            set => _bannerKey = !string.IsNullOrEmpty(value) ? value : _bannerKey;
        }

        public bool UseCustomBanner;
        public bool IsInArmy;
        public bool HasHeroes;
        public TroopGroupConfig Heroes { get; set; } = new TroopGroupConfig();
        public TroopGroupConfig Troops { get; set; } = new TroopGroupConfig();

        public void Normalize()
        {
            Heroes = Heroes ?? new TroopGroupConfig();
            Troops = Troops ?? new TroopGroupConfig();
            Heroes.Troops = Heroes.Troops ?? new System.Collections.Generic.List<TroopConfig>();
            Troops.Troops = Troops.Troops ?? new System.Collections.Generic.List<TroopConfig>();
            if (Heroes.Troops.Count == 0)
                HasHeroes = false;
        }

        public Banner ResolveBanner(
            bool isAttacker,
            BasicCharacterObject preferredGeneral = null)
        {
            if (UseCustomBanner)
            {
                Banner customBanner = TryCreateBanner(BannerKey);
                if (customBanner != null)
                    return customBanner;
            }

            BasicCharacterObject bannerCharacter =
                preferredGeneral
                ?? GetBannerCharacterConfig()?.CharacterObject;
            Banner characterBanner =
                (bannerCharacter as CharacterObject)?.HeroObject?.ClanBanner;
            if (characterBanner != null)
                return characterBanner;

            BasicCultureObject culture = bannerCharacter?.Culture;
            if (culture != null)
                return Utility.BannerFor(culture, isAttacker);

            return TryCreateBanner(DefaultBannerKey) ?? Banner.CreateRandomBanner();
        }

        public CharacterConfig GetBannerCharacterConfig()
        {
            if (HasHeroes)
            {
                CharacterConfig hero = Heroes.Troops
                    .Select(troop => troop?.Character)
                    .FirstOrDefault(character =>
                        character?.CharacterObject != null);
                if (hero != null)
                    return hero;
            }

            CharacterConfig troop = Troops.Troops
                .Where(troop => troop?.Number > 0)
                .Select(troop => troop.Character)
                .FirstOrDefault(character =>
                    character?.CharacterObject != null);
            return troop ?? Troops.Troops
                .Select(troopConfig => troopConfig?.Character)
                .FirstOrDefault(character =>
                    character?.CharacterObject != null);
        }

        public BasicCharacterObject GetFirstHeroCharacter(
            BasicCharacterObject excludedCharacter = null)
        {
            if (!HasHeroes)
                return null;

            return Heroes.Troops
                .Select(troop => troop?.Character?.CharacterObject)
                .FirstOrDefault(character =>
                    character != null && character != excludedCharacter);
        }

        public Tuple<uint, uint> ResolveColors(bool isAttacker)
        {
            ResolveAppearance(isAttacker, out _, out Tuple<uint, uint> colors);
            return colors;
        }

        public void ResolveAppearance(
            bool isAttacker,
            out Banner banner,
            out Tuple<uint, uint> colors,
            BasicCharacterObject preferredGeneral = null)
        {
            banner = ResolveBanner(isAttacker, preferredGeneral);
            uint color1 = banner.BannerDataList.Count > 0
                ? BannerManager.GetColor(banner.BannerDataList[0].ColorId)
                : uint.MaxValue;
            uint color2 = banner.BannerDataList.Count > 1
                ? BannerManager.GetColor(banner.BannerDataList[1].ColorId)
                : color1;
            colors = new Tuple<uint, uint>(color1, color2);
        }

        private static Banner TryCreateBanner(string bannerKey)
        {
            try
            {
                return string.IsNullOrEmpty(bannerKey) ? null : new Banner(bannerKey);
            }
            catch
            {
                return null;
            }
        }
    }
}
