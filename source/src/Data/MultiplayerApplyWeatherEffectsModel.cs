using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace EnhancedBattleTest.Data
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
