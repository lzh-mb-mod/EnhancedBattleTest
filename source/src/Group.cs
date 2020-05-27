using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class GroupInfo
    {

        public string StringId { get; }
        public TextObject Name { get; }

        public GroupInfo(string stringId, TextObject name)
        {
            StringId = stringId;
            Name = name;
        }
    }

    public abstract class Group
    {
        public abstract GroupInfo Info { get; }

        public abstract Dictionary<string, Character> CharactersInGroup { get; }
    }
}
