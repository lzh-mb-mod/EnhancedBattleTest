using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace EnhancedBattleTest
{

    public class EnhancedSPAgentStatCalculateModel : AgentStatCalculateModel
    {
        private BattleConfigBase _config;

        public EnhancedSPAgentStatCalculateModel(BattleConfigBase config)
        {
            _config = config;
        }
        public override void InitializeAgentStats(
          Agent agent,
          Equipment spawnEquipment,
          AgentDrivenProperties agentDrivenProperties,
          AgentBuildData agentBuildData)
        {
            agentDrivenProperties.ArmorEncumbrance = spawnEquipment.GetTotalWeightOfArmor(agent.IsHuman);
            if (!agent.IsHuman)
            {
                AgentDrivenProperties drivenProperties1 = agentDrivenProperties;
                EquipmentElement equipmentElement1 = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
                int internalValue = (int)equipmentElement1.Item.Id.InternalValue;
                drivenProperties1.AiSpeciesIndex = internalValue;
                AgentDrivenProperties drivenProperties2 = agentDrivenProperties;
                equipmentElement1 = spawnEquipment[EquipmentIndex.HorseHarness];
                double num1 = 0.800000011920929 + (equipmentElement1.Item != null ? 0.200000002980232 : 0.0);
                drivenProperties2.AttributeRiding = (float)num1;
                float num2 = 0.0f;
                for (int index = 1; index < 12; ++index)
                {
                    equipmentElement1 = spawnEquipment[index];
                    if (equipmentElement1.Item != null)
                    {
                        double num3 = (double)num2;
                        equipmentElement1 = spawnEquipment[index];
                        double bodyArmorHorse = (double)equipmentElement1.GetBodyArmorHorse();
                        num2 = (float)(num3 + bodyArmorHorse);
                    }
                }
                agentDrivenProperties.ArmorTorso = num2;
                equipmentElement1 = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
                ItemObject itemObject = equipmentElement1.Item;
                if (itemObject != null)
                {
                    float num3 = 1f;
                    if (!agent.Mission.Scene.IsAtmosphereIndoor)
                    {
                        if ((double)agent.Mission.Scene.GetRainDensity() > 0.0)
                            num3 *= 0.9f;
                        if (!MBMath.IsBetween(agent.Mission.Scene.TimeOfDay, 4f, 20.01f))
                            num3 *= 0.9f;
                    }
                    HorseComponent horseComponent = itemObject.HorseComponent;
                    EquipmentElement equipmentElement2 = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
                    EquipmentElement harness = spawnEquipment[EquipmentIndex.HorseHarness];
                    agentDrivenProperties.MountManeuver = (float)equipmentElement2.GetBaseHorseManeuver(harness);
                    agentDrivenProperties.MountSpeed = (float)((double)num3 * (double)(equipmentElement2.GetBaseHorseSpeed(harness) + 1) * 0.219999998807907);
                    agentDrivenProperties.MountChargeDamage = (float)equipmentElement2.GetBaseHorseCharge(harness) * 0.01f;
                    agentDrivenProperties.MountDifficulty = (float)equipmentElement2.Item.Difficulty;
                    agentDrivenProperties.TopSpeedReachDuration = Game.Current.BasicModels.RidingModel.CalculateAcceleration(equipmentElement2.Item, agent.RiderAgent?.Character);
                    if (agent.RiderAgent != null)
                    {
                        agentDrivenProperties.MountSpeed *= (float)(1.0 + (double)agent.RiderAgent.Character.GetSkillValue(DefaultSkills.Riding) * (1.0 / 1000.0));
                        agentDrivenProperties.MountManeuver *= (float)(1.0 + (double)agent.RiderAgent.Character.GetSkillValue(DefaultSkills.Riding) * 0.00039999998989515);
                    }
                }
            }
            else
            {
                agentDrivenProperties.ArmorHead = spawnEquipment.GetHeadArmorSum();
                agentDrivenProperties.ArmorTorso = spawnEquipment.GetHumanBodyArmorSum();
                agentDrivenProperties.ArmorLegs = spawnEquipment.GetLegArmorSum();
                agentDrivenProperties.ArmorArms = spawnEquipment.GetArmArmorSum();
                float aiLevel;
                var config = EnhancedCustomBattleConfig.Get();
                if (config.changeCombatAI)
                    aiLevel = config.combatAI / 100f;
                else
                    aiLevel = this.CalculateAILevel(agent);
                agentDrivenProperties.AiRangedHorsebackMissileRange = (float)(0.300000011920929 + 0.400000005960464 * (double)aiLevel);
                agentDrivenProperties.AiFacingMissileWatch = (float)((double)aiLevel * 0.0599999986588955 - 0.959999978542328);
                agentDrivenProperties.AiFlyingMissileCheckRadius = (float)(8.0 - 6.0 * (double)aiLevel);
                agentDrivenProperties.AiShootFreq = (float)(0.200000002980232 + 0.800000011920929 * (double)aiLevel);
                agentDrivenProperties.AiWaitBeforeShootFactor = agent._propertyModifiers.resetAiWaitBeforeShootFactor ? 0.0f : (float)(1.0 - 0.5 * (double)aiLevel);
                agentDrivenProperties.AIBlockOnDecideAbility = MBMath.Lerp(0.05f, 0.95f, MBMath.ClampFloat((float)((Math.Pow((double)MBMath.Lerp(-10f, 10f, aiLevel, 1E-05f), 3.0) + 1000.0) * 0.000500000023748726), 0.0f, 1f), 1E-05f);
                agentDrivenProperties.AIParryOnDecideAbility = MBMath.Lerp(0.05f, 0.95f, MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, aiLevel, 1E-05f), 4.0) * 0.0001f, 0.0f, 1f), 1E-05f);
                agentDrivenProperties.AiTryChamberAttackOnDecide = (float)(((double)aiLevel - 0.150000005960464) * 0.100000001490116);
                agentDrivenProperties.AIAttackOnParryChance = 0.3f;
                agentDrivenProperties.AiAttackOnParryTiming = (float)(0.300000011920929 * (double)aiLevel - 0.200000002980232);
                agentDrivenProperties.AIDecideOnAttackChance = 0.0f;
                agentDrivenProperties.AIParryOnAttackAbility = 0.5f * MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, aiLevel, 1E-05f), 4.0) * 0.0001f, 0.0f, 1f);
                agentDrivenProperties.AiKick = (float)(((double)aiLevel > 0.400000005960464 ? 0.400000005960464 : (double)aiLevel) - 0.100000001490116);
                agentDrivenProperties.AiAttackCalculationMaxTimeFactor = aiLevel;
                agentDrivenProperties.AiDecideOnAttackWhenReceiveHitTiming = (float)(-0.25 * (1.0 - (double)aiLevel));
                agentDrivenProperties.AiDecideOnAttackContinueAction = (float)(-0.5 * (1.0 - (double)aiLevel));
                agentDrivenProperties.AiDecideOnAttackingContinue = 0.1f * aiLevel;
                agentDrivenProperties.AIParryOnAttackingContinueAbility = MBMath.Lerp(0.05f, 0.95f, MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, aiLevel, 1E-05f), 4.0) * 0.0001f, 0.0f, 1f), 1E-05f);
                agentDrivenProperties.AIDecideOnRealizeEnemyBlockingAttackAbility = 0.5f * MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, aiLevel, 1E-05f), 5.0) * 1E-05f, 0.0f, 1f);
                agentDrivenProperties.AIRealizeBlockingFromIncorrectSideAbility = 0.5f * MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, aiLevel, 1E-05f), 5.0) * 1E-05f, 0.0f, 1f);
                agentDrivenProperties.AiAttackingShieldDefenseChance = (float)(0.200000002980232 + 0.300000011920929 * (double)aiLevel);
                agentDrivenProperties.AiAttackingShieldDefenseTimer = (float)(0.300000011920929 * (double)aiLevel - 0.300000011920929);
                agentDrivenProperties.AiRandomizedDefendDirectionChance = (float)(1.0 - Math.Log((double)aiLevel * 7.0 + 1.0, 2.0) * 0.333330005407333);
                agentDrivenProperties.AISetNoAttackTimerAfterBeingHitAbility = MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, aiLevel, 1E-05f), 2.0) * 0.01f, 0.05f, 0.95f);
                agentDrivenProperties.AISetNoAttackTimerAfterBeingParriedAbility = MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, aiLevel, 1E-05f), 2.0) * 0.01f, 0.05f, 0.95f);
                agentDrivenProperties.AISetNoDefendTimerAfterHittingAbility = MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, aiLevel, 1E-05f), 2.0) * 0.01f, 0.05f, 0.95f);
                agentDrivenProperties.AISetNoDefendTimerAfterParryingAbility = MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, aiLevel, 1E-05f), 2.0) * 0.01f, 0.05f, 0.95f);
                agentDrivenProperties.AIEstimateStunDurationPrecision = 1f - MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, aiLevel, 1E-05f), 2.0) * 0.01f, 0.05f, 0.95f);
                agentDrivenProperties.AiRaiseShieldDelayTimeBase = (float)(0.5 * (double)aiLevel - 0.75);
                agentDrivenProperties.AiUseShieldAgainstEnemyMissileProbability = (float)(0.100000001490116 + (double)aiLevel * 0.200000002980232);
                agentDrivenProperties.AiCheckMovementIntervalFactor = (float)(0.00499999988824129 * (1.0 - (double)aiLevel));
                agentDrivenProperties.AiMovemetDelayFactor = (float)(4.0 / (3.0 + (double)aiLevel));
                agentDrivenProperties.AiParryDecisionChangeValue = (float)(0.0500000007450581 + 0.699999988079071 * (double)aiLevel);
                agentDrivenProperties.AiDefendWithShieldDecisionChanceValue = (float)(0.300000011920929 + 0.699999988079071 * (double)aiLevel);
                agentDrivenProperties.AiMoveEnemySideTimeValue = (float)(0.5 * (double)aiLevel - 2.5);
                agentDrivenProperties.AiMinimumDistanceToContinueFactor = (float)(2.0 + 0.300000011920929 * (3.0 - (double)aiLevel));
                agentDrivenProperties.AiStandGroundTimerValue = (float)(0.5 * ((double)aiLevel - 1.0));
                agentDrivenProperties.AiStandGroundTimerMoveAlongValue = (float)(0.5 * (double)aiLevel - 1.0);
                agentDrivenProperties.AiHearingDistanceFactor = 1f + aiLevel;
                agentDrivenProperties.AiChargeHorsebackTargetDistFactor = (float)(1.5 * (3.0 - (double)aiLevel));
                float num = 1f - MBMath.ClampFloat(0.004f * (float)agent.Character.GetSkillValue(DefaultSkills.Bow), 0.0f, 0.99f);
                agentDrivenProperties.AiRangerLeadErrorMin = num * 0.2f;
                agentDrivenProperties.AiRangerLeadErrorMax = num * 0.3f;
                agentDrivenProperties.AiRangerVerticalErrorMultiplier = num * 0.1f;
                agentDrivenProperties.AiRangerHorizontalErrorMultiplier = num * ((float)Math.PI / 90f);
                agentDrivenProperties.AIAttackOnDecideChance = AgentStatCalculateModel.CalculateAIAttackOnDecideMaxValue;
                agentDrivenProperties.SetStat(DrivenProperty.UseRealisticBlocking, _config.useRealisticBlocking ? 1f : 0.0f);
            }
            foreach (DrivenPropertyBonusAgentComponent bonusAgentComponent in agent.Components.OfType<DrivenPropertyBonusAgentComponent>())
            {
                if (MBMath.IsBetween((int)bonusAgentComponent.DrivenProperty, 0, 56))
                {
                    float num = agentDrivenProperties.GetStat(bonusAgentComponent.DrivenProperty) + bonusAgentComponent.DrivenPropertyBonus;
                    agentDrivenProperties.SetStat(bonusAgentComponent.DrivenProperty, num);
                }
            }
        }

        public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            if (!agent.IsHuman)
                return;
            BasicCharacterObject character = agent.Character;
            MissionEquipment equipment = agent.Equipment;
            float totalWeightOfWeapons = equipment.GetTotalWeightOfWeapons();
            int weight = agent.Monster.Weight;
            float num1 = agentDrivenProperties.ArmorEncumbrance + totalWeightOfWeapons;
            EquipmentIndex wieldedItemIndex1 = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            EquipmentIndex wieldedItemIndex2 = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            if (wieldedItemIndex1 != EquipmentIndex.None)
            {
                ItemObject primaryItem = equipment[wieldedItemIndex1].PrimaryItem;
                float realWeaponLength = primaryItem.WeaponComponent.PrimaryWeapon.GetRealWeaponLength();
                totalWeightOfWeapons += 1.5f * primaryItem.Weight * MathF.Sqrt(realWeaponLength);
            }
            if (wieldedItemIndex2 != EquipmentIndex.None)
            {
                ItemObject primaryItem = equipment[wieldedItemIndex2].PrimaryItem;
                totalWeightOfWeapons += 1.5f * primaryItem.Weight;
            }
            agentDrivenProperties.WeaponsEncumbrance = totalWeightOfWeapons;
            EquipmentIndex wieldedItemIndex3 = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            WeaponComponentData weaponComponentData = wieldedItemIndex3 != EquipmentIndex.None ? equipment[wieldedItemIndex3].CurrentUsageItem : (WeaponComponentData)null;
            float inaccuracy;
            agentDrivenProperties.LongestRangedWeaponSlotIndex = (float)equipment.GetLongestRangedWeaponWithAimingError(out inaccuracy, agent);
            agentDrivenProperties.LongestRangedWeaponInaccuracy = inaccuracy;
            agentDrivenProperties.SwingSpeedMultiplier = (float)(0.930000007152557 + 0.000699999975040555 * (double)this.GetSkillValueForItem(character, weaponComponentData?.Item));
            agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = agentDrivenProperties.SwingSpeedMultiplier;
            agentDrivenProperties.ShieldBashStunDurationMultiplier = 1f;
            agentDrivenProperties.ReloadSpeed = (float)(0.930000007152557 + 0.000699999975040555 * (double)this.GetSkillValueForItem(character, weaponComponentData?.Item));
            agentDrivenProperties.WeaponInaccuracy = 0.0f;
            float aiLevel = this.CalculateAILevel(agent);
            agentDrivenProperties.AiWaitBeforeShootFactor = agent._propertyModifiers.resetAiWaitBeforeShootFactor ? 0.0f : (float)(1.0 - 0.5 * (double)aiLevel);
            if (weaponComponentData != null)
            {
                if (weaponComponentData.IsRangedWeapon)
                {
                    WeaponComponentData weapon = weaponComponentData;
                    agentDrivenProperties.WeaponStationaryAccuracyMultiplier = !weapon.WeaponFlags.HasAnyFlag<WeaponFlags>(WeaponFlags.UseHandAsThrowBase) ? (!weapon.WeaponFlags.HasAnyFlag<WeaponFlags>(WeaponFlags.StringHeldByHand) ? 0.5f : 1f) : 2f;
                    int skillValue = character.GetSkillValue(weaponComponentData.RelevantSkill);
                    agentDrivenProperties.WeaponInaccuracy = equipment.GetWeaponInaccuracy(agent, weapon);
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = (float)(500 - skillValue) * 0.00025f;
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = (float)(500 - skillValue) * 0.0002f;
                    if (agent.MountAgent != null)
                        agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = (float)(700 - character.GetSkillValue(weaponComponentData.RelevantSkill) - character.GetSkillValue(DefaultSkills.Riding)) * 0.00015f;
                    else if (weapon.RelevantSkill == DefaultSkills.Bow)
                        agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 3.5f;
                    else if (weapon.RelevantSkill == DefaultSkills.Crossbow)
                        agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 10f;
                    if (weapon.WeaponClass == WeaponClass.Bow)
                    {
                        agentDrivenProperties.WeaponUnsteadyBeginTime = (float)(1.0 + (double)character.GetSkillValue(weaponComponentData.RelevantSkill) * 0.00999999977648258);
                        agentDrivenProperties.WeaponUnsteadyEndTime = 2f + agentDrivenProperties.WeaponUnsteadyBeginTime;
                        agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.025f;
                    }
                    else
                    {
                        agentDrivenProperties.WeaponUnsteadyBeginTime = 0.0f;
                        agentDrivenProperties.WeaponUnsteadyEndTime = 0.0f;
                        agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.0f;
                    }
                }
                else if (weaponComponentData.WeaponFlags.HasAllFlags<WeaponFlags>(WeaponFlags.WideGrip))
                {
                    int skillValue = character.GetSkillValue(DefaultSkills.Polearm);
                    agentDrivenProperties.WeaponInaccuracy = MBMath.ClampFloat((float)(1.0 - (double)skillValue * 0.00999999977648258), 0.0f, 1f);
                    agentDrivenProperties.WeaponUnsteadyBeginTime = (float)(1.0 + (double)skillValue * 0.00499999988824129);
                    agentDrivenProperties.WeaponUnsteadyEndTime = (float)(3.0 + (double)skillValue * 0.00999999977648258);
                }
            }
            agentDrivenProperties.TopSpeedReachDuration = 2f / Math.Max((float)((200.0 + (double)character.GetSkillValue(DefaultSkills.Athletics)) / 300.0 * ((double)weight / ((double)weight + (double)num1))), 0.3f);
            float num2 = 1f;
            if (!agent.Mission.Scene.IsAtmosphereIndoor && (double)agent.Mission.Scene.GetRainDensity() > 0.0)
                num2 *= 0.9f;
            agentDrivenProperties.MaxSpeedMultiplier = num2 * Math.Min((float)((200.0 + (double)character.GetSkillValue(DefaultSkills.Athletics)) / 300.0 * ((double)weight * 2.0 / ((double)weight * 2.0 + (double)num1))), 1f);
            float managedParameter1 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMinMultiplier);
            float managedParameter2 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMaxMultiplier);
            float amount = Math.Min(num1 / (float)weight, 1f);
            agentDrivenProperties.CombatMaxSpeedMultiplier = Math.Min(MBMath.Lerp(managedParameter2, managedParameter1, amount, 1E-05f), 1f);
            agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder = 0.3f;
            Agent mountAgent = agent.MountAgent;
            float num3 = mountAgent != null ? mountAgent.GetAgentDrivenPropertyValue(DrivenProperty.AttributeRiding) : 1f;
            agentDrivenProperties.AttributeRiding = (float)character.GetSkillValue(DefaultSkills.Riding) * num3;
            agentDrivenProperties.AttributeHorseArchery = Game.Current.BasicModels.StrikeMagnitudeModel.CalculateHorseArcheryFactor(character);
            foreach (DrivenPropertyBonusAgentComponent bonusAgentComponent in agent.Components.OfType<DrivenPropertyBonusAgentComponent>())
            {
                if (!MBMath.IsBetween((int)bonusAgentComponent.DrivenProperty, 0, 56))
                {
                    float num4 = agentDrivenProperties.GetStat(bonusAgentComponent.DrivenProperty) + bonusAgentComponent.DrivenPropertyBonus;
                    agentDrivenProperties.SetStat(bonusAgentComponent.DrivenProperty, num4);
                }
            }
        }

        public override short CalculateConsumableMaxAmountAdder()
        {
            return 0;
        }

        private int GetSkillValueForItem(BasicCharacterObject characterObject, ItemObject primaryItem)
        {
            return characterObject.GetSkillValue(primaryItem != null ? primaryItem.RelevantSkill : DefaultSkills.Athletics);
        }
    }
}