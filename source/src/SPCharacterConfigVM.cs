using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedBattleTest
{
    public class SPCharacterConfigVM : CharacterConfigVM
    {

        public bool IsMultiplayer => false;
        public bool IsSinglePlayer => true;


        public override void SetConfig(CharacterConfig config)
        {
            throw new NotImplementedException();
        }

        public override void SelectedCharacterChanged(Character character)
        {
            throw new NotImplementedException();
        }
    }
}
