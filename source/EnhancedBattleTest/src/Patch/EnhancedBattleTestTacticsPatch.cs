using EnhancedBattleTest.Data;
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Patch
{
    public static class EnhancedBattleTestTacticsPatch
    {
        [HarmonyPatch]
        private static class TacticsSkillPatch
        {
            private static MethodBase TargetMethod()
            {
                MethodInfo interfaceMethod = typeof(IBattleCombatant).GetMethod(
                    nameof(IBattleCombatant.GetTacticsSkillAmount));
                InterfaceMapping mapping =
                    typeof(PartyBase).GetInterfaceMap(typeof(IBattleCombatant));
                int index = Array.IndexOf(
                    mapping.InterfaceMethods,
                    interfaceMethod);
                return mapping.TargetMethods[index];
            }

            private static bool Prefix(PartyBase __instance, ref int __result)
            {
                if (!EnhancedBattleTestPartyController.TryGetTacticLevel(
                        __instance,
                        out int tacticLevel))
                    return true;

                __result = tacticLevel;
                return false;
            }
        }

    }
}
