using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public class SPAgentOrigin : EnhancedBattleTestAgentOrigin
    {
        public override BasicCharacterObject Troop => SPCharacter.Character;

        public SPSpawnableCharacter SPCharacter { get; }

        public SPAgentOrigin(SPCombatant combatant, SPSpawnableCharacter character, IEnhancedBattleTestTroopSupplier troopSupplier, BattleSideEnum side, int rank = -1, UniqueTroopDescriptor uniqueNo = default)
            : base(combatant, troopSupplier, side, rank, uniqueNo)
        {
            SPCharacter = character;
        }
    }
}
