using EnhancedBattleTest.Data;
using EnhancedBattleTest.Patch;
using EnhancedBattleTest.Patch.Fix;
using EnhancedBattleTest.SinglePlayer;
using EnhancedBattleTest.UI;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using EnhancedBattleTest.src.Patch.Fix;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using Campaign = TaleWorlds.CampaignSystem.Campaign;
using MultiplayerGame = EnhancedBattleTest.GameMode.MultiplayerGame;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestSubModule : MBSubModuleBase
    {
        private readonly Harmony harmony = new Harmony("MissionAgentSpawnLogicForMpPatch");
        private readonly MethodInfo original = typeof(MissionAgentSpawnLogic).GetNestedType("MissionSide", BindingFlags.NonPublic).GetMethod("SpawnTroops", BindingFlags.Instance | BindingFlags.Public);
        private readonly MethodInfo prefix = typeof(Patch_MissionAgentSpawnLogic).GetMethod("SpawnTroops_Prefix");
        public static EnhancedBattleTestSubModule Instance { get; private set; }

        public static string ModuleId = "EnhancedBattleTest";

        public static string ModuleFolderPath = Path.Combine(BasePath.Name, "Modules", ModuleId);

        public static bool IsMultiplayer;

        public event Action<CharacterSelectionData> OnSelectCharacter;

        public static bool IsRealisticWeatherLoaded = false;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Instance = this;
            /*
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("EBTMultiplayerTest",
                new TextObject("{=EnhancedBattleTest_multiplayerbattleoption}Multiplayer Battle Test"), 3,
                () =>
                {
                    IsMultiplayer = true;
                    MBGameManager.StartNewGame(new EnhancedBattleTestGameManager<MultiplayerGame>());
                }, false));
            */
            //Module.CurrentModule.AddInitialStateOption(new InitialStateOption("EBTSingleplayerTest",
            //    new TextObject("{=EnhancedBattleTest_singleplayerbattleoption}Singleplayer Battle Test"), 3,
            //    () =>
            //    {
            //        IsMultiplayer = false;
            //        MBGameManager.StartNewGame(new EnhancedBattleTestSingleplayerGameManager());
            //    }, () => (false, new TextObject())));
            Patch_MapScreen.Patch();
            Patch_Hero.Patch();
            Patch_AssignPlayerRoleInTeamMissionController.Patch();
            Patch_DeploymentMissionController.Patch();
            Patch_BesiegerCamp.Patch();
            Patch_PartyBase.Patch();
            // Patch for correct weather in custom sieges            
            Patch_Initializer.Patch();
            Patch_MissionScreen.Patch();
            Patch_CampaignEventDispatcher.Patch();

            // If RealisticWeather mod is activated, add additional weather options
            if (TaleWorlds.Engine.Utilities.GetModulesNames().Select(ModuleHelper.GetModuleInfo).Contains(ModuleHelper.GetModuleInfo("RealisticWeather")))
            {
                IsRealisticWeatherLoaded = true;
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            game.GameTextManager.LoadGameTexts();

            gameStarterObject.AddModel(new EnhancedBattleTestMoraleModel());
        }

        protected override void OnSubModuleUnloaded()
        {
            Instance = null;
            base.OnSubModuleUnloaded();
        }

        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);
            if (game.GameType is Campaign campaign)
            {
                IsMultiplayer = false;
                BattleStarter.IsEnhancedBattleTestBattle = false;
                campaign.AddCampaignEventReceiver(new EnhancedBattleTestCampaignEventReceiver());
            }

            if (game.GameType is MultiplayerGame)
            {
                ApplyHarmonyPatch();
            }
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);

            BattleStarter.IsEnhancedBattleTestBattle = false;
            if (game.GameType is MultiplayerGame)
            {
                Unpatch();
            }
        }

        public void SelectCharacter(CharacterSelectionData data)
        {
            OnSelectCharacter?.Invoke(data);
        }

        private void ApplyHarmonyPatch()
        {
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        private void Unpatch()
        {
            harmony.UnpatchAll(harmony.Id);
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);

            if (BattleStarter.IsEnhancedBattleTestBattle)
            {
                mission.AddMissionBehavior(new EnhancedBattleTestMissionBehavior());

                if (IsRealisticWeatherLoaded) mission.AddMissionBehavior(new EBTRealisticWeatherMissionBehavior());
            }
        }
    }
}