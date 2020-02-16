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
        [ViewMethod("EnhancedBattleTestSelection")]
        public static MissionView[] OpenInitialMission(Mission mission)
        {
            var selectionView = new CharacterSelectionView();
            return new MissionView[]
            {
                selectionView,
                new EnhancedBattleTestView(selectionView)
            };
        }
        [ViewMethod("EnhancedBattleTestBattle")]
        public static MissionView[] OpenTestMission(Mission mission)
        {
            var missionViewList = new MissionView[]
            {
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionLeaveView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateMissionOrderUIHandler(mission),
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
            foreach (var missionLogic in this._mission.MissionLogics)
            {
                if (missionLogic is EnhancedBattleTestMissionController missionController)
                {
                    this.MissionScreen.CombatCamera.LookAt(missionController.initialFreeCameraPos, missionController.initialFreeCameraTarget, TL.Vec3.Up);
                    break;
                }
            }
        }
    }

}