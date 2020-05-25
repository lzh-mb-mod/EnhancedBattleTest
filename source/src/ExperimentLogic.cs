using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class ExperimentLogic : MissionLogic
    {
        private BattleConfigBase _config;

        public ExperimentLogic(BattleConfigBase config)
        {
            _config = config;
        }
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (Input.IsKeyPressed(InputKey.U))
            {
                _config.changeCombatAI = !_config.changeCombatAI;
                foreach (var agent in Mission.Agents.Where(agent => agent.IsHuman))
                {
                    if (_config.changeCombatAI)
                    {
                        AgentStatModel.SetAgentAIStat(agent, agent.AgentDrivenProperties, _config.combatAI);
                    }
                    else
                    {
                        MissionGameModels.Current.AgentStatCalculateModel.InitializeAgentStats(agent, agent.SpawnEquipment,
                            agent.AgentDrivenProperties, null);
                    }
                    agent.UpdateAgentProperties();
                }
            }
        }
    }
}
