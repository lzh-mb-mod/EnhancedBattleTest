using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class ResetMissionLogic : MissionLogic
    {
        private EnhancedTestBattleMissionController _controller;

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();

            _controller = Mission.GetMissionBehaviour<EnhancedTestBattleMissionController>();
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (Input.IsKeyPressed(InputKey.F12))
                ResetMission();
        }

        public void ResetMission()
        {
            Mission.ResetMission();
            _controller?.SpawnAgents();
            Utility.DisplayMessage("Mission reset.");
        }
    }
}
