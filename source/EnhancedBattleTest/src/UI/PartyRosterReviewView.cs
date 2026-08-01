using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace EnhancedBattleTest.UI
{
    public sealed class PartyRosterReviewView
    {
        private bool _isActive;
        private ScreenBase _screen;
        private GauntletLayer _layer;
        private GauntletMovieIdentifier _movie;
        private PartyRosterReviewVM _dataSource;

        public void Initialize(ScreenBase screen)
        {
            _screen = screen;
            _dataSource = new PartyRosterReviewVM(BeginReview, EndReview);
        }

        public void OnFinalize()
        {
            if (_isActive)
                EndReview();
            _dataSource?.OnFinalize();
            _dataSource = null;
            _screen = null;
        }

        private void BeginReview(PartyRosterReviewData data)
        {
            if (_isActive)
                return;
            _isActive = true;
            _layer = new GauntletLayer("GauntletLayer", 50)
            {
                IsFocusLayer = true
            };
            _movie = _layer.LoadMovie(
                nameof(PartyRosterReviewView),
                _dataSource);
            _screen.AddLayer(_layer);
            ScreenManager.TrySetFocus(_layer);
            _layer.InputRestrictions.SetInputRestrictions(
                true,
                InputUsageMask.All);
        }

        private void EndReview()
        {
            if (!_isActive)
                return;
            _isActive = false;
            _layer.InputRestrictions.ResetInputRestrictions();
            _layer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(_layer);
            _screen.RemoveLayer(_layer);
            _layer.ReleaseMovie(_movie);
            _movie = null;
            _layer = null;
        }
    }
}
