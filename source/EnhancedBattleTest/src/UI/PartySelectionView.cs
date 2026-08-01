using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace EnhancedBattleTest.UI
{
    public sealed class PartySelectionView
    {
        private bool _isActive;
        private ScreenBase _screen;
        private GauntletLayer _gauntletLayer;
        private GauntletMovieIdentifier _movie;
        private PartySelectionVM _dataSource;

        public void Initialize(ScreenBase screen)
        {
            _screen = screen;
            _dataSource = new PartySelectionVM(
                BeginSelection,
                EndSelection);
        }

        public void OnFinalize()
        {
            if (_isActive)
                EndSelection();
            _dataSource?.OnFinalize();
            _dataSource = null;
            _screen = null;
        }

        private void BeginSelection(PartySelectionData data)
        {
            if (_isActive)
                return;

            _isActive = true;
            _gauntletLayer = new GauntletLayer("GauntletLayer", 50)
            {
                IsFocusLayer = true
            };
            _movie = _gauntletLayer.LoadMovie(
                nameof(PartySelectionView),
                _dataSource);
            _screen.AddLayer(_gauntletLayer);
            ScreenManager.TrySetFocus(_gauntletLayer);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(
                true,
                InputUsageMask.All);
        }

        private void EndSelection()
        {
            if (!_isActive)
                return;

            _isActive = false;
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
