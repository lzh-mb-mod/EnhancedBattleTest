using System.Xml.Serialization;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public class TeamConfig
    {
        public TroopGroupConfig Troops;

        public CharacterConfig General;
        public bool HasGeneral;

        public string BannerKey = string.Empty;

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
        { }

        public TeamConfig(bool isMultiplayer)
        {
            Troops = new TroopGroupConfig(isMultiplayer);
            General = CharacterConfig.Create(isMultiplayer);
        }
    }
}
