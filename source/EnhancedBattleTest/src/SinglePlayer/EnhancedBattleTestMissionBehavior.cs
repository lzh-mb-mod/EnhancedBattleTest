using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.SinglePlayer
{
    public class EnhancedBattleTestMissionBehavior : MissionLogic
    {
        public override void OnBattleEnded()
        {
            base.OnBattleEnded();

            BattleStarter.MissionEnded();
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
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
