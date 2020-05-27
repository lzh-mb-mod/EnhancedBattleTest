using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class MPGroup : Group
    {
        public override GroupInfo Info { get; }
        public override Dictionary<string, Character> CharactersInGroup { get; }

        public MultiplayerClassDivisions.MPHeroClassGroup MpHeroClassGroup;

        public MPGroup(MultiplayerClassDivisions.MPHeroClassGroup group)
        {
            MpHeroClassGroup = group;
            Info = new GroupInfo(MpHeroClassGroup.StringId, MpHeroClassGroup.Name);
            CharactersInGroup = new Dictionary<string, Character>();
        }
    }
}
