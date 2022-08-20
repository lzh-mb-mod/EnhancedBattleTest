using EnhancedBattleTest.Data;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.GameMode
{
    public class EnhancedBattleTestSingleplayerGameManager : SandBoxGameManager
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
            if (CampaignSiegeTestStatic.IsSiegeTestBuild)
                CampaignSiegeTestStatic.DisableSiegeTest();
            Game.Current.GameStateManager.OnSavedGameLoadFinished();
            Game.Current.GameStateManager.CleanAndPushState((GameState)Game.Current.GameStateManager.CreateState<MapState>());
            PartyBase.MainParty.Visuals?.SetMapIconAsDirty();
            TaleWorlds.CampaignSystem.Campaign.Current.CampaignInformationManager.OnGameLoaded();
            foreach (Settlement settlement in Settlement.All)
                settlement.Party.Visuals.RefreshLevelMask(settlement.Party);
            CampaignEventDispatcher.Instance.OnGameLoadFinished();
            if (Game.Current.GameStateManager.ActiveState is MapState activeState)
                activeState.OnLoadingFinished();

            IsLoaded = true;
            Game.Current.GameStateManager.PushState(Game.Current.GameStateManager.CreateState<EnhancedBattleTestState>());
        }
        private void InitializeGameTexts(GameTextManager gameTextManager)
        {
            //gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/multiplayer_strings.xml");
            //gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/global_strings.xml");
            //gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/module_strings.xml");
            //gameTextManager.LoadGameTexts(BasePath.Name + "Modules/Native/ModuleData/native_strings.xml");
            gameTextManager.LoadGameTexts(ModuleHelper.GetXmlPath(EnhancedBattleTestSubModule.ModuleId,
                "module_strings"));
        }
    }
}
