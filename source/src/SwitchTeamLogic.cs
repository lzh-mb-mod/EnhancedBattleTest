using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class SwitchTeamLogic : MissionLogic
    {
        public delegate void SwitchTeamDelegate();
        
        public event SwitchTeamDelegate PreSwitchTeam;
        public event SwitchTeamDelegate PostSwitchTeam;

        public Agent enemyLeader;
        private Agent _thisLeader;
        private Agent _targetAgent;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (_thisLeader == null)
                _thisLeader = Mission.MainAgent;
            if (this.Mission.InputManager.IsKeyPressed(InputKey.Numpad5))
                this.SwapTeam();
        }

        private void SwapTeam()
        {
            _targetAgent = !Utility.IsAgentDead(this.enemyLeader) ? this.enemyLeader : this.Mission.PlayerEnemyTeam.Leader;
            if (_targetAgent == null)
                return;
            Agent previousAgent;
            if (!Utility.IsPlayerDead()) // MainAgent may be null because of free camera mode.
            {
                previousAgent = this.Mission.MainAgent;
                this.Mission.MainAgent.Controller = Agent.ControllerType.AI;
            }
            else
            {
                previousAgent = _thisLeader;
            }

            PreSwitchTeam?.Invoke();
            _targetAgent.Controller = Agent.ControllerType.Player;
            this.Mission.PlayerTeam = this.Mission.PlayerEnemyTeam;
            PostSwitchTeam?.Invoke();
            previousAgent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            this._thisLeader = Mission.MainAgent;
            this.enemyLeader = previousAgent;
            Utility.SetPlayerAsCommander();
        }
    }
}