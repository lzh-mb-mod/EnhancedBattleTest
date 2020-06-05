using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public interface ISpawnableCharacter
    {
        BasicCharacterObject Character { get; }
        bool IsFemale { get; }
        FormationClass FormationIndex { get; }
        bool IsPlayer { get; }
    }
}
