using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Missions.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace EnhancedBattleTest
{
    [ViewCreatorModule]
    public class EnhancedCustomBattleViews
    {
        [ViewMethod("EnhancedCustomBattleConfig")]
        public static MissionView[] OpenCustomBattleConfig(Mission mission)
        {
            var selectionView = new CharacterSelectionView(true);
            return new MissionView[]
            {
                selectionView,
                new EnhancedCustomBattleConfigView(selectionView),
                new MissionMenuView(EnhancedCustomBattleConfig.Get()),
            };
        }

        [ViewMethod("EnhancedCustomBattle")]
        public static MissionView[] OpenCustomBattleMission(Mission mission)
        {
            var config = EnhancedCustomBattleConfig.Get();
            var missionViewList = new List<MissionView>
            {
                new MissionMenuView(config),
                new FlyCameraMissionView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                //ViewCreator.CreatePlayerRoleSelectionUIHandler(mission),
                ViewCreator.CreateMissionBattleScoreUIHandler(mission, new CustomBattleScoreboardVM()),
                ViewCreator.CreateOptionsUIHandler(),
                new EnhancedMissionOrderUIHandler(),
                new SwitchTeamOrderTroopPlacer(),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                new MusicBattleMissionView(false),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                new MissionBoundaryWallView(),
                ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                // ViewCreator.CreateMissionBattleScoreUIHandler(mission, new CustomBattleScoreboardVM()),
                // missionViewList.Add(ViewCreator.CreateMissionScoreBoardUIHandler(mission, false));
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                new MissionCustomBattlePreloadView(),
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