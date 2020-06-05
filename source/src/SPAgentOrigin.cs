using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public class SPAgentOrigin : EnhancedBattleTestAgentOrigin
    {
        public override BasicCharacterObject Troop => SPCharacter.Character;

        public override FormationClass FormationIndex => SPCharacter.FormationIndex;

        public SPSpawnableCharacter SPCharacter { get; }

        public SPAgentOrigin(SPCombatant combatant, SPSpawnableCharacter character, IEnhancedBattleTestTroopSupplier troopSupplier, BattleSideEnum side, int rank = -1, UniqueTroopDescriptor uniqueNo = default)
            : base(combatant.Combatant, troopSupplier, side, rank, uniqueNo)
        {
            SPCharacter = character;
        }
    }
}
