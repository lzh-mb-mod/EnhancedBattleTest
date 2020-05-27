using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class SPGroup : Group
    {
        public FormationClass FormationClass;

        public override GroupInfo Info { get; }
        public override Dictionary<string, Character> CharactersInGroup { get; }

        public SPGroup(FormationClass formationClass)
        {
            FormationClass = formationClass;
            Info = new GroupInfo(FormationClass.ToString(),
                GameTexts.FindText("str_troop_group_name", ((int) FormationClass).ToString()));
            CharactersInGroup = new Dictionary<string, Character>();
        }
    }
}
