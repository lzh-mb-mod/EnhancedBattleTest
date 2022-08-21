using EnhancedBattleTest.Data;
using System.Collections.Generic;
using System.IO;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace EnhancedBattleTest.GameMode
{
    public class MultiplayerGame : GameType
    {
        public static MultiplayerGame Current => Game.Current.GameType as MultiplayerGame;

        protected override void OnInitialize()
        {
            Game currentGame = CurrentGame;
            InitializeGameTexts(currentGame.GameTextManager);
            IGameStarter gameStarter = new BasicGameStarter();
            InitializeGameModels(gameStarter);
            GameManager.InitializeGameStarter(currentGame, gameStarter);
            GameManager.OnGameStart(currentGame, gameStarter);
            MBObjectManager objectManager = currentGame.ObjectManager;
            currentGame.SetBasicModels(gameStarter.Models);
            currentGame.CreateGameManager();
            GameManager.BeginGameStart(currentGame);
            //CurrentGame.SetRandomGenerators();
            currentGame.InitializeDefaultGameObjects();
            currentGame.LoadBasicFiles();
            LoadXmls();
            objectManager.UnregisterNonReadyObjects();
            currentGame.SetDefaultEquipments(new Dictionary<string, Equipment>());
            ObjectManager.LoadXML("MPClassDivisions");
            objectManager.UnregisterNonReadyObjects();
            MultiplayerClassDivisions.Initialize();
            GameManager.OnNewCampaignStart(CurrentGame, (object)null);
            GameManager.OnAfterCampaignStart(CurrentGame);
            GameManager.OnGameInitializationFinished(CurrentGame);
            CurrentGame.AddGameHandler<ChatBox>();
        }

        private void InitializeGameTexts(GameTextManager currentGameGameTextManager)
        {
            currentGameGameTextManager.LoadGameTexts();
        }

        private void InitializeGameModels(IGameStarter gameStarter)
        {
            //gameStarter.AddModel(new MultiplayerAgentDecideKilledOrUnconsciousModel());
            gameStarter.AddModel(new MultiplayerAgentStatCalculateModel());
            gameStarter.AddModel(new MultiplayerApplyWeatherEffectsModel());
            //gameStarter.AddModel(new MultiplayerAgentApplyDamageModel());
            gameStarter.AddModel(new DefaultRidingModel());
            gameStarter.AddModel(new MultiplayerStrikeMagnitudeModel());
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
            //objectManager.RegisterNonSerializedType<FeatObject>("Feat", "Feats", 0U);
        }

        protected override void OnRegisterTypes(MBObjectManager objectManager)
        {
            objectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "MPCharacters", 43U);
            objectManager.RegisterType<BasicCultureObject>("Culture", "BasicCultures", 17U);
            objectManager.RegisterType<MultiplayerClassDivisions.MPHeroClass>("MPClassDivision", "MPClassDivisions",
                45U);
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
