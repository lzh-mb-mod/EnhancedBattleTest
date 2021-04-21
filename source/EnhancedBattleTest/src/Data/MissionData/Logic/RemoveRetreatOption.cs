using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData.Logic
{
    public class RemoveRetreatOption : MissionLogic
    {
        public override void AfterStart()
        {
            base.AfterStart();

            if (Mission.MissionTeamAIType != Mission.MissionTeamAITypeEnum.FieldBattle)
                return;

            foreach (var team in Mission.Teams)
            {
                team.RemoveTacticOption(typeof(TacticCoordinatedRetreat));
            }
        }
    }
}
