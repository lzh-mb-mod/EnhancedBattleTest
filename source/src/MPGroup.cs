using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class MPGroup : Group
    {
        public Dictionary<string, Character> CharactersInGroup { get; }

        public MultiplayerClassDivisions.MPHeroClassGroup MpHeroClassGroup;

        public MPGroup(MultiplayerClassDivisions.MPHeroClassGroup group, FormationClass formationClass)
            : base(new GroupInfo(group.StringId, group.Name, formationClass))
        {
            MpHeroClassGroup = group;
            CharactersInGroup = new Dictionary<string, Character>();
        }
    }
}
