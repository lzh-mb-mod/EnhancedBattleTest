using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.SinglePlayer
{
    public class EnhancedBattleTestMissionBehavior : MissionLogic
    {
        private bool ended = false;

        public override void OnSurrenderMission()
        {
            base.OnSurrenderMission();

            if (!ended)
            {
                ended = true;
                BattleStarter.BeforeMissionEnded();
            }
        }

        // Called when leaving battle before end
        public override void OnRetreatMission()
        {
            base.OnRetreatMission();
            if(!ended)
            {
                ended = true;
                BattleStarter.BeforeMissionEnded();                
            }
        }

        // Called on battle end score
        public override void OnMissionResultReady(MissionResult missionResult)
        {
            if (!ended)
            {
                ended = true;
                BattleStarter.BeforeMissionEnded();
            }
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            BattleStarter.MissionEnded();
        }

        public override void OnEarlyAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnEarlyAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            if (affectedAgent.Character != null && affectedAgent.IsHero)
            {
                affectedAgent.Health = affectedAgent.HealthLimit;
            }
        }
    }
}
