using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class SPCombatant : EnhancedBattleTestCombatant
    {
        private readonly List<SPSpawnableCharacter> _characters = new List<SPSpawnableCharacter>();

        public IEnumerable<SPSpawnableCharacter> SPCharacters => _characters.AsReadOnly();

        public override int NumberOfHealthyMembers => _characters.Count;

        public override IEnumerable<BasicCharacterObject> Characters =>
            SPCharacters.Select(character => character.Character);

        public SPCombatant(TextObject name, BattleSideEnum side, BasicCultureObject basicCulture, Tuple<uint, uint> primaryColorPair, Tuple<uint, uint> alternativeColorPair, Banner banner)
            : base(name, side, basicCulture, primaryColorPair, alternativeColorPair, banner)
        { }

        public SPCombatant(BattleSideEnum side, BasicCultureObject culture, Tuple<uint, uint> primaryColorPair, Banner banner)
            : base(GameTexts.FindText("str_ebt_side", side == BattleSideEnum.Attacker ? "Attacker" : "Defender"),
                side, culture, primaryColorPair, new Tuple<uint, uint>(primaryColorPair.Item2, primaryColorPair.Item1), banner)
        { }

        public static SPCombatant CreateParty(BattleSideEnum side, BasicCultureObject culture,
            TeamConfig teamConfig, bool isPlayerTeam)
        {
            bool isAttacker = side == BattleSideEnum.Attacker;
            var combatant = new SPCombatant(side, culture,
                new Tuple<uint, uint>(teamConfig.Color1, teamConfig.Color2),
                teamConfig.Banner);
            if (teamConfig.HasGeneral)
            {
                if (teamConfig.General is SPCharacterConfig general)
                    combatant.AddCharacter(
                        new SPSpawnableCharacter(general, (int)general.CharacterObject.DefaultFormationGroup,
                            general.FemaleRatio > 0.5, isPlayerTeam), 1);
            }
            for (int i = 0; i < teamConfig.Troops.Troops.Length; ++i)
            {
                var troopConfig = teamConfig.Troops.Troops[i];
                var spCharacter = troopConfig.Character as SPCharacterConfig;
                if (spCharacter == null)
                    continue;
                var femaleCount = (int)(troopConfig.Number * spCharacter.FemaleRatio);
                var maleCount = troopConfig.Number - femaleCount;
                combatant.AddCharacter(new SPSpawnableCharacter(spCharacter, i, true),
                    femaleCount);
                combatant.AddCharacter(new SPSpawnableCharacter(spCharacter, i, false), maleCount);
            }

            return combatant;
        }

        public override int GetTacticsSkillAmount()
        {
            return 0;
        }

        public void AddCharacter(SPSpawnableCharacter character, int number)
        {
            for (int index = 0; index < number; ++index)
                this._characters.Add(character);
            this.NumberOfAllMembers += number;
        }

        public void KillCharacter(SPSpawnableCharacter character)
        {
            this._characters.Remove(character);
        }
    }
}
