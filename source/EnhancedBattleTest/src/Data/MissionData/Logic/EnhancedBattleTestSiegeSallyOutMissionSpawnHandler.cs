using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData.Logic
{
    class EnhancedBattleTestSiegeSallyOutMissionSpawnHandler : MissionLogic
    {
        private const int MinNumberOfReinforcementTroops = 5;
        private const float BesiegerReinforcementTimerInSeconds = 10f;
        private const float BesiegerReinforcementNumberPercentage = 0.07f;
        private const float BesiegerInitialSpawnNumberPercentage = 0.1f;
        private MissionAgentSpawnLogic _missionAgentSpawnLogic;
        private readonly IEnhancedBattleTestCombatant[] _battleCombatants;
        private float _dtSum;

        public EnhancedBattleTestSiegeSallyOutMissionSpawnHandler(
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
            int defenderInitialSpawn = MathF.Floor((float)ofHealthyMembers1);
            int attackerInitialSpawn = MathF.Floor(ofHealthyMembers2 * 0.1f);
            _missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Defender, true);
            _missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Attacker, true);
            //_missionAgentSpawnLogic.InitWithSinglePhase(ofHealthyMembers1, ofHealthyMembers2, defenderInitialSpawn, attackerInitialSpawn, false, false);
            //_missionAgentSpawnLogic.ReserveReinforcement(BattleSideEnum.Attacker, ofHealthyMembers2 - attackerInitialSpawn);
        }

        public override void OnMissionTick(float dt)
        {
            if (!CheckTimer(dt))
                return;
            //_missionAgentSpawnLogic.CheckReinforcement(Math.Max(MathF.Floor(_battleCombatants[1].NumberOfHealthyMembers * 0.07f), 5));
        }

        private bool CheckTimer(float dt)
        {
            _dtSum += dt;
            if (_dtSum < 10.0)
                return false;
            _dtSum = 0.0f;
            return true;
        }

    }
}
