using System.Collections.Generic;
using SandBox.View.Missions;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.ViewModelCollection;

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
                new FlyCameraMissionView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateMissionLeaveView(),
                ViewCreator.CreateOptionsUIHandler(),
                new EnhancedMissionOrderUIHandler(),
                new SwitchTeamOrderTroopPlacer(),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                new EnhancedMusicBattleMissionView(false),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                new MissionFreeBattlePreloadView(config),
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