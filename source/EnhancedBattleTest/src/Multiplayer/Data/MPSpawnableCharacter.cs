using EnhancedBattleTest.Data;
using EnhancedBattleTest.Multiplayer.Config;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Multiplayer.Data
{
    public class MPSpawnableCharacter : ISpawnableCharacter
    {
        public MultiplayerClassDivisions.MPHeroClass HeroClass { get; }
        public int SelectedFirstPerk;
        public int SelectedSecondPerk;
        public bool IsHero;
        public bool IsFemale { get; }
        public FormationClass FormationIndex { get; }

        public bool IsGeneral { get; }

        public bool IsPlayer { get; }

        public BasicCharacterObject Character => IsHero ? HeroClass.HeroCharacter : HeroClass.TroopCharacter;
        public MPSpawnableCharacter(MPCharacterConfig config, int formationIndex, bool isFemale, bool isGeneral = false, bool isPlayer = false)
        {
            HeroClass = config.HeroClass;
            SelectedFirstPerk = config.SelectedFirstPerk;
            SelectedSecondPerk = config.SelectedSecondPerk;
            IsHero = config.IsHero;
            IsFemale = isFemale;
            FormationIndex = (FormationClass)formationIndex;
            IsGeneral = isGeneral;
            IsPlayer = isPlayer;
        }
    }
}
