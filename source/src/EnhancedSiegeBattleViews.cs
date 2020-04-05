using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Missions.SiegeWeapon;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace EnhancedBattleTest
{
    [ViewCreatorModule]
    public class EnhancedSiegeBattleViews
    {
        [ViewMethod("EnhancedSiegeBattleConfig")]
        public static MissionView[] OpenConfigMission(Mission mission)
        {
            var selectionView = new CharacterSelectionView(true);
            return new MissionView[]
            {
                selectionView,
                new EnhancedSiegeBattleConfigView(selectionView),
                new MissionMenuView(EnhancedSiegeBattleConfig.Get()),
            };
        }

        [ViewMethod("EnhancedSiegeBattle")]
        public static MissionView[] OpenSiegeMission(Mission mission)
        {
            var config = EnhancedSiegeBattleConfig.Get();
            MissionView missionOrderUiHandler = new SwitchTeamMissionOrderUIHandler();
            ISiegeDeploymentView siegeDeploymentView = missionOrderUiHandler as ISiegeDeploymentView;
            var missionViewList = new List<MissionView>
            {
                new MissionMenuView(config),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateMissionBattleScoreUIHandler(mission, new CustomBattleScoreboardVM()),
                ViewCreator.CreateOptionsUIHandler(),
                missionOrderUiHandler,
                new SwitchTeamOrderTroopPlacer(),
                //ViewCreator.CreatePlayerRoleSelectionUIHandler(mission),

                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                new MusicBattleMissionView(false),
                new SiegeMissionView(),
                new MissionEntitySelectionUIHandler(new Action<GameEntity>(siegeDeploymentView.OnEntitySelection), new Action<GameEntity>(siegeDeploymentView.OnEntityHover)),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                new MissionBoundaryWallView(),
                new MissionBoundaryMarker(new FlagFactory("swallowtail_banner"), 2f),
                ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                new SiegeDeploymentVisualizationMissionView(),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                new MissionCustomBattlePreloadView(),
                new InitializeCameraPosView(
                    config.isPlayerAttacker
                        ? config.FormationPosition
                        : config.FormationPosition + config.FormationDirection * config.Distance,
                    config.isPlayerAttacker ? config.FormationDirection : -config.FormationDirection),
            };

            if (!config.noAgentLabel)
            {
                missionViewList.Add(ViewCreator.CreateMissionAgentLabelUIHandler(mission));
            }

            if (!config.noKillNotification)
            {
                missionViewList.Add(ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler());
            }

            return missionViewList.ToArray();
        }
    }
}
