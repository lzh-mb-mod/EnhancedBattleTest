using System.Collections.Generic;
using System.IO;
using EnhancedBattleTest.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace EnhancedBattleTest.GameMode
{
    public class MultiplayerGame : GameType
    {
        public static MultiplayerGame Current => TaleWorlds.Core.Game.Current.GameType as MultiplayerGame;

        protected override void OnInitialize()
        {
            TaleWorlds.Core.Game currentGame = this.CurrentGame;
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
            currentGame.SetDefaultEquipments((IReadOnlyDictionary<string, Equipment>)new Dictionary<string, Equipment>());
            currentGame.CreateLists();
            ObjectManager.LoadXML("MPClassDivisions");
            objectManager.ClearEmptyObjects();
            MultiplayerClassDivisions.Initialize();
            GameManager.OnCampaignStart(this.CurrentGame, (object)null);
            GameManager.OnAfterCampaignStart(this.CurrentGame);
            GameManager.OnGameInitializationFinished(this.CurrentGame);
            CurrentGame.AddGameHandler<ChatBox>();
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
            gameStarter.AddModel(new MultiplayerAgentDecideKilledOrUnconsciousModel());
            gameStarter.AddModel(new MultiplayerAgentStatCalculateModel());
            gameStarter.AddModel(new MultiplayerApplyWeatherEffectsModel());
            gameStarter.AddModel(new MultiplayerAgentApplyDamageModel());
            gameStarter.AddModel(new DefaultRidingModel());
            gameStarter.AddModel(new MultiplayerStrikeMagnitudeModel());
            gameStarter.AddModel(new DefaultSkillList());
            gameStarter.AddModel(new MultiplayerBattleMoraleModel());
        }

        private void LoadXmls()
        {
            ObjectManager.LoadXML("Items");
            ObjectManager.LoadXML("MPCharacters");
            ObjectManager.LoadXML("BasicCultures");
        }

        protected override void BeforeRegisterTypes(MBObjectManager objectManager)
        {
            objectManager.RegisterNonSerializedType<FeatObject>("Feat", "Feats", 0U, true);
        }

        protected override void OnRegisterTypes(MBObjectManager objectManager)
        {
            objectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "MPCharacters", 43U, true);
            objectManager.RegisterType<BasicCultureObject>("Culture", "BasicCultures", 17U, true);
            objectManager.RegisterType<MultiplayerClassDivisions.MPHeroClass>("MPClassDivision", "MPClassDivisions",
                45U, true);
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
