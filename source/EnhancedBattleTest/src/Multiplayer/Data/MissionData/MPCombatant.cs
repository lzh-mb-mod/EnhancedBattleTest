using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data.MissionData;
using EnhancedBattleTest.Multiplayer.Config;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest.Multiplayer.Data.MissionData
{
    public class MPCombatant : EnhancedBattleTestCombatant
    {
        private readonly List<MPSpawnableCharacter> _characters = new List<MPSpawnableCharacter>();
        private int _tacticLevel;
        public IEnumerable<MPSpawnableCharacter> MPCharacters => _characters.AsReadOnly();
        public override IEnumerable<BasicCharacterObject> Characters => MPCharacters.Select(character => character.Character);

        public override int NumberOfHealthyMembers => _characters.Count;

        public MPCombatant(BattleSideEnum side, int tacticLevel, BasicCultureObject culture,
            Tuple<uint, uint> primaryColorPair, Tuple<uint, uint> alternativeColorPair, Banner banner)
            : base(GameTexts.FindText("str_ebt_side", side == BattleSideEnum.Attacker ? "Attacker" : "Defender"),
                side, culture, primaryColorPair, alternativeColorPair, banner)
        {
            _tacticLevel = tacticLevel;
        }

        public MPCombatant(BattleSideEnum side, int tacticLevel, BasicCultureObject culture, Tuple<uint, uint> primaryColorPair, Banner banner)
            : base(GameTexts.FindText("str_ebt_side", side == BattleSideEnum.Attacker ? "Attacker" : "Defender"),
                side, culture, primaryColorPair, new Tuple<uint, uint>(primaryColorPair.Item2, primaryColorPair.Item1), banner)
        {
            _tacticLevel = tacticLevel;
        }

        public static MPCombatant CreateParty(BattleSideEnum side, BasicCultureObject culture,
            TeamConfig teamConfig, bool isPlayerTeam)
        {

            return null;
        }

        public override int GetTacticsSkillAmount()
        {
            return _tacticLevel;
        }

        public void AddCharacter(MPSpawnableCharacter character, int number)
        {
            for (int index = 0; index < number; ++index)
                this._characters.Add(character);
            this.NumberOfAllMembers += number;
        }

        public void KillCharacter(MPSpawnableCharacter character)
        {
            this._characters.Remove(character);
        }
    }
}
