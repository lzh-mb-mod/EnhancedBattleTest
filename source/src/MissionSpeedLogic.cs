﻿using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    public enum MissionSpeed
    {
        Slow,
        Normal,
        Fast
    }

    class MissionSpeedLogic : MissionLogic
    {
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Input.IsKeyPressed(InputKey.P))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            MissionState.Current.Paused = !MissionState.Current.Paused;
            Utility.DisplayMessage("Mission " + (MissionState.Current.Paused ? "paused." : "continued."));
        }


        public void ResetSpeed()
        {
            Mission.Scene.SlowMotionFactor = 0.2f;
            SetSpeedMode(MissionSpeed.Normal);
        }

        public void SetSpeedMode(MissionSpeed speed)
        {
            switch (speed)
            {
                case MissionSpeed.Slow:
                    SetSlowMotionMode();
                    break;
                case MissionSpeed.Normal:
                    SetNormalMode();
                    break;
                case MissionSpeed.Fast:
                    SetFastForwardMode();
                    break;
            }
        }

        public void SetSlowMotionFactor(float factor)
        {
            if (!Mission.Scene.SlowMotionMode)
                SetSlowMotionMode();
            this.Mission.Scene.SlowMotionFactor = factor;
        }

        public MissionSpeed CurrentSpeed
        {
            get
            {
                if (Mission.IsFastForward)
                    return MissionSpeed.Fast;
                if (Mission.Scene.SlowMotionMode)
                    return MissionSpeed.Slow;
                return MissionSpeed.Normal;
            }
        }

        public void SetSlowMotionMode()
        {
            SetFastForwardModeImpl(false);
            SetSlowMotionModeImpl(true);
            Utility.DisplayMessage("Slow Motion Mode Enabled");
        }

        public void SetFastForwardMode()
        {
            SetSlowMotionModeImpl(false);
            SetFastForwardModeImpl(true);
            Utility.DisplayMessage("Fast Forward Mode Enabled");
        }

        public void SetNormalMode()
        {
            SetSlowMotionModeImpl(false);
            SetFastForwardModeImpl(false);
            Utility.DisplayMessage("Normal Mode Enabled");
        }

        private void SetSlowMotionModeImpl(bool enabled)
        {
            Mission.Scene.SlowMotionMode = enabled;
        }

        private void SetFastForwardModeImpl(bool enabled)
        {
            Mission.SetFastForwardingFromUI(enabled);
        }
    }
}