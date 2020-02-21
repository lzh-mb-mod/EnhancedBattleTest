
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;

namespace EnhancedBattleTest
{
    public static class EnhancedBattleTestMissoins
    {
        public static Mission OpenEnhancedBattleTestConfigMission()
        {
            return MissionState.OpenNew(
                "EnhancedBattleTestConfig",
                new MissionInitializerRecord("mp_skirmish_map_001a"),
                missionController => new MissionBehaviour[] {
                }
            , true, true, true);
        }

        public static Mission OpenEnhancedBattleTestMission(EnhancedBattleTestConfig config)
        {
            return MissionState.OpenNew(
                "EnhancedBattleTestBattle",
                new MissionInitializerRecord(config.SceneName),
                missionController => new MissionBehaviour[] {
                    new TestBattleMissionController(config),
                    new ControlTroopAfterPlayerDeadLogic(),
                    new SwitchTeamLogic(),
                    new SwitchFreeCameraLogic(), 
                    // new BattleTeam1MissionController(),
                    // new TaleWorlds.MountAndBlade.Source.Missions.SimpleMountedPlayerMissionController(),
                    new AgentBattleAILogic(),
                    new AgentVictoryLogic(),
                    new FieldBattleController(),
                    new MissionOptionsComponent(),
                    new MakeGruntVoiceLogic(),
                    // new MissionBoundaryPlacer(),
                }
            );
        }
    }
}