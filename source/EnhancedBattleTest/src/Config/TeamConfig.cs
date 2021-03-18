using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest.Config
{
    public class TeamConfig
    {
        [XmlIgnore]
        private string _bannerKey = "11.14.14.1536.1536.768.768.1.0.0.160.0.15.512.512.769.764.1.0.0";

        public string BannerKey
        {
            get => _bannerKey;
            set => _bannerKey = !value.IsStringNoneOrEmpty() ? value : _bannerKey;
        }

        [field: XmlIgnore]
        public TroopGroupConfig Generals { get; set; } = new TroopGroupConfig(EnhancedBattleTestSubModule.IsMultiplayer);

        public bool HasGeneral;

        [field: XmlIgnore]
        public TroopGroupConfig[] TroopGroups { get; set; } = new TroopGroupConfig[8];

        public int TacticLevel = 0;

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


        public TeamConfig()
        {
        }

        public TeamConfig(bool isMultiplayer)
        {
            for (int i = 0; i < TroopGroups.Length; ++i)
                TroopGroups[i] = new TroopGroupConfig(isMultiplayer);
        }
    }
}
