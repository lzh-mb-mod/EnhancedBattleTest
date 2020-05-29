using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public interface ISpawnableCharacter
    {
        BasicCharacterObject Character { get; }
        bool IsFemale { get; }
        FormationClass FormationIndex { get; }
        bool IsPlayer { get; }
    }
}
