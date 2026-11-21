using EnhancedBattleTest.Data;
using HarmonyLib;
using SandBox.ViewModelCollection;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem.Encounters;

namespace EnhancedBattleTest.Patch
{
    [HarmonyPatch(typeof(SPScoreboardVM), "OnBattleOver")]
    public static class EnhancedBattleTestScoreboardResultPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var isEncounterActive = AccessTools.PropertyGetter(
                typeof(PlayerEncounter), nameof(PlayerEncounter.IsActive));
            var useCampaignResult = AccessTools.Method(
                typeof(EnhancedBattleTestScoreboardResultPatch),
                nameof(UseCampaignResult));
            int replacements = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(isEncounterActive))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = useCampaignResult;
                    replacements++;
                }
                yield return instruction;
            }
            if (replacements != 1)
                throw new InvalidOperationException(
                    "Could not locate the scoreboard's campaign result check.");
        }

        private static bool UseCampaignResult()
        {
            // Test battles intentionally do not finalize campaign results or rewards.
            // Use the native MissionResult path instead of the unresolved MapEvent.
            return EnhancedBattleTestPartyController.Current == null
                   && PlayerEncounter.IsActive;
        }
    }
}
