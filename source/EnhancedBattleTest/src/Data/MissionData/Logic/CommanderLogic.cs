using System.ComponentModel;
using EnhancedBattleTest.Config;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData.Logic
{
    public class CommanderLogic : MissionLogic
    {
        private BattleConfig _config;

        public CommanderLogic(BattleConfig config)
        {
            _config = config;
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            this.Mission.OnMainAgentChanged += OnMainAgentChanged;
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();

            Mission.OnMainAgentChanged -= OnMainAgentChanged;
        }

        private void OnMainAgentChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.Mission.MainAgent != null)
                Utility.SetPlayerAsCommander(_config.BattleTypeConfig.PlayerType == PlayerType.Sergeant);
            else
                Utility.CancelPlayerAsCommander();
        }
    }
}
