using TaleWorlds.Core;

namespace EnhancedBattleTest.Data.MissionData
{
    public interface IEnhancedBattleTestTroopSupplier : IMissionTroopSupplier
    {
        void OnTroopWounded();
        void OnTroopKilled();
        void OnTroopRouted();
    }
}
