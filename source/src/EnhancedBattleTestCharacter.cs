using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    class EnhancedBattleTestCharacter : BasicCharacterObject
    {
        private bool _isHero;
        public override bool IsHero => _isHero;

        public void SetIsHero(bool isHero)
        {
            _isHero = isHero;
        }
    }
}
