using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class ReadPositionLogic : MissionLogic
    {
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (this.Mission.InputManager.IsKeyPressed(TaleWorlds.InputSystem.InputKey.I))
            {
                Agent mainAgent = this.Mission.MainAgent;
                Vec3 position = mainAgent != null ? mainAgent.Position : this.Mission.Scene.LastFinalRenderCameraPosition;
                string str = new WorldPosition(this.Mission.Scene, position).GetNavMesh().ToString() ?? "";
                Utility.DisplayMessage(
                    $"Position: {(object)position} | Navmesh: {(object)str} | Time: {(object)this.Mission.Time}");
                ModuleLogger.Log("INFO Position: {0}, Navigation Mesh: {1}", (object)position, (object)str);
            }
        }
    }
}
