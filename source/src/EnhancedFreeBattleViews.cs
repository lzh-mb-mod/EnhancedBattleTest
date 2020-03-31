using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    [ViewCreatorModule]
    public class EnhancedFreeBattleViews
    {
        [ViewMethod("EnhancedFreeBattleConfig")]
        public static MissionView[] OpenInitialMission(Mission mission)
        {
            var selectionView = new CharacterSelectionView(true);
            return new MissionView[]
            {
                selectionView,
                new EnhancedFreeBattleConfigView(selectionView),
                new MissionMenuView(EnhancedFreeBattleConfig.Get()),
            };
        }

        [ViewMethod("EnhancedFreeBattle")]
        public static MissionView[] OpenTestMission(Mission mission)
        {
            var config = EnhancedFreeBattleConfig.Get();
            var missionViewList = new List<MissionView>
            {
                new MissionMenuView(config),
                new MissionFreeBattlePreloadView(config),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionLeaveView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                new SwitchTeamMissionOrderUIHandler(),
                new SwitchTeamOrderTroopPlacer(),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                ViewCreator.CreateOptionsUIHandler(),
                new SpectatorCameraView(),
                new InitializeCameraPosView(
                    config.isPlayerAttacker
                        ? config.FormationPosition
                        : config.FormationPosition + config.FormationDirection * config.Distance,
                    config.isPlayerAttacker ? config.FormationDirection : -config.FormationDirection),
            };
            if (config.hasBoundary)
            {
                missionViewList.Add(ViewCreator.CreateMissionBoundaryCrossingView());
                missionViewList.Add(new MissionBoundaryWallView());
            }

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