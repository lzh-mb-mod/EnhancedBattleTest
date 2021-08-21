using EnhancedBattleTest.Data;
using EnhancedBattleTest.SinglePlayer.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest.SinglePlayer.Data
{
    public class SPSpawnableCharacter : ISpawnableCharacter
    {
        public CharacterObject CharacterObject;
        public BasicCharacterObject Character { get; }
        public bool IsFemale { get; }
        public FormationClass FormationIndex { get; }

        public bool IsGeneral { get; }
        public bool IsPlayer { get; }
        public bool IsHero => Character.IsHero;

        public SPSpawnableCharacter(SPCharacterConfig config, int formationIndex, bool isFemale, bool isGeneral = false, bool isPlayer = false)
        {
            Character = config.ActualCharacterObject;
            IsFemale = isFemale;
            FormationIndex = (FormationClass)formationIndex;
            IsGeneral = isGeneral;
            IsPlayer = isPlayer;
        }
    }
}
