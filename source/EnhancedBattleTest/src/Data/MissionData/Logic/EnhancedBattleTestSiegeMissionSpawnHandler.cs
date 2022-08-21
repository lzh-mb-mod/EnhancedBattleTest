using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData.Logic
{
    class EnhancedBattleTestSiegeMissionSpawnHandler : MissionLogic
    {
        private MissionAgentSpawnLogic _missionAgentSpawnLogic;
        private readonly IEnhancedBattleTestCombatant[] _battleCombatants;

        public EnhancedBattleTestSiegeMissionSpawnHandler(
            IEnhancedBattleTestCombatant defenderBattleCombatant,
            IEnhancedBattleTestCombatant attackerBattleCombatant)
        {
            _battleCombatants = new IEnhancedBattleTestCombatant[2]
            {
                defenderBattleCombatant,
                attackerBattleCombatant
            };
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            _missionAgentSpawnLogic = Mission.GetMissionBehavior<MissionAgentSpawnLogic>();
        }

        public override void AfterStart()
        {
            int ofHealthyMembers1 = _battleCombatants[0].NumberOfHealthyMembers;
            int ofHealthyMembers2 = _battleCombatants[1].NumberOfHealthyMembers;
            int defenderInitialSpawn = ofHealthyMembers1;
            int attackerInitialSpawn = ofHealthyMembers2;
            _missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Defender, false);
            _missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Attacker, false);
            //_missionAgentSpawnLogic.InitWithSinglePhase(ofHealthyMembers1, ofHealthyMembers2, defenderInitialSpawn, attackerInitialSpawn, false, false);
        }
    }
}

