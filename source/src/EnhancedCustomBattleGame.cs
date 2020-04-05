using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class EnhancedCustomBattleGame : TaleWorlds.Core.GameType
    {
        private Func<BattleConfigBase> _getConfig;

        public static EnhancedCustomBattleGame Current => Game.Current.GameType as EnhancedCustomBattleGame;

        public EnhancedCustomBattleGame(Func<BattleConfigBase> getConfig)
        {
            _getConfig = getConfig;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Game currentGame = this.CurrentGame;
            currentGame.FirstInitialize();
            this.AddGameTexts();
            IGameStarter gameStarter = new BasicGameStarter();
            this.AddGameModels(gameStarter);
            this.GameManager.OnGameStart(this.CurrentGame, gameStarter);
            currentGame.SecondInitialize(gameStarter.Models);
            currentGame.CreateGameManager();
            this.GameManager.BeginGameStart(this.CurrentGame);
            this.CurrentGame.RegisterBasicTypes();
            this.CurrentGame.ThirdInitialize();
            currentGame.CreateObjects();
            currentGame.InitializeDefaultGameObjects();
            currentGame.LoadBasicFiles(false);
            this.ObjectManager.LoadXML("Items");
            this.ObjectManager.LoadXML("MPCharacters");
            this.ObjectManager.LoadXML("BasicCultures");
            this.ObjectManager.LoadXML("MPClassDivisions");
            this.ObjectManager.ClearEmptyObjects();
            MultiplayerClassDivisions.Initialize();
            currentGame.SetDefaultEquipments((IReadOnlyDictionary<string, Equipment>)new Dictionary<string, Equipment>());
            if (currentGame.BasicModels.SkillList == null)
            {
                throw new Exception("Error: No Skill List");
            }
            currentGame.CreateLists();
            this.ObjectManager.ClearEmptyObjects();
            this.GameManager.OnCampaignStart(this.CurrentGame, (object)null);
            this.GameManager.OnAfterCampaignStart(this.CurrentGame);
            this.GameManager.OnGameInitializationFinished(this.CurrentGame);
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

        private void AddGameModels(IGameStarter basicGameStarter)
        {
            basicGameStarter.AddModel(new MultiplayerAgentDecideKilledOrUnconsciousModel());
            basicGameStarter.AddModel(new EnhancedSPAgentStatCalculateModel(_getConfig()));
            basicGameStarter.AddModel(new CustomBattleApplyWeatherEffectsModel());
            basicGameStarter.AddModel(new MultiplayerAgentApplyDamageModel());
            basicGameStarter.AddModel(new DefaultRidingModel());
            basicGameStarter.AddModel(new DefaultStrikeMagnitudeModel());
            basicGameStarter.AddModel(new CustomBattleSkillList());
            basicGameStarter.AddModel(new CustomBattleMoraleModel());
        }

        protected override void OnRegisterTypes()
        {
            base.OnRegisterTypes();
            this.ObjectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "MPCharacters", true);
            this.ObjectManager.RegisterType<BasicCultureObject>("Culture", "BasicCultures", true);
            this.ObjectManager.RegisterType<MultiplayerClassDivisions.MPHeroClass>("MPClassDivision", "MPClassDivisions", true);
        }

        private void AddGameTexts()
        {
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/multiplayer_strings.xml");
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/global_strings.xml");
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/module_strings.xml");
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/native_strings.xml");
            this.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/EnhancedBattleTest/ModuleData/module_strings.xml");
        }
    }

    class CustomBattleSkillList : SkillList
    {

        public override IEnumerable<SkillObject> GetSkillList()
        {
            yield return DefaultSkills.OneHanded;
            yield return DefaultSkills.TwoHanded;
            yield return DefaultSkills.Polearm;
            yield return DefaultSkills.Bow;
            yield return DefaultSkills.Crossbow;
            yield return DefaultSkills.Throwing;
            yield return DefaultSkills.Riding;
            yield return DefaultSkills.Athletics;
            yield return DefaultSkills.Tactics;
            yield return DefaultSkills.Scouting;
            yield return DefaultSkills.Roguery;
            yield return DefaultSkills.Crafting;
            yield return DefaultSkills.Charm;
            yield return DefaultSkills.Trade;
            yield return DefaultSkills.Leadership;
            yield return DefaultSkills.Steward;
            yield return DefaultSkills.Medicine;
            yield return DefaultSkills.Engineering;
        }
    }
}