using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest.Data
{
    public static class EnhancedBattleTestSaveGuard
    {
        public static bool IsSavingDisabled { get; private set; }
        private static bool _postBattleWarningPending;
        private static bool _savingSkippedMessageShown;

        public static void Disable()
        {
            IsSavingDisabled = true;
            _postBattleWarningPending = true;
            _savingSkippedMessageShown = false;
        }

        public static void Reset()
        {
            IsSavingDisabled = false;
            _postBattleWarningPending = false;
            _savingSkippedMessageShown = false;
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

        public static void ShowSavingDisabledMessage()
        {
            Utility.DisplayLocalizedText("str_ebt_save_disabled");
        }

        public static void ShowSavingDisabledMessageOnce()
        {
            if (_savingSkippedMessageShown)
                return;

            _savingSkippedMessageShown = true;
            ShowSavingDisabledMessage();
        }
    }
}
