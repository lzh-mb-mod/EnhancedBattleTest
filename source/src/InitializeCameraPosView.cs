using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    public class InitializeCameraPosView : MissionView
    {
        private Vec2 _initialPosition;
        private Vec2 _initialDirection;
        public InitializeCameraPosView(Vec2 initialPosition, Vec2 initalDirection)
            : base()
        {
            _initialPosition = initialPosition;
            _initialDirection = initalDirection;
        }
        public override void OnMissionScreenActivate()
        {
            Utility.SetInitialCameraPos(MissionScreen.CombatCamera, _initialPosition, _initialDirection);
        }
    }
}