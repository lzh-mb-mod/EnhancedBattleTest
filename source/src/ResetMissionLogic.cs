using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    class ResetMissionLogic : MissionLogic
    {
        private EnhancedFreeBattleMissionController _controller;

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();

            _controller = Mission.GetMissionBehaviour<EnhancedFreeBattleMissionController>();
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
            Mission.PlayerTeam.PlayerOrderController.Owner = null;
            Mission.PlayerEnemyTeam.PlayerOrderController.Owner = null;
            _controller?.SpawnAgents();
            Utility.DisplayLocalizedText("str_mission_reset");
        }
    }
}
