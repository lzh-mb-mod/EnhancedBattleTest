using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    [ViewCreatorModule]
    public class EnhancedTestBattleViews
    {
        [ViewMethod("EnhancedTestBattleConfig")]
        public static MissionView[] OpenInitialMission(Mission mission)
        {
            var selectionView = new CharacterSelectionView(true);
            return new MissionView[]
            {
                selectionView,
                new EnhancedTestBattleConfigView(selectionView),
            };
        }

        [ViewMethod("EnhancedBattleTestBattle")]
        public static MissionView[] OpenTestMission(Mission mission)
        {
            var missionViewList = new List<MissionView>
            {
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionLeaveView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateMissionOrderUIHandler(mission),
                ViewCreator.CreateOrderTroopPlacerView(mission),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                ViewCreator.CreateOptionsUIHandler(),
                new SpectatorCameraView(),
                new EnhancedTestBattleView(mission),
            };
            if (EnhancedTestBattleConfig.Get().hasBoundary)
            {
                missionViewList.Add(ViewCreator.CreateMissionBoundaryCrossingView());
                missionViewList.Add(new MissionBoundaryWallView());
            }
            return missionViewList.ToArray();
        }
    }
}