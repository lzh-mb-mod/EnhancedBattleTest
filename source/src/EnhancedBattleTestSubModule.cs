using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestSubModule : TaleWorlds.MountAndBlade.MBSubModuleBase
    {
        private static EnhancedBattleTestSubModule _instance;
        private bool _initialized;
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            ModuleLogger.Writer.WriteLine("EnhancedBattleTestSubModule::OnSubModuleLoad");
            EnhancedBattleTestSubModule._instance = this;
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption(
              "EBTtestbattle",
              new TextObject("{=EBTtestbattle}EBT Test Battle", (Dictionary<string, TextObject>)null),
              1,
              () =>
              {
                  MBGameManager.StartNewGame(new EnhancedBattleTestGameManager(new EnhancedTestBattleGame(EnhancedTestBattleConfig.Get()),
                      () =>
                      {
                          var state = GameStateManager.Current.CreateState<TopState>();
                          TopState.status = TopStateStatus.openConfig;
                          state.openConfigMission = () => EnhancedBattleTestMissions.OpenTestBattleConfigMission();
                          GameStateManager.Current.PushState(state);
                      }));
              },
              false
            ));
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption(
                "EBTcustomebattle",
                new TextObject("{=EBTcustomebattle}EBT Custom Battle"),
                2,
                () =>
                {
                    MBGameManager.StartNewGame(new EnhancedBattleTestGameManager(new EnhancedCustomBattleGame(EnhancedCustomBattleConfig.Get()),
                        () =>
                        {
                            var state = GameStateManager.Current.CreateState<TopState>();
                            TopState.status = TopStateStatus.openConfig;
                            state.openConfigMission = () => EnhancedBattleTestMissions.OpenCustomBattleConfigMission();
                            GameStateManager.Current.PushState(state);
                        }));
                },
                false));
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption(
                "EBTsiegebattle",
                new TextObject("{=EBTsiegebattle}EBT Siege Battle"),
                2,
                () =>
                {
                    MBGameManager.StartNewGame(new EnhancedBattleTestGameManager(new EnhancedTestBattleGame(EnhancedSiegeBattleConfig.Get()),
                        () =>
                        {
                            var state = GameStateManager.Current.CreateState<TopState>();
                            TopState.status = TopStateStatus.openConfig;
                            state.openConfigMission = () => EnhancedBattleTestMissions.OpenSiegeBattleConfigMission();
                            GameStateManager.Current.PushState(state);
                        }));
                },
                false));
        }

        protected override void OnSubModuleUnloaded()
        {
            ModuleLogger.Writer.WriteLine("EnhancedBattleTestSubModule::OnSubModuleUnloaded");
            ModuleLogger.Writer.Close();
            EnhancedBattleTestSubModule._instance = (EnhancedBattleTestSubModule)null;
            base.OnSubModuleUnloaded();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (this._initialized)
                return;
            this._initialized = true;
        }

        protected override void OnApplicationTick(float dt)
        {
            // ModuleLogger.Writer.WriteLine("EnhancedBattleTestSubModule::OnApplicationTick {0}", dt);
            base.OnApplicationTick(dt);
        }
    }
}