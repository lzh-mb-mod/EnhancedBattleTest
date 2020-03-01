using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class EnhancedCustomBattleMissionSpawnHandler : MissionLogic
    {
        private MissionAgentSpawnLogic _missionAgentSpawnLogic;
        private EnhancedCustomBattleCombatant _defenderParty;
        private EnhancedCustomBattleCombatant _attackerParty;

        public EnhancedCustomBattleMissionSpawnHandler()
        {
        }

        public EnhancedCustomBattleMissionSpawnHandler(
            EnhancedCustomBattleCombatant defenderParty,
            EnhancedCustomBattleCombatant attackerParty)
        {
            this._defenderParty = defenderParty;
            this._attackerParty = attackerParty;
        }

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
            this._missionAgentSpawnLogic = this.Mission.GetMissionBehaviour<MissionAgentSpawnLogic>();
        }

        public override void AfterStart()
        {
            int ofHealthyMembers1 = this._defenderParty.NumberOfHealthyMembers;
            int ofHealthyMembers2 = this._attackerParty.NumberOfHealthyMembers;
            int defenderInitialSpawn = ofHealthyMembers1;
            int attackerInitialSpawn = ofHealthyMembers2;
            this._missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Defender, true);
            this._missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Attacker, true);
            this._missionAgentSpawnLogic.InitWithSinglePhase(ofHealthyMembers1, ofHealthyMembers2, defenderInitialSpawn, attackerInitialSpawn, true, true, 1f);
        }
    }
}
