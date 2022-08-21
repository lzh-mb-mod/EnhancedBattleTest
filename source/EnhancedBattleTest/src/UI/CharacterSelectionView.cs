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
        private IGauntletMovie _movie;
        private ScreenBase _screen;
        private GauntletLayer _gauntletLayer;
        private bool _isLastActiveGameStateActive;
        private bool _isLastActiveGameStatePaused;

        public void Initialize(ScreenBase screen, CharacterCollection characterCollection, bool isMultiplayer)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                _screen = screen;
                _characterCollection = characterCollection;
                _dataSource = new CharacterSelectionVM(_characterCollection, BeginSelection, EndSelection, isMultiplayer);
            }

            _isActive = false;
        }

        public void OnFinalize()
        {
            if (!_isInitialized)
                return;
            _isInitialized = false;
            _screen = null;
            _isActive = false;
            _characterCollection = null;
            _dataSource.OnFinalize();
            _dataSource = null;
            _gauntletLayer = null;
            _movie = null;
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
            _isLastActiveGameStateActive = GameStateManager.Current.ActiveStateDisabledByUser;
            GameStateManager.Current.RegisterActiveStateDisableRequest(this);
            MBCommon.PauseGameEngine();
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
            _gauntletLayer = new GauntletLayer(50, "GauntletLayer");
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
        }
    }
}
