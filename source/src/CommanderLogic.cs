using System.ComponentModel;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class CommanderLogic : MissionLogic
    {
        private BattleConfig _config;

        public CommanderLogic(BattleConfig config)
        {
            _config = config;
        }

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();

            this.Mission.OnMainAgentChanged += OnMainAgentChanged;
        }

        public override void OnRemoveBehaviour()
        {
            base.OnRemoveBehaviour();

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
