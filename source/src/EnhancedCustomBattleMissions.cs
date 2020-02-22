using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionSpawnHandlers;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace EnhancedBattleTest
{
    public class EnhancedCustomBattleMissions
    {
        public static Mission OpenCustomBattleConfigMission()
        {
            return MissionState.OpenNew(
                "EnhancedCustomBattleConfig",
                new MissionInitializerRecord("mp_skirmish_map_001a"),
                missionController => new MissionBehaviour[] {
                }, true, true, true);
        }
        public static Mission OpenCustomBattleMission(EnhancedCustomBattleConfig config)
        {
            var playerCulture = config.PlayerTroopHeroClass.Culture;
            var playerParty = new CustomBattleCombatant(playerCulture.Name, playerCulture,
                new Banner(playerCulture.BannerKey, playerCulture.BackgroundColor1, playerCulture.ForegroundColor1))
            {
                Side = BattleSideEnum.Attacker
            };
            var player = config.PlayerHeroClass.HeroCharacter;
            playerParty.AddCharacter(config.PlayerTroopHeroClass.TroopCharacter, config.playerSoldierCount);
            var enemyCulture = config.EnemyTroopHeroClass.Culture;
            var enemyParty = new CustomBattleCombatant(enemyCulture.Name, enemyCulture,
                new Banner(enemyCulture.BannerKey, enemyCulture.BackgroundColor2, enemyCulture.ForegroundColor2))
            {
                Side = BattleSideEnum.Defender
            };
            var enemyGeneral = config.PlayerHeroClass.HeroCharacter;
            enemyParty.AddCharacter(config.EnemyTroopHeroClass.TroopCharacter, config.enemySoldierCount);
            return OpenCustomBattleMission(config.SceneName, player, enemyGeneral, playerParty, enemyParty, true, null,
                "", "", 10f);
        }
        public static Mission OpenCustomBattleMission(
     string scene,
     BasicCharacterObject player,
     BasicCharacterObject enemyGeneral,
     CustomBattleCombatant playerParty,
     CustomBattleCombatant enemyParty,
     bool isPlayerGeneral,
     BasicCharacterObject playerSideGeneralCharacter,
     string sceneLevels = "",
     string seasonString = "",
     float timeOfDay = 6f)
        {
            BattleSideEnum playerSide = playerParty.Side;
            bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
            IMissionTroopSupplier[] troopSuppliers = new IMissionTroopSupplier[2];
            CustomBattleTroopSupplier battleTroopSupplier1 = new CustomBattleTroopSupplier(playerParty, true);
            troopSuppliers[(int)playerParty.Side] = (IMissionTroopSupplier)battleTroopSupplier1;
            CustomBattleTroopSupplier battleTroopSupplier2 = new CustomBattleTroopSupplier(enemyParty, false);
            troopSuppliers[(int)enemyParty.Side] = (IMissionTroopSupplier)battleTroopSupplier2;
            bool isPlayerSergeant = !isPlayerGeneral;
            AtmosphereInfo atmosphereInfo1;
            if (!string.IsNullOrEmpty(seasonString))
                atmosphereInfo1 = new AtmosphereInfo()
                {
                    AtmosphereName = ""
                };
            else
                atmosphereInfo1 = (AtmosphereInfo)null;
            AtmosphereInfo atmosphereInfo2 = atmosphereInfo1;
            if (atmosphereInfo2 != null)
                atmosphereInfo2.TimeInfo.TimeOfDay = timeOfDay;
            return MissionState.OpenNew("EnhancedCustomBattle", new MissionInitializerRecord(scene)
            {
                DoNotUseLoadingScreen = true,
                PlayingInCampaignMode = false,
                AtmosphereOnCampaign = atmosphereInfo2,
                SceneLevels = sceneLevels,
                TimeOfDay = timeOfDay
            }, (InitializeMissionBehvaioursDelegate)(missionController => (IEnumerable<MissionBehaviour>)new MissionBehaviour[]
           {
               new EnhancedCustomBattleMissionController(player, enemyGeneral),
               new ControlTroopAfterPlayerDeadLogic(),
               new SwitchTeamLogic(),
               new SwitchFreeCameraLogic(),
               new MissionOptionsComponent(),
               new BattleEndLogic(),
               new MissionCombatantsLogic(new IBattleCombatant[]{playerParty, enemyParty}, (IBattleCombatant) playerParty, !isPlayerAttacker ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty, Mission.MissionTeamAITypeEnum.FieldBattle, isPlayerSergeant),
               //new BattleObserverMissionLogic(),
               new CustomBattleAgentLogic(),
               new MissionAgentSpawnLogic(troopSuppliers, playerSide),
               new CustomBattleMissionSpawnHandler(!isPlayerAttacker ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty),
               new AgentBattleAILogic(),
               new AgentVictoryLogic(),
               new MissionHardBorderPlacer(),
               new MissionBoundaryPlacer(),
               new MissionBoundaryCrossingHandler(),
               new MissionSimulationHandler(),
               new BattleMissionAgentInteractionLogic(),
               new AgentFadeOutLogic(),
               new AgentMoraleInteractionLogic(),
               new AssignPlayerRoleInTeamMissionController(isPlayerGeneral, isPlayerSergeant, false, isPlayerSergeant ? Enumerable.Repeat<string>(player.StringId, 1).ToList<string>() : new List<string>(), FormationClass.NumberOfRegularFormations),
               new CreateBodyguardMissionBehavior(isPlayerAttacker & isPlayerGeneral ? player.GetName().ToString() : (isPlayerAttacker & isPlayerSergeant ? playerSideGeneralCharacter?.GetName()?.ToString() : (string) null), !isPlayerAttacker & isPlayerGeneral ? player.GetName().ToString() : (!isPlayerAttacker & isPlayerSergeant ? playerSideGeneralCharacter?.GetName()?.ToString() : (string) null), (string) null, (string) null, true),
               new HighlightsController(),
               new BattleHighlightsController(),
           }), true, true, true);
        }
    }
}