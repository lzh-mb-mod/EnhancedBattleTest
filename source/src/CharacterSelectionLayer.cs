using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class CharacterSelectionLayer : GlobalLayer
    {
        private bool _isInitialized;
        private bool _isActive;
        private CharacterCollection _characterCollection;
        private CharacterSelectionVM _dataSource;
        private GauntletMovie _movie;
        private GauntletLayer _gauntletLayer;
        private bool _isLastActiveGameStateActive;
        private bool _isLastActiveGameStatePaused;

        public void Initialize(CharacterCollection characterCollection, bool isMultiplayer)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                _characterCollection = characterCollection;
                _dataSource = new CharacterSelectionVM(_characterCollection, BeginSelection, EndSelection, isMultiplayer);
                _gauntletLayer = new GauntletLayer(50, "GauntletLayer");
                _movie = _gauntletLayer.LoadMovie(nameof(CharacterSelectionLayer), _dataSource);
                Layer = _gauntletLayer;
                ScreenManager.AddGlobalLayer((GlobalLayer)this, true);
            }

            _isActive = false;
            ScreenManager.SetSuspendLayer(this.Layer, true);
        }

        public void OnFinalize()
        {
            if (!_isInitialized)
                return;
            _isInitialized = false;
            _isActive = false;
            _characterCollection = null;
            ScreenManager.RemoveGlobalLayer(this);
            _dataSource.OnFinalize();
            _dataSource = null;
            _gauntletLayer = null;
            _movie = null;
            Layer = null;
        }

        public void BeginSelection(CharacterSelectionData data)
        {
            if (_isActive)
                return;
            _isActive = true;
            ScreenManager.SetSuspendLayer(Layer, false);
            Layer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(Layer);
            Layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _isLastActiveGameStatePaused = data.PauseGameActiveState;
            if (!_isLastActiveGameStatePaused)
                return;
            _isLastActiveGameStateActive = GameStateManager.Current.ActiveStateDisabledByUser;
            GameStateManager.Current.ActiveStateDisabledByUser = true;
            MBCommon.PauseGameEngine();
        }

        private void EndSelection()
        {
            if (!_isActive)
                return;
            _isActive = false;
            Layer.InputRestrictions.ResetInputRestrictions();
            ScreenManager.SetSuspendLayer(Layer, true);
            Layer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(Layer);
            if (!_isLastActiveGameStatePaused)
                return;
            GameStateManager.Current.ActiveStateDisabledByUser = _isLastActiveGameStateActive;
            MBCommon.UnPauseGameEngine();
        }
    }
}
