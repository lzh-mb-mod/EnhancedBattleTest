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

namespace EnhancedBattleTest.Data.MissionData.View
{
    [ViewCreatorModule]
    public static class EnhancedBattleTestViews
    {
        [ViewMethod("EnhancedBattleTestFieldBattle")]
        public static MissionView[] OpenBattleMission(Mission mission)
        {
            MissionView missionOrderUiHandler = ViewCreator.CreateMissionOrderUIHandler();
            var siegeDeploymentView = missionOrderUiHandler as ISiegeDeploymentView;

            return new List<MissionView>
            {
                new MissionCampaignView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(false),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionBattleScoreUIHandler(
                    mission,
                    new EnhancedBattleTestScoreBoardVM()),
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
                new MissionDeploymentBoundaryMarker(
                    new BorderFlagEntityFactory("swallowtail_banner"),
                    2f),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                new MissionBoundaryWallView(),
                ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
                new MissionFormationTargetSelectionHandler(),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                new EnhancedBattleTestPreloadView(),
                new MissionCampaignBattleSpectatorView(),
                ViewCreator.CreatePhotoModeView(),
                new MissionEntitySelectionUIHandler(
                    new Action<GameEntity>(siegeDeploymentView.OnEntitySelection),
                    new Action<GameEntity>(siegeDeploymentView.OnEntityHover)),
                ViewCreator.CreateMissionOrderOfBattleUIHandler(
                    mission,
                    new SPOrderOfBattleVM())
            }.ToArray();
        }
    }
}
