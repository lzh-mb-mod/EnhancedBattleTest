using System.IO;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Path = System.IO.Path;

namespace EnhancedBattleTest.src
{
    public class EnhancedBattleTestSingleplayerGameManager : CampaignGameManager
    {
        private int _seed = 1234;
        protected override void DoLoadingForGameManager(
          GameManagerLoadingSteps gameManagerLoadingStep,
          out GameManagerLoadingSteps nextStep)
        {
            nextStep = GameManagerLoadingSteps.None;
            switch (gameManagerLoadingStep)
            {
                case GameManagerLoadingSteps.PreInitializeZerothStep:
                    nextStep = GameManagerLoadingSteps.FirstInitializeFirstStep;
                    break;
                case GameManagerLoadingSteps.FirstInitializeFirstStep:
                    MBGameManager.LoadModuleData(false);
                    nextStep = GameManagerLoadingSteps.WaitSecondStep;
                    break;
                case GameManagerLoadingSteps.WaitSecondStep:
                        MBGameManager.StartNewGame();
                    nextStep = GameManagerLoadingSteps.SecondInitializeThirdState;
                    break;
                case GameManagerLoadingSteps.SecondInitializeThirdState:
                    MBGlobals.InitializeReferences();
                    {
                        MBDebug.Print("Initializing new game begin...", 0, Debug.DebugColor.White, 17592186044416UL);
                        var game = new EnhancedBattleTestSingleplayerGame();
                        Game.CreateGame(game, this);
                        game.SetLoadingParameters(Campaign.GameLoadingType.NewCampaign, this._seed);
                        MBDebug.Print("Initializing new game end...", 0, Debug.DebugColor.White, 17592186044416UL);
                    }
                    Game.Current.DoLoading();
                    nextStep = GameManagerLoadingSteps.PostInitializeFourthState;
                    break;
                case GameManagerLoadingSteps.PostInitializeFourthState:
                    bool flag = true;
                    foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
                        flag = flag && subModule.DoLoading(Game.Current);
                    nextStep = flag ? GameManagerLoadingSteps.FinishLoadingFifthStep : GameManagerLoadingSteps.PostInitializeFourthState;
                    break;
                case GameManagerLoadingSteps.FinishLoadingFifthStep:
                    nextStep = Game.Current.DoLoading() ? GameManagerLoadingSteps.None : GameManagerLoadingSteps.FinishLoadingFifthStep;
                    break;
            }
        }

        public override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);

            InitializeGameTexts(Game.Current.GameTextManager);
        }

        public override void OnLoadFinished()
        {
            base.OnLoadFinished();


            Game.Current.GameStateManager.CleanAndPushState(Game.Current.GameStateManager.CreateState<EnhancedBattleTestState>());
        }
        private void InitializeGameTexts(GameTextManager gameTextManager)
        {
            //gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/multiplayer_strings.xml");
            //gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/global_strings.xml");
            //gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/module_strings.xml");
            //gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/native_strings.xml");
            gameTextManager.LoadGameTexts(Path.Combine(EnhancedBattleTestSubModule.ModuleFolderPath, "ModuleData",
                "module_strings.xml"));
        }
    }
}
