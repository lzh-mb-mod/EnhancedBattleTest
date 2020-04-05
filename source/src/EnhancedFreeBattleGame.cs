using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class EnhancedFreeBattleGame : TaleWorlds.Core.GameType
    {
        private Func<BattleConfigBase> _getConfig;
        public static EnhancedFreeBattleGame Current => Game.Current.GameType as EnhancedFreeBattleGame;

        public EnhancedFreeBattleGame(Func<BattleConfigBase> getConfig)
        {
            _getConfig = getConfig;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Game currentGame = this.CurrentGame;
            currentGame.FirstInitialize();
            this.AddGameTexts();
            IGameStarter gameStarter = (IGameStarter)new BasicGameStarter();
            this.AddGameModels(gameStarter);
            this.GameManager.OnGameStart(this.CurrentGame, gameStarter);
            currentGame.SecondInitialize(gameStarter.Models);
            currentGame.CreateGameManager();
            currentGame.ThirdInitialize();
            this.GameManager.BeginGameStart(this.CurrentGame);
            currentGame.RegisterBasicTypes();
            currentGame.CreateObjects();
            currentGame.InitializeDefaultGameObjects();
            currentGame.LoadBasicFiles(false);
            this.ObjectManager.LoadXML("Items");
            this.ObjectManager.LoadXML("MPCharacters");
            this.ObjectManager.LoadXML("BasicCultures");
            currentGame.CreateLists();
            this.ObjectManager.LoadXML("MPClassDivisions");
            this.ObjectManager.ClearEmptyObjects();
            MultiplayerClassDivisions.Initialize();
            this.GameManager.OnCampaignStart(this.CurrentGame, null);
            this.GameManager.OnAfterCampaignStart(this.CurrentGame);

            
            this.GameManager.OnGameInitializationFinished(this.CurrentGame);
            this.CurrentGame.AddGameHandler<ChatBox>();
        }

        private void AddGameTexts()
        {
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/multiplayer_strings.xml");
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/global_strings.xml");
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/module_strings.xml");
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/native_strings.xml");
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/EnhancedBattleTest/ModuleData/module_strings.xml");


        }

        private void AddGameModels(IGameStarter basicGameStarter)
        {

            basicGameStarter.AddModel(new EnhancedBattleTestSkillList());
            basicGameStarter.AddModel(new DefaultRidingModel());
            basicGameStarter.AddModel(new EnhancedMPStrikeMagnitudeModel());
            basicGameStarter.AddModel(new MultiplayerAgentDecideKilledOrUnconsciousModel());
            basicGameStarter.AddModel(new EnhancedMPAgentStatCalculateModel(_getConfig()));
            basicGameStarter.AddModel(new MultiplayerAgentApplyDamageModel());
            basicGameStarter.AddModel(new MultiplayerBattleMoraleModel());
        }

        protected override void OnRegisterTypes()
        {
            base.OnRegisterTypes();
            this.ObjectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "MPCharacters", true);
            this.ObjectManager.RegisterType<BasicCultureObject>("Culture", "BasicCultures", true);
            this.ObjectManager.RegisterType<MultiplayerClassDivisions.MPHeroClass>("MPClassDivision", "MPClassDivisions", true);
        }

        protected override void DoLoadingForGameType(
            GameTypeLoadingStates gameTypeLoadingState,
            out GameTypeLoadingStates nextState)
        {
            nextState = GameTypeLoadingStates.None;
            switch (gameTypeLoadingState)
            {
                case GameTypeLoadingStates.InitializeFirstStep:
                    this.CurrentGame.Initialize();
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

    class EnhancedBattleTestSkillList : SkillList
    {
        internal EnhancedBattleTestSkillList()
        {
        }

        public override IEnumerable<SkillObject> GetSkillList()
        {
            return DefaultSkills.GetAllSkills();
        }
    }
}