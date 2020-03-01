using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    public class EnhancedTestBattleConfigView : MissionView
    {
        private GauntletLayer _gauntletLayer;
        private EnhancedTestBattleConfigVM _dataSource;
        private CharacterSelectionView _selectionView;

        public EnhancedTestBattleConfigView(CharacterSelectionView selectionView)
        {
            this._selectionView = selectionView;
            this.ViewOrderPriorty = 22;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            Open();
        }
        public override void OnMissionScreenFinalize()
        {
            Close();
            base.OnMissionScreenFinalize();
        }
        public override bool OnEscape()
        {
            base.OnEscape();
            this._dataSource.GoBack();
            return true;
        }

        public void Open()
        {
            this._dataSource = new EnhancedTestBattleConfigVM(_selectionView, (config) =>
                {
                    EnhancedTestBattleMissions.OpenEnhancedTestBattleMission(config);
                }, (param) => { this.Mission.EndMission(); });

            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty, "GauntletLayer");
            this._gauntletLayer.LoadMovie(nameof(EnhancedTestBattleConfigView), this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, TaleWorlds.Library.InputUsageMask.All);
            this.MissionScreen.AddLayer(this._gauntletLayer);
        }

        public void Close()
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
        }

    }
}