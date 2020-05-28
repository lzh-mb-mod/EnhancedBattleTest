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
        public bool IsFemale;
        public FormationClass FormationIndex;
        public bool IsPlayer;

        public BasicCharacterObject Character => IsHero ? HeroClass.HeroCharacter : HeroClass.TroopCharacter;
        public MPSpawnableCharacter(MPCharacterConfig config, int i, bool isPlayer = false)
        {
            HeroClass = config.HeroClass;
            SelectedFirstPerk = config.SelectedFirstPerk;
            SelectedSecondPerk = config.SelectedSecondPerk;
            IsHero = config.IsHero;
            IsFemale = config.IsFemale;
            FormationIndex = (FormationClass)i;
            IsPlayer = isPlayer;
        }
    }
}
