using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace EnhancedBattleTest
{
    public class MultiplayerApplyWeatherEffectsModel : ApplyWeatherEffectsModel
    {
        public override void ApplyWeatherEffects()
        {
            Mission.Current.SetBowMissileSpeedModifier(1f);
            Mission.Current.SetCrossbowMissileSpeedModifier(1f);
            Mission.Current.SetMissileRangeModifier(1f);
        }
    }
}
