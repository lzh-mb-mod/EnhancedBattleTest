using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class MPGroup : Group
    {
        public Dictionary<string, Character> CharactersInGroup { get; }

        public MultiplayerClassDivisions.MPHeroClassGroup MpHeroClassGroup;

        public MPGroup(MultiplayerClassDivisions.MPHeroClassGroup group, FormationClass formationClass)
            :base(new GroupInfo(group.StringId, group.Name, formationClass))
        {
            MpHeroClassGroup = group;
            CharactersInGroup = new Dictionary<string, Character>();
        }
    }
}
