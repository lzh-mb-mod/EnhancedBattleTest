namespace EnhancedBattleTest
{
    public class BattleConfig
    {
        public TeamConfig PlayerTeamConfig = new TeamConfig();
        public TeamConfig EnemyTeamConfig = new TeamConfig();
        public BattleTypeConfig BattleTypeConfig = new BattleTypeConfig();
        public SiegeMachineConfig SiegeMachineConfig = new SiegeMachineConfig();

        public BattleConfig()
        { }

        public BattleConfig(bool isMultiplayer)
        {
            PlayerTeamConfig = new TeamConfig(isMultiplayer);
            EnemyTeamConfig = new TeamConfig(isMultiplayer);
        }
    }
}
