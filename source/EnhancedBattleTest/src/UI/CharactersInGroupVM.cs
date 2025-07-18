using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.UI.Basic;
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

        public bool IsMultiplayer => Collection.IsMultiplayer;
        public bool IsSinglePlayer => !Collection.IsMultiplayer;

        public TextVM CharacterText { get; }

        public SelectorVM<SelectorItemVM> Characters { get; }
        public CharacterConfigVM Character { get; }

        public static CharactersInGroupVM Create(CharacterCollection collection)
        {
            return collection.IsMultiplayer
                ? (CharactersInGroupVM)new MPCharactersInGroupVM(collection)
                : new SPCharactersInGroupVM(collection);
        }

        protected CharactersInGroupVM(CharacterCollection collection)
        {
            Collection = collection;

            CharacterText = new TextVM(GameTexts.FindText("str_ebt_character"));
            Characters = new SelectorVM<SelectorItemVM>(0, null);

            Character = CharacterConfigVM.Create(Collection.IsMultiplayer);
        }

        public abstract void SelectedCultureAndGroupChanged(string cultureId, Group group, bool updateInstantly = true);

        public void SetConfig(TeamConfig teamConfig, CharacterConfig config, bool isAttacker)
        {
            Config = config;
            OnSetConfig(config);
            Characters.SelectedIndex = -1;
            Characters.SelectedIndex = CharactersInCurrentGroup.FindIndex(c => c.StringId == Config.Character.StringId);
            Character.SetConfig(teamConfig, Config, isAttacker);
        }

        protected abstract void OnSetConfig(CharacterConfig config);

        protected void RefreshCharacterList()
        {
            Characters.Refresh(CharactersInCurrentGroup.Select(character => character.Name), 0, OnSelectedCharacterChanged);
        }

        private void OnSelectedCharacterChanged(SelectorVM<SelectorItemVM> characters)
        {
            Character?.SelectedCharacterChanged(CharactersInCurrentGroup.ElementAtOrDefault(characters.SelectedIndex));
        }
    }
}
