using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Missions.Singleplayer;

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
                new MissionCustomBattlePreloadView(),
                ViewCreator.CreateOptionsUIHandler(),
                new MusicBattleMissionView(config.SceneName.Contains("siege")),
                ViewCreator.CreatePlayerRoleSelectionUIHandler(mission),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionLeaveView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateMissionOrderUIHandler(mission),
                ViewCreator.CreateOrderTroopPlacerView(mission),
                // ViewCreator.CreateMissionBattleScoreUIHandler(mission, new CustomBattleScoreboardVM()),
                // missionViewList.Add(ViewCreator.CreateMissionScoreBoardUIHandler(mission, false));
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                new MissionBoundaryWallView(),
                new SpectatorCameraView(),
            };

            if (!config.noAgentLabel)
            {
                missionViewList.Add(ViewCreator.CreateMissionAgentLabelUIHandler(mission));
            }

            return missionViewList.ToArray();
        }
    }
}