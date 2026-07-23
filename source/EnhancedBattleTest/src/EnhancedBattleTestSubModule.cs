using EnhancedBattleTest.Data;
using EnhancedBattleTest.GameMode;
using EnhancedBattleTest.Patch;
using EnhancedBattleTest.UI;
using HarmonyLib;
using System;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestSubModule : MBSubModuleBase
    {
        private const string HarmonyId = "mod.enhancedbattletest";
        private Harmony _harmony;

        public static EnhancedBattleTestSubModule Instance { get; private set; }

        public const string ModuleId = "EnhancedBattleTest";

        public static string ModuleFolderPath = Path.Combine(BasePath.Name, "Modules", ModuleId);

        public event Action<CharacterSelectionData> OnSelectCharacter;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Instance = this;
            _harmony = new Harmony(HarmonyId);
            _harmony.PatchAll(typeof(EnhancedBattleTestMapMenuPatch).Assembly);
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (gameStarterObject is CampaignGameStarter campaignGameStarter)
                campaignGameStarter.AddBehavior(new EnhancedBattleTestCampaignBehavior());
        }

        protected override void OnSubModuleUnloaded()
        {
            EnhancedBattleTestPartyController.Cleanup();
            _harmony?.UnpatchAll(HarmonyId);
            Instance = null;
            base.OnSubModuleUnloaded();
        }

        public static void OpenBattleTest()
        {
            if (Campaign.Current == null)
                return;

            Game.Current.GameStateManager.PushState(
                Game.Current.GameStateManager.CreateState<EnhancedBattleTestState>());
        }

        public void SelectCharacter(CharacterSelectionData data)
        {
            OnSelectCharacter?.Invoke(data);
        }
    }
}
