using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace EnhancedBattleTest.Data.MissionData.Logic
{
    public sealed class EnhancedBattleTestNoPlayerDeploymentLogic : MissionLogic
    {
        private readonly DefaultBattleMissionAgentSpawnLogic _spawnLogic;
        private bool _restoreDeploymentBypass;

        public EnhancedBattleTestNoPlayerDeploymentLogic(
            DefaultBattleMissionAgentSpawnLogic spawnLogic)
        {
            _spawnLogic = spawnLogic;
        }

        public override void EarlyStart()
        {
            if (!BattleInitializationModel.BypassPlayerDeployment)
            {
                BattleInitializationModel.SetBypassPlayerDeployment(true);
                _restoreDeploymentBypass = true;
            }
        }

        public override void AfterStart()
        {
            Mission.SetMissionMode(MissionMode.Battle, true);
        }

        public override void OnMissionTick(float dt)
        {
            if (!Mission.IsDeploymentFinished)
                return;

            _spawnLogic.SetReinforcementsSpawnEnabled(true);
            RestoreDeploymentBypass();
            Mission.RemoveMissionBehavior(this);
        }

        public override void OnRemoveBehavior()
        {
            RestoreDeploymentBypass();
            base.OnRemoveBehavior();
        }

        private void RestoreDeploymentBypass()
        {
            if (!_restoreDeploymentBypass)
                return;

            BattleInitializationModel.SetBypassPlayerDeployment(false);
            _restoreDeploymentBypass = false;
        }
    }
}
