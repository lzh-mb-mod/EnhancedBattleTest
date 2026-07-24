using EnhancedBattleTest.Data;
using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Patch
{
    public static class EnhancedBattleTestBannerPatch
    {
        [HarmonyPatch(
            typeof(PartyBase),
            nameof(PartyBase.Banner),
            MethodType.Getter)]
        private static class PartyBannerPatch
        {
            private static void Postfix(PartyBase __instance, ref Banner __result)
            {
                if (EnhancedBattleTestPartyController
                    .TryGetTemporaryPartyAppearance(
                        __instance,
                        out Banner banner,
                        out _))
                    __result = banner;
            }
        }

        [HarmonyPatch(
            typeof(PartyBase),
            nameof(PartyBase.PrimaryColorPair),
            MethodType.Getter)]
        private static class PartyColorsPatch
        {
            private static void Postfix(
                PartyBase __instance,
                ref Tuple<uint, uint> __result)
            {
                if (EnhancedBattleTestPartyController
                    .TryGetTemporaryPartyAppearance(
                        __instance,
                        out _,
                        out Tuple<uint, uint> colors))
                    __result = colors;
            }
        }

        [HarmonyPatch(
            typeof(PartyGroupAgentOrigin),
            nameof(PartyGroupAgentOrigin.Banner),
            MethodType.Getter)]
        private static class AgentOriginBannerPatch
        {
            private static void Postfix(
                PartyGroupAgentOrigin __instance,
                ref Banner __result)
            {
                if (EnhancedBattleTestPartyController
                    .TryGetTemporaryPartyAppearance(
                        __instance.Party,
                        out Banner banner,
                        out _))
                    __result = banner;
            }
        }

        [HarmonyPatch(
            typeof(PartyGroupAgentOrigin),
            nameof(PartyGroupAgentOrigin.IsPartyUnderPlayerCommand))]
        private static class PartyUnderPlayerCommandPatch
        {
            private static bool Prefix(PartyBase party, ref bool __result)
            {
                if (!EnhancedBattleTestPartyController.IsTestParty(party))
                    return true;

                __result =
                    EnhancedBattleTestPartyController.IsPartyInPlayerTeam(party);
                return false;
            }
        }
    }
}
