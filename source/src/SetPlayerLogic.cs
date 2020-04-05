using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class SetPlayerLogic : MissionLogic
    {
        private BasicCharacterObject _playerCharacter;
        public SetPlayerLogic(BasicCharacterObject playerCharacter)
        {
            _playerCharacter = playerCharacter;
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);

            if (agent.Character == _playerCharacter)
            {
                // add player to MissionOrderVM, or it will crash after switching to free camera if player team AI is enabled and move player to other formation.
                agent.Controller = Agent.ControllerType.AI;
                Mission.GetMissionBehaviour<SwitchTeamMissionOrderUIHandler>()?.dataSource?.AddTroops(agent);
                agent.Controller = Agent.ControllerType.Player;
            }
        }
    }
}
