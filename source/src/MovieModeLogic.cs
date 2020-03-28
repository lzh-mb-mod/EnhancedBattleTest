using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Engine.Screens;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Screen;

namespace EnhancedBattleTest
{
    class MovieModeLogic : MissionLogic
    {
        public void AgentLabel(bool enable)
        {
            if (enable)
                Mission.AddMissionBehaviour(ViewCreator.CreateMissionAgentLabelUIHandler(Mission));
        }
    }
}
