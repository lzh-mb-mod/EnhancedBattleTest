using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace EnhancedBattleTest
{
    [ViewCreatorModule]
    public class EnhancedBattleTestViews
    {
        [ViewMethod("EnhancedBattleTestConfig")]
        public static MissionView[] OpenInitialMission(Mission mission)
        {
            var selectionView = new CharacterSelectionView();
            return new MissionView[]
            {
                selectionView,
                new EnhancedBattleTestConfigView(selectionView),
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
                //ViewCreator.CreateMissionFlagMarkerUIHandler(),
                ViewCreator.CreateOptionsUIHandler(),
                // missionViewList.Add(ViewCreator.CreateMissionBoundaryCrossingView());
                // missionViewList.Add((MissionView) new MissionBoundaryWallView());
                new SpectatorCameraView(),
                new EnhancedBattleTestView(mission),
            };
            return missionViewList;
        }
    }
}