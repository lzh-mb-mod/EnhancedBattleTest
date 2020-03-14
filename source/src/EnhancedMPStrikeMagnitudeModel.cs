using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    class EnhancedMPStrikeMagnitudeModel : StrikeMagnitudeCalculationModel
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
