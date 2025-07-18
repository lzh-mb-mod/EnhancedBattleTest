﻿using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
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

        public override ExplainedNumber GetEffectivePartyMorale(
            MobileParty mobileParty,
            bool includeDescription = false)
        {
            ExplainedNumber explainedNumber = new ExplainedNumber(50f, includeDescription);
            return explainedNumber;
        }
    }
}
