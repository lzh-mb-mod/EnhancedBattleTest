using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.Data
{
    public static class EnhancedBattleTestSaveGuard
    {
        public static bool RequiresManualSaveConfirmation { get; private set; }
        private static bool _postBattleWarningPending;
        private static bool _autosaveDisabledMessageShown;

        public static void MarkCampaignChanged()
        {
            RequiresManualSaveConfirmation = true;
            _postBattleWarningPending = true;
            _autosaveDisabledMessageShown = false;
        }

        public static void Reset()
        {
            RequiresManualSaveConfirmation = false;
            _postBattleWarningPending = false;
            _autosaveDisabledMessageShown = false;
        }

        public static bool ConsumePostBattleWarning()
        {
            if (!_postBattleWarningPending)
                return false;

            _postBattleWarningPending = false;
            return true;
        }

        public static void ShowCampaignStateWarning(
            bool requireConfirmation,
            Action onConfirm = null)
        {
            InformationManager.ShowInquiry(
                new InquiryData(
                    GameTexts.FindText("str_ebt_campaign_state_warning_title").ToString(),
                    GameTexts.FindText("str_ebt_campaign_state_warning").ToString(),
                    true,
                    requireConfirmation,
                    requireConfirmation
                        ? GameTexts.FindText("str_continue").ToString()
                        : GameTexts.FindText("str_ok").ToString(),
                    requireConfirmation
                        ? GameTexts.FindText("str_cancel").ToString()
                        : string.Empty,
                    onConfirm,
                    null),
                false,
                false);
        }

        public static void ShowManualSaveConfirmation(Action onConfirm)
        {
            InformationManager.ShowInquiry(
                new InquiryData(
                    GameTexts.FindText("str_ebt_confirm_manual_save_title").ToString(),
                    GameTexts.FindText("str_ebt_confirm_manual_save").ToString(),
                    true,
                    true,
                    new TextObject("{=bV75iwKa}Save").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    onConfirm,
                    null),
                pauseGameActiveState: true);
        }

        public static void ShowAutosaveDisabledMessageOnce()
        {
            if (_autosaveDisabledMessageShown)
                return;

            _autosaveDisabledMessageShown = true;
            Utility.DisplayLocalizedText("str_ebt_autosave_disabled");
        }
    }
}
