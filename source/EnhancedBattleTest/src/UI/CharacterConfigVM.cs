using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest.UI
{
    public abstract class CharacterConfigVM : ViewModel
    {
        public abstract void SetConfig(
            PartyConfig partyConfig,
            CharacterConfig config,
            bool isAttacker,
            BasicCharacterObject preferredBannerCharacter,
            bool useSelectedCharacterForBanner);
        public abstract void SelectedCharacterChanged(Character character);

        public static CharacterConfigVM Create()
        {
            return new SPCharacterConfigVM();
        }
    }
}
