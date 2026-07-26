using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.UI.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;

namespace EnhancedBattleTest.UI
{
    public abstract class CharactersInGroupVM : ViewModel
    {
        protected readonly CharacterCollection Collection;

        protected List<Character> CharactersInCurrentGroup;

        protected CharacterConfig Config;
        private readonly Action _clearAllFilters;
        private SelectorVM<SelectorItemVM> _characters;

        public TextVM CharacterText { get; }
        public TextVM ClearAllFiltersText { get; }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> Characters
        {
            get => _characters;
            private set
            {
                if (_characters == value)
                    return;

                _characters = value;
                OnPropertyChangedWithValue(value, nameof(Characters));
            }
        }
        public CharacterConfigVM Character { get; }

        public static CharactersInGroupVM Create(
            CharacterCollection collection,
            Action clearAllFilters,
            Action<bool> heroFilterVisibilityChanged)
        {
            return new SPCharactersInGroupVM(
                collection,
                clearAllFilters,
                heroFilterVisibilityChanged);
        }

        protected CharactersInGroupVM(
            CharacterCollection collection,
            Action clearAllFilters)
        {
            Collection = collection;
            _clearAllFilters = clearAllFilters;

            CharacterText = new TextVM(GameTexts.FindText("str_ebt_character"));
            ClearAllFiltersText =
                new TextVM(GameTexts.FindText("str_ebt_clear_all_filters"));
            Characters = new SelectorVM<SelectorItemVM>(0, null);

            Character = CharacterConfigVM.Create();
        }

        public abstract void SelectedCultureAndGroupChanged(
            string factionCultureId,
            string clanCultureId,
            Group group,
            bool updateInstantly = true);

        public void SetConfig(
            PartyConfig partyConfig,
            CharacterConfig config,
            bool isAttacker,
            BasicCharacterObject preferredBannerCharacter,
            bool useSelectedCharacterForBanner)
        {
            Config = config;
            OnSetConfig(config);
            Characters.SelectedIndex = -1;
            Characters.SelectedIndex = CharactersInCurrentGroup.FindIndex(c => c.StringId == Config.Character.StringId);
            Character.SetConfig(
                partyConfig,
                Config,
                isAttacker,
                preferredBannerCharacter,
                useSelectedCharacterForBanner);
        }

        protected abstract void OnSetConfig(CharacterConfig config);
        public abstract void ClearFilters(bool updateInstantly);

        public void ClearAllFilters()
        {
            _clearAllFilters?.Invoke();
        }

        protected void RefreshCharacterList()
        {
            int selectedIndex = Config?.Character == null
                ? -1
                : CharactersInCurrentGroup.FindIndex(character =>
                    character.StringId == Config.Character.StringId);
            Characters.SetOnChangeAction(null);
            var refreshedCharacters = new SelectorVM<SelectorItemVM>(
                CharactersInCurrentGroup.Select(character => character.Name),
                selectedIndex,
                null);
            refreshedCharacters.SetOnChangeAction(OnSelectedCharacterChanged);
            Characters = refreshedCharacters;
        }

        private void OnSelectedCharacterChanged(SelectorVM<SelectorItemVM> characters)
        {
            Character?.SelectedCharacterChanged(CharactersInCurrentGroup.ElementAtOrDefault(characters.SelectedIndex));
        }
    }
}
