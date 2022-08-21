using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData.Logic
{
    public class EnhancedBattleTestMissionSpawnHandler : MissionLogic
    {
        private MissionAgentSpawnLogic _missionAgentSpawnLogic;
        private readonly IEnhancedBattleTestCombatant _defenderParty;
        private readonly IEnhancedBattleTestCombatant _attackerParty;

        public EnhancedBattleTestMissionSpawnHandler()
        {
        }

        public EnhancedBattleTestMissionSpawnHandler(
            IEnhancedBattleTestCombatant defenderParty,
            IEnhancedBattleTestCombatant attackerParty)
        {
            _defenderParty = defenderParty;
            _attackerParty = attackerParty;
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            _missionAgentSpawnLogic = Mission.GetMissionBehavior<MissionAgentSpawnLogic>();
        }

        public override void AfterStart()
        {
            int ofHealthyMembers1 = _defenderParty.NumberOfHealthyMembers;
            int ofHealthyMembers2 = _attackerParty.NumberOfHealthyMembers;
            int defenderInitialSpawn = ofHealthyMembers1;
            int attackerInitialSpawn = ofHealthyMembers2;
            _missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Defender, true);
            _missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Attacker, true);
            //_missionAgentSpawnLogic.InitWithSinglePhase(ofHealthyMembers1, ofHealthyMembers2, defenderInitialSpawn, attackerInitialSpawn, true, true);
        }
    }
}
