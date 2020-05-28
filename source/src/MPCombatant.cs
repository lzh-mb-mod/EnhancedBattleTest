using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class MPCombatant : IEnhancedBattleTestCombatant
    {
        private readonly List<MPSpawnableCharacter> _characters = new List<MPSpawnableCharacter>();
        public IEnumerable<MPSpawnableCharacter> MPCharacters => _characters.AsReadOnly();
        public IEnumerable<BasicCharacterObject> Characters => MPCharacters.Select(character => character.Character);

        public int NumberOfAllMembers { get; private set; }

        public int NumberOfHealthyMembers => this._characters.Count;

        public TextObject Name { get; }
        public BattleSideEnum Side { get; set; }
        public BasicCultureObject BasicCulture { get; }
        public Tuple<uint, uint> PrimaryColorPair { get; }
        public Tuple<uint, uint> AlternativeColorPair { get; }
        public Banner Banner { get; }

        public MPCombatant(BattleSideEnum side, BasicCultureObject culture, Tuple<uint, uint> primaryColorPair, Tuple<uint, uint> alternativeColorPair, Banner banner)
        {
            Name = GameTexts.FindText("str_ebt_side", side == BattleSideEnum.Attacker ? "Attacker" : "Defender");
            Side = side;
            BasicCulture = culture;
            PrimaryColorPair = primaryColorPair;
            AlternativeColorPair = alternativeColorPair;
            Banner = banner;
        }

        public MPCombatant(BattleSideEnum side, BasicCultureObject culture, Tuple<uint, uint> primaryColorPair, Banner banner)
        {
            Name = GameTexts.FindText("str_ebt_side", side == BattleSideEnum.Attacker ? "Attacker" : "Defender");
            Side = side;
            BasicCulture = culture;
            PrimaryColorPair = primaryColorPair;
            AlternativeColorPair = new Tuple<uint, uint>(PrimaryColorPair.Item2, PrimaryColorPair.Item1);
            Banner = banner;
        }

        public static MPCombatant CreateParty(BattleSideEnum side, BasicCultureObject culture,
            TeamConfig teamConfig, bool isPlayerTeam)
        {
            bool isAttacker = side == BattleSideEnum.Attacker;
            var combatant = new MPCombatant(side, culture,
                new Tuple<uint, uint>(Utility.BackgroundColor(culture, isAttacker),
                    Utility.ForegroundColor(culture, isAttacker)),
                Utility.BannerFor(culture, isAttacker));
            if (teamConfig.HasGeneral)
            {
                if (teamConfig.General is MPCharacterConfig general)
                    combatant.AddCharacter(
                        new MPSpawnableCharacter(general, (int)general.CharacterObject.DefaultFormationGroup,
                            isPlayerTeam), 1);
            }
            for (int i = 0; i < teamConfig.Troops.Troops.Length; ++i)
            {
                var troopConfig = teamConfig.Troops.Troops[i];
                combatant.AddCharacter(new MPSpawnableCharacter((MPCharacterConfig)troopConfig.Character, i),
                    troopConfig.Number);
            }

            return combatant;
        }

        public int GetTacticsSkillAmount()
        {
            return 0;
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
