using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class SwitchFreeCameraLogic : MissionLogic
    {
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
            Agent target = this.Mission.PlayerTeam.PlayerOrderController.Owner;
            if (Utility.IsAgentDead(target))
            {
                Utility.DisplayMessage("Player agent is dead.");
                this.Mission.GetMissionBehaviour<ControlTroopAfterPlayerDeadLogic>()?.ControlTroopAfterDead();
                return;
            }

            target.Controller = Agent.ControllerType.Player;
            Utility.DisplayMessage("Switch to player agent.");
        }

        private void SwitchToFreeCamera()
        {
            this.Mission.MainAgent.Controller = Agent.ControllerType.AI;
            this.Mission.MainAgent = null;
            Utility.DisplayMessage("Switch to free camera.");
        }
    }
}