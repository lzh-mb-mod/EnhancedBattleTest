using EnhancedBattleTest.Data;
using EnhancedBattleTest.GameMode;
using EnhancedBattleTest.Patch;
using EnhancedBattleTest.UI;
using HarmonyLib;
using SandBox.View;
using System;
using System.Collections.Generic;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

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
        public event Action<PartySelectionData> OnSelectParty;
        public event Action<PartyRosterReviewData> OnReviewParty;

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
            EnhancedBattleTestSaveGuard.Reset();

            if (gameStarterObject is CampaignGameStarter campaignGameStarter)
                campaignGameStarter.AddBehavior(new EnhancedBattleTestCampaignBehavior());
        }

        public override void OnGameEnd(Game game)
        {
            EnhancedBattleTestPartyController.Cleanup();
            EnhancedBattleTestSaveGuard.Reset();
            base.OnGameEnd(game);
        }

        protected override void OnSubModuleUnloaded()
        {
            EnhancedBattleTestPartyController.Cleanup();
            EnhancedBattleTestSaveGuard.Reset();
            Instance = null;
            base.OnSubModuleUnloaded();
        }

        public static void OpenBattleTest()
        {
            if (Campaign.Current == null)
                return;

            if (EnhancedBattleTestSaveGuard.IsSavingDisabled)
            {
                OpenBattleTestConfiguration();
                return;
            }

            var options = new List<InquiryElement>
            {
                new InquiryElement(
                    true,
                    GameTexts.FindText("str_ebt_save_before_battle").ToString(),
                    null),
                new InquiryElement(
                    false,
                    GameTexts.FindText("str_ebt_continue_without_saving").ToString(),
                    null)
            };

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    GameTexts.FindText("str_ebt_save_before_battle_title").ToString(),
                    GameTexts.FindText("str_ebt_save_before_battle_description").ToString(),
                    options,
                    true,
                    1,
                    1,
                    GameTexts.FindText("str_continue").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    selectedOptions =>
                    {
                        if ((bool)selectedOptions[0].Identifier)
                        {
                            ScreenManager.PushScreen(
                                SandBoxViewCreator.CreateSaveLoadScreen(true));
                            return;
                        }

                        OpenBattleTestConfiguration();
                    },
                    null));
        }

        private static void OpenBattleTestConfiguration()
        {
            Game.Current.GameStateManager.PushState(
                Game.Current.GameStateManager.CreateState<EnhancedBattleTestState>());
        }

        public void SelectCharacter(CharacterSelectionData data)
        {
            OnSelectCharacter?.Invoke(data);
        }

        public void SelectParty(PartySelectionData data)
        {
            OnSelectParty?.Invoke(data);
        }

        public void ReviewParty(PartyRosterReviewData data)
        {
            OnReviewParty?.Invoke(data);
        }
    }
}
