using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.UI.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace EnhancedBattleTest.UI
{
    public class CharacterSelectionData
    {
        public PartyConfig PartyConfig;
        public CharacterConfig Config;
        public bool IsAttacker;
        public Action<CharacterConfig> SelectAction;
        public bool PauseGameActiveState;
        public BasicCharacterObject PreferredBannerCharacter;
        public bool UseSelectedCharacterForBanner;

        public CharacterSelectionData(
            PartyConfig partyConfig,
            CharacterConfig config,
            bool isAttacker,
            Action<CharacterConfig> selectAction,
            bool pauseGameActiveState,
            BasicCharacterObject preferredBannerCharacter,
            bool useSelectedCharacterForBanner)
        {
            PartyConfig = partyConfig;
            Config = config;
            IsAttacker = isAttacker;
            SelectAction = selectAction;
            PauseGameActiveState = pauseGameActiveState;
            PreferredBannerCharacter = preferredBannerCharacter;
            UseSelectedCharacterForBanner = useSelectedCharacterForBanner;
        }
    }

    public class CharacterSelectionVM : ViewModel
    {
        private readonly Action<CharacterSelectionData> _beginSelection;
        private List<Group> _groupsInSelection;
        private readonly Action _endSelection;
        private readonly CharacterCollection _characterCollection;
        private readonly List<string> _factionCulturesInSelection =
            new List<string>();
        private readonly List<string> _clanCulturesInSelection =
            new List<string>();
        private CharacterSelectionData _data;
        private bool _updateInstantly = true;
        private bool _suspendFilterRefresh;
        private bool _areHeroFiltersVisible = true;
        private string _factionCultureSearchText = string.Empty;
        private string _clanCultureSearchText = string.Empty;

        public TextVM TitleText { get; }
        public TextVM FactionCultureText { get; }
        public TextVM ClanCultureText { get; }
        public TextVM GroupText { get; }

        public TextVM DoneText { get; }
        public TextVM CancelText { get; }

        public SelectorVM<SelectorItemVM> FactionCultures { get; }
        public SelectorVM<SelectorItemVM> ClanCultures { get; }

        public SelectorVM<SelectorItemVM> Groups { get; }

        public CharactersInGroupVM Characters { get; }

        [DataSourceProperty]
        public bool AreHeroFiltersVisible
        {
            get => _areHeroFiltersVisible;
            private set
            {
                if (_areHeroFiltersVisible == value)
                    return;

                _areHeroFiltersVisible = value;
                OnPropertyChangedWithValue(value, nameof(AreHeroFiltersVisible));
            }
        }

        [DataSourceProperty]
        public string FactionCultureSearchText
        {
            get => _factionCultureSearchText;
            set
            {
                value = value ?? string.Empty;
                if (_factionCultureSearchText == value)
                    return;

                _factionCultureSearchText = value;
                OnPropertyChangedWithValue(
                    value,
                    nameof(FactionCultureSearchText));
                if (!_suspendFilterRefresh)
                    RefreshFactionCultures();
            }
        }

        [DataSourceProperty]
        public string ClanCultureSearchText
        {
            get => _clanCultureSearchText;
            set
            {
                value = value ?? string.Empty;
                if (_clanCultureSearchText == value)
                    return;

                _clanCultureSearchText = value;
                OnPropertyChangedWithValue(
                    value,
                    nameof(ClanCultureSearchText));
                if (!_suspendFilterRefresh)
                    RefreshClanCultures();
            }
        }

        public CharacterSelectionVM(
            CharacterCollection characterCollection,
            Action<CharacterSelectionData> beginSelection,
            Action endSelection)
        {
            _beginSelection = beginSelection;
            _endSelection = endSelection;
            EnhancedBattleTestSubModule.Instance.OnSelectCharacter += this.Open;
            _characterCollection = characterCollection;

            TitleText = new TextVM(GameTexts.FindText("str_ebt_select_character"));
            FactionCultureText =
                new TextVM(GameTexts.FindText("str_ebt_faction_culture"));
            ClanCultureText =
                new TextVM(GameTexts.FindText("str_ebt_clan_culture"));
            GroupText = new TextVM(GameTexts.FindText("str_ebt_group"));
            DoneText = new TextVM(GameTexts.FindText("str_done"));
            CancelText = new TextVM(GameTexts.FindText("str_cancel"));

            Characters = CharactersInGroupVM.Create(
                _characterCollection,
                ClearAllFilters,
                visible => AreHeroFiltersVisible = visible);
            Groups = new SelectorVM<SelectorItemVM>(0, null);
            _groupsInSelection = new List<Group>();
            FactionCultures =
                new SelectorVM<SelectorItemVM>(0, OnFilterChanged);
            ClanCultures =
                new SelectorVM<SelectorItemVM>(0, OnFilterChanged);
            RefreshFactionCultures();
            RefreshClanCultures();
            RefreshGroups();
        }

        public override void OnFinalize()
        {
            EnhancedBattleTestSubModule.Instance.OnSelectCharacter -= this.Open;
        }

        private void RefreshGroups()
        {
            _groupsInSelection = _characterCollection.GroupsInCultures.Values
                .SelectMany(list => list)
                .DistinctBy(group => group.Info.FormationClass)
                .ToList();
            var groups = _groupsInSelection.Select(group => group.Info.Name)
                .Prepend(GameTexts.FindText("str_ebt_all"))
                .ToList();
            int index = Groups.SelectedIndex;
            if (index >= groups.Count)
                index = 0;
            RefreshSelector(Groups, groups, index, OnSelectedGroupChanged);
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
            if (!_suspendFilterRefresh)
                UpdateCharacterFilters();
        }

        private void OnFilterChanged(SelectorVM<SelectorItemVM> selector)
        {
            if (!_suspendFilterRefresh)
                UpdateCharacterFilters();
        }

        private void UpdateCharacterFilters()
        {
            Characters.SelectedCultureAndGroupChanged(
                SelectedCultureId(
                    FactionCultures,
                    _factionCulturesInSelection),
                SelectedCultureId(
                    ClanCultures,
                    _clanCulturesInSelection),
                SelectedGroup(Groups),
                _updateInstantly);
        }

        private static string SelectedCultureId(
            SelectorVM<SelectorItemVM> selector,
            List<string> cultures)
        {
            return selector == null || selector.SelectedIndex < 1
                ? null
                : cultures[selector.SelectedIndex - 1];
        }

        private Group SelectedGroup(SelectorVM<SelectorItemVM> groups)
        {
            return groups == null || groups.SelectedIndex < 1 || groups.SelectedIndex > _groupsInSelection.Count
                ? null
                : _groupsInSelection[groups.SelectedIndex - 1];
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
            FactionCultureSearchText = string.Empty;
            ClanCultureSearchText = string.Empty;
            var characterObject =
                (character as SinglePlayer.Data.SPCharacter)?.CharacterObject;
            FactionCultures.SelectedIndex =
                _factionCulturesInSelection.IndexOf(
                    characterObject?.HeroObject?.MapFaction?.Culture?.StringId)
                + 1;
            ClanCultures.SelectedIndex =
                _clanCulturesInSelection.IndexOf(
                    characterObject?.HeroObject?.Clan?.Culture?.StringId) + 1;
            Groups.SelectedIndex = _groupsInSelection.FindIndex(group =>
                group.Info.FormationClass
                == character.GroupInfo.FormationClass) + 1;
            Characters.SetConfig(
                data.PartyConfig,
                data.Config,
                data.IsAttacker,
                data.PreferredBannerCharacter,
                data.UseSelectedCharacterForBanner);
            _updateInstantly = true;
        }

        private void ClearAllFilters()
        {
            _updateInstantly = false;
            _suspendFilterRefresh = true;
            FactionCultureSearchText = string.Empty;
            ClanCultureSearchText = string.Empty;
            FactionCultures.SelectedIndex = 0;
            ClanCultures.SelectedIndex = 0;
            Groups.SelectedIndex = 0;
            Characters.ClearFilters(false);
            RefreshFactionCultures();
            RefreshClanCultures();
            _suspendFilterRefresh = false;
            _updateInstantly = true;
            UpdateCharacterFilters();
        }

        private void RefreshFactionCultures()
        {
            RefreshCultures(
                FactionCultures,
                _factionCulturesInSelection,
                FactionCultureSearchText);
        }

        private void RefreshClanCultures()
        {
            RefreshCultures(
                ClanCultures,
                _clanCulturesInSelection,
                ClanCultureSearchText);
        }

        private void RefreshCultures(
            SelectorVM<SelectorItemVM> selector,
            List<string> culturesInSelection,
            string searchText)
        {
            string selectedCultureId =
                SelectedCultureId(selector, culturesInSelection);
            string search = searchText.Trim();
            culturesInSelection.Clear();
            culturesInSelection.AddRange(_characterCollection.Cultures
                .Where(cultureId =>
                    string.IsNullOrEmpty(search)
                    || Contains(CultureName(cultureId).ToString(), search)
                    || Contains(cultureId, search)));
            var cultures = culturesInSelection
                .Select(CultureName)
                .Prepend(GameTexts.FindText("str_ebt_all"))
                .ToList();
            int index = selectedCultureId == null
                ? 0
                : culturesInSelection.IndexOf(selectedCultureId) + 1;
            RefreshSelector(
                selector,
                cultures,
                Math.Max(index, 0),
                OnFilterChanged);
        }

        private static TextObject CultureName(string cultureId)
        {
            return cultureId == "null"
                ? GameTexts.FindText("str_ebt_null_culture")
                : MBObjectManager.Instance
                    .GetObject<BasicCultureObject>(cultureId).Name;
        }

        private static bool Contains(string value, string search)
        {
            return value?.IndexOf(
                       search,
                       StringComparison.CurrentCultureIgnoreCase) >= 0;
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
