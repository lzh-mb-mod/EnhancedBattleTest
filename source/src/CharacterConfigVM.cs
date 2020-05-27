using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace EnhancedBattleTest
{
    public abstract class CharacterConfigVM : ViewModel
    {
        public abstract void SetConfig(CharacterConfig config);
        public abstract void SelectedCharacterChanged(Character character);

        public static CharacterConfigVM Create(bool isMultiplayer)
        {
            return isMultiplayer ? (CharacterConfigVM)new MPCharacterConfigVM() : new SPCharacterConfigVM();
        }
    }
}
