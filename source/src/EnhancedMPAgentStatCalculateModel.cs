using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace EnhancedBattleTest
{
    public class EnhancedMPAgentStatCalculateModel : AgentStatCalculateModel
    {
        private BattleConfigBase _config;

        public EnhancedMPAgentStatCalculateModel(BattleConfigBase config)
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
                EnhancedMPAgentStatCalculateModel.InitializeHorseAgentStats(agent, spawnEquipment, agentDrivenProperties);
            else
                agentDrivenProperties = this.InitializeHumanAgentStats(agent, agentDrivenProperties, agentBuildData);
            foreach (DrivenPropertyBonusAgentComponent bonusAgentComponent in agent.Components.OfType<DrivenPropertyBonusAgentComponent>())
            {
                if (MBMath.IsBetween((int)bonusAgentComponent.DrivenProperty, 0, 56))
                {
                    float num = agentDrivenProperties.GetStat(bonusAgentComponent.DrivenProperty) + bonusAgentComponent.DrivenPropertyBonus;
                    agentDrivenProperties.SetStat(bonusAgentComponent.DrivenProperty, num);
                }
            }
        }

        private AgentDrivenProperties InitializeHumanAgentStats(
          Agent agent,
          AgentDrivenProperties agentDrivenProperties,
          AgentBuildData agentBuildData)
        {
            MultiplayerClassDivisions.MPHeroClass classForCharacter = Utility.GetMPHeroClassForCharacter(agent.Character);
            if (classForCharacter != null)
            {
                this.FillAgentStatsFromData(ref agentDrivenProperties, classForCharacter, agent, agentBuildData?.AgentMissionPeer);
                agentDrivenProperties.SetStat(DrivenProperty.UseRealisticBlocking, _config.useRealisticBlocking ? 1f : 0.0f);
            }
            float num1 = 0.5f;
            float num2 = 0.5f;
            if (_config.changeCombatAI)
            {
                num1 = _config.combatAI / 100f;
                num2 = _config.combatAI / 100f;
            }
            else if (classForCharacter != null)
            {
                num1 = (float)classForCharacter.MeleeAI / 100f;
                num2 = (float)classForCharacter.RangedAI / 100f;
            }
            else
            {
                Utility.DisplayLocalizedText("str_error_no_hero_class");
            }
            float amount = MBMath.ClampFloat(num1, 0.0f, 1f);
            float num3 = MBMath.ClampFloat(num2, 0.0f, 1f);
            agentDrivenProperties.AiRangedHorsebackMissileRange = (float)(0.300000011920929 + 0.400000005960464 * (double)num3);
            agentDrivenProperties.AiFacingMissileWatch = (float)((double)amount * 0.0599999986588955 - 0.959999978542328);
            agentDrivenProperties.AiFlyingMissileCheckRadius = (float)(8.0 - 6.0 * (double)amount);
            agentDrivenProperties.AiShootFreq = (float)(0.200000002980232 + 0.800000011920929 * (double)num3);
            agentDrivenProperties.AiWaitBeforeShootFactor = agent._propertyModifiers.resetAiWaitBeforeShootFactor ? 0.0f : (float)(1.0 - 0.5 * (double)num3);
            agentDrivenProperties.AIBlockOnDecideAbility = MBMath.Lerp(0.05f, 0.95f, MBMath.ClampFloat((float)((Math.Pow((double)MBMath.Lerp(-10f, 10f, amount, 1E-05f), 3.0) + 1000.0) * 0.000500000023748726), 0.0f, 1f), 1E-05f);
            agentDrivenProperties.AIParryOnDecideAbility = MBMath.Lerp(0.05f, 0.95f, MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, amount, 1E-05f), 4.0) * 0.0001f, 0.0f, 1f), 1E-05f);
            agentDrivenProperties.AiTryChamberAttackOnDecide = (float)(((double)amount - 0.150000005960464) * 0.100000001490116);
            agentDrivenProperties.AIAttackOnParryChance = 0.3f;
            agentDrivenProperties.AiAttackOnParryTiming = (float)(0.300000011920929 * (double)amount - 0.200000002980232);
            agentDrivenProperties.AIDecideOnAttackChance = 0.0f;
            agentDrivenProperties.AIParryOnAttackAbility = 0.5f * MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, amount, 1E-05f), 4.0) * 0.0001f, 0.0f, 1f);
            agentDrivenProperties.AiKick = (float)(((double)amount > 0.400000005960464 ? 0.400000005960464 : (double)amount) - 0.100000001490116);
            agentDrivenProperties.AiAttackCalculationMaxTimeFactor = amount;
            agentDrivenProperties.AiDecideOnAttackWhenReceiveHitTiming = (float)(-0.25 * (1.0 - (double)amount));
            agentDrivenProperties.AiDecideOnAttackContinueAction = (float)(-0.5 * (1.0 - (double)amount));
            agentDrivenProperties.AiDecideOnAttackingContinue = 0.1f * amount;
            agentDrivenProperties.AIParryOnAttackingContinueAbility = MBMath.Lerp(0.05f, 0.95f, MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, amount, 1E-05f), 4.0) * 0.0001f, 0.0f, 1f), 1E-05f);
            agentDrivenProperties.AIDecideOnRealizeEnemyBlockingAttackAbility = 0.5f * MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, amount, 1E-05f), 5.0) * 1E-05f, 0.0f, 1f);
            agentDrivenProperties.AIRealizeBlockingFromIncorrectSideAbility = 0.5f * MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, amount, 1E-05f), 5.0) * 1E-05f, 0.0f, 1f);
            agentDrivenProperties.AiAttackingShieldDefenseChance = (float)(0.200000002980232 + 0.300000011920929 * (double)amount);
            agentDrivenProperties.AiAttackingShieldDefenseTimer = (float)(0.300000011920929 * (double)amount - 0.300000011920929);
            agentDrivenProperties.AiRandomizedDefendDirectionChance = (float)(1.0 - Math.Log((double)amount * 7.0 + 1.0, 2.0) * 0.333330005407333);
            agentDrivenProperties.AISetNoAttackTimerAfterBeingHitAbility = MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, amount, 1E-05f), 2.0) * 0.01f, 0.05f, 0.95f);
            agentDrivenProperties.AISetNoAttackTimerAfterBeingParriedAbility = MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, amount, 1E-05f), 2.0) * 0.01f, 0.05f, 0.95f);
            agentDrivenProperties.AISetNoDefendTimerAfterHittingAbility = MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, amount, 1E-05f), 2.0) * 0.01f, 0.05f, 0.95f);
            agentDrivenProperties.AISetNoDefendTimerAfterParryingAbility = MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, amount, 1E-05f), 2.0) * 0.01f, 0.05f, 0.95f);
            agentDrivenProperties.AIEstimateStunDurationPrecision = 1f - MBMath.ClampFloat((float)Math.Pow((double)MBMath.Lerp(0.0f, 10f, amount, 1E-05f), 2.0) * 0.01f, 0.05f, 0.95f);
            agentDrivenProperties.AiRaiseShieldDelayTimeBase = (float)(0.5 * (double)amount - 0.75);
            agentDrivenProperties.AiUseShieldAgainstEnemyMissileProbability = (float)(0.100000001490116 + (double)amount * 0.200000002980232);
            agentDrivenProperties.AiCheckMovementIntervalFactor = (float)(0.00499999988824129 * (1.0 - (double)amount));
            agentDrivenProperties.AiMovemetDelayFactor = (float)(4.0 / (3.0 + (double)amount));
            agentDrivenProperties.AiParryDecisionChangeValue = (float)(0.0500000007450581 + 0.699999988079071 * (double)amount);
            agentDrivenProperties.AiDefendWithShieldDecisionChanceValue = (float)(0.300000011920929 + 0.699999988079071 * (double)amount);
            agentDrivenProperties.AiMoveEnemySideTimeValue = (float)(0.5 * (double)amount - 2.5);
            agentDrivenProperties.AiMinimumDistanceToContinueFactor = (float)(2.0 + 0.300000011920929 * (3.0 - (double)amount));
            agentDrivenProperties.AiStandGroundTimerValue = (float)(0.5 * ((double)amount - 1.0));
            agentDrivenProperties.AiStandGroundTimerMoveAlongValue = (float)(0.5 * (double)amount - 1.0);
            agentDrivenProperties.AiHearingDistanceFactor = 1f + amount;
            agentDrivenProperties.AiChargeHorsebackTargetDistFactor = (float)(1.5 * (3.0 - (double)amount));
            float num4 = 1f - MBMath.ClampFloat(0.004f * (float)agent.Character.GetSkillValue(DefaultSkills.Bow), 0.0f, 0.99f);
            agentDrivenProperties.AiRangerLeadErrorMin = num4 * -0.3f;
            agentDrivenProperties.AiRangerLeadErrorMax = num4 * 0.2f;
            agentDrivenProperties.AiRangerVerticalErrorMultiplier = num4 * 0.1f;
            agentDrivenProperties.AiRangerHorizontalErrorMultiplier = num4 * ((float)Math.PI / 90f);
            agentDrivenProperties.AIAttackOnDecideChance = AgentStatCalculateModel.CalculateAIAttackOnDecideMaxValue;
            agent.HealthLimit = classForCharacter == null ? 100f : (float)classForCharacter.Health;
            agent.Health = agent.HealthLimit;
            return agentDrivenProperties;
        }

        private static void InitializeHorseAgentStats(
          Agent agent,
          Equipment spawnEquipment,
          AgentDrivenProperties agentDrivenProperties)
        {
            AgentDrivenProperties drivenProperties1 = agentDrivenProperties;
            EquipmentElement equipmentElement1 = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
            HorseComponent horseComponent1 = equipmentElement1.Item.HorseComponent;
            int num1 = horseComponent1 != null ? horseComponent1.Monster.FamilyType : 0;
            drivenProperties1.AiSpeciesIndex = num1;
            AgentDrivenProperties drivenProperties2 = agentDrivenProperties;
            equipmentElement1 = spawnEquipment[EquipmentIndex.HorseHarness];
            double num2 = 0.800000011920929 + (equipmentElement1.Item != null ? 0.200000002980232 : 0.0);
            drivenProperties2.AttributeRiding = (float)num2;
            float num3 = 0.0f;
            for (int index = 1; index < 12; ++index)
            {
                equipmentElement1 = spawnEquipment[index];
                if (equipmentElement1.Item != null)
                {
                    double num4 = (double)num3;
                    equipmentElement1 = spawnEquipment[index];
                    double bodyArmorHorse = (double)equipmentElement1.GetBodyArmorHorse();
                    num3 = (float)(num4 + bodyArmorHorse);
                }
            }
            agentDrivenProperties.ArmorTorso = num3;
            equipmentElement1 = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
            ItemObject itemObject = equipmentElement1.Item;
            if (itemObject == null)
                return;
            HorseComponent horseComponent2 = itemObject.HorseComponent;
            EquipmentElement equipmentElement2 = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
            EquipmentElement harness = spawnEquipment[EquipmentIndex.HorseHarness];
            agentDrivenProperties.MountManeuver = (float)equipmentElement2.GetBaseHorseManeuver(harness);
            agentDrivenProperties.MountSpeed = (float)(equipmentElement2.GetBaseHorseSpeed(harness) + 1) * 0.22f;
            agentDrivenProperties.MountChargeDamage = (float)equipmentElement2.GetBaseHorseCharge(harness) * 0.01f;
            agentDrivenProperties.MountDifficulty = (float)equipmentElement2.Item.Difficulty;
            agentDrivenProperties.TopSpeedReachDuration = Game.Current.BasicModels.RidingModel.CalculateAcceleration(equipmentElement2.Item, agent.RiderAgent?.Character);
            if (agent.RiderAgent == null)
                return;
            agentDrivenProperties.MountSpeed *= (float)(1.0 + (double)agent.RiderAgent.Character.GetSkillValue(DefaultSkills.Riding) * (1.0 / 500.0));
            agentDrivenProperties.MountManeuver *= (float)(1.0 + (double)agent.RiderAgent.Character.GetSkillValue(DefaultSkills.Riding) * 0.0007999999797903);
        }

        public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            if (!agent.IsHuman)
                return;
            BasicCharacterObject character = agent.Character;
            MissionEquipment equipment = agent.Equipment;
            float totalWeightOfWeapons = equipment.GetTotalWeightOfWeapons();
            EquipmentIndex wieldedItemIndex1 = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            EquipmentIndex wieldedItemIndex2 = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            if (wieldedItemIndex1 != EquipmentIndex.None)
            {
                ItemObject primaryItem = equipment[wieldedItemIndex1].PrimaryItem;
                WeaponComponent weaponComponent = primaryItem.WeaponComponent;
                float realWeaponLength = weaponComponent.PrimaryWeapon.GetRealWeaponLength();
                totalWeightOfWeapons += (weaponComponent.GetItemType() == ItemObject.ItemTypeEnum.Bow ? 4f : 1.5f) * primaryItem.Weight * MathF.Sqrt(realWeaponLength);
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
            int weight = agent.Monster.Weight;
            MultiplayerClassDivisions.MPHeroClass classForCharacter = Utility.GetMPHeroClassForCharacter(agent.Character);
            agentDrivenProperties.MaxSpeedMultiplier = (float)(1.04999995231628 * ((double)classForCharacter.MovementSpeedMultiplier * (100.0 / (100.0 + (double)totalWeightOfWeapons))));
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
            agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder = 0.3f;
            Agent mountAgent = agent.MountAgent;
            float num1 = mountAgent != null ? mountAgent.GetAgentDrivenPropertyValue(DrivenProperty.AttributeRiding) : 1f;
            agentDrivenProperties.AttributeRiding = (float)character.GetSkillValue(DefaultSkills.Riding) * num1;
            agentDrivenProperties.AttributeHorseArchery = Game.Current.BasicModels.StrikeMagnitudeModel.CalculateHorseArcheryFactor(character);
            foreach (DrivenPropertyBonusAgentComponent bonusAgentComponent in agent.Components.OfType<DrivenPropertyBonusAgentComponent>())
            {
                if (!MBMath.IsBetween((int)bonusAgentComponent.DrivenProperty, 0, 56))
                {
                    float num2 = agentDrivenProperties.GetStat(bonusAgentComponent.DrivenProperty) + bonusAgentComponent.DrivenPropertyBonus;
                    agentDrivenProperties.SetStat(bonusAgentComponent.DrivenProperty, num2);
                }
            }
        }

        public override short CalculateConsumableMaxAmountAdder()
        {
            return 0;
        }

        public override float GetDifficultyModifier()
        {
            return 1;
        }

        private void FillAgentStatsFromData(
          ref AgentDrivenProperties agentDrivenProperties,
          MultiplayerClassDivisions.MPHeroClass heroClass,
          Agent agent,
          MissionPeer missionPeer)
        {
            float num = 0.0f;
            bool isGeneral = agent.Formation.FormationIndex == Utility.CommanderFormationClass();
            bool isPlayerTeam = agent.Team.IsPlayerTeam;
            ClassInfo info;
            if (isGeneral)
            {
                info = isPlayerTeam ? _config.playerClass : _config.enemyClass;
            }
            else
            {
                info = isPlayerTeam ? _config.playerTroops[agent.Formation.Index] : _config.enemyTroops[agent.Formation.Index];
            }
            num = MPPerkObject.GetArmorBonusFromPerks(isGeneral, Utility.GetAllSelectedPerks(heroClass, new[] { info.selectedFirstPerk, info.selectedSecondPerk }));
            agentDrivenProperties.ArmorHead = (float)heroClass.ArmorValue + num;
            agentDrivenProperties.ArmorTorso = (float)heroClass.ArmorValue + num;
            agentDrivenProperties.ArmorLegs = (float)heroClass.ArmorValue + num;
            agentDrivenProperties.ArmorArms = (float)heroClass.ArmorValue + num;
            agentDrivenProperties.TopSpeedReachDuration = heroClass.TopSpeedReachDuration;
            float managedParameter1 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMinMultiplier);
            float managedParameter2 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMaxMultiplier);
            agentDrivenProperties.CombatMaxSpeedMultiplier = managedParameter1 + (managedParameter2 - managedParameter1) * heroClass.CombatMovementSpeedMultiplier;
        }

        private int GetSkillValueForItem(BasicCharacterObject characterObject, ItemObject primaryItem)
        {
            return characterObject.GetSkillValue(primaryItem != null ? primaryItem.RelevantSkill : DefaultSkills.Athletics);
        }

        public static float CalculateMaximumSpeedMultiplier(Agent agent)
        {
            return Utility.GetMPHeroClassForCharacter(agent.Character).MovementSpeedMultiplier;
        }
    }
}
