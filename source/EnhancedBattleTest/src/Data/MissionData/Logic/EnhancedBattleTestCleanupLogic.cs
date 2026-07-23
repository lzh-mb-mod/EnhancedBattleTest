using EnhancedBattleTest.Data;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData.Logic
{
    public sealed class EnhancedBattleTestCleanupLogic : MissionLogic
    {
        public override void OnRemoveBehavior()
        {
            EnhancedBattleTestPartyController.Cleanup();
            base.OnRemoveBehavior();
        }
    }
}
