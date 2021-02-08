using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data.MissionData;
using EnhancedBattleTest.SinglePlayer.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
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
        }

        public SPCombatant(PartyBase party, BattleSideEnum side, int tacticLevel, BasicCultureObject culture, Tuple<uint, uint> primaryColorPair, Banner banner)
            : base(GameTexts.FindText("str_ebt_side", side == BattleSideEnum.Attacker ? "Attacker" : "Defender"),
                side, culture, primaryColorPair, new Tuple<uint, uint>(primaryColorPair.Item2, primaryColorPair.Item1), banner)
        {
            Combatant = party;
            _tacticLevel = tacticLevel;
        }

        public static SPCombatant CreateParty(PartyBase party, BattleSideEnum side, BasicCultureObject culture,
            TeamConfig teamConfig, bool isPlayerTeam)
        {
            party.Owner = null;
            if (teamConfig.HasGeneral)
            {
                var characterObject = (teamConfig.General as SPCharacterConfig)?.ActualCharacterObject;
                if (characterObject?.IsHero ?? false)
                    party.Owner = characterObject.HeroObject;
            }
            Utility.FillPartyMembers(party, side, culture, teamConfig, isPlayerTeam);
            bool isAttacker = side == BattleSideEnum.Attacker;
            var combatant = new SPCombatant(party, side, teamConfig.TacticLevel, culture,
                new Tuple<uint, uint>(teamConfig.Color1, teamConfig.Color2),
                teamConfig.Banner);
            if (teamConfig.HasGeneral)
            {
                if (teamConfig.General is SPCharacterConfig general)
                {
                    combatant.AddCharacter(
                        new SPSpawnableCharacter(general, (int)general.CharacterObject.DefaultFormationGroup,
                            general.FemaleRatio > 0.5, isPlayerTeam), 1);
                }
            }
            for (int i = 0; i < teamConfig.Troops.Troops.Length; ++i)
            {
                var troopConfig = teamConfig.Troops.Troops[i];
                if (troopConfig.Character is SPCharacterConfig spCharacter)
                {
                    var femaleCount = (int)(troopConfig.Number * spCharacter.FemaleRatio + 0.49);
                    var maleCount = troopConfig.Number - femaleCount;
                    combatant.AddCharacter(new SPSpawnableCharacter(spCharacter, i, true), femaleCount);
                    combatant.AddCharacter(new SPSpawnableCharacter(spCharacter, i, false), maleCount);
                }
            }

            return combatant;
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
