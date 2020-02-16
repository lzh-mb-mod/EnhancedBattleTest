// Decompiled with JetBrains decompiler
// Type: TaleWorlds.MountAndBlade.CustomGameManager
// Assembly: TaleWorlds.MountAndBlade, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D5D21862-28AB-45FC-8C12-16AF95A20751
// Assembly location: D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord - Beta\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.dll

using TaleWorlds.Core;
using TaleWorlds.Engine.Screens;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Modbed
{
    public class EnhancedBattleTestGameManager : MBGameManager
    {
        protected override void DoLoadingForGameManager(
            GameManagerLoadingSteps gameManagerLoadingStep,
            out GameManagerLoadingSteps nextStep)
        {
            ModuleLogger.Writer.WriteLine("EnhancedBattleTestGameManager.DoLoadingForGameManager {0}", gameManagerLoadingStep);
            ModuleLogger.Writer.Flush();
            nextStep = GameManagerLoadingSteps.None;
            switch (gameManagerLoadingStep)
            {
                case GameManagerLoadingSteps.PreInitializeZerothStep:
                    MBGameManager.LoadModuleData(false);
                    MBGlobals.InitializeReferences();
                    new Game(new EnhancedBattleTestGame(), this).DoLoading();
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
            ModuleLogger.Writer.WriteLine("EnhancedBattleTestGameManager.OnLoadFinished");
            ModuleLogger.Writer.Flush();

            base.OnLoadFinished();
            //Game.Current.GameStateManager.CleanAndPushState((GameState)Game.Current.GameStateManager.CreateState<EditorState>(), 0);
            NewMission();
        }

        public void NewMission()
        {
            MBMultiplayerOptionsAccessor.SetFriendlyFireDamageMeleeFriendPercent(90);
            MBMultiplayerOptionsAccessor.SetFriendlyFireDamageMeleeSelfPercent(50);
            MBMultiplayerOptionsAccessor.SetFriendlyFireDamageRangedFriendPercent(50);
            MBMultiplayerOptionsAccessor.SetFriendlyFireDamageRangedSelfPercent(20);
            
            //string scene = "mp_skirmish_map_001a";
            //string scene = "mp_sergeant_map_001";
            // string scene = "mp_test_bora";
            // string scene = "battle_test";
            // string scene = "mp_duel_001_winter";
            // string scene = "mp_sergeant_map_001";
            // string scene = "mp_tdm_map_001";
            // string scene = "scn_world_map";
            // string scene = "mp_compact";
            MissionState.OpenNew(
                "EnhancedBattleTest",
                new MissionInitializerRecord(EnhancedBattleTestParams.Get().scene),
                missionController => new MissionBehaviour[] {
                    new EnhancedBattleTestMissionController(showSelectViewFirst:true),
                    // new BattleTeam1MissionController(),
                    // new TaleWorlds.MountAndBlade.Source.Missions.SimpleMountedPlayerMissionController(),
                    new AgentBattleAILogic(),
                    new AgentVictoryLogic(),
                    new FieldBattleController(),
                    new MissionOptionsComponent(),
                    new EnhancedBattleTestMakeGruntLogic(),
                    // new MissionBoundaryPlacer(),
                }
            );
        }
    }
}