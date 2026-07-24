using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using TaleWorlds.Library;

namespace EnhancedBattleTest.UI
{
    public abstract class CharacterConfigVM : ViewModel
    {
        public abstract void SetConfig(PartyConfig partyConfig, CharacterConfig config, bool isAttacker);
        public abstract void SelectedCharacterChanged(Character character);

        public static CharacterConfigVM Create()
        {
            return new SPCharacterConfigVM();
        }
    }
}
