using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest
{
    public class TeamConfig
    {
        public TroopGroupConfig Troops;

        public CharacterConfig General;
        public bool HasGeneral;

        public string BannerKey = Banner.CreateRandomBanner().Serialize();
        public uint Color1 = 0;
        public uint Color2 = 1;

        [XmlIgnore]
        public Banner Banner => new Banner(BannerKey, Color1, Color2);

        public TeamConfig()
        { }

        public TeamConfig(bool isMultiplayer)
        {
            Troops = new TroopGroupConfig(isMultiplayer);
            General = CharacterConfig.Create(isMultiplayer);
        }
    }
}
