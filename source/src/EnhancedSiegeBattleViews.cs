using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
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
            var missionViewList = new List<MissionView>
            {
                new MissionMenuView(config),
                new MissionCustomBattlePreloadView(),
                new PauseView(),
                ViewCreator.CreateOptionsUIHandler(),
                //ViewCreator.CreatePlayerRoleSelectionUIHandler(mission),
                //ViewCreator.CreateMissionBattleScoreUIHandler(mission, new CustomBattleScoreboardVM()),

                new MissionEntitySelectionUIHandler(),
                new MissionBoundaryMarker(new FlagFactory("flag_rope_cloth")),
                new SiegeDeploymentVisualizationMissionView(),
                new SiegeMissionView(),

                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionLeaveView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateMissionOrderUIHandler(mission),
                ViewCreator.CreateOrderTroopPlacerView(mission),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                new MissionBoundaryWallView(),
                new SpectatorCameraView(),
                new InitializeCameraPosView(config.FormationPosition, config.FormationDirection),
            };

            if (!config.noAgentLabel)
            {
                missionViewList.Add(ViewCreator.CreateMissionAgentLabelUIHandler(mission));
            }
            return missionViewList.ToArray();
        }
    }
}
