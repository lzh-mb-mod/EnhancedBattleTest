using EnhancedBattleTest.SinglePlayer;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace EnhancedBattleTest.Data
{
    class EnhancedBattleTestMoraleModel : DefaultPartyMoraleModel
    {
        public override int GetDailyStarvationMoralePenalty(PartyBase party)
        {
            return BattleStarter.IsEnhancedBattleTestBattle ? 0 : base.GetDailyStarvationMoralePenalty(party);            
        }

        public override int GetDailyNoWageMoralePenalty(MobileParty party)
        {
            return BattleStarter.IsEnhancedBattleTestBattle ? 0 : base.GetDailyNoWageMoralePenalty(party);
        }

        public override float GetStandardBaseMorale(PartyBase party)
        {
            return BattleStarter.IsEnhancedBattleTestBattle ? 100 : base.GetStandardBaseMorale(party);            
        }

        public override float GetVictoryMoraleChange(PartyBase party)
        {
            return BattleStarter.IsEnhancedBattleTestBattle ? 0 : base.GetVictoryMoraleChange(party);
        }

        public override float GetDefeatMoraleChange(PartyBase party)
        {
            return BattleStarter.IsEnhancedBattleTestBattle ? 0 : base.GetDefeatMoraleChange(party);
        }

        public override ExplainedNumber GetEffectivePartyMorale(
            MobileParty mobileParty,
            bool includeDescription = false)
        {
            if (BattleStarter.IsEnhancedBattleTestBattle)
            {
                ExplainedNumber explainedNumber = new ExplainedNumber(50f, includeDescription);
                return explainedNumber;
            } else
            {
                return base.GetEffectivePartyMorale(mobileParty, includeDescription);
            }
        }
    }
}
