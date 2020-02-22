using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    public class EnhancedTestBattleView : MissionView
    {
        private readonly Mission _mission;
        public EnhancedTestBattleView(Mission mission)
            : base()
        {
            this._mission = mission;
        }
        public override void OnMissionScreenActivate()
        {
            foreach (var missionLogic in this._mission.MissionLogics)
            {
                if (missionLogic is EnhancedTestBattleMissionController missionController)
                {
                    this.MissionScreen.CombatCamera.LookAt(missionController.initialFreeCameraPos, missionController.initialFreeCameraTarget, TaleWorlds.Library.Vec3.Up);
                    break;
                }
            }
        }
    }
}