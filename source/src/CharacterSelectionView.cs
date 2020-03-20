using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    public class CharacterSelectionView : MissionView
    {
        private CharacterSelectionVM _dataSource;
        private GauntletLayer _gauntletLayer;
        private GauntletMovie _movie;
        private CharacterSelectionParams _params;
        private bool _withPerks;
        private bool _isOpen;
        private bool _toOpen;

        public CharacterSelectionView(bool withPerks)
        {
            this._params = null;
            this._withPerks = withPerks;
            this.ViewOrderPriorty = 23;
            this._isOpen = this._toOpen = false;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();

        }
        public override void OnMissionScreenFinalize()
        {
            if (this._gauntletLayer != null)
            {
                this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
                this.MissionScreen.RemoveLayer(this._gauntletLayer);
                this._gauntletLayer = null;
            }
            if (this._dataSource != null)
            {
                this._dataSource.OnFinalize();
                this._dataSource = null;
            }
            base.OnMissionScreenFinalize();
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (this._toOpen)
            {
                this._toOpen = false;
                this.OnOpen();
            }

            if (this._isOpen)
            {
                var input = this._gauntletLayer.Input;
                if (input.IsKeyPressed(InputKey.F5))
                {
                    this._movie.WidgetFactory.CheckForUpdates();
                    this.HandleLoadMovie();
                }
            }
        }
        public override bool OnEscape()
        {
            if (!this._isOpen)
                return base.OnEscape();
            this.OnClose();
            return true;
        }

        public void Open(CharacterSelectionParams p)
        {
            this._params = p;
            this._toOpen = true;
        }
        private void OnOpen()
        {
            if (this._isOpen)
                return;
            this._isOpen = true;

            this._dataSource = new CharacterSelectionVM(this._params);
            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty, "GauntletLayer") {IsFocusLayer = true};
            this.MissionScreen.AddLayer(this._gauntletLayer);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            ScreenManager.TrySetFocus((ScreenLayer)this._gauntletLayer);
            this.HandleLoadMovie();
        }

        public void OnClose()
        {
            if (!this._isOpen)
                return;
            this._isOpen = false;
            this.MissionScreen.RemoveLayer(this._gauntletLayer);
            this.MissionScreen.SetDisplayDialog(false);
            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
            this._gauntletLayer = null;
            this._dataSource.OnFinalize();
            this._dataSource = null;
        }

        private void HandleLoadMovie()
        {
            var vm = this._dataSource;
            this._movie = this._gauntletLayer.LoadMovie(_withPerks ? "CharacterSelectionViewWithPerks" : "CharacterSelectionViewWithoutPerks", (ViewModel)this._dataSource);

            var culturesListPanel = this._movie.RootView.Target.FindChild("Cultures", true) as ListPanel;
            var groupsListPanel = this._movie.RootView.Target.FindChild("Groups", true) as ListPanel;
            var charactersListPanel = this._movie.RootView.Target.FindChild("Characters", true) as ListPanel;
            var firstPerkListPanel = this._movie.RootView.Target.FindChild("FirstPerks", true) as ListPanel;
            var secondPerkListPanel = this._movie.RootView.Target.FindChild("SecondPerks", true) as ListPanel;

            culturesListPanel.IntValue = vm.SelectedCultureIndex;
            groupsListPanel.IntValue = vm.SelectedGroupIndex;
            charactersListPanel.IntValue = vm.SelectedCharacterIndex;
            if (firstPerkListPanel != null)
                firstPerkListPanel.IntValue = vm.SelectedFirstPerkIndex;
            if (secondPerkListPanel != null)
                secondPerkListPanel.IntValue = vm.SelectedSecondPerkIndex;

            ModuleLogger.Log("vm.SelectedCharacterIndex {0}", vm.SelectedCharacterIndex);


            culturesListPanel.SelectEventHandlers.Add(w =>
            {
                vm.SelectedCultureChanged(w as ListPanel);
                groupsListPanel.IntValue = vm.SelectedGroupIndex;
                charactersListPanel.IntValue = vm.SelectedCharacterIndex;
                if (firstPerkListPanel != null)
                    firstPerkListPanel.IntValue = vm.SelectedFirstPerkIndex;
                if (secondPerkListPanel != null)
                    secondPerkListPanel.IntValue = vm.SelectedSecondPerkIndex;
            });
            groupsListPanel.SelectEventHandlers.Add(w =>
            {
                vm.SelectedGroupChanged(w as ListPanel);
                charactersListPanel.IntValue = vm.SelectedCharacterIndex;
                if (firstPerkListPanel != null)
                    firstPerkListPanel.IntValue = vm.SelectedFirstPerkIndex;
                if (secondPerkListPanel != null)
                    secondPerkListPanel.IntValue = vm.SelectedSecondPerkIndex;
            });
            charactersListPanel.SelectEventHandlers.Add(w =>
            {
                vm.SelectedCharacterChanged(w as ListPanel);
                if (firstPerkListPanel != null)
                    firstPerkListPanel.IntValue = vm.SelectedFirstPerkIndex;
                if (secondPerkListPanel != null)
                    secondPerkListPanel.IntValue = vm.SelectedSecondPerkIndex;
            });
            firstPerkListPanel?.SelectEventHandlers.Add(w => { vm.SelectedFirstPerkChanged(w as ListPanel); });
            secondPerkListPanel?.SelectEventHandlers.Add(w => { vm.SelectedSecondPerkChanged(w as ListPanel); });
        }
    }
}