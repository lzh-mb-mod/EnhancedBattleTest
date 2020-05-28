using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace EnhancedBattleTest
{
    [ViewCreatorModule]
    public class EnhancedBattleTestViews
    {
        [ViewMethod("MPBattle")]
        public static MissionView[] OpenMPBattleMission(Mission mission)
        {
            return new List<MissionView>
            {
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionBattleScoreUIHandler(mission, new CustomBattleScoreboardVM()),
                ViewCreator.CreateOptionsUIHandler(),
                ViewCreator.CreateMissionOrderUIHandler(),
                new OrderTroopPlacer(),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                new MusicBattleMissionView(false),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                new MissionBoundaryWallView(),
                ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                new MissionAgentContourControllerView(),
                new EnhancedBattleTestPreloadView()
            }.ToArray();
        }
        [ViewMethod("MPSiegeBattle")]
        public static MissionView[] OpenMPSiegeBattleMission(Mission mission)
        {
            MissionView missionOrderUiHandler = ViewCreator.CreateMissionOrderUIHandler();
            ISiegeDeploymentView siegeDeploymentView = missionOrderUiHandler as ISiegeDeploymentView;
            List<MissionView> missionViewList = new List<MissionView>
            {
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionBattleScoreUIHandler(mission, new CustomBattleScoreboardVM()),
                ViewCreator.CreateOptionsUIHandler(),
                missionOrderUiHandler,
                new OrderTroopPlacer(),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
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
                new MissionAgentContourControllerView(),
                new EnhancedBattleTestPreloadView()
            };
            return missionViewList.ToArray();
        }
    }
}
