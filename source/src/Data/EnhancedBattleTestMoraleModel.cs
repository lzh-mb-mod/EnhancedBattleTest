using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.Data
{
    class EnhancedBattleTestMoraleModel : DefaultPartyMoraleModel
    {
        public override int GetDailyStarvationMoralePenalty(PartyBase party)
        {
            return 0;
        }

        public override int GetDailyNoWageMoralePenalty(MobileParty party)
        {
            return 0;
        }

        public override float GetStandardBaseMorale(PartyBase party)
        {
            return 100;
        }

        public override float GetVictoryMoraleChange(PartyBase party)
        {
            return 0;
        }

        public override float GetDefeatMoraleChange(PartyBase party)
        {
            return 0;
        }

        public override float GetEffectivePartyMorale(MobileParty party, StatExplainer explanation = null)
        {
            ExplainedNumber explainedNumber = new ExplainedNumber(100f, explanation, (TextObject)null);
            return explainedNumber.ResultNumber;
        }

        public override int NumberOfDesertersDueToPaymentRatio(MobileParty mobileParty)
        {
            return 0;
        }
    }
}
