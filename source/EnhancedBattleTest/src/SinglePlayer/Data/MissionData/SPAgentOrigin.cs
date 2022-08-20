using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.Data.MissionData;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.SinglePlayer.Data.MissionData
{
    public class SPAgentOrigin : EnhancedBattleTestAgentOrigin
    {
        public override BasicCharacterObject Troop => SPCharacter.Character;
        public override ISpawnableCharacter SpawnableCharacter => SPCharacter;

        public override FormationClass FormationIndex => SPCharacter.FormationIndex;

        public SPSpawnableCharacter SPCharacter { get; }

        public PartyAgentOrigin PartyAgentOrigin;
        public IBattleCombatant CultureCombatant { get; }

        public override Banner Banner => CultureCombatant.Banner;

        public override uint FactionColor => CultureCombatant.PrimaryColorPair.Item1;

        public override uint FactionColor2 => CultureCombatant.PrimaryColorPair.Item2;

        public SPAgentOrigin(SPCombatant combatant, SPSpawnableCharacter character, IEnhancedBattleTestTroopSupplier troopSupplier, BattleSideEnum side, int rank = -1, UniqueTroopDescriptor uniqueNo = default)
            : base(combatant.Combatant, troopSupplier, side, rank, uniqueNo)
        {
            SPCharacter = character;
            CultureCombatant = combatant;
            PartyAgentOrigin = new PartyAgentOrigin(combatant.Combatant, character.CharacterObject, rank,
                uniqueNo);
        }

        public override Agent SpawnTroop(BattleSideEnum side, bool hasFormation, bool spawnWithHorse, bool isReinforcement,
            bool enforceSpawningOnInitialPoint, int formationTroopCount, int formationTroopIndex, bool isAlarmed,
            bool wieldInitialWeapons, bool forceDismounted, Vec3? initialPosition,
            Vec2? initialDirection, string specialActionSet = null)
        {
            BasicCharacterObject troop = Troop;
            var team = IsUnderPlayersCommand ? Mission.Current.PlayerTeam : Mission.Current.PlayerEnemyTeam;
            if (SPCharacter.IsPlayer && !forceDismounted)
                spawnWithHorse = true;
            AgentBuildData agentBuildData = new AgentBuildData(this)
                .Team(team).Banner(Banner)
                .ClothingColor1(team.Color).ClothingColor2(team.Color2)
                .NoHorses(!spawnWithHorse).CivilianEquipment(Mission.Current.DoesMissionRequireCivilianEquipment);
            agentBuildData.IsFemale(SPCharacter.IsFemale);
            if (!SPCharacter.IsPlayer)
                agentBuildData.IsReinforcement(isReinforcement).SpawnOnInitialPoint(enforceSpawningOnInitialPoint);
            if (initialPosition.HasValue && initialDirection.HasValue)
            {
                Vec3 vec3 = initialPosition.Value;
                agentBuildData.InitialPosition(in vec3);
                Vec2 vec2 = initialDirection.Value;
                agentBuildData.InitialDirection(in vec2);
            }
            else if (SpawnableCharacter.IsGeneral && Mission.Current.GetFormationSpawnClass(team.Side, FormationClass.NumberOfRegularFormations) == FormationClass.NumberOfRegularFormations)
            {
                Mission.Current.GetFormationSpawnFrame(team.Side, FormationClass.NumberOfRegularFormations, false, out var spawnPosition, out var direction);
                Vec3 groundVec3 = spawnPosition.GetGroundVec3();
                agentBuildData.InitialPosition(in groundVec3).InitialDirection(in direction);
            }
            else if (!hasFormation)
            {
                Mission.Current.GetFormationSpawnFrame(team.Side, FormationClass.NumberOfAllFormations, false, out var spawnPosition, out var direction);
                Vec3 groundVec3 = spawnPosition.GetGroundVec3();
                agentBuildData.InitialPosition(in groundVec3).InitialDirection(in direction);
            }

            if (spawnWithHorse)
                agentBuildData.MountKey(MountCreationKey.GetRandomMountKey(
                    troop.Equipment[EquipmentIndex.ArmorItemEndSlot].Item, troop.GetMountKeySeed()).ToString());
            if (hasFormation)
            {
                Formation formation = team.GetFormation(SPCharacter.FormationIndex);
                agentBuildData.Formation(formation);
                agentBuildData.FormationTroopCount(formationTroopCount).FormationTroopIndex(formationTroopIndex);
            }
            if (SPCharacter.IsPlayer)
                agentBuildData.Controller(Agent.ControllerType.Player);
            if (troop.IsHero)
            {
                agentBuildData.Equipment(troop.GetFirstEquipment(agentBuildData.AgentCivilianEquipment).Clone(false));
            }
            else
            {
                var equipmentModifierType = BattleConfig.Instance.BattleTypeConfig.EquipmentModifierType;
                var equipment = Equipment.GetRandomEquipmentElements(troop, equipmentModifierType == EquipmentModifierType.Random,
                    agentBuildData.AgentCivilianEquipment,
                    agentBuildData.AgentEquipmentSeed);
                if (equipmentModifierType == EquipmentModifierType.Average)
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
            if (agent.Character.IsHero)
                agent.SetAgentFlags(agent.GetAgentFlags() | AgentFlag.IsUnique); 
            if (agent.IsAIControlled & isAlarmed)
                agent.SetWatchState(Agent.WatchState.Alarmed);
            if (wieldInitialWeapons)
                agent.WieldInitialWeapons();
            if (!string.IsNullOrEmpty(specialActionSet))
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
