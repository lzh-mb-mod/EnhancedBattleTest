using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data
{
    public interface ISpawnableCharacter
    {
        BasicCharacterObject Character { get; }
        bool IsFemale { get; }
        FormationClass FormationIndex { get; }
        bool IsGeneral { get; }
        bool IsPlayer { get; }
    }
}
