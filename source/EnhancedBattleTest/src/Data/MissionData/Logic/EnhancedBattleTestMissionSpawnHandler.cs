using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData.Logic
{
    public sealed class EnhancedBattleTestMissionSpawnHandler : MissionLogic
    {
        private readonly PartyBase _defenderParty;
        private readonly PartyBase _attackerParty;
        private MissionAgentSpawnLogic _missionAgentSpawnLogic;

        public EnhancedBattleTestMissionSpawnHandler(
            PartyBase defenderParty,
            PartyBase attackerParty)
        {
            _defenderParty = defenderParty;
            _attackerParty = attackerParty;
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            _missionAgentSpawnLogic =
                Mission.GetMissionBehavior<MissionAgentSpawnLogic>();
        }

        public override void AfterStart()
        {
            int defenderCount = _defenderParty.NumberOfHealthyMembers;
            int attackerCount = _attackerParty.NumberOfHealthyMembers;
            _missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Defender, true);
            _missionAgentSpawnLogic.SetSpawnHorses(BattleSideEnum.Attacker, true);
            MissionSpawnSettings spawnSettings = MissionSpawnSettings.CreateDefaultSpawnSettings();
            _missionAgentSpawnLogic.InitWithSinglePhase(
                defenderCount,
                attackerCount,
                defenderCount,
                attackerCount,
                true,
                true,
                in spawnSettings);
        }
    }
}
