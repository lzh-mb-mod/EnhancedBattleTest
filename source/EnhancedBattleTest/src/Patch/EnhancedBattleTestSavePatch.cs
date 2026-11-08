using EnhancedBattleTest.Data;
using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;

namespace EnhancedBattleTest.Patch
{
    public static class EnhancedBattleTestSavePatch
    {
        private static bool _manualSaveConfirmed;

        [HarmonyPatch(typeof(SaveHandler), "SetSaveArgs")]
        private static class SetSaveArgsPatch
        {
            private static readonly Type SaveModeType =
                AccessTools.Method(typeof(SaveHandler), "SetSaveArgs")
                    .GetParameters()[0].ParameterType;
            private static readonly int AutoSaveMode =
                Convert.ToInt32(Enum.Parse(SaveModeType, "AutoSave"));
            private static readonly int SaveAsMode =
                Convert.ToInt32(Enum.Parse(SaveModeType, "SaveAs"));

            // Harmony 2.0.4 passes the private SaveMode enum as its underlying int.
            private static bool Prefix(SaveHandler __instance, int __0, string __1)
            {
                if (EnhancedBattleTestPartyController.Current != null)
                    return false;

                if (!EnhancedBattleTestSaveGuard.RequiresManualSaveConfirmation)
                    return true;

                if (__0 == AutoSaveMode)
                {
                    EnhancedBattleTestSaveGuard.ShowAutosaveDisabledMessageOnce();
                    return false;
                }

                if (_manualSaveConfirmed)
                    return true;

                return __0 == SaveAsMode
                    ? AllowManualSave(() => __instance.SaveAs(__1))
                    : AllowManualSave(
                        __instance.QuickSaveCurrentGame,
                        skipIfInquiryActive: true);
            }
        }

        [HarmonyPatch(typeof(EscapeMenuItemVM), nameof(EscapeMenuItemVM.ExecuteAction))]
        private static class SaveAndExitPatch
        {
            private const string SaveAndExitTextId = "AbEh2y8o";

            private static bool Prefix(EscapeMenuItemVM __instance, TextObject ____itemObj)
            {
                // The native action hides inquiries and sets the exit-on-save flag.
                // Confirm before invoking it so cancellation does neither.
                return ____itemObj?.GetID() != SaveAndExitTextId
                       || AllowManualSave(__instance.ExecuteAction);
            }
        }

        private static bool AllowManualSave(Action save, bool skipIfInquiryActive = false)
        {
            if (EnhancedBattleTestPartyController.Current != null)
                return false;

            if (!EnhancedBattleTestSaveGuard.RequiresManualSaveConfirmation
                || _manualSaveConfirmed)
                return true;

            if (skipIfInquiryActive && InformationManager.IsAnyInquiryActive?.Invoke() == true)
                return false;

            Campaign campaign = Campaign.Current;
            EnhancedBattleTestSaveGuard.ShowManualSaveConfirmation(() =>
            {
                if (Campaign.Current != campaign
                    || !EnhancedBattleTestSaveGuard.RequiresManualSaveConfirmation
                    || EnhancedBattleTestPartyController.Current != null)
                    return;

                _manualSaveConfirmed = true;
                try
                {
                    save();
                }
                finally
                {
                    _manualSaveConfirmed = false;
                }
            });
            return false;
        }
    }
}
