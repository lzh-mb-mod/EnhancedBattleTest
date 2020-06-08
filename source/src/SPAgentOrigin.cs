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
        public IBattleCombatant CultureCombatant { get; }

        public override Banner Banner => CultureCombatant.Banner;

        public override uint FactionColor => CultureCombatant.PrimaryColorPair.Item1;

        public override uint FactionColor2 => CultureCombatant.PrimaryColorPair.Item2;

        public SPAgentOrigin(SPCombatant combatant, SPSpawnableCharacter character, IEnhancedBattleTestTroopSupplier troopSupplier, BattleSideEnum side, int rank = -1, UniqueTroopDescriptor uniqueNo = default)
            : base(combatant.Combatant, troopSupplier, side, rank, uniqueNo)
        {
            SPCharacter = character;
            CultureCombatant = combatant;
            PartyAgentOrigin = new PartyAgentOrigin(combatant.Combatant, character.Character, rank,
                uniqueNo);
        }
    }
}
