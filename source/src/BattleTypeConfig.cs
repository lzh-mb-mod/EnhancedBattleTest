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
    public enum EquipmentModifierType
    {
        Random,
        Average,
        None,
    }
    public class BattleTypeConfig
    {
        public BattleType BattleType;
        public PlayerType PlayerType;
        public BattleSideEnum PlayerSide = BattleSideEnum.Attacker;
        public EquipmentModifierType EquipmentModifierType = EquipmentModifierType.Random;
    }
}
