
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    [ViewCreatorModule]
    public class EnhancedCustomBattleViews
    {
        [ViewMethod("EnhancedCustomBattleConfig")]
        public static MissionView[] OpenCustomBattleConfig(Mission mission)
        {
            var selectionView = new CharacterSelectionView(false);
            return new MissionView[]
            {
                selectionView,
                new EnhancedCustomBattleConfigView(selectionView),
            };
        }

        [ViewMethod("EnhancedCustomBattle")]
        public static MissionView[] OpenCustomBattleMission(Mission mission)
        {
            return new MissionView[]
            {
                ViewCreator.CreateOptionsUIHandler(),
                new MusicBattleMissionView(false), 
                //ViewCreator.CreateMissionBattleScoreUIHandler(mission, new CustomBattleScoreboardVM()),
                ViewCreator.CreatePlayerRoleSelectionUIHandler(mission),

                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionLeaveView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(),
                ViewCreator.CreateMissionOrderUIHandler(mission),
                ViewCreator.CreateOrderTroopPlacerView(mission),
                //ViewCreator.CreateMissionBattleScoreUIHandler(mission, new CustomBattleScoreboardVM()),
                // missionViewList.Add(ViewCreator.CreateMissionScoreBoardUIHandler(mission, false));
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                ViewCreator.CreateMissionFlagMarkerUIHandler(),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                new MissionBoundaryWallView(),
                new SpectatorCameraView(),
            };
        }
    }
}