using EnhancedBattleTest.Data;
using EnhancedBattleTest.GameMode;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace EnhancedBattleTest.UI
{
    [GameStateScreen(typeof(EnhancedBattleTestState))]
    public class EnhancedBattleTestScreen : ScreenBase, IGameStateListener
    {
        private readonly EnhancedBattleTestState _state;
        private readonly TextObject _title;
        private EnhancedBattleTestVM _dataSource;
        private GauntletLayer _gauntletLayer;
        private IGauntletMovie _gauntletMovie;
        private bool _isMovieLoaded;

        public CharacterSelectionView CharacterSelectionView;

        public EnhancedBattleTestScreen(EnhancedBattleTestState state)
        {
            _state = state;
            _title = EnhancedBattleTestSubModule.IsMultiplayer
                ? GameTexts.FindText("str_ebt_multiplayer_battle_option")
                : GameTexts.FindText("str_ebt_singleplayer_battle_option");

        }
        void IGameStateListener.OnActivate()
        {
        }

        void IGameStateListener.OnDeactivate()
        {
        }

        void IGameStateListener.OnInitialize()
        {
        }

        void IGameStateListener.OnFinalize()
        {
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _dataSource = new EnhancedBattleTestVM(_state, _title);
            _gauntletLayer = new GauntletLayer(1) { IsFocusLayer = true };
            LoadMovie();
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            ScreenManager.TrySetFocus(_gauntletLayer);
            _dataSource.SetActiveState(true);
            AddLayer(_gauntletLayer);

            var collection = CharacterCollection.Create(EnhancedBattleTestSubModule.IsMultiplayer);
            collection.Initialize();
            CharacterSelectionView = new CharacterSelectionView();
            CharacterSelectionView.Initialize(this, collection, EnhancedBattleTestSubModule.IsMultiplayer);
        }

        protected override void OnFinalize()
        {
            UnloadMovie();
            RemoveLayer(this._gauntletLayer);
            _dataSource = null;
            _gauntletLayer = null;

            CharacterSelectionView.OnFinalize();
            base.OnFinalize();
        }

        protected override void OnActivate()
        {
            LoadMovie();
            _dataSource?.SetActiveState(true);
            LoadingWindow.DisableGlobalLoadingWindow();
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            UnloadMovie();
            _dataSource?.SetActiveState(false);
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);

            if (_gauntletLayer.Input.IsHotKeyReleased("Exit"))
                _dataSource.ExecuteBack();
        }

        private void LoadMovie()
        {
            if (_isMovieLoaded)
                return;
            _gauntletMovie = _gauntletLayer.LoadMovie(nameof(EnhancedBattleTestScreen), _dataSource);
            _isMovieLoaded = true;
        }

        private void UnloadMovie()
        {
            if (!_isMovieLoaded)
                return;
            _gauntletLayer.ReleaseMovie(_gauntletMovie);
            _gauntletMovie = null;
            _isMovieLoaded = false;
        }
    }
}
