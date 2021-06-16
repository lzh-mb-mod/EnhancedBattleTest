using System.Collections.Generic;
using EnhancedBattleTest.UI.MissionUI;
using SandBox.View.Missions;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest.Data.MissionData.View
{
    [ViewCreatorModule]
    public class EnhancedBattleTestViews
    {
        [ViewMethod("EnhancedBattleTestFieldBattle")]
        public static MissionView[] OpenBattleMission(Mission mission)
        {
            return new List<MissionView>
            {
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionBattleScoreUIHandler(mission, new EnhancedBattleTestScoreBoardVM()),
                ViewCreator.CreateOptionsUIHandler(),
                ViewCreator.CreateMissionOrderUIHandler(),
                new OrderTroopPlacer(),
                new MissionSingleplayerUIHandler(),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
                new MusicBattleMissionView(false),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                new MissionBoundaryWallView(),
                ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                ViewCreator.CreatePlayerRoleSelectionUIHandler(),
                ViewCreator.CreatePhotoModeView(),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                new EnhancedBattleTestPreloadView()
            }.ToArray();
        }
        [ViewMethod("EnhancedBattleTestSiegeBattle")]
        public static MissionView[] OpenSiegeBattleMission(Mission mission)
        {
            MissionView missionOrderUiHandler = ViewCreator.CreateMissionOrderUIHandler();
            ISiegeDeploymentView siegeDeploymentView = missionOrderUiHandler as ISiegeDeploymentView;
            List<MissionView> missionViewList = new List<MissionView>
            {
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionBattleScoreUIHandler(mission, new EnhancedBattleTestScoreBoardVM()),
                ViewCreator.CreateOptionsUIHandler(),
                missionOrderUiHandler,
                new OrderTroopPlacer(),
                new MissionSingleplayerUIHandler(),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
                new MusicBattleMissionView(true),
                new SiegeMissionView(),
                new MissionEntitySelectionUIHandler(siegeDeploymentView.OnEntitySelection,
                    siegeDeploymentView.OnEntityHover),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                new MissionBoundaryMarker(new FlagFactory("swallowtail_banner")),
                ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                new SiegeDeploymentVisualizationMissionView(),
                ViewCreator.CreatePlayerRoleSelectionUIHandler(),
                ViewCreator.CreatePhotoModeView(),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                new EnhancedBattleTestPreloadView()
            };
            return missionViewList.ToArray();
        }
    }
}
