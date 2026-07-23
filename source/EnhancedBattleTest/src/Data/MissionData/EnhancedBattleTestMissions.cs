using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data.MissionData.Logic;
using SandBox.Missions.MissionLogics;
using SandBox.Missions.MissionLogics.Towns;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace EnhancedBattleTest.Data.MissionData
{
    [MissionManager]
    public static class EnhancedBattleTestMissions
    {
        public static Mission OpenMission(BattleConfig config, string mapName)
        {
            if (config.BattleTypeConfig.BattleType == BattleType.Siege)
                return null;

            try
            {
                EnhancedBattleTestPartyController.BattleContext context =
                    EnhancedBattleTestPartyController.Create(config);
                return OpenFieldMission(config, mapName, context);
            }
            catch
            {
                EnhancedBattleTestPartyController.Cleanup();
                throw;
            }
        }

        [MissionMethod]
        private static Mission OpenFieldMission(
            BattleConfig config,
            string scene,
            EnhancedBattleTestPartyController.BattleContext context)
        {
            BattleSideEnum playerSide = config.BattleTypeConfig.PlayerSide;
            bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
            bool hasPlayer = config.PlayerTeamConfig.HasGeneral
                             && config.PlayerTeamConfig.Generals.Troops.Count > 0;
            bool isPlayerGeneral = hasPlayer
                                   && config.BattleTypeConfig.PlayerType == PlayerType.Commander;
            bool isPlayerSergeant = hasPlayer
                                    && config.BattleTypeConfig.PlayerType == PlayerType.Sergeant;

            PartyBase playerParty = context.PlayerParty.Party;
            PartyBase enemyParty = context.EnemyParty.Party;
            PartyBase defenderParty = isPlayerAttacker ? enemyParty : playerParty;
            PartyBase attackerParty = isPlayerAttacker ? playerParty : enemyParty;

            List<string> playerHeroIds = context.PlayerPriorityCharacterIds.ToList();
            TextObject playerGeneralName = GetGeneralName(config.PlayerTeamConfig);
            TextObject enemyGeneralName = GetGeneralName(config.EnemyTeamConfig);
            AtmosphereInfo atmosphereInfo = AtmosphereModel.CreateAtmosphereInfoForMission(
                config.MapConfig.Season,
                config.MapConfig.TimeOfDay);

            Mission mission = MissionState.OpenNew(
                "Battle",
                new MissionInitializerRecord(scene)
                {
                    DoNotUseLoadingScreen = false,
                    PlayingInCampaignMode = true,
                    AtmosphereOnCampaign = atmosphereInfo,
                    TimeOfDay = config.MapConfig.TimeOfDay
                },
                missionController => new MissionBehavior[]
                {
                    new EnhancedBattleTestCleanupLogic(),
                    new EnhancedBattleTestPlayerAgentLogic(context.PlayerCharacter),
                    new CommanderLogic(config),
                    new MissionAgentSpawnLogic(
                        context.TroopSuppliers,
                        playerSide,
                        Mission.BattleSizeType.Battle),
                    new BattlePowerCalculationLogic(),
                    new BattleSpawnLogic("battle_set"),
                    new EnhancedBattleTestMissionSpawnHandler(defenderParty, attackerParty),
                    new CampaignMissionComponent(),
                    new BattleAgentLogic(),
                    new MountAgentLogic(),
                    new BannerBearerLogic(),
                    new MissionOptionsComponent(),
                    new BattleEndLogic(),
                    new BattleReinforcementsSpawnController(),
                    new MissionCombatantsLogic(
                        context.MapEvent.InvolvedParties,
                        playerParty,
                        defenderParty,
                        attackerParty,
                        Mission.MissionTeamAITypeEnum.FieldBattle,
                        isPlayerSergeant),
                    new BattleObserverMissionLogic(),
                    new AgentHumanAILogic(),
                    new AgentVictoryLogic(),
                    new BattleSurgeonLogic(),
                    new MissionAgentPanicHandler(),
                    new BattleMissionAgentInteractionLogic(),
                    new AgentMoraleInteractionLogic(),
                    new AssignPlayerRoleInTeamMissionController(
                        isPlayerGeneral,
                        isPlayerSergeant,
                        false,
                        playerHeroIds),
                    new SandboxGeneralsAndCaptainsAssignmentLogic(
                        isPlayerAttacker ? playerGeneralName : enemyGeneralName,
                        isPlayerAttacker ? enemyGeneralName : playerGeneralName),
                    new EquipmentControllerLeaveLogic(),
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(),
                    new MissionBoundaryCrossingHandler(),
                    new HighlightsController(),
                    new BattleHighlightsController(),
                    new DeploymentMissionController(isPlayerAttacker),
                    new BattleDeploymentHandler(isPlayerAttacker)
                });
            return mission;
        }

        private static TextObject GetGeneralName(TeamConfig config)
        {
            return config.HasGeneral
                ? config.Generals.Troops
                    .Select(troop => troop.Character.CharacterObject)
                    .FirstOrDefault(character => character != null)
                    ?.Name
                : null;
        }
    }
}
