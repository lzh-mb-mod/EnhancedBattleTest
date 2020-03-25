using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class SwitchTeamLogic : MissionLogic
    {
        public delegate void SwitchTeamDelegate();
        
        public event SwitchTeamDelegate PreSwitchTeam;
        public event SwitchTeamDelegate PostSwitchTeam;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            
            if (this.Mission.InputManager.IsKeyPressed(InputKey.F9))
                this.SwapTeam();
        }

        public void SwapTeam()
        {
            var targetAgent = !Utility.IsAgentDead(this.Mission.PlayerEnemyTeam.PlayerOrderController.Owner)
                ? this.Mission.PlayerEnemyTeam.PlayerOrderController.Owner
                : this.Mission.PlayerEnemyTeam.Leader;
            if (targetAgent == null)
            {
                Utility.DisplayMessage("Enemy has been wiped out.");
                return;
            }
            if (!Utility.IsPlayerDead()) // MainAgent may be null because of free camera mode.
            {
                this.Mission.MainAgent.Controller = Agent.ControllerType.AI;
                this.Mission.MainAgent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            }
            Utility.DisplayMessage("Switched to the enemy team.");

            PreSwitchTeam?.Invoke();
            this.Mission.PlayerTeam = this.Mission.PlayerEnemyTeam;
            targetAgent.Controller = Agent.ControllerType.Player;
            PostSwitchTeam?.Invoke();
        }
    }
}