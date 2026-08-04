#if DEBUG
using EnhancedBattleTest.Data.MissionData;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace EnhancedBattleTest.UI
{
    public sealed class ExposureAdjustmentMissionView : MissionView
    {
        private const string MovieName = "ExposureAdjustmentView";
        private readonly AtmosphereModel.ExposureInfo _initialExposureInfo;
        private ExposureAdjustmentVM _dataSource;
        private GauntletLayer _gauntletLayer;
        private IGauntletMovie _movie;

        public ExposureAdjustmentMissionView(
            AtmosphereModel.ExposureInfo initialExposureInfo)
        {
            _initialExposureInfo = initialExposureInfo;
            ViewOrderPriority = 100;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            _dataSource = new ExposureAdjustmentVM(
                _initialExposureInfo.Min ?? 0f,
                _initialExposureInfo.Max ?? 0f,
                _initialExposureInfo.Target ?? 0f,
                Apply,
                Close);
        }

        public override void OnMissionScreenFinalize()
        {
            Close();
            _dataSource?.OnFinalize();
            _dataSource = null;
            base.OnMissionScreenFinalize();
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            IInputContext input = _gauntletLayer?.Input ?? Input;
            if (input.IsControlDown() && input.IsKeyReleased(InputKey.E))
            {
                if (_gauntletLayer == null)
                    Open();
                else
                    Close();
                return;
            }

            if (_gauntletLayer != null
                && input.IsKeyReleased(InputKey.Escape))
            {
                Close();
            }
        }

        public override bool OnEscape()
        {
            if (_gauntletLayer == null)
                return base.OnEscape();

            Close();
            return true;
        }

        private void Open()
        {
            if (_gauntletLayer != null)
                return;

            _gauntletLayer = new GauntletLayer(
                ViewOrderPriority,
                nameof(ExposureAdjustmentMissionView))
            {
                IsFocusLayer = true
            };
            _movie = _gauntletLayer.LoadMovie(
                MovieName,
                _dataSource);
            MissionScreen.AddLayer(_gauntletLayer);
            ScreenManager.TrySetFocus(_gauntletLayer);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(
                true,
                InputUsageMask.All);
        }

        private void Close()
        {
            if (_gauntletLayer == null)
                return;

            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            _gauntletLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(_gauntletLayer);
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer.ReleaseMovie(_movie);
            _movie = null;
            _gauntletLayer = null;
        }

        private void Apply(
            float minExposure,
            float maxExposure,
            float targetExposure)
        {
            Scene scene = Mission?.Scene;
            if (scene == null)
                return;

            scene.SetMinExposure(minExposure);
            scene.SetMaxExposure(maxExposure);
            scene.SetTargetExposure(targetExposure);
        }

    }
}
#endif
