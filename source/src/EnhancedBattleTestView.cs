using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestView : MissionView
    {
        private readonly Mission _mission;
        public EnhancedBattleTestView(Mission mission)
            : base()
        {
            this._mission = mission;
        }
        public override void OnMissionScreenActivate()
        {
            foreach (var missionLogic in this._mission.MissionLogics)
            {
                if (missionLogic is TestBattleMissionController missionController)
                {
                    this.MissionScreen.CombatCamera.LookAt(missionController.initialFreeCameraPos, missionController.initialFreeCameraTarget, TaleWorlds.Library.Vec3.Up);
                    break;
                }
            }
        }
    }
}