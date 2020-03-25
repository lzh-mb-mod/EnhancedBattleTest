using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class DisableDyingLogic : MissionLogic
    {
        private BattleConfigBase _config;

        public DisableDyingLogic(BattleConfigBase config)
        {
            _config = config;
        }
        public override void AfterStart()
        {
            base.AfterStart();
            Mission.DisableDying = _config.disableDying;
            PrintDyingStatus();
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (this.Mission.InputManager.IsKeyPressed(InputKey.F11))
            {
                SetDisableDying(!Mission.DisableDying);
            }
        }

        public void SetDisableDying(bool disableDying)
        {
            Mission.DisableDying = disableDying;
            PrintDyingStatus();
        }

        private void PrintDyingStatus()
        {
            Utility.DisplayMessage(Mission.DisableDying ? "Dying Disabled." : "Dying Enabled.");
        }
    }
}
