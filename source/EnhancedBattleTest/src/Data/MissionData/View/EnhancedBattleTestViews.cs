using EnhancedBattleTest.UI.MissionUI;
using SandBox.View.Missions;
using SandBox.ViewModelCollection;
using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.View.MissionViews.Sound;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;

namespace EnhancedBattleTest.Data.MissionData.View
{
    [ViewCreatorModule]
    public class EnhancedBattleTestViews
    {
        [ViewMethod("EnhancedBattleTestFieldBattle")]
        public static MissionView[] OpenBattleMission(Mission mission)
        {
            MissionView missionOrderUiHandler = ViewCreator.CreateMissionOrderUIHandler();
            ISiegeDeploymentView siegeDeploymentView = missionOrderUiHandler as ISiegeDeploymentView;

            return new List<MissionView>
            {
                ViewCreator.CreateMissionSingleplayerEscapeMenu(false),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionBattleScoreUIHandler(mission, new EnhancedBattleTestScoreBoardVM()),
                ViewCreator.CreateOptionsUIHandler(),
                ViewCreator.CreateMissionMainAgentEquipDropView(mission),
                missionOrderUiHandler,
                new OrderTroopPlacer(),
                new MissionSingleplayerViewHandler(),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
                ViewCreator.CreateMissionAgentLockVisualizerView(mission),
                new MusicBattleMissionView(false),
                new DeploymentMissionView(),
                new MissionDeploymentBoundaryMarker((IEntityFactory) new BorderFlagEntityFactory("swallowtail_banner")),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                new MissionBoundaryWallView(),
                ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
                new MissionFormationTargetSelectionHandler(),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                //new MissionPreloadView(),
                new EnhancedBattleTestPreloadView(),
                new MissionCampaignBattleSpectatorView(),
                ViewCreator.CreatePhotoModeView(),
                new MissionEntitySelectionUIHandler(new Action<GameEntity>(siegeDeploymentView.OnEntitySelection), new Action<GameEntity>(siegeDeploymentView.OnEntityHover)),
                ViewCreator.CreateMissionOrderOfBattleUIHandler(mission, (OrderOfBattleVM) new SPOrderOfBattleVM()),
                //new EnhancedBattleTestPreloadView()
            }.ToArray();
        }
        [ViewMethod("EnhancedBattleTestSiegeBattle")]
        public static MissionView[] OpenSiegeBattleMission(Mission mission)
        {
            MissionView missionOrderUiHandler = ViewCreator.CreateMissionOrderUIHandler();
            ISiegeDeploymentView siegeDeploymentView = missionOrderUiHandler as ISiegeDeploymentView;
            List<MissionView> missionViewList = new List<MissionView>
            {   new MissionCampaignView(),
                new MissionConversationCameraView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(false),
                ViewCreator.CreateOptionsUIHandler(),
                ViewCreator.CreateMissionMainAgentEquipDropView(mission),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionBattleScoreUIHandler(mission, new EnhancedBattleTestScoreBoardVM()),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
                ViewCreator.CreateMissionAgentLockVisualizerView(mission),
                missionOrderUiHandler,
                new OrderTroopPlacer(),
                new MissionSingleplayerViewHandler(),
                new MusicBattleMissionView(true),
                new DeploymentMissionView(),
                new MissionDeploymentBoundaryMarker((IEntityFactory) new BorderFlagEntityFactory("swallowtail_banner")),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                ViewCreator.CreatePhotoModeView(),
                ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
                new MissionFormationTargetSelectionHandler(),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                new MissionEntitySelectionUIHandler(siegeDeploymentView.OnEntitySelection,
                    siegeDeploymentView.OnEntityHover),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                new EnhancedBattleTestPreloadView(),
                new MissionCampaignBattleSpectatorView(),
                ViewCreator.CreateMissionOrderOfBattleUIHandler(mission, (OrderOfBattleVM) new SPOrderOfBattleVM()),
                ViewCreator.CreateMissionSiegeEngineMarkerView(mission)
            };
            return missionViewList.ToArray();
        }
    }
}
