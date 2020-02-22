using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using EnhancedBattleTest;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Screen;
using TL = TaleWorlds.Library;
using TaleWorlds.TwoDimension;

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
                  MBGameManager.StartNewGame(new EnhancedTestBattleGameManager());
              },
              false
            ));
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption(
                "EBTcustomebattle",
                new TextObject("{=EBTcustomebattle}EBT Custom Battle"),
                2,
                () =>
                {
                    MBGameManager.StartNewGame(new EnhancedCustomBattleGameManager());
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