using EnhancedBattleTest.Data;
using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace EnhancedBattleTest.Patch
{
    public static class EnhancedBattleTestSavePatch
    {
        [HarmonyPatch(typeof(SaveHandler), "SetSaveArgs")]
        private static class SetSaveArgsPatch
        {
            private static bool Prefix()
            {
                if (!EnhancedBattleTestSaveGuard.IsSavingDisabled)
                    return true;

                EnhancedBattleTestSaveGuard.ShowSavingDisabledMessageOnce();
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveHandler), "SaveTick")]
        private static class SaveTickPatch
        {
            private static bool Prefix()
            {
                if (!EnhancedBattleTestSaveGuard.IsSavingDisabled)
                    return true;

                EnhancedBattleTestSaveGuard.ShowSavingDisabledMessageOnce();
                return false;
            }
        }

        [HarmonyPatch(
            typeof(SaveHandler),
            nameof(SaveHandler.QuickSaveCurrentGame))]
        private static class QuickSaveCurrentGamePatch
        {
            private static bool Prefix()
            {
                return AllowManualSave();
            }
        }

        [HarmonyPatch(typeof(SaveHandler), nameof(SaveHandler.SaveAs))]
        private static class SaveAsPatch
        {
            private static bool Prefix()
            {
                return AllowManualSave();
            }
        }

        private static bool AllowManualSave()
        {
            if (!EnhancedBattleTestSaveGuard.IsSavingDisabled)
                return true;

            EnhancedBattleTestSaveGuard.ShowSavingDisabledMessage();
            return false;
        }
    }
}
