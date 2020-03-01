using System;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class TeleportPlayerLogic : MissionLogic
    {
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            Agent playerAgent = this.Mission.PlayerTeam.PlayerOrderController.Owner;
            if (this.Mission.InputManager.IsKeyPressed(InputKey.L) &&
                this.Mission.MainAgent == null &&
                !Utility.IsAgentDead(playerAgent))
            {
                TeleportAgent(playerAgent);
            }
        }
        public void TeleportAgent(Agent agent)
        {
            try
            {
                Vec3 groundVec3 = new WorldPosition(this.Mission.Scene, this.Mission.Scene.LastFinalRenderCameraPosition).GetGroundVec3();
                Vec3 vec3 = -this.Mission.Scene.LastFinalRenderCameraFrame.rotation.u;
                agent.LookDirection = vec3;
                agent.TeleportToPosition(groundVec3);
                agent.Controller = Agent.ControllerType.Player;
                Utility.DisplayMessage($"Teleport player to {(object) groundVec3}");
            }
            catch (Exception ex)
            {
                Utility.DisplayMessage(ex.Message);
            }
        }
    }
}
