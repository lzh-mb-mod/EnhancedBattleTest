using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{

    public class SPGroup : Group
    {
        public FormationClass FormationClass;

        public Dictionary<Occupation, CharactersInOccupation> OccupationsInGroup { get; }

        public SPGroup(FormationClass formationClass)
            : base(
                new GroupInfo(formationClass.ToString(),
                GameTexts.FindText("str_troop_group_name", ((int) formationClass).ToString()),
                formationClass))
        {
            FormationClass = formationClass;
            OccupationsInGroup = new Dictionary<Occupation, CharactersInOccupation>();
        }
    }
}
