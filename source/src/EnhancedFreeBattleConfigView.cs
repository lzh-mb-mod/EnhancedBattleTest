using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    public class EnhancedFreeBattleConfigView : MissionView
    {
        private GauntletLayer _gauntletLayer;
        private EnhancedFreeBattleConfigVM _dataSource;
        private CharacterSelectionView _selectionView;
        private MissionMenuView _missionMenuView;

        public EnhancedFreeBattleConfigView(CharacterSelectionView selectionView)
        {
            this._selectionView = selectionView;
            this.ViewOrderPriorty = 22;
        }

        public override void OnMissionScreenInitialize()
        {
            _missionMenuView = Mission.GetMissionBehaviour<MissionMenuView>();
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
            this._dataSource = new EnhancedFreeBattleConfigVM(_selectionView, _missionMenuView, (config) =>
            {
                this.Mission.EndMission();
                GameStateManager.Current.PopStateRPC(0);
                EnhancedBattleTestMissions.OpenFreeBattleMission(config);
            }, (param) =>
            {
                TopState.status = TopStateStatus.exit;
                this.Mission.EndMission();
            });

            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty, "GauntletLayer");
            this._gauntletLayer.LoadMovie(nameof(EnhancedFreeBattleConfigView), this._dataSource);
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