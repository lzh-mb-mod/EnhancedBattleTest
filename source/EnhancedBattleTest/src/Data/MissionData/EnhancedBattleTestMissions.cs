using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data.MissionData.Logic;
using SandBox.Missions.MissionLogics;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
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
        public static Mission OpenMission(
            BattleConfig config,
            string mapName,
            TerrainType terrainType)
        {
            try
            {
                EnhancedBattleTestPartyController.BattleContext context =
                    EnhancedBattleTestPartyController.Create(config);
                return config.BattleTypeConfig.BattleType == BattleType.Siege
                    ? OpenSiegeMission(config, mapName, terrainType, context)
                    : OpenFieldMission(config, mapName, terrainType, context);
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
            TerrainType terrainType,
            EnhancedBattleTestPartyController.BattleContext context)
        {
            BattleSideEnum playerSide = config.BattleTypeConfig.PlayerSide;
            bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
            bool isPlayerGeneral =
                context.PlayerCharacter != null
                && config.BattleTypeConfig.PlayerType == PlayerType.Commander;
            bool isPlayerSergeant =
                context.PlayerCharacter != null
                && config.BattleTypeConfig.PlayerType == PlayerType.Sergeant;

            PartyBase playerParty = context.PlayerParty.Party;
            PartyBase enemyParty = context.EnemyParty.Party;
            PartyBase defenderParty = isPlayerAttacker ? enemyParty : playerParty;
            PartyBase attackerParty = isPlayerAttacker ? playerParty : enemyParty;

            List<string> playerHeroIds = context.PlayerPriorityCharacterIds.ToList();
            TextObject playerGeneralName = GetGeneralName(config.PlayerTeamConfig.PrimaryParty);
            TextObject enemyGeneralName = GetGeneralName(config.EnemyTeamConfig.PrimaryParty);
            AtmosphereInfo atmosphereInfo = AtmosphereModel.CreateAtmosphereInfoForMission(
                config.MapConfig.DayOfYear,
                config.MapConfig.TimeOfDay,
                config.MapConfig.Weather,
                config.MapConfig.FogDensity);
            bool isDayInWinter =
                AtmosphereModel.GetSeasonIndex(config.MapConfig.DayOfYear)
                == (int)CampaignTime.Seasons.Winter;
            bool usesWinterWeather =
                atmosphereInfo.TimeInfo.Season
                == (int)CampaignTime.Seasons.Winter;
            if (isDayInWinter || usesWinterWeather)
                terrainType = TerrainType.Snow;

            var initializer = new MissionInitializerRecord(scene)
            {
                DoNotUseLoadingScreen = false,
                PlayingInCampaignMode = true,
                AtmosphereOnCampaign = atmosphereInfo,
                TerrainType = (int)terrainType,
                DecalAtlasGroup = config.BattleTypeConfig.BattleType
                    == BattleType.Village
                    ? (int)DecalAtlasGroup.Town
                    : (int)DecalAtlasGroup.Battle,
                RandomTerrainSeed = MBRandom.RandomInt(10000)
            };
            var spawnLogic = new MissionAgentSpawnLogic(
                context.TroopSuppliers,
                playerSide,
                Mission.BattleSizeType.Battle);
            bool hasPlayerCharacter = context.PlayerCharacter != null;
            Mission mission = MissionState.OpenNew(
                "Battle",
                initializer,
                missionController => new MissionBehavior[]
                {
                    new EnhancedBattleTestCleanupLogic(),
                    new EnhancedBattleTestEnvironmentLogic(
                        config.MapConfig.TimeOfDay,
                        atmosphereInfo.RainInfo.Density,
                        usesWinterWeather
                            ? atmosphereInfo.SnowInfo.Density
                            : 0f,
                        atmosphereInfo.FogInfo.Density,
                        atmosphereInfo.FogInfo.Color,
                        atmosphereInfo.FogInfo.Falloff),
                    new EnhancedBattleTestPlayerAgentLogic(
                        context.PlayerCharacter,
                        context.PlayerParty.Party),
                    spawnLogic,
                    new BattlePowerCalculationLogic(),
                    new BattleSpawnLogic("battle_set"),
                    new SandBoxBattleMissionSpawnHandler(),
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
                        false),
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
                        config.PlayerTeamConfig.PrimaryParty.IsInArmy,
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
                    hasPlayerCharacter
                        ? new BattleDeploymentMissionController(isPlayerAttacker)
                        : null,
                    hasPlayerCharacter
                        ? new BattleDeploymentHandler(isPlayerAttacker)
                        : null,
                    !hasPlayerCharacter
                        ? new EnhancedBattleTestNoPlayerDeploymentLogic(
                            spawnLogic)
                        : null
                });
            if (hasPlayerCharacter)
                mission.SetPlayerCanTakeControlOfAnotherAgentWhenDead();
            return mission;
        }

        [MissionMethod]
        private static Mission OpenSiegeMission(
            BattleConfig config,
            string scene,
            TerrainType terrainType,
            EnhancedBattleTestPartyController.BattleContext context)
        {
            BattleSideEnum playerSide = config.BattleTypeConfig.PlayerSide;
            bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
            bool isPlayerGeneral =
                context.PlayerCharacter != null
                && config.BattleTypeConfig.PlayerType == PlayerType.Commander;
            bool isPlayerSergeant =
                context.PlayerCharacter != null
                && config.BattleTypeConfig.PlayerType == PlayerType.Sergeant;

            PartyBase playerParty = context.PlayerParty.Party;
            PartyBase enemyParty = context.EnemyParty.Party;
            PartyBase defenderParty = isPlayerAttacker ? enemyParty : playerParty;
            PartyBase attackerParty = isPlayerAttacker ? playerParty : enemyParty;
            List<string> playerHeroIds =
                context.PlayerPriorityCharacterIds.ToList();
            TextObject playerGeneralName =
                GetGeneralName(config.PlayerTeamConfig.PrimaryParty);
            TextObject enemyGeneralName =
                GetGeneralName(config.EnemyTeamConfig.PrimaryParty);

            AtmosphereInfo atmosphereInfo =
                AtmosphereModel.CreateAtmosphereInfoForMission(
                    config.MapConfig.DayOfYear,
                    config.MapConfig.TimeOfDay,
                    config.MapConfig.Weather,
                    config.MapConfig.FogDensity);
            bool usesWinterWeather =
                atmosphereInfo.TimeInfo.Season
                == (int)CampaignTime.Seasons.Winter;
            if (usesWinterWeather)
                terrainType = TerrainType.Snow;

            List<MissionSiegeWeapon> attackerSiegeWeapons =
                CreateSiegeWeapons(
                    config.SiegeMachineConfig.AttackerMeleeMachines.Concat(
                        config.SiegeMachineConfig.AttackerRangedMachines));
            List<MissionSiegeWeapon> defenderSiegeWeapons =
                CreateSiegeWeapons(config.SiegeMachineConfig.DefenderMachines);
            bool hasAnySiegeTower = attackerSiegeWeapons.Any(
                weapon => weapon.Type == DefaultSiegeEngineTypes.SiegeTower);
            float[] wallHitPointPercentages =
                CreateWallHitPointPercentages(
                    config.MapConfig.BreachedWallCount);

            Mission mission = MissionState.OpenNew(
                "SiegeMissionWithDeployment",
                new MissionInitializerRecord(scene)
                {
                    DoNotUseLoadingScreen = false,
                    PlayingInCampaignMode = true,
                    AtmosphereOnCampaign = atmosphereInfo,
                    SceneLevels = GetSiegeSceneLevels(
                        config.MapConfig.SceneLevel),
                    TerrainType = (int)terrainType,
                    DecalAtlasGroup = (int)DecalAtlasGroup.Town
                },
                missionController =>
                {
                    var behaviors = new List<MissionBehavior>
                    {
                        new EnhancedBattleTestCleanupLogic(),
                        new EnhancedBattleTestEnvironmentLogic(
                            config.MapConfig.TimeOfDay,
                            atmosphereInfo.RainInfo.Density,
                            usesWinterWeather
                                ? atmosphereInfo.SnowInfo.Density
                                : 0f,
                            atmosphereInfo.FogInfo.Density,
                            atmosphereInfo.FogInfo.Color,
                            atmosphereInfo.FogInfo.Falloff),
                        new EnhancedBattleTestPlayerAgentLogic(
                            context.PlayerCharacter,
                            context.PlayerParty.Party),
                        new BattleSpawnLogic("battle_set"),
                        new MissionOptionsComponent(),
                        new CampaignMissionComponent(),
                        new BattleEndLogic(),
                        new BattleReinforcementsSpawnController(),
                        new MissionCombatantsLogic(
                            context.MapEvent.InvolvedParties,
                            playerParty,
                            defenderParty,
                            attackerParty,
                            Mission.MissionTeamAITypeEnum.Siege,
                            false),
                        new SiegeMissionPreparationHandler(
                            false,
                            false,
                            wallHitPointPercentages,
                            hasAnySiegeTower),
                        new MissionAgentSpawnLogic(
                            context.TroopSuppliers,
                            playerSide,
                            Mission.BattleSizeType.Siege),
                        new BattlePowerCalculationLogic(),
                        new SandBoxSiegeMissionSpawnHandler(),
                        new BattleObserverMissionLogic(),
                        new BattleAgentLogic(),
                        new BattleSurgeonLogic(),
                        new MountAgentLogic(),
                        new BannerBearerLogic(),
                        new AgentHumanAILogic(),
                        new AmmoSupplyLogic(
                            new List<BattleSideEnum>
                            {
                                BattleSideEnum.Defender
                            }),
                        new AgentVictoryLogic(),
                        new AssignPlayerRoleInTeamMissionController(
                            isPlayerGeneral,
                            isPlayerSergeant,
                            config.PlayerTeamConfig.PrimaryParty.IsInArmy,
                            playerHeroIds),
                        new SandboxGeneralsAndCaptainsAssignmentLogic(
                            isPlayerAttacker
                                ? playerGeneralName
                                : enemyGeneralName,
                            isPlayerAttacker
                                ? enemyGeneralName
                                : playerGeneralName,
                            null,
                            null,
                            false),
                        new MissionAgentPanicHandler(),
                        new MissionBoundaryPlacer(),
                        new MissionBoundaryCrossingHandler(),
                        new AgentMoraleInteractionLogic(),
                        new HighlightsController(),
                        new BattleHighlightsController(),
                        new EquipmentControllerLeaveLogic(),
                        new MissionSiegeEnginesLogic(
                            defenderSiegeWeapons,
                            attackerSiegeWeapons),
                        new SiegeDeploymentHandler(isPlayerAttacker),
                        new SiegeDeploymentMissionController(isPlayerAttacker)
                    };
                    return behaviors;
                });
            if (context.PlayerCharacter != null)
                mission.SetPlayerCanTakeControlOfAnotherAgentWhenDead();
            return mission;
        }

        private static List<MissionSiegeWeapon> CreateSiegeWeapons(
            IEnumerable<string> machineIds)
        {
            return (machineIds ?? Enumerable.Empty<string>())
                .Select(Utility.GetSiegeEngineType)
                .Where(type => type != null)
                .Select(MissionSiegeWeapon.CreateDefaultWeapon)
                .ToList();
        }

        private static float[] CreateWallHitPointPercentages(
            int breachedWallCount)
        {
            var percentages = new[] { 1f, 1f };
            if (breachedWallCount == 1)
                percentages[MBRandom.RandomInt(2)] = 0f;
            else if (breachedWallCount >= 2)
                percentages[0] = percentages[1] = 0f;
            return percentages;
        }

        private static string GetSiegeSceneLevels(int sceneLevel)
        {
            int level = sceneLevel < 1 || sceneLevel > 3 ? 3 : sceneLevel;
            return $"level_{level} siege";
        }

        private static TextObject GetGeneralName(PartyConfig config)
        {
            return config.HasHeroes
                ? config.Heroes.Troops
                    .Select(troop => troop.Character.CharacterObject)
                    .FirstOrDefault(character => character != null)
                    ?.Name
                : null;
        }
    }
}
