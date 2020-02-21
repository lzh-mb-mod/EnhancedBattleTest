using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class SwitchTeamLogic : MissionLogic
    {
        public delegate void ReloadOrderUIDelegate();

        public event ReloadOrderUIDelegate ReloadOrderUI;

        public Agent enemyLeader;
        public Agent thisLeader;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (thisLeader == null)
                thisLeader = Mission.MainAgent;
            if (this.Mission.InputManager.IsKeyPressed(InputKey.Numpad5))
                this.SwapTeam();
        }

        private void SwapTeam()
        {
            var targetAgent = !Utility.IsAgentDead(this.enemyLeader) ? this.enemyLeader : this.Mission.PlayerEnemyTeam.Leader;
            if (targetAgent == null)
                return;
            Agent previousAgent;
            if (!Utility.IsPlayerDead()) // MainAgent may be now because of free camera mode.
            {
                previousAgent = this.Mission.MainAgent;
                this.Mission.MainAgent.Controller = Agent.ControllerType.AI;
            }
            else
            {
                previousAgent = thisLeader;
            }
            this.Mission.PlayerTeam = this.Mission.PlayerEnemyTeam;
            targetAgent.Controller = Agent.ControllerType.Player;
            previousAgent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            this.thisLeader = Mission.MainAgent;
            this.enemyLeader = previousAgent;
            Utility.SetPlayerAsCommander();
            ReloadOrderUI?.Invoke();
        }
    }
}