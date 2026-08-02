using EnhancedBattleTest.Data;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace EnhancedBattleTest.UI
{
    public class CharacterSelectionView
    {
        private bool _isInitialized;
        private bool _isActive;
        private CharacterCollection _characterCollection;
        private CharacterSelectionVM _dataSource;
        private GauntletMovieIdentifier _movie;
        private ScreenBase _screen;
        private GauntletLayer _gauntletLayer;
        private bool _isLastActiveGameStatePaused;

        public void Initialize(ScreenBase screen, CharacterCollection characterCollection)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                _screen = screen;
                _characterCollection = characterCollection;
                _dataSource = new CharacterSelectionVM(
                    _characterCollection,
                    BeginSelection,
                    EndSelection);
            }

            _isActive = false;
        }

        public void OnFinalize()
        {
            if (!_isInitialized)
                return;
            if (_isActive)
                EndSelection();
            _isInitialized = false;
            _screen = null;
            _characterCollection = null;
            _dataSource.OnFinalize();
            _dataSource = null;
            GameStateManager.Current.UnregisterActiveStateDisableRequest(this);
        }

        public void BeginSelection(CharacterSelectionData data)
        {
            if (_isActive)
                return;
            _isActive = true;
            CreateLayer();
            _isLastActiveGameStatePaused = data.PauseGameActiveState;
            if (!_isLastActiveGameStatePaused)
                return;
            GameStateManager.Current.RegisterActiveStateDisableRequest(this);
            MBCommon.PauseGameEngine();
        }

        public bool OnEscape()
        {
            if (!_isActive)
                return false;

            EndSelection();
            return true;
        }

        private void EndSelection()
        {
            if (!_isActive)
                return;
            _isActive = false;
            RemoveLayer();
            if (!_isLastActiveGameStatePaused)
                return;
            GameStateManager.Current.UnregisterActiveStateDisableRequest(this);
            MBCommon.UnPauseGameEngine();
        }

        private void CreateLayer()
        {
            _gauntletLayer = new GauntletLayer("GauntletLayer", 50);
            _movie = _gauntletLayer.LoadMovie(nameof(CharacterSelectionView), _dataSource);
            _screen.AddLayer(_gauntletLayer);
            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
        }

        private void RemoveLayer()
        {
            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            _gauntletLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(_gauntletLayer);
            _screen.RemoveLayer(_gauntletLayer);
            _gauntletLayer.ReleaseMovie(_movie);
            _movie = null;
            _gauntletLayer = null;
        }
    }
}
