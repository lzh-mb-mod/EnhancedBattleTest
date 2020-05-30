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
        public TeamConfig TeamConfig;
        public CharacterConfig Config;
        public bool IsAttacker;
        public Action<CharacterConfig> SelectAction;
        public bool PauseGameActiveState;

        public CharacterSelectionData(TeamConfig teamConfig, CharacterConfig config, bool isAttacker, Action<CharacterConfig> selectAction, bool pauseGameActiveState)
        {
            TeamConfig = teamConfig;
            Config = config;
            IsAttacker = isAttacker;
            SelectAction = selectAction;
            PauseGameActiveState = pauseGameActiveState;
        }
    }

    public class CharacterSelectionVM : ViewModel
    {
        private readonly Action<CharacterSelectionData> _beginSelection;
        private readonly Action _endSelection;
        private readonly CharacterCollection _characterCollection;
        private CharacterSelectionData _data;
        private bool _updateInstantly = true;

        public TextVM TitleText { get; }
        public TextVM CultureText { get; }
        public TextVM GroupText { get; }

        public TextVM DoneText { get; }
        public TextVM CancelText { get; }

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
            DoneText = new TextVM(GameTexts.FindText("str_done"));
            CancelText = new TextVM(GameTexts.FindText("str_cancel"));

            Characters = CharactersInGroupVM.Create(_characterCollection);
            Groups = new SelectorVM<SelectorItemVM>(0, null);
            Cultures = new SelectorVM<SelectorItemVM>(
                _characterCollection.Cultures.Select(culture => culture.Name).Prepend(TextObject.Empty), 0,
                OnSelectedCultureChanged);
        }

        public override void OnFinalize()
        {
            EnhancedBattleTestSubModule.Instance.OnSelectCharacter -= this.Open;
        }

        private void OnSelectedCultureChanged(SelectorVM<SelectorItemVM> cultures)
        {
            var selectedCulture = SelectedCulture(cultures);
            if (selectedCulture == null)
            {
                var groups = _characterCollection.GroupsInCultures.Values.SelectMany(list => list)
                    .DistinctBy(group => group.Info.FormationClass).Select(g => g.Info.Name).Prepend(TextObject.Empty)
                    .ToList();
                var index = Groups.SelectedIndex;
                if (index >= groups.Count)
                    index = 0;
                RefreshSelector(Groups, groups, index, OnSelectedGroupChanged);
            }
            else
            {
                var groups = _characterCollection.GroupsInCultures[selectedCulture.StringId]
                    .Select(group => group.Info.Name)
                    .Prepend(TextObject.Empty).ToList();
                var index = Groups.SelectedIndex;
                if (index >= groups.Count)
                    index = 0;
                RefreshSelector(Groups, groups, index, OnSelectedGroupChanged);
            }
        }

        private void RefreshSelector(SelectorVM<SelectorItemVM> selector, List<TextObject> texts, int index, Action<SelectorVM<SelectorItemVM>> action)
        {
            if (selector == null)
                return;
            var bindings = new MBBindingList<SelectorItemVM>();
            foreach (var textObject in texts)
            {
                bindings.Add(new SelectorItemVM(textObject));
            }

            selector.SetOnChangeAction(null);
            selector.ItemList = bindings;
            selector.SelectedIndex = -1;
            selector.SetOnChangeAction(action);
            selector.SelectedIndex = index;
        }


        private void OnSelectedGroupChanged(SelectorVM<SelectorItemVM> groups)
        {
            Characters.SelectedCultureAndGroupChanged(SelectedCulture(Cultures), SelectedGroup(groups), _updateInstantly);
        }

        private BasicCultureObject SelectedCulture(SelectorVM<SelectorItemVM> cultures)
        {
            return cultures == null || cultures.SelectedIndex < 1 ? null : _characterCollection.Cultures[cultures.SelectedIndex - 1];
        }

        private Group SelectedGroup(SelectorVM<SelectorItemVM> groups)
        {
            var selectedCulture = SelectedCulture(Cultures);
            return selectedCulture == null || groups == null || groups.SelectedIndex < 1
                ? null
                : _characterCollection.GroupsInCultures[selectedCulture.StringId][groups.SelectedIndex - 1];
        }

        private void Open(CharacterSelectionData data)
        {
            SetData(data);
            _beginSelection?.Invoke(data);
        }

        private void SetData(CharacterSelectionData data)
        {
            _data = data;
            var character = data.Config.Character;
            _updateInstantly = false;
            Cultures.SelectedIndex = _characterCollection.Cultures.IndexOf(character.Culture) + 1;
            Groups.SelectedIndex = _characterCollection.GroupsInCultures[character.Culture.StringId]
                .FindIndex(group => group.Info.StringId == character.GroupInfo.StringId) + 1;
            Characters.SetConfig(data.TeamConfig, data.Config, data.IsAttacker);
            _updateInstantly = true;
        }

        private void Done()
        {
            _data.SelectAction?.Invoke(_data.Config);
            Close();
        }

        private void Close()
        {
            _endSelection?.Invoke();
        }
    }
}
