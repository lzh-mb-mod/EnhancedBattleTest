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

        public TroopGroupConfig(bool isMultiplayer, bool isGeneralTroopGroup = false)
        {
            Troops = isGeneralTroopGroup
                ? new List<TroopConfig>(1) {new TroopConfig(isMultiplayer)}
                : new List<TroopConfig>();
        }
    }
}
