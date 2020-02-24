
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace EnhancedBattleTest
{
    public static class EnhancedTestBattleMissions
    {
        public static Mission OpenEnhancedTestBattleConfigMission()
        {
            return MissionState.OpenNew(
                "EnhancedTestBattleConfig",
                new MissionInitializerRecord("mp_skirmish_map_001a"),
                missionController => new MissionBehaviour[] {
                }
            , true, true, true);
        }

        public static Mission OpenEnhancedTestBattleMission(EnhancedTestBattleConfig config)
        {
            return MissionState.OpenNew(
                "EnhancedBattleTestBattle",
                new MissionInitializerRecord(config.SceneName),
                missionController => new MissionBehaviour[] {
                    new EnhancedTestBattleMissionController(config),
                    new ControlTroopAfterPlayerDeadLogic(),
                    new SwitchTeamLogic(),
                    new SwitchFreeCameraLogic(),
                    new MakeGruntVoiceLogic(),
                    // new BattleTeam1MissionController(),
                    // new TaleWorlds.MountAndBlade.Source.Missions.SimpleMountedPlayerMissionController(),
                    new AgentBattleAILogic(),
                    new AgentVictoryLogic(),
                    new FieldBattleController(),
                    new MissionOptionsComponent(),
                    new MissionSimulationHandler(),
                    new BattleMissionAgentInteractionLogic(),
                    new AgentFadeOutLogic(),
                    new AgentMoraleInteractionLogic(),
                    new HighlightsController(),
                    new BattleHighlightsController(),
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(),
                    new MissionBoundaryCrossingHandler(),
                }
            );
        }
    }
}