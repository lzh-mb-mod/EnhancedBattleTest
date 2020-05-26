using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{

    public class MultiplayerStrikeMagnitudeModel : StrikeMagnitudeCalculationModel
    {
        public override float CalculateStrikeMagnitudeForSwing(
            float swingSpeed,
            float impactPoint,
            float weaponWeight,
            float weaponLength,
            float weaponInertia,
            float weaponCoM,
            float extraLinearSpeed)
        {
            return CombatStatCalculator.CalculateStrikeMagnitudeForSwing(swingSpeed, impactPoint, weaponWeight, weaponLength, weaponInertia, weaponCoM, extraLinearSpeed);
        }

        public override float CalculateStrikeMagnitudeForThrust(
            float thrustWeaponSpeed,
            float weaponWeight,
            float extraLinearSpeed,
            bool isThrown = false)
        {
            return CombatStatCalculator.CalculateStrikeMagnitudeForThrust(thrustWeaponSpeed, weaponWeight, extraLinearSpeed, isThrown);
        }

        public override float ComputeRawDamage(
            DamageTypes damageType,
            float magnitude,
            float armorEffectiveness,
            float absorbedDamageRatio)
        {
            return CombatStatCalculator.ComputeRawDamageOld(damageType, magnitude, armorEffectiveness, absorbedDamageRatio);
        }

        public override float CalculateHorseArcheryFactor(BasicCharacterObject characterObject)
        {
            return 100f;
        }
    }
}
