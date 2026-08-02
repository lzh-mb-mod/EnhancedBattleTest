using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData.Logic
{
    public sealed class EnhancedBattleTestNoPlayerDeploymentLogic : MissionLogic
    {
        private readonly DefaultBattleMissionAgentSpawnLogic _spawnLogic;

        public EnhancedBattleTestNoPlayerDeploymentLogic(
            DefaultBattleMissionAgentSpawnLogic spawnLogic)
        {
            _spawnLogic = spawnLogic;
        }

        public override void AfterStart()
        {
            Mission.SetMissionMode(MissionMode.Battle, true);
        }

        public override void OnMissionTick(float dt)
        {
            if (!_spawnLogic.IsInitialSpawnOver)
                return;

            Mission.OnDeploymentFinished();
            Mission.OnAfterDeploymentFinished();
            _spawnLogic.SetReinforcementsSpawnEnabled(true);
            Mission.RemoveMissionBehavior(this);
        }
    }
}
