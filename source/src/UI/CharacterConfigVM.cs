using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using TaleWorlds.Library;

namespace EnhancedBattleTest.UI
{
    public abstract class CharacterConfigVM : ViewModel
    {
        public abstract void SetConfig(TeamConfig teamConfig, CharacterConfig config, bool isAttacker);
        public abstract void SelectedCharacterChanged(Character character);

        public static CharacterConfigVM Create(bool isMultiplayer)
        {
            if (isMultiplayer)
            {
                return new MPCharacterConfigVM();
            }
            else
            {
                return new SPCharacterConfigVM();
            }
        }
    }
}
