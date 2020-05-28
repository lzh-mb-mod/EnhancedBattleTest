using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class CharactersInGroupVM : ViewModel
    {
        private readonly CharacterCollection _collection;

        private List<Character> _charactersInCurrentGroup;

        private CharacterConfig _config;

        public TextVM CharacterText { get; }

        public SelectorVM<SelectorItemVM> Characters { get; }
        public CharacterConfigVM Character { get; }

        public CharactersInGroupVM(CharacterCollection collection, bool isMultiplayer)
        {
            _collection = collection;

            CharacterText = new TextVM(GameTexts.FindText("str_ebt_character"));
            Characters = new SelectorVM<SelectorItemVM>(0, null);

            Character = CharacterConfigVM.Create(isMultiplayer);
        }

        public void SelectedCultureAndGroupChanged(BasicCultureObject culture, Group group)
        {
            if (culture == null)
                _charactersInCurrentGroup = _collection.GroupsInCultures.Values
                    .SelectMany(groups => groups.SelectMany(g => g.CharactersInGroup.Values)).ToList();
            else if (group == null)
                _charactersInCurrentGroup = _collection.GroupsInCultures[culture.StringId]
                    .SelectMany(g => g.CharactersInGroup.Values).ToList();
            else
                _charactersInCurrentGroup = group.CharactersInGroup.Values.ToList();
            RefreshCharacterList();
        }

        public void SetConfig(CharacterConfig config, bool isAttacker)
        {
            _config = config;
            Character.SetConfig(_config, isAttacker);
            Characters.SelectedIndex = _charactersInCurrentGroup.FindIndex(c => c.StringId == _config.Character.StringId);
        }

        private void RefreshCharacterList()
        {
            Characters.Refresh(_charactersInCurrentGroup.Select(character => character.Name), 0, OnSelectedCharacterChanged);
        }

        private void OnSelectedCharacterChanged(SelectorVM<SelectorItemVM> characters)
        {
            Character?.SelectedCharacterChanged(_charactersInCurrentGroup.ElementAtOrDefault(characters.SelectedIndex));
        }
    }
}
