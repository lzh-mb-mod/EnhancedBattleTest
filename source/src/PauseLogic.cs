using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    class PauseLogic : MissionLogic
    {
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Input.IsKeyPressed(InputKey.P))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            MissionState.Current.Paused = !MissionState.Current.Paused;
            Utility.DisplayMessage("Mission " + (MissionState.Current.Paused ? "paused." : "unpaused."));
        }
    }
}
