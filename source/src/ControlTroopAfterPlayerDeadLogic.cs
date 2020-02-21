using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class ControlTroopAfterPlayerDeadLogic : MissionLogic
    {
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (this.Mission.InputManager.IsKeyPressed(TaleWorlds.InputSystem.InputKey.F))
            {
                if (Utility.IsPlayerDead())
                {
                    Agent closestAllyAgent = this.Mission.GetClosestAllyAgent(this.Mission.PlayerTeam, new WorldPosition(this.Mission.Scene, this.Mission.Scene.LastFinalRenderCameraPosition).GetGroundVec3(), 0) ??
                                             this.Mission.PlayerTeam.Leader;
                    if (closestAllyAgent != null)
                    {
                        Utility.DisplayMessage("Taking control of ally troops nearby.");
                        //if (this._playerAgent != null && this._playerAgent.IsActive())
                        //    this._playerAgent.Controller = Agent.ControllerType.AI;
                        closestAllyAgent.Controller = Agent.ControllerType.Player;
                        this.Mission.MainAgent = closestAllyAgent;
                        //this._playerAgent = closestAllyAgent;
                        Utility.SetPlayerAsCommander();
                    }
                    else
                        Utility.DisplayMessage("No ally troop nearby.");
                }
            }
        }
    }
}