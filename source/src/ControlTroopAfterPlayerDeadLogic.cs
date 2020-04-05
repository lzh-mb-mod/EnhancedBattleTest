using System.ComponentModel;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    class ControlTroopAfterPlayerDeadLogic : MissionLogic
    {

        public void ControlTroopAfterDead()
        {
            // Mission.MainAgent may be null because of free camera mode.
            if (Utility.IsPlayerDead() && Utility.IsAgentDead(this.Mission.PlayerTeam.PlayerOrderController.Owner))
            {
                Agent closestAllyAgent = this.Mission.GetClosestAllyAgent(this.Mission.PlayerTeam, new WorldPosition(this.Mission.Scene, this.Mission.Scene.LastFinalRenderCameraPosition).GetGroundVec3(), 1000) ??
                                         this.Mission.PlayerTeam.Leader;
                if (closestAllyAgent != null)
                {
                    Utility.DisplayLocalizedText("str_control_troop");
                    closestAllyAgent.Controller = Agent.ControllerType.Player;
                }
                else
                    Utility.DisplayLocalizedText("str_no_troop_to_control");
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