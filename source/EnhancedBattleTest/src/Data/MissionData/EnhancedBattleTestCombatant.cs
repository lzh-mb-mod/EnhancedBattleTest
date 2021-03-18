using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.Data.MissionData
{
    public class TroopCountPair
    {
        public int FemaleCount;
        public int MaleCount;

        public TroopCountPair(int femaleCount, int maleCount)
        {
            FemaleCount = femaleCount;
            MaleCount = maleCount;
        }
    }
    public abstract class EnhancedBattleTestCombatant : IEnhancedBattleTestCombatant
    {
        protected EnhancedBattleTestCombatant(TextObject name, BattleSideEnum side, BasicCultureObject basicCulture, Tuple<uint, uint> primaryColorPair, Tuple<uint, uint> alternativeColorPair, Banner banner)
        {
            Name = name;
            Side = side;
            BasicCulture = basicCulture;
            PrimaryColorPair = primaryColorPair;
            AlternativeColorPair = alternativeColorPair;
            Banner = banner;
        }

        public abstract int GetTacticsSkillAmount();

        public TextObject Name { get; }
        public BattleSideEnum Side { get; set; }
        public BasicCultureObject BasicCulture { get; }
        public Tuple<uint, uint> PrimaryColorPair { get; }
        public Tuple<uint, uint> AlternativeColorPair { get; }
        public Banner Banner { get; }
        public int NumberOfAllMembers { get; protected set; }
        public abstract int NumberOfHealthyMembers { get; }
        public abstract IEnumerable<BasicCharacterObject> Characters { get; }
    }
}
