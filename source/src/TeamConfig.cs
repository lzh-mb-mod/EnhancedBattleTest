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
        public TroopGroupConfig Troops;
        public CharacterConfig General;
        public bool HasGeneral;

        public TeamConfig()
        { }

        public TeamConfig(bool isMultiplayer)
        {
            Troops = new TroopGroupConfig(isMultiplayer);
            General = CharacterConfig.Create(isMultiplayer);
        }
    }
}
