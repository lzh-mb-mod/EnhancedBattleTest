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
            var selectionView = new CharacterSelectionView();
            var orderView = new EnhancedBattleTestMissionOrderUIHandler();
            var missionViewList = new MissionView[]
            {
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionLeaveView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateOrderTroopPlacerView(mission),
                // missionViewList.Add(ViewCreator.CreateMissionScoreBoardUIHandler(mission, false));
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                ViewCreator.CreateMissionFlagMarkerUIHandler(),
                ViewCreator.CreateOptionsUIHandler(),
                // missionViewList.Add(ViewCreator.CreateMissionBoundaryCrossingView());
                // missionViewList.Add((MissionView) new MissionBoundaryWallView());
                //missionViewList.Add((MissionView) new SpectatorCameraView());
                new EnhancedBattleTestMissionView(mission),
                selectionView,
                orderView,
                new EnhancedBattleTestView(selectionView, orderView)
            };
            return missionViewList;
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

}