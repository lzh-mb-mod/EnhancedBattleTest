using System.Collections.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TL = TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Modbed
{
    [ViewCreatorModule]
    public class EnhancedBattleTestMissionViews
    {
        [ViewMethod("EnhancedBattleTest")]
        public static MissionView[] OpenTestMission(Mission mission)
        {
            List<MissionView> missionViewList = new List<MissionView>();
            missionViewList.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission));
            // missionViewList.Add(ViewCreator.CreateMissionAgentLabelUIHandler(mission));
            missionViewList.Add(ViewCreator.CreateOrderTroopPlacerView(mission));
            // missionViewList.Add(ViewCreator.CreateMissionScoreBoardUIHandler(mission, false));
            missionViewList.Add(ViewCreator.CreateMissionKillNotificationUIHandler());
            missionViewList.Add(new MissionItemContourControllerView());
            missionViewList.Add(new MissionAgentContourControllerView());
            missionViewList.Add(ViewCreator.CreateMissionFlagMarkerUIHandler());
            // missionViewList.Add(ViewCreator.CreateOptionsUIHandler());
            // missionViewList.Add(ViewCreator.CreateMissionBoundaryCrossingView());
            // missionViewList.Add((MissionView) new MissionBoundaryWallView());
            // missionViewList.Add((MissionView) new SpectatorCameraView());
            missionViewList.Add(ViewCreator.CreateMissionOrderUIHandler());
            missionViewList.Add(new EnhancedBattleTestMissionView(mission));
            var selectionView = new CharacterSelectionView();
            missionViewList.Add(selectionView);
            missionViewList.Add(new EnhancedBattleTestSelectMissionView(selectionView));
            return missionViewList.ToArray();
        }
    }

    public class EnhancedBattleTestMissionView : MissionView
    {
        private Mission _mission;
        public EnhancedBattleTestMissionView(Mission mission)
            : base()
        {
            this._mission = mission;
        }
        public override void OnMissionScreenActivate()
        {
            var battleTestMissionController = this.Mission.GetMissionBehaviour<EnhancedBattleTestMissionController>();
            battleTestMissionController.freeCameraInitialPos = pos =>
            {
                this.MissionScreen.CombatCamera.Position = pos;
            };

        }
    }

    public class EnhancedBattleTestSelectMissionView : MissionView
    {
        private GauntletLayer _gauntletLayer;
        private EnhancedBattleTestVM _dataSource;
        private EnhancedBattleTestMissionController _missionController;
        private CharacterSelectionView _selectionView;
        private bool _isOpen;
        private bool _toOpen;

        public EnhancedBattleTestSelectMissionView(CharacterSelectionView selectionView)
        {
            this._selectionView = selectionView;
            this.ViewOrderPriorty = 22;
            this._isOpen = this._toOpen = false;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._missionController = this.Mission.GetMissionBehaviour<EnhancedBattleTestMissionController>();
            this._toOpen = true;
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
                this._missionController.ShouldStart(param);
                this.OnClose();
            }, (param) =>
            {
                this._missionController.ShouldExit();
                this.Mission.EndMission();
                //this.OnClose();
            });

            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty, "GauntletLayer");
            this._gauntletLayer.LoadMovie("EnhancedBattleTestSelectMissionView", this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, TL.InputUsageMask.All);
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