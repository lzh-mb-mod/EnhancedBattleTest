namespace EnhancedBattleTest.Config
{
    public class TroopGroupConfig
    {
        private TroopConfig[] _troops = new TroopConfig[8];

        public TroopConfig[] Troops
        {
            get => _troops;
            set => _troops = value != null && value.Length == 8 ? value : _troops;
        }

        public TroopGroupConfig()
        {
            for (int i = 0; i < _troops.Length; ++i)
            {
                Troops[i] = new TroopConfig(EnhancedBattleTestSubModule.IsMultiplayer);
            }
        }
        public TroopGroupConfig(bool isMultiplayer)
        {
            for (int i = 0; i < Troops.Length; ++i)
                Troops[i] = new TroopConfig(isMultiplayer);
        }
    }
}
