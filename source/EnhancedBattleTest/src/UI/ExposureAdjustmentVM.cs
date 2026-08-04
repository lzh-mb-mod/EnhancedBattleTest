#if DEBUG
using EnhancedBattleTest.UI.Basic;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest.UI
{
    public sealed class ExposureAdjustmentVM : ViewModel
    {
        private readonly Action<float, float, float> _apply;
        private readonly Action _close;

        public TextVM TitleText { get; }
        public TextVM DescriptionText { get; }
        public TextVM MinExposureText { get; }
        public TextVM MaxExposureText { get; }
        public TextVM TargetExposureText { get; }
        public TextVM ApplyText { get; }
        public TextVM CloseText { get; }

        public NumberVM<float> MinExposure { get; }
        public NumberVM<float> MaxExposure { get; }
        public NumberVM<float> TargetExposure { get; }

        public ExposureAdjustmentVM(
            float minExposure,
            float maxExposure,
            float targetExposure,
            Action<float, float, float> apply,
            Action close)
        {
            _apply = apply;
            _close = close;
            TitleText = new TextVM(
                GameTexts.FindText("str_ebt_exposure_adjustment"));
            DescriptionText = new TextVM(
                GameTexts.FindText("str_ebt_exposure_adjustment_description"));
            MinExposureText = new TextVM(
                GameTexts.FindText("str_ebt_min_exposure"));
            MaxExposureText = new TextVM(
                GameTexts.FindText("str_ebt_max_exposure"));
            TargetExposureText = new TextVM(
                GameTexts.FindText("str_ebt_target_exposure"));
            ApplyText = new TextVM(GameTexts.FindText("str_ebt_apply"));
            CloseText = new TextVM(GameTexts.FindText("str_close"));

            MinExposure = new NumberVM<float>(
                minExposure,
                -20f,
                5f,
                false);
            MaxExposure = new NumberVM<float>(
                maxExposure,
                -20f,
                5f,
                false);
            TargetExposure = new NumberVM<float>(
                targetExposure,
                -20f,
                5f,
                false);
        }

        private void Apply()
        {
            _apply?.Invoke(
                MinExposure.Value,
                MaxExposure.Value,
                TargetExposure.Value);
        }

        private void Close()
        {
            _close?.Invoke();
        }
    }
}
#endif
