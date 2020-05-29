using System;
using SandBox;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestGameManager<T> : MBGameManager where T : GameType, new()
    {
        protected override void DoLoadingForGameManager(
            GameManagerLoadingSteps gameManagerLoadingStep,
            out GameManagerLoadingSteps nextStep)
        {
            nextStep = GameManagerLoadingSteps.None;
            switch (gameManagerLoadingStep)
            {
                case GameManagerLoadingSteps.PreInitializeZerothStep:
                    MBGameManager.LoadModuleData(false);
                    MBGlobals.InitializeReferences();
                    Game.CreateGame(new T(), this).DoLoading();
                    nextStep = GameManagerLoadingSteps.FirstInitializeFirstStep;
                    break;
                case GameManagerLoadingSteps.FirstInitializeFirstStep:
                    bool flag = true;
                    foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
                        flag = flag && subModule.DoLoading(Game.Current);
                    nextStep = flag ? GameManagerLoadingSteps.WaitSecondStep : GameManagerLoadingSteps.FirstInitializeFirstStep;
                    break;
                case GameManagerLoadingSteps.WaitSecondStep:
                    MBGameManager.StartNewGame();
                    nextStep = GameManagerLoadingSteps.SecondInitializeThirdState;
                    break;
                case GameManagerLoadingSteps.SecondInitializeThirdState:
                    nextStep = Game.Current.DoLoading() ? GameManagerLoadingSteps.PostInitializeFourthState : GameManagerLoadingSteps.SecondInitializeThirdState;
                    break;
                case GameManagerLoadingSteps.PostInitializeFourthState:
                    nextStep = GameManagerLoadingSteps.FinishLoadingFifthStep;
                    break;
                case GameManagerLoadingSteps.FinishLoadingFifthStep:
                    nextStep = GameManagerLoadingSteps.None;
                    break;
            }
        }

        public override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);

            InitializeGameModels(gameStarter);
        }


        private void InitializeGameModels(IGameStarter gameStarter)
        {
        }

        public override void OnLoadFinished()
        {
            base.OnLoadFinished();
            Game.Current.GameStateManager.CleanAndPushState(Game.Current.GameStateManager.CreateState<EnhancedBattleTestState>());
        }
    }
}