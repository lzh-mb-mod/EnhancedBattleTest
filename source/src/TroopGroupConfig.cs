namespace EnhancedBattleTest
{
    public class TroopGroupConfig
    {
        public TroopConfig[] Troops = new TroopConfig[8];

        public TroopGroupConfig()
        {
            for (int i = 0; i < Troops.Length; ++i)
                Troops[i] = new TroopConfig();
        }
    }
}
