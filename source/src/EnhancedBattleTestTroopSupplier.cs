using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
