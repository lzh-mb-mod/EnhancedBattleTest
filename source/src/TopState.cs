using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public enum TopStateStatus
    {
        exit, openConfig, none
    }
    public class TopState : GameState
    {
        public static TopStateStatus status = TopStateStatus.openConfig;

        public Action openConfigMission;

        public override bool IsMenuState => true;

        protected override void OnActivate()
        {
            base.OnActivate();

            switch (status)
            {
                case TopStateStatus.exit:
                    this.GameStateManager.PopState();
                    status = TopStateStatus.openConfig;
                    break;
                case TopStateStatus.openConfig:
                    openConfigMission?.Invoke();
                    status = TopStateStatus.none;
                    break;
                case TopStateStatus.none:
                    status = TopStateStatus.openConfig;
                    break;
            }
        }
    }
}
