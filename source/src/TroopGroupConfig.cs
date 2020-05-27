namespace EnhancedBattleTest
{
    public class TroopGroupConfig
    {
        public TroopConfig[] Troops = new TroopConfig[8];

        public TroopGroupConfig()
        { }
        public TroopGroupConfig(bool isMultiplayer)
        {
            for (int i = 0; i < Troops.Length; ++i)
                Troops[i] = new TroopConfig(isMultiplayer);
        }
    }
}
