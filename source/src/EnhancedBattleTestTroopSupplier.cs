using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public interface IEnhancedBattleTestTroopSupplier : IMissionTroopSupplier
    {
        void OnTroopWounded();
        void OnTroopKilled();
        void OnTroopRouted();
    }
}
