using System.Collections.Generic;

namespace EnhancedBattleTest.Config
{
    public class TroopGroupConfig
    {
        public List<TroopConfig> Troops { get; set; }

        public TroopGroupConfig()
        {
            Troops = new List<TroopConfig>();
        }

        public TroopGroupConfig(bool isGeneralTroopGroup)
        {
            Troops = isGeneralTroopGroup
                ? new List<TroopConfig>(1) {new TroopConfig()}
                : new List<TroopConfig>();
        }
    }
}
