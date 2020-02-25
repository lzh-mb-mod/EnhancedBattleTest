using System.ComponentModel;
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
            this.Mission.OnMainAgentChanged += OnMainAgentChanged;
        }

        public override void OnRemoveBehaviour()
        {
            this.Mission.OnMainAgentChanged -= OnMainAgentChanged;
        }
        private void OnMainAgentChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.Mission.MainAgent != null)
                Utility.SetPlayerAsCommander();
        }

        public void ControlTroopAfterDead()
        {
            // Mission.MainAgent may be null because of free camera mode.
            if (Utility.IsPlayerDead() && (_switchFreeCameraLogic == null || Utility.IsAgentDead(_switchFreeCameraLogic.playerAgentBackup)))
            {
                Agent closestAllyAgent = this.Mission.GetClosestAllyAgent(this.Mission.PlayerTeam, new WorldPosition(this.Mission.Scene, this.Mission.Scene.LastFinalRenderCameraPosition).GetGroundVec3(), 1000) ??
                                         this.Mission.PlayerTeam.Leader;
                if (closestAllyAgent != null)
                {
                    Utility.DisplayMessage("Taking control of an ally troop.");
                    closestAllyAgent.Controller = Agent.ControllerType.Player;
                }
                else
                    Utility.DisplayMessage("No ally troop to control.");
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (this.Mission.InputManager.IsKeyPressed(TaleWorlds.InputSystem.InputKey.F))
            {
                ControlTroopAfterDead();
            }
        }
    }
}