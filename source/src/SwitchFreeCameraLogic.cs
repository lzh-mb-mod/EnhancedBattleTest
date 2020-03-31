using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class SwitchFreeCameraLogic : MissionLogic
    {
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (this.Mission.InputManager.IsKeyPressed(TaleWorlds.InputSystem.InputKey.F10))
            {
                this.SwitchCamera();
            }
        }

        public void SwitchCamera()
        {
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
                Utility.DisplayLocalizedText("str_player_dead");
                this.Mission.GetMissionBehaviour<ControlTroopAfterPlayerDeadLogic>()?.ControlTroopAfterDead();
                return;
            }

            target.Controller = Agent.ControllerType.Player;
            Utility.DisplayLocalizedText("str_switch_to_player");
        }

        private void SwitchToFreeCamera()
        {
            this.Mission.MainAgent.Controller = Agent.ControllerType.AI;
            this.Mission.MainAgent = null;
            Utility.DisplayLocalizedText("str_switch_to_free_camera");
        }
    }
}