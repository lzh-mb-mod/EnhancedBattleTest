using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public enum BattleType
    {
        Field,
        Siege,
        Village,
    }

    public enum PlayerType
    {
        Commander,
        Sergeant,
    }
    public class BattleTypeConfig
    {
        public BattleType BattleType;
        public PlayerType PlayerType;
        public BattleSideEnum PlayerSide = BattleSideEnum.Attacker;
    }
}
