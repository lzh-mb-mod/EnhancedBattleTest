using TaleWorlds.Core;

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
