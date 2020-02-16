// Decompiled with JetBrains decompiler
// Type: TaleWorlds.MountAndBlade.GauntletUI.MissionOrderGauntletUIHandler
// Assembly: TaleWorlds.MountAndBlade.GauntletUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CDAE3EFC-97F8-4CDA-9155-600EC085A7FB
// Assembly location: C:\game\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord - Beta\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.GauntletUI.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions.Order;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Screen;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace Modbed
{
    // delay initialization.
    public class EnhancedBattleTestMissionOrderUIHandler : MissionOrderGauntletUIHandler
    {
        private bool _isInitialized = false;
        private EnhancedBattleTestMissionController _controller;

        public override void OnMissionScreenInitialize()
        {
            _controller = Mission.Current.GetMissionBehaviour<EnhancedBattleTestMissionController>();
            if (!_controller.ShowSelectViewFirst)
            {
                base.OnMissionScreenInitialize();
                this._isInitialized = true;
            }
        }

        public override void OnMissionScreenActivate()
        {
            if (!_controller.ShowSelectViewFirst)
                base.OnMissionScreenActivate();
        }

        public override void OnMissionScreenTick(float dt)
        {
            if (_isInitialized)
                base.OnMissionScreenTick(dt);
        }

        public override void OnMissionScreenFinalize()
        {
            if (_isInitialized)
                base.OnMissionScreenFinalize();
        }

        public void EnhancedBattleInitialize()
        {
            base.OnMissionScreenInitialize();
            base.OnMissionScreenActivate();
            _isInitialized = true;
        }
    }
}
