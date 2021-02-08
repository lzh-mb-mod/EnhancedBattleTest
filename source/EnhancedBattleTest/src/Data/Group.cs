using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.Data
{
    public class GroupInfo
    {
        public string StringId { get; }
        public TextObject Name { get; }

        public FormationClass FormationClass { get; }

        public GroupInfo(string stringId, TextObject name, FormationClass formationClass)
        {
            StringId = stringId;
            Name = name;
            FormationClass = formationClass;
        }
    }

    public class Group
    {
        public GroupInfo Info { get; }

        public Group(GroupInfo info)
        {
            Info = info;
        }
    }
}
