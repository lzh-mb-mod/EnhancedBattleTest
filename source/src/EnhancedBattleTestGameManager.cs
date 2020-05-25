using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
        public class EnhancedBattleTestGameManager : MBGameManager
        {
            private GameType _gameType;
            private Action _startMission;
            public EnhancedBattleTestGameManager(GameType gameType, Action startMission)
            {
                _gameType = gameType;
                _startMission = startMission;
            }
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
                        Game.CreateGame(_gameType, this).DoLoading();
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


            public override void OnLoadFinished()
            {
                base.OnLoadFinished();
                _startMission();
            }
        }
    }