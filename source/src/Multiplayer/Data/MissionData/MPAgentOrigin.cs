using EnhancedBattleTest.Data;
using EnhancedBattleTest.Data.MissionData;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Multiplayer.Data.MissionData
{
    public class MPAgentOrigin : EnhancedBattleTestAgentOrigin
    {
        public override BasicCharacterObject Troop => MPCharacter.Character;
        public override ISpawnableCharacter SpawnableCharacter => MPCharacter;
        public override FormationClass FormationIndex => MPCharacter.FormationIndex;

        public MPSpawnableCharacter MPCharacter { get; }

        public MPAgentOrigin(MPCombatant combatant, MPSpawnableCharacter character, MPTroopSupplier troopSupplier, BattleSideEnum side, int rank = -1, UniqueTroopDescriptor uniqueNo = default)
            : base(combatant, troopSupplier, side, rank, uniqueNo)
        {
            MPCharacter = character;
        }

        public override Agent SpawnTroop(BattleSideEnum side, bool hasFormation, bool spawnWithHorse, bool isReinforcement,
            bool enforceSpawningOnInitialPoint, int formationTroopCount, int formationTroopIndex, bool isAlarmed,
            bool wieldInitialWeapons, bool forceDismounted = false, string specialActionSet = null,
            MatrixFrame? initFrame = null)
        {
            BasicCharacterObject troop = Troop;
            var team = IsUnderPlayersCommand ? Mission.Current.PlayerTeam : Mission.Current.PlayerEnemyTeam;
            MatrixFrame frame = initFrame ?? Mission.Current
                .GetFormationSpawnFrame(team.Side, FormationClass.NumberOfRegularFormations, false).ToGroundMatrixFrame();
            if (MPCharacter.IsPlayer && !forceDismounted)
                spawnWithHorse = true;
            AgentBuildData agentBuildData = new AgentBuildData(this).Team(team).Banner(Banner)
                .ClothingColor1(team.Color).ClothingColor2(team.Color2)
                .NoHorses(!spawnWithHorse).CivilianEquipment(Mission.Current.DoesMissionRequireCivilianEquipment);
            var equipment = Utility.GetNewEquipmentsForPerks(MPCharacter.HeroClass,
                MPCharacter.IsHero,
                MPCharacter.SelectedFirstPerk, MPCharacter.SelectedSecondPerk,
                MPCharacter.IsHero, Seed);
            agentBuildData.Equipment(equipment);
            agentBuildData.IsFemale(MPCharacter.IsFemale);
            if (!MPCharacter.IsPlayer)
                agentBuildData.IsReinforcement(isReinforcement).SpawnOnInitialPoint(enforceSpawningOnInitialPoint);
            if (!hasFormation || MPCharacter.IsPlayer)
                agentBuildData.InitialFrame(frame);
            if (spawnWithHorse)
                agentBuildData.MountKey(MountCreationKey.GetRandomMountKey(
                    troop.Equipment[EquipmentIndex.ArmorItemEndSlot].Item, troop.GetMountKeySeed()));
            if (hasFormation && !MPCharacter.IsPlayer)
            {
                Formation formation = team.GetFormation(MPCharacter.FormationIndex);
                agentBuildData.Formation(formation);
                agentBuildData.FormationTroopCount(formationTroopCount).FormationTroopIndex(formationTroopIndex);
            }
            if (MPCharacter.IsPlayer)
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
    }
}
