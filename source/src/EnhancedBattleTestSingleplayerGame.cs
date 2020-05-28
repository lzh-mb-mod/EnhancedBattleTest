using SandBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestSingleplayerGame : GameType
    {
        public static EnhancedBattleTestSingleplayerGame Current => Game.Current.GameType as EnhancedBattleTestSingleplayerGame;

        protected override void OnInitialize()
        {
            Game currentGame = this.CurrentGame;
            currentGame.FirstInitialize(false);
            InitializeGameTexts(currentGame.GameTextManager);
            IGameStarter gameStarter = new BasicGameStarter();
            InitializeGameModels(gameStarter);
            GameManager.OnGameStart(currentGame, gameStarter);
            MBObjectManager objectManager = currentGame.ObjectManager;
            currentGame.SecondInitialize(gameStarter.Models);
            currentGame.CreateGameManager();
            GameManager.BeginGameStart(currentGame);
            currentGame.ThirdInitialize();
            currentGame.CreateObjects();
            currentGame.InitializeDefaultGameObjects();
            currentGame.LoadBasicFiles(false);
            LoadXmls();
            objectManager.ClearEmptyObjects();
            currentGame.SetDefaultEquipments((IReadOnlyDictionary<string, Equipment>)new Dictionary<string, Equipment>());
            currentGame.CreateLists();
            objectManager.ClearEmptyObjects();
            this.GameManager.OnCampaignStart(this.CurrentGame, (object)null);
            this.GameManager.OnAfterCampaignStart(this.CurrentGame);
            this.GameManager.OnGameInitializationFinished(this.CurrentGame);
        }

        private void InitializeGameTexts(GameTextManager currentGameGameTextManager)
        {
            currentGameGameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/multiplayer_strings.xml");
            currentGameGameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/global_strings.xml");
            currentGameGameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/module_strings.xml");
            currentGameGameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/native_strings.xml");
            currentGameGameTextManager.LoadGameTexts(Path.Combine(EnhancedBattleTestSubModule.ModuleFolderPath, "ModuleData",
                "module_strings.xml"));
        }

        private void InitializeGameModels(IGameStarter gameStarter)
        {
            gameStarter.AddModel(new DefaultAgentDecideKilledOrUnconsciousModel());
            gameStarter.AddModel(new SandboxAgentStatCalculateModel());
            gameStarter.AddModel(new SandboxApplyWeatherEffectsModel());
            gameStarter.AddModel(new SandboxAgentApplyDamageModel());
            gameStarter.AddModel(new DefaultRidingModel());
            gameStarter.AddModel(new DefaultStrikeMagnitudeModel());
            gameStarter.AddModel(new DefaultSkillList());
            gameStarter.AddModel(new DefaultBattleMoraleModel());
        }

        private void LoadXmls()
        {
            ObjectManager.LoadXML("Items");
            ObjectManager.LoadXML("NPCCharacters");
            ObjectManager.LoadXML("SPCultures");
        }

        protected override void BeforeRegisterTypes(MBObjectManager objectManager)
        {
            objectManager.RegisterNonSerializedType<FeatObject>("Feat", "Feats", 0U, true);
        }

        protected override void OnRegisterTypes(MBObjectManager objectManager)
        {
            objectManager.RegisterType<CharacterObject>("NPCCharacter", "NPCCharacters", 16U, true);
            objectManager.RegisterType<CultureObject>("Culture", "SPCultures", 17U, true);
            objectManager.RegisterType<PerkObject>("Perk", "Perks", 19U, true);
        }

        public override void OnStateChanged(GameState oldState)
        {
        }

        protected override void DoLoadingForGameType(GameTypeLoadingStates gameTypeLoadingState, out GameTypeLoadingStates nextState)
        {
            nextState = GameTypeLoadingStates.None;
            switch (gameTypeLoadingState)
            {
                case GameTypeLoadingStates.InitializeFirstStep:
                    CurrentGame.Initialize();
                    nextState = GameTypeLoadingStates.WaitSecondStep;
                    break;
                case GameTypeLoadingStates.WaitSecondStep:
                    nextState = GameTypeLoadingStates.LoadVisualsThirdState;
                    break;
                case GameTypeLoadingStates.LoadVisualsThirdState:
                    nextState = GameTypeLoadingStates.PostInitializeFourthState;
                    break;
            }
        }

        public override void OnDestroy()
        {
        }
    }
}
