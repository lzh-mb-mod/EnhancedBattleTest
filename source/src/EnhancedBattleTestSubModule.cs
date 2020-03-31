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
            EnhancedBattleTestSubModule._instance = this;
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption(
              "EBTFreeBattle",
              new TextObject("{=freebattleoption}EnhancedBattleTest Free Battle"), 
              3,
              () =>
              {
                  MBGameManager.StartNewGame(new EnhancedBattleTestGameManager(
                      new EnhancedFreeBattleGame(EnhancedFreeBattleConfig.Get),
                      () =>
                      {
                          var state = GameStateManager.Current.CreateState<TopState>();
                          TopState.status = TopStateStatus.openConfig;
                          state.openConfigMission = () => EnhancedBattleTestMissions.OpenFreeBattleConfigMission();
                          GameStateManager.Current.PushState(state);
                      }));
              },
              false
            ));
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption(
                "EBTcustomebattle",
                new TextObject("{=custombattleoption}EnhancedBattleTest Custom Battle"),
                3,
                () =>
                {
                    MBGameManager.StartNewGame(new EnhancedBattleTestGameManager(
                        new EnhancedCustomBattleGame(EnhancedCustomBattleConfig.Get),
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
                new TextObject("{=siegebattleoption}EnhancedBattleTest Siege Battle"),
                3,
                () =>
                {
                    MBGameManager.StartNewGame(new EnhancedBattleTestGameManager(
                        new EnhancedFreeBattleGame(EnhancedSiegeBattleConfig.Get),
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