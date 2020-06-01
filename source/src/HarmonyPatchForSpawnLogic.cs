using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class HarmonyPatchForSpawnLogic
    {
        public static bool SpawnTroops(int number, bool isReinforcement, bool enforceSpawningOnInitialPoint, ref int __result,
            IMissionTroopSupplier ____troopSupplier,
            bool ____spawnWithHorses, BattleSideEnum ____side)
        {
            if (number <= 0)
            {
                __result = 0;
                return false;
            }
            int formationTroopIndex = 0;
            List<IAgentOriginBase> list = ____troopSupplier.SupplyTroops(number).ToList();
            Mission.Current.ResetTotalWidth();
            for (int index = 0; index < 8; ++index)
            {
                int formationTroopCount = 0;
                bool isMounted = false;
                foreach (IAgentOriginBase agentOriginBase in list)
                {
                    var agentOrigin = agentOriginBase as EnhancedBattleTestAgentOrigin;
                    if (agentOrigin != null)
                    {
                        FormationClass formationIndex = agentOrigin.FormationIndex;
                        if ((FormationClass)index == formationIndex)
                        {
                            isMounted = isMounted || agentOrigin.Troop.HasMount();
                            ++formationTroopCount;
                        }
                    }
                }

                float num1 = isMounted ? 3f : 1f;
                float num2 = isMounted ? 0.75f : 0.6f;
                Mission.Current.SetTotalWidthBeforeNewFormation(num1 * (float)Math.Pow(formationTroopCount, num2));
                foreach (IAgentOriginBase agentOriginBase in list)
                {
                    var mpAgentOrigin = agentOriginBase as MPAgentOrigin;
                    var spAgentOrigin = agentOriginBase as SPAgentOrigin;
                    if (mpAgentOrigin != null)
                    {
                        FormationClass formationClass = mpAgentOrigin.MPCharacter.FormationIndex;
                        if ((FormationClass) index == formationClass)
                        {
                            SpawnTroop(mpAgentOrigin, ____side, !mpAgentOrigin.MPCharacter.IsPlayer, ____spawnWithHorses, isReinforcement,
                                enforceSpawningOnInitialPoint, formationTroopCount, formationTroopIndex, true, true,
                                false, null, new MatrixFrame?());
                            ++formationTroopIndex;
                        }
                    }
                    else if (spAgentOrigin != null)
                    {
                        FormationClass formationClass = spAgentOrigin.SPCharacter.FormationIndex;
                        if ((FormationClass)index == formationClass)
                        {
                            SpawnTroop(spAgentOrigin, ____side, !spAgentOrigin.SPCharacter.IsPlayer, ____spawnWithHorses, isReinforcement,
                                enforceSpawningOnInitialPoint, formationTroopCount, formationTroopIndex, true, true,
                                false, null, new MatrixFrame?());
                            ++formationTroopIndex;
                        }
                    }
                }
            }
            if (formationTroopIndex > 0)
            {
                foreach (Team team in Mission.Current.Teams)
                    team.ExpireAIQuerySystem();
                Debug.Print(formationTroopIndex + " troops spawned on " + ____side + " side.", 0, Debug.DebugColor.DarkGreen, 64UL);
            }
            foreach (Team team in Mission.Current.Teams)
            {
                foreach (Formation formation in team.Formations)
                {
                    typeof(Formation).GetField("GroupSpawnIndex", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(formation, 0);
                }
            }
            __result = formationTroopIndex;
            return false;
        }

        private static Agent SpawnTroop(
            MPAgentOrigin agentOrigin,
            BattleSideEnum side,
            bool hasFormation,
            bool spawnWithHorse,
            bool isReinforcement,
            bool enforceSpawningOnInitialPoint,
            int formationTroopCount,
            int formationTroopIndex,
            bool isAlarmed,
            bool wieldInitialWeapons,
            bool forceDismounted = false,
            string specialActionSet = null,
            MatrixFrame? initFrame = null)
        {
            BasicCharacterObject troop = agentOrigin.Troop;
            var team = agentOrigin.IsUnderPlayersCommand ? Mission.Current.PlayerTeam : Mission.Current.PlayerEnemyTeam;
            MatrixFrame frame = initFrame ?? Mission.Current
                .GetFormationSpawnFrame(team.Side, FormationClass.NumberOfRegularFormations, false).ToGroundMatrixFrame();
            if (agentOrigin.MPCharacter.IsPlayer && !forceDismounted)
                spawnWithHorse = true;
            AgentBuildData agentBuildData = new AgentBuildData(agentOrigin).Team(team).Banner(agentOrigin.Banner)
                .ClothingColor1(team.Color).ClothingColor2(team.Color2).TroopOrigin(agentOrigin)
                .NoHorses(!spawnWithHorse).CivilianEquipment(Mission.Current.DoesMissionRequireCivilianEquipment);
            var equipment = Utility.GetNewEquipmentsForPerks(agentOrigin.MPCharacter.HeroClass,
                agentOrigin.MPCharacter.IsHero,
                agentOrigin.MPCharacter.SelectedFirstPerk, agentOrigin.MPCharacter.SelectedSecondPerk,
                agentOrigin.MPCharacter.IsHero, agentOrigin.Seed);
            agentBuildData.Equipment(equipment);
            agentBuildData.IsFemale(agentOrigin.MPCharacter.IsFemale);
            if (!agentOrigin.MPCharacter.IsPlayer)
                agentBuildData.IsReinforcement(isReinforcement).SpawnOnInitialPoint(enforceSpawningOnInitialPoint);
            if (!hasFormation || agentOrigin.MPCharacter.IsPlayer)
                agentBuildData.InitialFrame(frame);
            if (spawnWithHorse)
                agentBuildData.MountKey(MountCreationKey.GetRandomMountKey(
                    troop.Equipment[EquipmentIndex.ArmorItemEndSlot].Item, troop.GetMountKeySeed()));
            if (hasFormation && !agentOrigin.MPCharacter.IsPlayer)
            {
                Formation formation = team.GetFormation(agentOrigin.MPCharacter.FormationIndex);
                agentBuildData.Formation(formation);
                agentBuildData.FormationTroopCount(formationTroopCount).FormationTroopIndex(formationTroopIndex);
            }
            if (agentOrigin.MPCharacter.IsPlayer)
                agentBuildData.Controller(Agent.ControllerType.Player);
            Agent agent = Mission.Current.SpawnAgent(agentBuildData, false, formationTroopCount);
            if (agent.IsAIControlled & isAlarmed)
                agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            if (wieldInitialWeapons)
                agent.WieldInitialWeapons();
            if (!specialActionSet.IsStringNoneOrEmpty())
            {
                AnimationSystemData animationSystemData =
                    agentBuildData.AgentMonster.FillAnimationSystemData(MBGlobals.GetActionSet(specialActionSet),
                        agent.Character.GetStepSize(), false);
                AgentVisualsNativeData agentVisualsNativeData =
                    agentBuildData.AgentMonster.FillAgentVisualsNativeData();
                agentBuildData.AgentMonster.FillAgentVisualsNativeData();
                agent.SetActionSet(ref agentVisualsNativeData, ref animationSystemData);
            }
            return agent;
        }
        private static Agent SpawnTroop(
            SPAgentOrigin agentOrigin,
            BattleSideEnum side,
            bool hasFormation,
            bool spawnWithHorse,
            bool isReinforcement,
            bool enforceSpawningOnInitialPoint,
            int formationTroopCount,
            int formationTroopIndex,
            bool isAlarmed,
            bool wieldInitialWeapons,
            bool forceDismounted = false,
            string specialActionSet = null,
            MatrixFrame? initFrame = null)
        {
            BasicCharacterObject troop = agentOrigin.Troop;
            var team = agentOrigin.IsUnderPlayersCommand ? Mission.Current.PlayerTeam : Mission.Current.PlayerEnemyTeam;
            MatrixFrame frame = initFrame ?? Mission.Current
                .GetFormationSpawnFrame(team.Side, FormationClass.NumberOfRegularFormations, false).ToGroundMatrixFrame();
            if (agentOrigin.SPCharacter.IsPlayer && !forceDismounted)
                spawnWithHorse = true;
            AgentBuildData agentBuildData = new AgentBuildData(agentOrigin).Team(team).Banner(agentOrigin.Banner)
                .ClothingColor1(team.Color).ClothingColor2(team.Color2).TroopOrigin(agentOrigin)
                .NoHorses(!spawnWithHorse).CivilianEquipment(Mission.Current.DoesMissionRequireCivilianEquipment);
            agentBuildData.IsFemale(agentOrigin.SPCharacter.IsFemale);
            if (!agentOrigin.SPCharacter.IsPlayer)
                agentBuildData.IsReinforcement(isReinforcement).SpawnOnInitialPoint(enforceSpawningOnInitialPoint);
            if (!hasFormation || agentOrigin.SPCharacter.IsPlayer)
                agentBuildData.InitialFrame(frame);
            if (spawnWithHorse)
                agentBuildData.MountKey(MountCreationKey.GetRandomMountKey(
                    troop.Equipment[EquipmentIndex.ArmorItemEndSlot].Item, troop.GetMountKeySeed()));
            if (hasFormation && !agentOrigin.SPCharacter.IsPlayer)
            {
                Formation formation = team.GetFormation(agentOrigin.SPCharacter.FormationIndex);
                agentBuildData.Formation(formation);
                agentBuildData.FormationTroopCount(formationTroopCount).FormationTroopIndex(formationTroopIndex);
            }
            if (agentOrigin.SPCharacter.IsPlayer)
                agentBuildData.Controller(Agent.ControllerType.Player);
            bool averageModifier = false;
            if (troop.IsHero)
            {
                agentBuildData.Equipment(troop.GetFirstEquipment(agentBuildData.AgentCivilianEquipment).Clone(false));
            }
            else
            {
                var equipment = Equipment.GetRandomEquipmentElements(troop, false,
                    agentBuildData.AgentCivilianEquipment,
                    agentBuildData.AgentEquipmentSeed);
                if (averageModifier)
                {
                    for (EquipmentIndex index = EquipmentIndex.Weapon0;
                        index < EquipmentIndex.NumEquipmentSetSlots;
                        ++index)
                    {
                        var equipmentElement = equipment.GetEquipmentFromSlot(index);
                        if (equipmentElement.Item != null)
                        {
                            if (equipmentElement.Item.HasArmorComponent)
                                equipmentElement.SetModifier(
                                    Utility.AverageItemModifier(equipmentElement.Item.ArmorComponent
                                        .ItemModifierGroup));
                            else if (equipmentElement.Item.HasHorseComponent)
                                equipmentElement.SetModifier(
                                    Utility.AverageItemModifier(equipmentElement.Item.HorseComponent
                                        .ItemModifierGroup));
                        }
                    }
                }
                agentBuildData.Equipment(equipment);
            }
            Agent agent = Mission.Current.SpawnAgent(agentBuildData, false, formationTroopCount);
            if (agent.IsAIControlled & isAlarmed)
                agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
            if (wieldInitialWeapons)
                agent.WieldInitialWeapons();
            if (!specialActionSet.IsStringNoneOrEmpty())
            {
                AnimationSystemData animationSystemData =
                    agentBuildData.AgentMonster.FillAnimationSystemData(MBGlobals.GetActionSet(specialActionSet),
                        agent.Character.GetStepSize(), false);
                AgentVisualsNativeData agentVisualsNativeData =
                    agentBuildData.AgentMonster.FillAgentVisualsNativeData();
                agentBuildData.AgentMonster.FillAgentVisualsNativeData();
                agent.SetActionSet(ref agentVisualsNativeData, ref animationSystemData);
            }
            return agent;
        }
    }
}
