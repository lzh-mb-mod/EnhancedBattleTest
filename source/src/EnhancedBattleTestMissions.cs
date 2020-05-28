using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.MissionSpawnHandlers;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace EnhancedBattleTest
{
    [MissionManager]
    public static class EnhancedBattleTestMissions
    {

        private static AtmosphereInfo CreateAtmosphereInfoForMission(string seasonString = "")
        {
            string[] strArray = new string[12]
            {
                "TOD_01_00_SemiCloudy",
                "TOD_02_00_SemiCloudy",
                "TOD_03_00_SemiCloudy",
                "TOD_04_00_SemiCloudy",
                "TOD_05_00_SemiCloudy",
                "TOD_06_00_SemiCloudy",
                "TOD_07_00_SemiCloudy",
                "TOD_08_00_SemiCloudy",
                "TOD_09_00_SemiCloudy",
                "TOD_10_00_SemiCloudy",
                "TOD_11_00_SemiCloudy",
                "TOD_12_00_SemiCloudy"
            };
            int index = new Random().Next(0, strArray.Length);
            string str = strArray[index];
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            dictionary.Add("spring", 0);
            dictionary.Add("summer", 1);
            dictionary.Add("fall", 2);
            dictionary.Add("winter", 3);
            int num = 0;
            dictionary.TryGetValue(seasonString, out num);
            return new AtmosphereInfo()
            {
                AtmosphereName = str,
                TimeInfo = new TimeInformation() { Season = num }
            };
        }
        public static Mission OpenMission(BattleConfig config)
        {
            return EnhancedBattleTestSubModule.IsMultiplayer
                ? OpenMultiplayerMission(config)
                : OpenSingleplayerMission(config);
        }



        public static Mission OpenMultiplayerMission(BattleConfig config)
        {
            var playerCulture = Utility.GetCulture(config.PlayerTeamConfig);
            var playerSide = config.BattleTypeConfig.PlayerSide;

            var enemyCulture = Utility.GetCulture(config.EnemyTeamConfig);
            var enemySide = playerSide.GetOppositeSide();
            MPCombatant[] parties = new MPCombatant[2]
            {
                MPCombatant.CreateParty(playerSide, playerCulture, config.PlayerTeamConfig, true),
                MPCombatant.CreateParty(enemySide, enemyCulture, config.EnemyTeamConfig, false)
            };

            if (config.MapConfig.IsSiege)
            {
                var attackerSiegeWeaponCount = GetSiegeWeaponCount(config.SiegeMachineConfig.AttackerMeleeMachines)
                    .Union(GetSiegeWeaponCount(config.SiegeMachineConfig.AttackerRangedMachines))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                var defenderSiegeWeaponCount = GetSiegeWeaponCount(config.SiegeMachineConfig.DefenderMachines);

                int wallHitPoint = config.MapConfig.WallHitPoint;
                var hitPointPercentages = new float[2];
                if (wallHitPoint == 50)
                {
                    int i = MBRandom.RandomInt(2);
                    hitPointPercentages[i] = 0;
                    hitPointPercentages[1 - i] = 1;
                }
                else
                {
                    hitPointPercentages[0] = wallHitPoint / 100.0f;
                    hitPointPercentages[1] = wallHitPoint / 100.0f;
                }

                return OpenMPSiegeMissionWithDeployment(config.MapConfig.MapName, config, parties[0], parties[1], hitPointPercentages,
                    attackerSiegeWeaponCount, defenderSiegeWeaponCount, false, false);
            }
            else
            {
                return OpenMPBattleMission(config.MapConfig.MapName, config, parties[0], parties[1]);
            }
        }


        [MissionMethod]
        public static Mission OpenMPSiegeMissionWithDeployment(
            string scene,
            BattleConfig config,
            MPCombatant playerParty,
            MPCombatant enemyParty,
            float[] wallHitPointPercentages,
            Dictionary<SiegeEngineType, int> siegeWeaponsCountOfAttackers,
            Dictionary<SiegeEngineType, int> siegeWeaponsCountOfDefenders,
            bool isSallyOut = false,
            bool isReliefForceAttack = false,
            float timeOfDay = 6f)
        {
            bool hasAnySiegeTower = siegeWeaponsCountOfAttackers.ContainsKey(DefaultSiegeEngineTypes.SiegeTower);

            string levelString;
            switch (config.MapConfig.SceneLevel)
            {
                case 1:
                    levelString = "level_1";
                    break;
                case 2:
                    levelString = "level_2";
                    break;
                default:
                    levelString = "level_3";
                    break;
            }

            var sceneLevelString =
                (!isSallyOut | isReliefForceAttack) ? levelString + " siege" : levelString + " sally";
            var playerSide = config.BattleTypeConfig.PlayerSide;
            var enemySide = config.BattleTypeConfig.PlayerSide.GetOppositeSide();
            bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
            IMissionTroopSupplier[] troopSuppliers = new IMissionTroopSupplier[2];
            troopSuppliers[(int)playerSide] = new MPTroopSupplier(playerParty);
            troopSuppliers[(int)enemySide] = new MPTroopSupplier(enemyParty);
            bool isPlayerGeneral = config.BattleTypeConfig.PlayerType == PlayerType.Commander;
            bool isPlayerSergeant = !isPlayerGeneral;
            AtmosphereInfo atmosphereInfo = CreateAtmosphereInfoForMission(config.MapConfig.Season);

            var attackerSiegeWeapons =
                siegeWeaponsCountOfAttackers.ToDictionary(pair => pair.Key.GetType(), pair => pair.Value);
            var defenderSiegeWeapons =
                siegeWeaponsCountOfDefenders.ToDictionary(pair => pair.Key.GetType(), pair => pair.Value);
            return MissionState.OpenNew("MPSiegeBattle", new MissionInitializerRecord(scene)
            {
                PlayingInCampaignMode = false,
                AtmosphereOnCampaign = atmosphereInfo,
                SceneLevels = sceneLevelString,
                TimeOfDay = timeOfDay
            }, mission =>
            {
                List<MissionBehaviour> missionBehaviourList = new List<MissionBehaviour>()
                {
                    new BattleSpawnLogic(isSallyOut
                        ? "sally_out_set"
                        : (isReliefForceAttack ? "relief_force_attack_set" : "battle_set")),
                    new MissionOptionsComponent(),
                    new BattleEndLogic(),
                    new MissionCombatantsLogic(null, playerParty,
                        !isPlayerAttacker ? enemyParty : playerParty,
                        isPlayerAttacker ? playerParty : enemyParty,
                        !isSallyOut ? Mission.MissionTeamAITypeEnum.Siege : Mission.MissionTeamAITypeEnum.SallyOut,
                        isPlayerSergeant),
                    new SiegeMissionPreparationHandler(isSallyOut, isReliefForceAttack, wallHitPointPercentages,
                        hasAnySiegeTower),
                    new MissionAgentSpawnLogic(troopSuppliers, playerSide),
                    new BattleObserverMissionLogic(),
                    new CustomBattleAgentLogic(),
                    new AgentBattleAILogic(),
                    new AmmoSupplyLogic(new List<BattleSideEnum>() {BattleSideEnum.Defender}),
                    new AgentVictoryLogic(),
                    new SiegeMissionController(
                        attackerSiegeWeapons,
                        defenderSiegeWeapons,
                        isPlayerAttacker, isSallyOut),
                    new SiegeDeploymentHandler(isPlayerAttacker,
                        isPlayerAttacker ? attackerSiegeWeapons : defenderSiegeWeapons),
                    new MissionBoundaryPlacer(),
                    new MissionBoundaryCrossingHandler(),
                    new AgentMoraleInteractionLogic(),
                    new HighlightsController(),
                    new BattleHighlightsController(),
                    new AssignPlayerRoleInTeamMissionController(isPlayerGeneral, isPlayerSergeant, false, null)
                };
                if (isSallyOut)
                {
                    missionBehaviourList.Add(new EnhancedBattleTestSiegeSallyOutMissionSpawnHandler(
                        isPlayerAttacker ? enemyParty : playerParty,
                        isPlayerAttacker ? playerParty : enemyParty));
                }
                else
                {
                    missionBehaviourList.Add(new EnhancedBattleTestSiegeMissionSpawnHandler(
                        isPlayerAttacker ? enemyParty : playerParty,
                        isPlayerAttacker ? playerParty : enemyParty));
                    missionBehaviourList.Add(new AgentFadeOutLogic());
                }
                return missionBehaviourList;
            });
        }


        [MissionMethod]
        public static Mission OpenMPBattleMission(
            string scene,
            BattleConfig config,
            MPCombatant playerParty,
            MPCombatant enemyParty,
            float timeOfDay = 6f)
        {
            var playerSide = config.BattleTypeConfig.PlayerSide;
            var enemySide = config.BattleTypeConfig.PlayerSide.GetOppositeSide();
            var player = config.PlayerTeamConfig.General as MPCharacterConfig;
            if (player == null)
                return null;
            var playerCharacter = player.CharacterObject;
            bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
            var playerSideLeaderExceptPlayer =
                (config.PlayerTeamConfig.Troops.Troops.FirstOrDefault(config => config.Number > 0)?.Character as
                    MPCharacterConfig)?.CharacterObject;
            IMissionTroopSupplier[] troopSuppliers = new IMissionTroopSupplier[2];
            troopSuppliers[(int)playerSide] = new MPTroopSupplier(playerParty);
            troopSuppliers[(int)enemySide] = new MPTroopSupplier(enemyParty);
            bool isPlayerGeneral = config.BattleTypeConfig.PlayerType == PlayerType.Commander;
            bool isPlayerSergeant = !isPlayerGeneral;
            AtmosphereInfo atmosphereInfo = CreateAtmosphereInfoForMission(config.MapConfig.Season);
            return MissionState.OpenNew("MPBattle", new MissionInitializerRecord(scene)
            {
                DoNotUseLoadingScreen = false,
                PlayingInCampaignMode = false,
                AtmosphereOnCampaign = atmosphereInfo,
                SceneLevels = "",
                TimeOfDay = timeOfDay
            }, missionController =>
                new MissionBehaviour[19]
                {
                    new MissionOptionsComponent(),
                    new BattleEndLogic(),
                    new MissionCombatantsLogic((IEnumerable<IBattleCombatant>) null, (IBattleCombatant) playerParty,
                        !isPlayerAttacker ? (IBattleCombatant) playerParty : (IBattleCombatant) enemyParty,
                        isPlayerAttacker ? (IBattleCombatant) playerParty : (IBattleCombatant) enemyParty,
                        Mission.MissionTeamAITypeEnum.FieldBattle, isPlayerSergeant),
                    new BattleObserverMissionLogic(),
                    new CustomBattleAgentLogic(),
                    new MissionAgentSpawnLogic(troopSuppliers, playerSide),
                    new EnhancedBattleTestMissionSpawnHandler(!isPlayerAttacker ? playerParty : enemyParty,
                        isPlayerAttacker ? playerParty : enemyParty),
                    new AgentBattleAILogic(),
                    new AgentVictoryLogic(),
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(),
                    new MissionBoundaryCrossingHandler(),
                    new BattleMissionAgentInteractionLogic(),
                    new AgentFadeOutLogic(),
                    new AgentMoraleInteractionLogic(),
                    new AssignPlayerRoleInTeamMissionController(isPlayerGeneral, isPlayerSergeant, false,
                        isPlayerSergeant
                            ? Enumerable.Repeat<string>(playerCharacter.StringId, 1).ToList<string>()
                            : new List<string>(), FormationClass.NumberOfRegularFormations),
                    new CreateBodyguardMissionBehavior(
                        isPlayerAttacker & isPlayerGeneral
                            ? playerCharacter.Name.ToString()
                            : (isPlayerAttacker & isPlayerSergeant
                                ? playerSideLeaderExceptPlayer?.GetName().ToString()
                                : (string) null),
                        !isPlayerAttacker & isPlayerGeneral
                            ? playerCharacter.GetName().ToString()
                            : (!isPlayerAttacker & isPlayerSergeant
                                ? playerSideLeaderExceptPlayer?.GetName().ToString()
                                : (string) null), (string) null, (string) null, true),
                    new HighlightsController(),
                    new BattleHighlightsController()
                }, true, true, true);
        }

        private static Dictionary<SiegeEngineType, int> GetSiegeWeaponCount(List<string> siegeWeaponIds)
        {
            Dictionary<SiegeEngineType, int> siegeWeaponsCount = new Dictionary<SiegeEngineType, int>();
            foreach (var attackerMeleeMachine in siegeWeaponIds)
            {
                var siegeWeapon = Utility.GetSiegeEngineType(attackerMeleeMachine);
                if (!siegeWeaponsCount.ContainsKey(siegeWeapon))
                    siegeWeaponsCount.Add(siegeWeapon, 0);
                siegeWeaponsCount[siegeWeapon]++;
            }

            return siegeWeaponsCount;
        }

        public static Mission OpenSingleplayerMission(BattleConfig config)
        {
            throw new NotImplementedException();
        }
    }
}
