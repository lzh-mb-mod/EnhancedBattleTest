using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Config
{
    public class TeamConfig
    {
        [XmlIgnore]
        private string _bannerKey = "11.14.14.1536.1536.768.768.1.0.0.160.0.15.512.512.769.764.1.0.0";

        public string BannerKey
        {
            get => _bannerKey;
            set => _bannerKey = !string.IsNullOrEmpty(value) ? value : _bannerKey;
        }

        public bool CustomBanner { get; set; }

        [field: XmlIgnore]
        public TroopGroupConfig Generals { get; set; } =
            new TroopGroupConfig(EnhancedBattleTestSubModule.IsMultiplayer, true);

        public bool HasGeneral { get; set; }

        [field: XmlIgnore]
        public TroopGroupConfig Troops { get; set; } =
            new TroopGroupConfig(EnhancedBattleTestSubModule.IsMultiplayer);

        public bool CustomTacticLevel { get; set; }

        public int TacticLevel { get; set; } = 0;

        [XmlIgnore]
        public uint Color1 => Banner.BannerDataList.Count > 0
            ? BannerManager.GetColor(Banner.BannerDataList[0].ColorId)
            : uint.MaxValue;

        [XmlIgnore]
        public uint Color2 => Banner.BannerDataList.Count > 1
            ? BannerManager.GetColor(Banner.BannerDataList[1].ColorId)
            : uint.MaxValue;

        [XmlIgnore]
        public Banner Banner => new Banner(BannerKey);

        public Banner GetPreviewBanner()
        {
            return CustomBanner || EnhancedBattleTestSubModule.IsMultiplayer
                ? new Banner(BannerKey)
                : (HasGeneral
                    ? (Generals.Troops[0].Character.CharacterObject as CharacterObject).HeroObject?.ClanBanner
                    : null) ?? CampaignData.NeutralFaction.Banner;
        }

        public TeamConfig()
        {
        }
    }
}
