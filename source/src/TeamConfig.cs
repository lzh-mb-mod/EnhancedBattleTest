using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public class TeamConfig
    {
        public TroopGroupConfig Troops = new TroopGroupConfig();
        public CharacterConfig General = new CharacterConfig();
        public bool HasGeneral;
    }
}
