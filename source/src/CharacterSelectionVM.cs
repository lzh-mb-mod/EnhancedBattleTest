using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class CharacterSelectionData
    {
        public CharacterConfig Config;
        public Action<CharacterConfig> SelectAction;
        public bool PauseGameActiveState;

        public CharacterSelectionData(CharacterConfig config, Action<CharacterConfig> selectAction, bool pauseGameActiveState)
        {
            Config = config;
            SelectAction = selectAction;
            PauseGameActiveState = pauseGameActiveState;
        }
    }

    public class CharacterSelectionVM : ViewModel
    {
        private readonly Action<CharacterSelectionData> _beginSelection;
        private readonly Action _endSelection;
        private CharacterSelectionData _data;
        private readonly CharacterCollection _characterCollection;

        public TextVM TitleText { get; }
        public TextVM CultureText { get; }
        public TextVM GroupText { get; }

        public SelectorVM<SelectorItemVM> Cultures { get; }

        public SelectorVM<SelectorItemVM> Groups { get; }

        public CharactersInGroupVM Characters { get; }


        public CharacterSelectionVM(CharacterCollection characterCollection, Action<CharacterSelectionData> beginSelection, Action endSelection, bool isMultiplayer)
        {
            _beginSelection = beginSelection;
            _endSelection = endSelection;
            EnhancedBattleTestSubModule.Instance.OnSelectCharacter += this.Open;
            _characterCollection = characterCollection;

            TitleText = new TextVM(GameTexts.FindText("str_ebt_select_character"));
            CultureText = new TextVM(GameTexts.FindText("str_ebt_culture"));
            GroupText = new TextVM(GameTexts.FindText("str_ebt_group"));

            Characters = new CharactersInGroupVM(_characterCollection, isMultiplayer);
            Cultures = new SelectorVM<SelectorItemVM>(
                _characterCollection.Cultures.Select(culture => culture.Name).Prepend(TextObject.Empty), 0,
                OnSelectedCultureChanged);
            Groups = new SelectorVM<SelectorItemVM>(0, null);
            OnSelectedCultureChanged(Cultures);
        }

        public override void OnFinalize()
        {
            EnhancedBattleTestSubModule.Instance.OnSelectCharacter -= this.Open;
        }

        private void OnSelectedCultureChanged(SelectorVM<SelectorItemVM> cultures)
        {
            var selectedCulture = SelectedCulture();
            if (selectedCulture == null)
            {
                Groups?.Refresh(Enumerable.Empty<TextObject>().Prepend(TextObject.Empty), 0, OnSelectedGroupChanged);
            }
            else
            {
                Groups?.Refresh(
                    _characterCollection.GroupsInCultures[selectedCulture.StringId].Select(group => group.Info.Name)
                        .Prepend(TextObject.Empty), 0, OnSelectedGroupChanged);
            }
        }

        private void OnSelectedGroupChanged(SelectorVM<SelectorItemVM> groups)
        {
            Characters.SelectedCultureAndGroupChanged(SelectedCulture(), SelectedGroup());
        }

        private BasicCultureObject SelectedCulture()
        {
            return Cultures == null || Cultures.SelectedIndex < 1 ? null : _characterCollection.Cultures[Cultures.SelectedIndex - 1];
        }

        private Group SelectedGroup()
        {
            var selectedCulture = SelectedCulture();
            return selectedCulture == null || Groups == null || Groups.SelectedIndex < 1
                ? null
                : _characterCollection.GroupsInCultures[selectedCulture.StringId][Groups.SelectedIndex - 1];
        }

        private void Open(CharacterSelectionData data)
        {
            SetData(data);
            _beginSelection?.Invoke(data);
        }

        private void SetData(CharacterSelectionData data)
        {
            var character = data.Config.Character;
            Cultures.SelectedIndex = _characterCollection.Cultures.IndexOf(character.Culture) + 1;
            Groups.SelectedIndex = _characterCollection.GroupsInCultures[character.Culture.StringId]
                .FindIndex(group => group.Info.StringId == character.GroupInfo.StringId) + 1;
            Characters.SetConfig(data.Config);
        }

        public void Close()
        {
            _endSelection?.Invoke();
        }
    }
}
