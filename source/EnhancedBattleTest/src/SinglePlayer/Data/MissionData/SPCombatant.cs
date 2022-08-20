using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data.MissionData;
using EnhancedBattleTest.SinglePlayer.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.SinglePlayer.Data.MissionData
{
    public class SPCombatant : EnhancedBattleTestCombatant
    {
        public PartyBase Combatant { get; }
        private readonly List<SPSpawnableCharacter> _characters = new List<SPSpawnableCharacter>();
        private readonly int _tacticLevel;

        public IEnumerable<SPSpawnableCharacter> SPCharacters => _characters.AsReadOnly();

        public override int NumberOfHealthyMembers => _characters.Count;

        public override IEnumerable<BasicCharacterObject> Characters =>
            SPCharacters.Select(character => character.Character);

        public SPCombatant(PartyBase party, TextObject name, int tacticLevel, BattleSideEnum side, BasicCultureObject basicCulture,
            Tuple<uint, uint> primaryColorPair, Tuple<uint, uint> alternativeColorPair, Banner banner)
            : base(name, side, basicCulture, primaryColorPair, alternativeColorPair, banner)
        {
            Combatant = party;
            _tacticLevel = tacticLevel;
            General = party.General;
        }

        public SPCombatant(PartyBase party, BattleSideEnum side, int tacticLevel, BasicCultureObject culture, Tuple<uint, uint> primaryColorPair, Banner banner)
            : base(GameTexts.FindText("str_ebt_side", side == BattleSideEnum.Attacker ? "Attacker" : "Defender"),
                side, culture, primaryColorPair, new Tuple<uint, uint>(primaryColorPair.Item2, primaryColorPair.Item1), banner)
        {
            Combatant = party;
            _tacticLevel = tacticLevel;
            General = party.General;
        }

        public static SPCombatant CreateParty(PartyBase party, BattleSideEnum side, BasicCultureObject culture,
            TeamConfig teamConfig, bool isPlayerTeam)
        {
            return null;
        }

        public override int GetTacticsSkillAmount()
        {
            return _tacticLevel;
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
