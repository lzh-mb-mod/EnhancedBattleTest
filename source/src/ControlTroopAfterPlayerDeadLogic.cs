using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class ControlTroopAfterPlayerDeadLogic : MissionLogic
    {
        private SwitchFreeCameraLogic _switchFreeCameraLogic;

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
            _switchFreeCameraLogic = this.Mission.GetMissionBehaviour<SwitchFreeCameraLogic>();
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (this.Mission.InputManager.IsKeyPressed(TaleWorlds.InputSystem.InputKey.F))
            {
                // Mission.MainAgent may be null because of free camera mode.
                if (Utility.IsPlayerDead() && (_switchFreeCameraLogic == null || Utility.IsAgentDead(_switchFreeCameraLogic.playerAgentBackup)))
                {
                    Agent closestAllyAgent = this.Mission.GetClosestAllyAgent(this.Mission.PlayerTeam, new WorldPosition(this.Mission.Scene, this.Mission.Scene.LastFinalRenderCameraPosition).GetGroundVec3(), 0) ??
                                             this.Mission.PlayerTeam.Leader;
                    if (closestAllyAgent != null)
                    {
                        Utility.DisplayMessage("Taking control of an ally troop.");
                        //if (this._playerAgent != null && this._playerAgent.IsActive())
                        //    this._playerAgent.Controller = Agent.ControllerType.AI;
                        closestAllyAgent.Controller = Agent.ControllerType.Player;
                        this.Mission.MainAgent = closestAllyAgent;
                        //this._playerAgent = closestAllyAgent;
                        Utility.SetPlayerAsCommander();
                    }
                    else
                        Utility.DisplayMessage("No ally troop to control.");
                }
            }
        }
    }
}