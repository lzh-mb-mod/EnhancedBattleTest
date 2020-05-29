using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public class SPSpawnableCharacter
    {
        public CharacterObject Character;
        public bool IsFemale { get; }
        public FormationClass FormationIndex;
        public bool IsPlayer { get; }
        public bool IsHero => Character.IsHero;

        public SPSpawnableCharacter(SPCharacterConfig config, int formationIndex, bool isFemale, bool isPlayer = false)
        {
            Character = config.ActualCharacterObject;
            IsFemale = isFemale;
            FormationIndex = (FormationClass)formationIndex;
            IsPlayer = isPlayer;
        }
    }
}
