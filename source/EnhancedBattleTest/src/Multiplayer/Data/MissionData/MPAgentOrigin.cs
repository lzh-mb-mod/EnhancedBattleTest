using EnhancedBattleTest.Data;
using EnhancedBattleTest.Data.MissionData;
using TaleWorlds.Core;
using TaleWorlds.Engine;
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

        public override Agent SpawnTroop(BattleSideEnum side, bool hasFormation, bool spawnWithHorse,
            bool isReinforcement,
            bool enforceSpawningOnInitialPoint, int formationTroopCount, int formationTroopIndex, bool isAlarmed,
            bool wieldInitialWeapons, bool forceDismounted,
            Vec3? initialPosition,
            Vec2? initialDirection, string specialActionSet = null)
        {
            BasicCharacterObject troop = Troop;
            var team = IsUnderPlayersCommand ? Mission.Current.PlayerTeam : Mission.Current.PlayerEnemyTeam;
            Mission.Current
                .GetFormationSpawnFrame(team.Side, FormationClass.NumberOfRegularFormations, isReinforcement,
                    out var formationPosition,
                    out var formationDirection);
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
            //if (!MPCharacter.IsPlayer)
                //agentBuildData.IsReinforcement(isReinforcement).SpawnOnInitialPoint(enforceSpawningOnInitialPoint);
            if (initialPosition.HasValue && initialDirection.HasValue)
            {
                Vec3 vec3 = initialPosition.Value;
                agentBuildData.InitialPosition(in vec3);
                Vec2 vec2 = initialDirection.Value;
                agentBuildData.InitialDirection(in vec2);
            }
            else if (SpawnableCharacter.IsGeneral && Mission.Current.GetFormationSpawnClass(team.Side, FormationClass.NumberOfRegularFormations, isReinforcement) == FormationClass.NumberOfRegularFormations)
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
                Formation formation = team.GetFormation(MPCharacter.FormationIndex);
                agentBuildData.Formation(formation);
                agentBuildData.FormationTroopCount(formationTroopCount).FormationTroopIndex(formationTroopIndex);
            }
            if (MPCharacter.IsPlayer)
                agentBuildData.Controller(Agent.ControllerType.Player);
            Agent agent = Mission.Current.SpawnAgent(agentBuildData, false, formationTroopCount);
            if (agent.IsAIControlled & isAlarmed)
                agent.SetWatchState(Agent.WatchState.Alarmed);
            if (wieldInitialWeapons)
                agent.WieldInitialWeapons();
            //if (!string.IsNullOrEmpty(specialActionSet))
            //{
            //    AnimationSystemData animationSystemData =
            //        agentBuildData.AgentMonster.FillAnimationSystemData(MBGlobals.GetActionSet(specialActionSet),
            //            agent.Character.GetStepSize(), false);
            //    AgentVisualsNativeData agentVisualsNativeData =
            //        agentBuildData.AgentMonster.FillAgentVisualsNativeData();
            //    agentBuildData.AgentMonster.FillAgentVisualsNativeData();
            //    agent.SetActionSet(ref agentVisualsNativeData, ref animationSystemData);
            //}
            return agent;
        }
    }
}
