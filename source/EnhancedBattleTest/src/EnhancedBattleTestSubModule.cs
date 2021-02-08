using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.GameMode;
using EnhancedBattleTest.Patch;
using EnhancedBattleTest.UI;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using Module = TaleWorlds.MountAndBlade.Module;
using MultiplayerGame = EnhancedBattleTest.GameMode.MultiplayerGame;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestSubModule : MBSubModuleBase
    {
        private readonly Harmony harmony = new Harmony("MissionAgentSpawnLogicForMpPatch");
        private readonly MethodInfo original = typeof(MissionAgentSpawnLogic).GetNestedType("MissionSide", BindingFlags.NonPublic).GetMethod("SpawnTroops", BindingFlags.Instance | BindingFlags.Public);
        private readonly MethodInfo prefix = typeof(Patch_MissionAgentSpawnLogic).GetMethod("SpawnTroops_Prefix");
        public static EnhancedBattleTestSubModule Instance { get; private set; }

        public static string ModuleFolderName = "EnhancedBattleTest";

        public static string ModuleFolderPath = Path.Combine(BasePath.Name, "Modules", ModuleFolderName);

        public static bool IsMultiplayer;

        public event Action<CharacterSelectionData> OnSelectCharacter;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            EnhancedBattleTestSubModule.Instance = this;
            /*
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("EBTMultiplayerTest",
                new TextObject("{=EnhancedBattleTest_multiplayerbattleoption}Multiplayer Battle Test"), 3,
                () =>
                {
                    IsMultiplayer = true;
                    MBGameManager.StartNewGame(new EnhancedBattleTestGameManager<MultiplayerGame>());
                }, false));
            */
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("EBTSingleplayerTest",
                new TextObject("{=EnhancedBattleTest_singleplayerbattleoption}Singleplayer Battle Test"), 3,
                () =>
                {
                    IsMultiplayer = false;
                    MBGameManager.StartNewGame(new EnhancedBattleTestSingleplayerGameManager());
                }, () => false));
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            gameStarterObject.AddModel(new EnhancedBattleTestMoraleModel());
        }

        protected override void OnSubModuleUnloaded()
        {
            EnhancedBattleTestSubModule.Instance = (EnhancedBattleTestSubModule)null;
            base.OnSubModuleUnloaded();
        }

        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);

            if (game.GameType is MultiplayerGame || game.GameType is Campaign)
            {
                ApplyHarmonyPatch();
            }
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);

            if (game.GameType is MultiplayerGame || game.GameType is Campaign)
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
    }
}