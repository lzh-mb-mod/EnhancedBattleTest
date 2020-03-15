using System.Collections.Generic;
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
                new MissionInitializerRecord("scn_character_creation_scene"),
                missionController => new MissionBehaviour[] {
                },
                true, true, true);
        }

        public static Mission OpenEnhancedTestBattleMission(EnhancedTestBattleConfig config)
        {
            return MissionState.OpenNew(
                "EnhancedBattleTestBattle",
                new MissionInitializerRecord(config.SceneName),
                missionController =>
                {
                    var behaviors = new List<MissionBehaviour>
                    {
                        new EnhancedTestBattleMissionController(config),
                        new CommanderLogic(),
                        new ControlTroopAfterPlayerDeadLogic(),
                        new TrainingLogic(EnhancedTestBattleConfig.Get()),
                        new SwitchTeamLogic(),
                        new SwitchFreeCameraLogic(),
                        new MakeGruntVoiceLogic(),
                        new TeleportPlayerLogic(),
                        new AgentBattleAILogic(),
                        new AgentVictoryLogic(),
                        new MissionOptionsComponent(),
                        new BattleMissionAgentInteractionLogic(),
                        new AgentFadeOutLogic(),
                        new AgentMoraleInteractionLogic(),
                        new HighlightsController(),
                        new BattleHighlightsController(),
                    };
                    if (config.hasBoundary)
                    {
                        behaviors.Add(new MissionBoundaryPlacer());
                        behaviors.Add(new MissionHardBorderPlacer());
                        behaviors.Add(new MissionBoundaryCrossingHandler());
                    }
                    return behaviors;
                });
        }
    }
}