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
    public class SPCharacter : Character
    {

        public CharacterObject CharacterObject;
        public override string StringId => CharacterObject.StringId;
        public override TextObject Name => CharacterObject.Name;
        public override BasicCultureObject Culture => CharacterObject.Culture;
        public override GroupInfo GroupInfo { get; }

        public SPCharacter(CharacterObject characterObject, GroupInfo groupInfo)
        {
            CharacterObject = characterObject;
            GroupInfo = groupInfo;
        }
    }
}
