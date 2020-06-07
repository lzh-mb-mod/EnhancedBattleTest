using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public class SPAgentOrigin : EnhancedBattleTestAgentOrigin
    {
        public override BasicCharacterObject Troop => SPCharacter.Character;

        public override FormationClass FormationIndex => SPCharacter.FormationIndex;

        public SPSpawnableCharacter SPCharacter { get; }

        public PartyAgentOrigin PartyAgentOrigin;

        public SPAgentOrigin(SPCombatant combatant, SPSpawnableCharacter character, IEnhancedBattleTestTroopSupplier troopSupplier, BattleSideEnum side, int rank = -1, UniqueTroopDescriptor uniqueNo = default)
            : base(combatant, troopSupplier, side, rank, uniqueNo)
        {
            SPCharacter = character;
            PartyAgentOrigin = new PartyAgentOrigin(combatant.Combatant, character.Character, rank,
                uniqueNo);
        }
    }
}
