using EnhancedBattleTest.Data;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;

namespace EnhancedBattleTest.Patch
{
    public static class EnhancedBattleTestHeroStatePatch
    {
        [HarmonyPatch(typeof(PartyGroupAgentOrigin), nameof(PartyGroupAgentOrigin.SetKilled))]
        private static class SetKilledPatch
        {
            private static bool Prefix(PartyGroupAgentOrigin __instance)
            {
                if (!ShouldProtect(__instance))
                    return true;

                __instance.SetWounded();
                return false;
            }
        }

        [HarmonyPatch(
            typeof(PartyGroupAgentOrigin),
            nameof(PartyGroupAgentOrigin.OnAgentRemoved))]
        private static class OnAgentRemovedPatch
        {
            private static bool Prefix(PartyGroupAgentOrigin __instance)
            {
                return !ShouldProtect(__instance);
            }
        }

        [HarmonyPatch(
            typeof(SkillLevelingManager),
            nameof(SkillLevelingManager.OnCombatHit))]
        private static class OnCombatHitPatch
        {
            private static bool Prefix()
            {
                return EnhancedBattleTestPartyController.Current == null;
            }
        }

        [HarmonyPatch(
            typeof(CampaignEventDispatcher),
            nameof(CampaignEventDispatcher.OnCharacterDefeated))]
        private static class OnCharacterDefeatedPatch
        {
            private static bool Prefix(Hero winner, Hero loser)
            {
                return !EnhancedBattleTestPartyController.IsParticipatingHero(winner)
                       && !EnhancedBattleTestPartyController.IsParticipatingHero(loser);
            }
        }

        [HarmonyPatch(
            typeof(CampaignEventDispatcher),
            nameof(CampaignEventDispatcher.OnHeroWounded))]
        private static class OnHeroWoundedPatch
        {
            private static bool Prefix(Hero woundedHero)
            {
                return !EnhancedBattleTestPartyController.IsParticipatingHero(woundedHero);
            }
        }

        [HarmonyPatch(typeof(Hero), "OnAddedToParty")]
        private static class OnAddedToPartyPatch
        {
            private static bool Prefix(MobileParty mobileParty)
            {
                return !EnhancedBattleTestPartyController
                    .ShouldIgnoreHeroPartyChange(mobileParty);
            }
        }

        [HarmonyPatch(typeof(Hero), "OnRemovedFromParty")]
        private static class OnRemovedFromPartyPatch
        {
            private static bool Prefix(MobileParty mobileParty)
            {
                return !EnhancedBattleTestPartyController
                    .ShouldIgnoreHeroPartyChange(mobileParty);
            }
        }

        private static bool ShouldProtect(PartyGroupAgentOrigin origin)
        {
            return origin?.Troop?.IsHero == true
                   && EnhancedBattleTestPartyController.IsTestParty(origin.Party);
        }
    }
}
