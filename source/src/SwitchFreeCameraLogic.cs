using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class SwitchFreeCameraLogic : MissionLogic
    {
        public Agent playerAgentBackup;
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (!Utility.IsPlayerDead())
            {
                playerAgentBackup = this.Mission.MainAgent;
            }

            if (this.Mission.InputManager.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Numpad6))
            {
                this.SwitchCamera();
            }
        }

        private void SwitchCamera()
        {
            ModuleLogger.Log("SwitchCamera");
            if (Utility.IsPlayerDead())
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
            if (Utility.IsAgentDead(playerAgentBackup))
            {
                Utility.DisplayMessage("No player agent.");
                return;
            }

            this.playerAgentBackup.Controller = Agent.ControllerType.Player;
            Utility.DisplayMessage("Switch to player agent.");
        }

        private void SwitchToFreeCamera()
        {
            this.playerAgentBackup = this.Mission.MainAgent;
            this.Mission.MainAgent.Controller = Agent.ControllerType.AI;
            //this.Mission.MainAgent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            this.Mission.MainAgent = null;
            Utility.DisplayMessage("Switch to free camera.");
        }
    }
}