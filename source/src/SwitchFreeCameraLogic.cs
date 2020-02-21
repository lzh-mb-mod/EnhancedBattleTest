using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class SwitchFreeCameraLogic : MissionLogic
    {
        private Agent _playerAgentBackup;
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (this.Mission.InputManager.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Numpad6))
            {
                this.SwitchCamera();
            }
        }

        private void SwitchCamera()
        {
            ModuleLogger.Log("SwitchCamera");
            if (this.Mission.MainAgent == null)
            {
                SwitchToAgent();
            }
            else
            {
                SwitchToFreeCamera();
            }
        }

        private void SwitchToAgent()
        {
            if (Utility.IsAgentDead(_playerAgentBackup))
            {
                Utility.DisplayMessage("No player agent.");
                return;
            }

            this._playerAgentBackup.Controller = Agent.ControllerType.Player;
            Utility.DisplayMessage("Switch to player agent.");
        }

        private void SwitchToFreeCamera()
        {
            this._playerAgentBackup = this.Mission.MainAgent;
            this.Mission.MainAgent.Controller = Agent.ControllerType.AI;
            this.Mission.MainAgent = null;
            Utility.DisplayMessage("Switch to free camera.");
        }
    }
}