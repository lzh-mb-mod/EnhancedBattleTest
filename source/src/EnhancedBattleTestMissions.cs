using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.MissionSpawnHandlers;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestMissions
    {
        public static Mission OpenFreeBattleConfigMission()
        {
            return MissionState.OpenNew(
                "EnhancedFreeBattleConfig",
                new MissionInitializerRecord("scn_character_creation_scene")
                {
                    DoNotUseLoadingScreen = true,
                },
                missionController => new MissionBehaviour[] {
                },
                true, true, true);
        }

        public static Mission OpenFreeBattleMission(EnhancedFreeBattleConfig config)
        {
            var playerSide = config.isPlayerAttacker ? BattleSideEnum.Attacker : BattleSideEnum.Defender;
            var playerCulture = config.GetPlayerTeamCulture();
            var playerParty = new CustomBattleCombatant(playerCulture.Name, playerCulture,
                new Banner(playerCulture.BannerKey, Utility.BackgroundColor(playerCulture, config.isPlayerAttacker), Utility.ForegroundColor(playerCulture, config.isPlayerAttacker)))
            {
                Side = playerSide
            };
            //Utility.AddCharacter(playerParty, config.playerClass, true, Utility.CommanderFormationClass(), true);
            //Utility.AddCharacter(playerParty, config.playerTroops[0], false, FormationClass.Infantry);
            //Utility.AddCharacter(playerParty, config.playerTroops[1], false, FormationClass.Ranged);
            //Utility.AddCharacter(playerParty, config.playerTroops[2], false, FormationClass.Cavalry);

            var enemyCulture = config.GetEnemyTeamCulture();
            var enemyParty = new CustomBattleCombatant(enemyCulture.Name, enemyCulture,
                new Banner(enemyCulture.BannerKey, Utility.BackgroundColor(enemyCulture, !config.isPlayerAttacker), Utility.ForegroundColor(enemyCulture, !config.isPlayerAttacker)))
            {
                Side = playerSide.GetOppositeSide()
            };
            //Utility.AddCharacter(enemyParty, config.enemyClass, true, Utility.CommanderFormationClass());
            //Utility.AddCharacter(enemyParty, config.enemyTroops[0], false, FormationClass.Infantry);
            //Utility.AddCharacter(enemyParty, config.enemyTroops[1], false, FormationClass.Ranged);
            //Utility.AddCharacter(enemyParty, config.enemyTroops[2], false, FormationClass.Cavalry);
            
            CustomBattleCombatant defenderParty =
                !config.isPlayerAttacker ? playerParty : enemyParty;
            CustomBattleCombatant attackerParty =
                config.isPlayerAttacker ? playerParty : enemyParty;
            return MissionState.OpenNew(
                "EnhancedFreeBattle",
                new MissionInitializerRecord(config.SceneName),
                missionController =>
                {
                    var behaviors = new List<MissionBehaviour>
                    {
                        new EnhancedFreeBattleMissionController(config),
                        new EnhancedBattleEndLogic(config),
                        new EnhancedMissionCombatantsLogic((IEnumerable<IBattleCombatant>) null, (IBattleCombatant) playerParty,
                            defenderParty, attackerParty, Mission.MissionTeamAITypeEnum.FieldBattle, false, config),
                        new AdjustSceneLogic(config),
                        new ControlTroopAfterPlayerDeadLogic(),
                        new CommanderLogic(),
                        new TeamAIEnableLogic(config),
                        new DisableDeathLogic(config),
                        new SwitchTeamLogic(),
                        new SwitchFreeCameraLogic(),
                        new ResetMissionLogic(),
                        new MissionSpeedLogic(),
                        new ReadPositionLogic(),
                        new TeleportPlayerLogic(),
                        new AgentBattleAILogic(),
                        new AgentVictoryLogic(),
                        new MissionOptionsComponent(),
                        new BattleMissionAgentInteractionLogic(),
                        new AgentFadeOutLogic(),
                        new AgentMoraleInteractionLogic(),
                        new HighlightsController(),
                        new BattleHighlightsController(),
                        new FieldBattleController(),
                    };
                    if (config.makeGruntVoice)
                    {
                        behaviors.Add(new MakeGruntVoiceLogic());
                    }
                    if (config.hasBoundary)
                    {
                        behaviors.Add(new MissionBoundaryPlacer());
                        behaviors.Add(new MissionHardBorderPlacer());
                        behaviors.Add(new MissionBoundaryCrossingHandler());
                    }
                    return behaviors;
                });
        }
        public static Mission OpenCustomBattleConfigMission()
        {
            return MissionState.OpenNew(
                "EnhancedCustomBattleConfig",
                new MissionInitializerRecord("scn_character_creation_scene")
                {
                    DoNotUseLoadingScreen = true,
                },
                missionController => new MissionBehaviour[] {
                }, true, true, true);
        }
        public static Mission OpenCustomBattleMission(EnhancedCustomBattleConfig config)
        {
            var playerSide = config.isPlayerAttacker ? BattleSideEnum.Attacker : BattleSideEnum.Defender;
            var playerCulture = config.GetPlayerTeamCulture();
            var playerParty = new CustomBattleCombatant(playerCulture.Name, playerCulture,
                new Banner(playerCulture.BannerKey, Utility.BackgroundColor(playerCulture, config.isPlayerAttacker), Utility.ForegroundColor(playerCulture, config.isPlayerAttacker)))
            {
                Side = playerSide
            };
            var player = Utility.AddCharacter(playerParty, config.playerClass, true, Utility.CommanderFormationClass());
            Utility.AddCharacter(playerParty, config.playerTroops[0], false, FormationClass.Infantry);
            Utility.AddCharacter(playerParty, config.playerTroops[1], false, FormationClass.Ranged);
            Utility.AddCharacter(playerParty, config.playerTroops[2], false, FormationClass.Cavalry);

            var enemyCulture = config.GetEnemyTeamCulture();
            var enemyParty = new CustomBattleCombatant(enemyCulture.Name, enemyCulture,
                new Banner(enemyCulture.BannerKey, Utility.BackgroundColor(enemyCulture, !config.isPlayerAttacker), Utility.ForegroundColor(enemyCulture, !config.isPlayerAttacker)))
            {
                Side = playerSide.GetOppositeSide()
            };
            Utility.AddCharacter(enemyParty, config.enemyClass, true, Utility.CommanderFormationClass());
            Utility.AddCharacter(enemyParty, config.enemyTroops[0], false, FormationClass.Infantry);
            Utility.AddCharacter(enemyParty, config.enemyTroops[1], false, FormationClass.Ranged);
            Utility.AddCharacter(enemyParty, config.enemyTroops[2], false, FormationClass.Cavalry);

            return OpenCustomBattleMission(config, playerParty, enemyParty, player, true, null,
                "", "");
        }
        private static Mission OpenCustomBattleMission(
            EnhancedCustomBattleConfig config,
            CustomBattleCombatant playerParty,
            CustomBattleCombatant enemyParty,
            BasicCharacterObject player,
            bool isPlayerGeneral,
            BasicCharacterObject playerSideGeneralCharacter,
            string sceneLevels = "",
            string seasonString = "",
            float timeOfDay = 6f)
        {
            BattleSideEnum playerSide = playerParty.Side;
            bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
            IMissionTroopSupplier[] troopSuppliers = new IMissionTroopSupplier[2];
            var playerTroopSupplier = new EnhancedTroopSupplier(playerParty);
            troopSuppliers[(int)playerParty.Side] = playerTroopSupplier;
            var enemyTroopSupplier = new EnhancedTroopSupplier(enemyParty);
            troopSuppliers[(int)enemyParty.Side] = enemyTroopSupplier;
            bool isPlayerSergeant = !isPlayerGeneral;
            AtmosphereInfo atmosphereInfo;
            if (!string.IsNullOrEmpty(seasonString))
                atmosphereInfo = new AtmosphereInfo()
                {
                    AtmosphereName = ""
                };
            else
                atmosphereInfo = (AtmosphereInfo)null;
            if (atmosphereInfo != null)
                atmosphereInfo.TimeInfo.TimeOfDay = timeOfDay;
            CustomBattleCombatant defenderParty =
                !isPlayerAttacker ? playerParty : enemyParty;
            CustomBattleCombatant attackerParty =
                isPlayerAttacker ? playerParty : enemyParty;
            return MissionState.OpenNew("EnhancedCustomBattle", new MissionInitializerRecord(config.SceneName)
            {
                DoNotUseLoadingScreen = false,
                AtmosphereOnCampaign = atmosphereInfo,
                SceneLevels = sceneLevels,
                TimeOfDay = timeOfDay
            }, (InitializeMissionBehvaioursDelegate)(missionController => (IEnumerable<MissionBehaviour>)new MissionBehaviour[]
           {
               new AdjustSceneLogic(config),
               new ControlTroopAfterPlayerDeadLogic(),
               new CommanderLogic(),
               new SetPlayerLogic(player),
               new TeamAIEnableLogic(config),
               new DisableDeathLogic(EnhancedCustomBattleConfig.Get()),
               new SwitchTeamLogic(),
               new SwitchFreeCameraLogic(),
               new MissionSpeedLogic(),
               new ReadPositionLogic(),
               new TeleportPlayerLogic(),
               new MissionOptionsComponent(),
               new BattleObserverMissionLogic(),
               new BattleEndLogic(),
               new EnhancedMissionCombatantsLogic((IEnumerable<IBattleCombatant>) null, (IBattleCombatant) playerParty,
                   defenderParty, attackerParty, Mission.MissionTeamAITypeEnum.FieldBattle, isPlayerSergeant, config),
               //new BattleObserverMissionLogic(),
               new CustomBattleAgentLogic(),
               new MissionAgentSpawnLogic(troopSuppliers, playerSide), // player's team may change
               //new SpawnPlayerLogic(playerParty, playerTroopSupplier, player, true),
               new CustomBattleMissionSpawnHandler(defenderParty, attackerParty),
               new AgentBattleAILogic(),
               new AgentVictoryLogic(),
               new MissionHardBorderPlacer(),
               new MissionBoundaryPlacer(),
               new MissionBoundaryCrossingHandler(),
               new BattleMissionAgentInteractionLogic(),
               new AgentFadeOutLogic(),
               new AgentMoraleInteractionLogic(),
               //new AssignPlayerRoleInTeamMissionController(isPlayerGeneral, isPlayerSergeant, false, isPlayerSergeant ? Enumerable.Repeat<string>(player.StringId, 1).ToList<string>() : new List<string>(), FormationClass.NumberOfRegularFormations),
               //new CreateBodyguardMissionBehavior(isPlayerAttacker & isPlayerGeneral ? player.GetName().ToString() : (isPlayerAttacker & isPlayerSergeant ? playerSideGeneralCharacter?.GetName()?.ToString() : enemyCharacter.Name.ToString()), !isPlayerAttacker & isPlayerGeneral ? player.GetName().ToString() : (!isPlayerAttacker & isPlayerSergeant ? playerSideGeneralCharacter?.GetName()?.ToString() : enemyCharacter.Name.ToString()), (string) null, (string) null, true),
               new HighlightsController(),
               new BattleHighlightsController(),
               //new FieldBattleController(),
           }), true, true, true);
        }


        private static Type GetSiegeWeaponType(SiegeEngineType siegeWeaponType)
        {
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ladder)
                return typeof(SiegeLadder);
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ballista)
                return typeof(Ballista);
            if (siegeWeaponType == DefaultSiegeEngineTypes.FireBallista)
                return typeof(FireBallista);
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ram || siegeWeaponType == DefaultSiegeEngineTypes.ImprovedRam)
                return typeof(BatteringRam);
            if (siegeWeaponType == DefaultSiegeEngineTypes.SiegeTower)
                return typeof(SiegeTower);
            if (siegeWeaponType == DefaultSiegeEngineTypes.Onager || siegeWeaponType == DefaultSiegeEngineTypes.Catapult)
                return typeof(Mangonel);
            if (siegeWeaponType == DefaultSiegeEngineTypes.FireOnager || siegeWeaponType == DefaultSiegeEngineTypes.FireCatapult)
                return typeof(FireMangonel);
            return siegeWeaponType == DefaultSiegeEngineTypes.Trebuchet || siegeWeaponType == DefaultSiegeEngineTypes.Bricole ? typeof(Trebuchet) : (Type)null;
        }
        private static Dictionary<Type, int> GetSiegeWeaponTypes(
            Dictionary<SiegeEngineType, int> values)
        {
            Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
            foreach (KeyValuePair<SiegeEngineType, int> keyValuePair in values)
                dictionary.Add(EnhancedBattleTestMissions.GetSiegeWeaponType(keyValuePair.Key), keyValuePair.Value);
            return dictionary;
        }
        public static Mission OpenSiegeBattleConfigMission()
        {
            return MissionState.OpenNew(
                "EnhancedSiegeBattleConfig",
                new MissionInitializerRecord("scn_character_creation_scene")
                {
                    DoNotUseLoadingScreen = true,
                },
                missionController => new MissionBehaviour[] {
                },
                true, true, true);
        }

        public static Mission OpenSiegeBattleMission(EnhancedSiegeBattleConfig config)
        {
            var playerSide = config.isPlayerAttacker ? BattleSideEnum.Attacker : BattleSideEnum.Defender;
            var playerCulture = config.GetPlayerTeamCulture();
            var playerParty = new CustomBattleCombatant(playerCulture.Name, playerCulture,
                new Banner(playerCulture.BannerKey, Utility.BackgroundColor(playerCulture, config.isPlayerAttacker), Utility.ForegroundColor(playerCulture, config.isPlayerAttacker)))
            {
                Side = playerSide
            };
            Utility.AddCharacter(playerParty, config.playerClass, true, Utility.CommanderFormationClass(), true);
            Utility.AddCharacter(playerParty, config.playerTroops[0], false, FormationClass.Infantry);
            Utility.AddCharacter(playerParty, config.playerTroops[1], false, FormationClass.Ranged);
            Utility.AddCharacter(playerParty, config.playerTroops[2], false, FormationClass.Cavalry);

            var enemyCulture = config.GetEnemyTeamCulture();
            var enemyParty = new CustomBattleCombatant(enemyCulture.Name, enemyCulture,
                new Banner(enemyCulture.BannerKey, Utility.BackgroundColor(enemyCulture, !config.isPlayerAttacker), Utility.ForegroundColor(enemyCulture, !config.isPlayerAttacker)))
            {
                Side = playerSide.GetOppositeSide()
            };
            Utility.AddCharacter(enemyParty, config.enemyClass, true, Utility.CommanderFormationClass());
            Utility.AddCharacter(enemyParty, config.enemyTroops[0], false, FormationClass.Infantry);
            Utility.AddCharacter(enemyParty, config.enemyTroops[1], false, FormationClass.Ranged);
            Utility.AddCharacter(enemyParty, config.enemyTroops[2], false, FormationClass.Cavalry);

            return OpenSiegeBattleMission(config, playerParty, enemyParty, true, new float[] { 1 },
                    false, new Dictionary<SiegeEngineType, int>()
                    {
                        //{ DefaultSiegeEngineTypes.Ladder, 1 }
                    }, new Dictionary<SiegeEngineType, int>()
                    {
                        //{ DefaultSiegeEngineTypes.Ladder, 1 }
                    }, config.isPlayerAttacker);
        }

        public static Mission OpenSiegeBattleMission(
            EnhancedSiegeBattleConfig config,
            CustomBattleCombatant playerParty,
            CustomBattleCombatant enemyParty,
            bool isPlayerGeneral,
            float[] wallHitPointPercentages,
            bool hasAnySiegeTower,
            Dictionary<SiegeEngineType, int> siegeWeaponsCountOfAttackers,
            Dictionary<SiegeEngineType, int> siegeWeaponsCountOfDefenders,
            bool isPlayerAttacker,
            int sceneUpgradeLevel = 0,
            string seasonString = "",
            bool isSallyOut = false,
            bool isReliefForceAttack = false,
            float timeOfDay = 6f)
        {
            string sceneLevel;
            switch (sceneUpgradeLevel)
            {
                case 1:
                    sceneLevel = "level_1";
                    break;
                case 2:
                    sceneLevel = "level_2";
                    break;
                default:
                    sceneLevel = "level_3";
                    break;
            }

            sceneLevel += isSallyOut ? " sally" : " siege";
            BattleSideEnum playerSide = playerParty.Side;
            IMissionTroopSupplier[] troopSuppliers = new IMissionTroopSupplier[2];
            var playerTroopSupplier = new EnhancedTroopSupplier(playerParty);
            troopSuppliers[(int)playerParty.Side] = playerTroopSupplier;
            var enemyTroopSupplier = new EnhancedTroopSupplier(enemyParty);
            troopSuppliers[(int)enemyParty.Side] = enemyTroopSupplier;
            var player = Utility.ApplyPerks(config.playerClass, true);
            player.CurrentFormationClass = Utility.CommanderFormationClass();
            bool isPlayerSergeant = !isPlayerGeneral;
            AtmosphereInfo atmosphereInfo;
            if (!string.IsNullOrEmpty(seasonString))
                atmosphereInfo = new AtmosphereInfo()
                {
                    AtmosphereName = ""
                };
            else
                atmosphereInfo = (AtmosphereInfo)null;
            if (atmosphereInfo != null)
                atmosphereInfo.TimeInfo.TimeOfDay = timeOfDay;
            CustomBattleCombatant defenderParty =
                !isPlayerAttacker ? playerParty : enemyParty;
            CustomBattleCombatant attackerParty =
                isPlayerAttacker ? playerParty : enemyParty;

            return MissionState.OpenNew(
                "EnhancedSiegeBattle",
                new MissionInitializerRecord(config.SceneName)
                {
                    DoNotUseLoadingScreen = true,
                    AtmosphereOnCampaign = atmosphereInfo,
                    SceneLevels = sceneLevel,
                    TimeOfDay = timeOfDay
                },
                missionController =>
                {
                    var missionBehaviourList = new List<MissionBehaviour>
                    {
                        //new BattleSpawnLogic(isSallyOut ? "sally_out_set" : (isReliefForceAttack ? "relief_force_attack_set" : "battle_set")),
                        new AddEntityLogic(config),
                        new MissionOptionsComponent(),
                        new BattleObserverMissionLogic(),
                        new BattleEndLogic(),
                        new EnhancedMissionCombatantsLogic((IEnumerable<IBattleCombatant>) null,
                            (IBattleCombatant) playerParty, defenderParty, attackerParty,
                            !isSallyOut ? Mission.MissionTeamAITypeEnum.Siege : Mission.MissionTeamAITypeEnum.SallyOut,
                            isPlayerSergeant, config),
                        new SiegeMissionPreparationHandler(isSallyOut, isReliefForceAttack, wallHitPointPercentages, hasAnySiegeTower),
                        new MissionAgentSpawnLogic(troopSuppliers, playerSide), // player's team may change
                        //new BattleObserverMissionLogic(),
                        new CustomBattleAgentLogic(),
                        new AgentBattleAILogic(),
                        new AmmoSupplyLogic(new List<BattleSideEnum>()
                        {
                            BattleSideEnum.Defender
                        }),
                        new AgentVictoryLogic(),
                        new SiegeMissionController(EnhancedBattleTestMissions.GetSiegeWeaponTypes(siegeWeaponsCountOfAttackers), EnhancedBattleTestMissions.GetSiegeWeaponTypes(siegeWeaponsCountOfDefenders), isPlayerAttacker, isSallyOut),
                        new SiegeDeploymentHandler(isPlayerAttacker, isPlayerAttacker ? EnhancedBattleTestMissions.GetSiegeWeaponTypes(siegeWeaponsCountOfAttackers) : EnhancedBattleTestMissions.GetSiegeWeaponTypes(siegeWeaponsCountOfDefenders)),

                        new AdjustSceneLogic(config),
                        new ControlTroopAfterPlayerDeadLogic(),
                        new CommanderLogic(),
                        new SetPlayerLogic(player),
                        new TeamAIEnableLogic(config),
                        new DisableDeathLogic(config),
                        new SwitchTeamLogic(),
                        new SwitchFreeCameraLogic(),
                        new MissionSpeedLogic(),
                        new ReadPositionLogic(),
                        new TeleportPlayerLogic(),
                        new AgentMoraleInteractionLogic(),
                        new BattleMissionAgentInteractionLogic(),
                        new HighlightsController(),
                        new BattleHighlightsController(),
                        //new AssignPlayerRoleInTeamMissionController(isPlayerGeneral, isPlayerSergeant, false, (List<string>) null, FormationClass.NumberOfRegularFormations),
                        new MissionBoundaryPlacer(),
                        new MissionHardBorderPlacer(),
                        new MissionBoundaryCrossingHandler(),
                    };
                    if (!isSallyOut)
                    {
                        missionBehaviourList.Add(new CustomSiegeMissionSpawnHandler(defenderParty, attackerParty));
                        missionBehaviourList.Add(new AgentFadeOutLogic());
                    }
                    else
                    {
                        missionBehaviourList.Add(new CustomSiegeSallyOutMissionSpawnHandler(defenderParty, attackerParty));
                    }

                    return missionBehaviourList;
                });
        }
    }
}
