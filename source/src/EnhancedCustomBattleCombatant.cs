using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class EnhancedCustomBattleCombatant : IBattleCombatant
    {
        private List<BasicCharacterObject> _characters;

        public TextObject Name { get; private set; }

        public BattleSideEnum Side { get; set; }

        public BasicCultureObject Culture { get; private set; }

        public Tuple<uint, uint> PrimaryColorPair
        {
            get
            {
                return new Tuple<uint, uint>(this.Banner.GetPrimaryColor(), this.Banner.GetPrimaryColor());
            }
        }

        public Tuple<uint, uint> AlternativeColorPair
        {
            get
            {
                return new Tuple<uint, uint>(this.Banner.GetPrimaryColor(), this.Banner.GetPrimaryColor());
            }
        }

        public Banner Banner { get; private set; }

        public int GetTacticsSkillAmount()
        {
            return this._characters.Any<BasicCharacterObject>() ? this._characters.Max<BasicCharacterObject>((Func<BasicCharacterObject, int>)(h => h.GetSkillValue(DefaultSkills.Tactics))) : 0;
        }

        public IEnumerable<BasicCharacterObject> Characters
        {
            get
            {
                return (IEnumerable<BasicCharacterObject>)this._characters.AsReadOnly();
            }
        }

        public int NumberOfAllMembers { get; private set; }

        public int NumberOfHealthyMembers
        {
            get
            {
                return this._characters.Count;
            }
        }

        public EnhancedCustomBattleCombatant(TextObject name, BasicCultureObject culture, Banner banner)
        {
            this.Name = name;
            this.Culture = culture;
            this.Banner = banner;
            this._characters = new List<BasicCharacterObject>();
        }

        public void AddCharacter(ClassInfo info, bool isHero, FormationClass formationClass)
        {
            for (int index = 0; index < info.troopCount; ++index)
            {
                BasicCharacterObject character = Utility.ApplyPerks(info, isHero);
                character.CurrentFormationClass = formationClass;
                this._characters.Add(character);
            }
            this.NumberOfAllMembers += info.troopCount;
        }

        public void KillCharacter(BasicCharacterObject character)
        {
            this._characters.Remove(character);
        }
    }
}
