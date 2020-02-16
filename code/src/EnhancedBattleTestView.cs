using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Modbed
{
    public class EnhancedBattleTestView : MissionView
    {
        private GauntletLayer _gauntletLayer;
        private EnhancedBattleTestVM _dataSource;
        private EnhancedBattleTestMissionController _missionController;
        private CharacterSelectionView _selectionView;
        private EnhancedBattleTestMissionOrderUIHandler _orderView;
        private bool _isOpen;
        private bool _toOpen;

        public EnhancedBattleTestView(CharacterSelectionView selectionView, EnhancedBattleTestMissionOrderUIHandler orderView)
        {
            this._selectionView = selectionView;
            this._orderView = orderView;
            this.ViewOrderPriorty = 22;
            this._isOpen = this._toOpen = false;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._missionController = this.Mission.GetMissionBehaviour<EnhancedBattleTestMissionController>();
            this._toOpen = this._missionController.ShowSelectViewFirst;
        }
        public override void OnMissionScreenFinalize()
        {
            if (this._gauntletLayer != null)
            {
                this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
                this.MissionScreen.RemoveLayer(_gauntletLayer);
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
            if (this._toOpen && this.MissionScreen.SetDisplayDialog(true))
            {
                this._toOpen = false;
                this.OnOpen();
            }
        }
        public override bool OnEscape()
        {
            if (!this._isOpen)
                return base.OnEscape();
            this.OnClose();
            return true;
        }

        public void OnOpen()
        {
            if (this._isOpen)
                return;
            this._isOpen = true;
            this._dataSource = new EnhancedBattleTestVM(_selectionView, (param) =>
            {
                this._missionController.BattleTestParams = param;
                this._missionController.AddTeams();
                this._orderView.EnhancedBattleInitialize();
                this._missionController.SpawnAgents();
                this.OnClose();
            }, (param) =>
            {
                this.Mission.EndMission();
            });

            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty, "GauntletLayer");
            this._gauntletLayer.LoadMovie(nameof(EnhancedBattleTestView), this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, TaleWorlds.Library.InputUsageMask.All);
            this.MissionScreen.AddLayer(this._gauntletLayer);
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

            if (this._dataSource != null)
            {
                this._dataSource.OnFinalize();
                this._dataSource = null;
            }
        }
    }
}