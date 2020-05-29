using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class MPSpawnableCharacter
    {
        public MultiplayerClassDivisions.MPHeroClass HeroClass { get; }
        public int SelectedFirstPerk;
        public int SelectedSecondPerk;
        public bool IsHero;
        public bool IsFemale { get; }
        public FormationClass FormationIndex { get; }
        public bool IsPlayer { get; }

        public BasicCharacterObject Character => IsHero ? HeroClass.HeroCharacter : HeroClass.TroopCharacter;
        public MPSpawnableCharacter(MPCharacterConfig config, int formationIndex, bool isFemale, bool isPlayer = false)
        {
            HeroClass = config.HeroClass;
            SelectedFirstPerk = config.SelectedFirstPerk;
            SelectedSecondPerk = config.SelectedSecondPerk;
            IsHero = config.IsHero;
            IsFemale = isFemale;
            FormationIndex = (FormationClass)formationIndex;
            IsPlayer = isPlayer;
        }
    }
}
