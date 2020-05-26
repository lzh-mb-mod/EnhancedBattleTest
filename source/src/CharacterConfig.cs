using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest
{
    public class CharacterConfig : ViewModel
    {
        public string CharacterId;

        public CharacterConfig()
        {
            if (EnhancedBattleTestSubModule.IsMultiplayer)
            {
                CharacterId = Game.Current.ObjectManager.GetObjectTypeList<BasicCharacterObject>().First().StringId;
            }
        }
    }
}
