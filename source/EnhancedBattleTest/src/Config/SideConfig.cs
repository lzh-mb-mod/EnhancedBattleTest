using System.Collections.Generic;
using System.Xml.Serialization;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Config
{
    public class SideConfig
    {
        [field: XmlIgnore]
        public List<TeamConfig> Teams { get; set; } = new List<TeamConfig>();

        public SideConfig()
        {
        }
    }
}
