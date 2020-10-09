using EnhancedBattleTest.Data;
using SandBox;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using Path = System.IO.Path;

namespace EnhancedBattleTest.GameMode
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
                    LoadModuleData(false);
                    nextStep = GameManagerLoadingSteps.WaitSecondStep;
                    break;
                case GameManagerLoadingSteps.WaitSecondStep:
                    StartNewGame();
                    nextStep = GameManagerLoadingSteps.SecondInitializeThirdState;
                    break;
                case GameManagerLoadingSteps.SecondInitializeThirdState:
                    MBGlobals.InitializeReferences();
                    {
                        MBDebug.Print("Initializing new game begin...");
                        var campaign = new Campaign();
                        TaleWorlds.Core.Game.CreateGame(campaign, this);
                        campaign.SetLoadingParameters(TaleWorlds.CampaignSystem.Campaign.GameLoadingType.NewCampaign, _seed);
                        MBDebug.Print("Initializing new game end...");
                    }
                    TaleWorlds.Core.Game.Current.DoLoading();
                    nextStep = GameManagerLoadingSteps.PostInitializeFourthState;
                    break;
                case GameManagerLoadingSteps.PostInitializeFourthState:
                    bool flag = true;
                    foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
                        flag = flag && subModule.DoLoading(TaleWorlds.Core.Game.Current);
                    nextStep = flag ? GameManagerLoadingSteps.FinishLoadingFifthStep : GameManagerLoadingSteps.PostInitializeFourthState;
                    break;
                case GameManagerLoadingSteps.FinishLoadingFifthStep:
                    nextStep = TaleWorlds.Core.Game.Current.DoLoading() ? GameManagerLoadingSteps.None : GameManagerLoadingSteps.FinishLoadingFifthStep;
                    EnhancedBattleTestPartyController.Initialize();
                    break;
            }
        }

        public override void OnGameStart(TaleWorlds.Core.Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);

            InitializeGameTexts(TaleWorlds.Core.Game.Current.GameTextManager);
        }

        public override void OnGameEnd(TaleWorlds.Core.Game game)
        {
            base.OnGameEnd(game);

            EnhancedBattleTestPartyController.OnGameEnd();
        }

        public override void OnLoadFinished()
        {
            base.OnLoadFinished();


            TaleWorlds.Core.Game.Current.GameStateManager.CleanAndPushState(TaleWorlds.Core.Game.Current.GameStateManager.CreateState<EnhancedBattleTestState>());
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
