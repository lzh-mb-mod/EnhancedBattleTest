using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;

namespace EnhancedBattleTest.SinglePlayer
{
    public class EnhancedBattleTestCampaignEventReceiver : CampaignEventReceiver
    {
        public override void CanHeroDie(Hero hero, KillCharacterAction.KillCharacterActionDetail causeOfDeath, ref bool result)
        {
            base.CanHeroDie(hero, causeOfDeath, ref result);

            if (hero.PartyBelongedTo?.PartyComponent is EnhancedBattleTestPartyComponent)
            {
                result = false;
            }
        }
    }
}
