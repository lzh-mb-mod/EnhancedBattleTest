using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public interface IEnhancedBattleTestCombatant : IBattleCombatant
    {
        int NumberOfAllMembers { get; } 
        int NumberOfHealthyMembers { get; }

        IEnumerable<BasicCharacterObject> Characters { get; }
    }
}
