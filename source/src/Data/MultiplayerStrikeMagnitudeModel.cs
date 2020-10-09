using TaleWorlds.Core;

namespace EnhancedBattleTest.Data
{

    public class MultiplayerStrikeMagnitudeModel : StrikeMagnitudeCalculationModel
    {
        public override float CalculateStrikeMagnitudeForSwing(BasicCharacterObject attackerCharacter,
            BasicCharacterObject attackerCaptainCharacter, float swingSpeed,
            float impactPointAsPercent, float weaponWeight, float weaponLength, float weaponInertia, float weaponCoM,
            float extraLinearSpeed, bool doesAttackerHaveMount, WeaponClass weaponClass)
        {
            return CombatStatCalculator.CalculateStrikeMagnitudeForSwing(swingSpeed, impactPointAsPercent, weaponWeight, weaponLength, weaponInertia, weaponCoM, extraLinearSpeed);
        }

        public override float CalculateStrikeMagnitudeForThrust(BasicCharacterObject attackerCharacter,
            BasicCharacterObject attackerCaptainCharacter, float thrustWeaponSpeed,
            float weaponWeight, float extraLinearSpeed, bool doesAttackerHaveMount, WeaponClass weaponClass,
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
