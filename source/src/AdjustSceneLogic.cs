using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class AdjustSceneLogic : MissionLogic
    {
        private BattleConfigBase _config;

        public AdjustSceneLogic(BattleConfigBase config)
        {
            _config = config;
        }

        public override void EarlyStart()
        {
            AdjustScene();
        }

        private void AdjustScene()
        {
            var scene = this.Mission.Scene;
            if (this._config.SkyBrightness >= 0)
            {
                scene.SetSkyBrightness(this._config.SkyBrightness);
            }

            if (this._config.RainDensity >= 0)
            {
                scene.SetRainDensity(this._config.RainDensity);
            }
        }
    }
}
