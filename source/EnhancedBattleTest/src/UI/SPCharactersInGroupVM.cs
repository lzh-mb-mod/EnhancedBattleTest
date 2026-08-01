using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.SinglePlayer.Config;
using EnhancedBattleTest.SinglePlayer.Data;
using EnhancedBattleTest.UI.Basic;
using System.Collections.Generic;
using System.Linq;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class SPCharactersInGroupVM : CharactersInGroupVM
    {
        private enum HeroFilter
        {
            All,
            Hero,
            NonHero
        }

        private readonly List<Occupation> _occupations = new List<Occupation>();
        private readonly List<CharacterObject> _characters;
        private readonly List<IFaction> _factions;
        private readonly List<Clan> _clans;
        private readonly List<IFaction> _factionsInSelection =
            new List<IFaction>();
        private readonly List<Clan> _clansInSelection = new List<Clan>();
        private readonly Action<bool> _heroFiltersEnabledChanged;
        private string _factionCultureId;
        private string _clanCultureId;
        private Group _group;
        private string _searchText = string.Empty;
        private string _factionSearchText = string.Empty;
        private string _clanSearchText = string.Empty;
        private bool _suspendFilterUpdates;
        private HeroFilter? _requiredHeroFilter;
        public TextVM OccupationText { get; }
        public TextVM SearchTextLabel { get; }
        public TextVM HeroFilterText { get; }
        public TextVM FactionText { get; }
        public TextVM ClanText { get; }
        public SelectorVM<SelectorItemVM> Occupations { get; }
        public SelectorVM<SelectorItemVM> HeroFilters { get; }
        public SelectorVM<SelectorItemVM> Factions { get; }
        public SelectorVM<SelectorItemVM> Clans { get; }
        public bool AreHeroFiltersEnabled =>
            HeroFilters == null
            || (HeroFilter)HeroFilters.SelectedIndex != HeroFilter.NonHero;

        [DataSourceProperty]
        public float HeroFiltersAlpha =>
            AreHeroFiltersEnabled ? 1f : 0.45f;

        [DataSourceProperty]
        public bool IsHeroFilterEnabled => !_requiredHeroFilter.HasValue;

        [DataSourceProperty]
        public float HeroFilterAlpha =>
            IsHeroFilterEnabled ? 1f : 0.45f;

        [DataSourceProperty]
        public string FactionSearchText
        {
            get => _factionSearchText;
            set
            {
                value = value ?? string.Empty;
                if (_factionSearchText == value)
                    return;

                _factionSearchText = value;
                OnPropertyChangedWithValue(value, nameof(FactionSearchText));
                if (!_suspendFilterUpdates)
                    RefreshFactions();
            }
        }

        [DataSourceProperty]
        public string ClanSearchText
        {
            get => _clanSearchText;
            set
            {
                value = value ?? string.Empty;
                if (_clanSearchText == value)
                    return;

                _clanSearchText = value;
                OnPropertyChangedWithValue(value, nameof(ClanSearchText));
                if (!_suspendFilterUpdates)
                    RefreshClans();
            }
        }

        [DataSourceProperty]
        public string SearchText
        {
            get => _searchText;
            set
            {
                value = value ?? string.Empty;
                if (_searchText == value)
                    return;

                _searchText = value;
                OnPropertyChangedWithValue(value, nameof(SearchText));
                if (!_suspendFilterUpdates)
                    UpdateCharacterList();
            }
        }

        public SPCharactersInGroupVM(
            CharacterCollection collection,
            Action clearAllFilters,
            Action<bool> heroFiltersEnabledChanged)
            : base(collection, clearAllFilters)
        {
            _heroFiltersEnabledChanged = heroFiltersEnabledChanged;
            OccupationText = new TextVM(new TextObject("{=GZxFIeiJ}Occupation"));
            SearchTextLabel = new TextVM(GameTexts.FindText("str_ebt_search"));
            HeroFilterText =
                new TextVM(GameTexts.FindText("str_ebt_hero_filter"));
            FactionText = new TextVM(GameTexts.FindText("str_ebt_faction"));
            ClanText = new TextVM(GameTexts.FindText("str_ebt_clan"));
            for (Occupation occupation = Occupation.NotAssigned;
                occupation < Occupation.NumberOfOccupations;
                ++occupation)
            {
                _occupations.Add(occupation);
            }

            Occupations = new SelectorVM<SelectorItemVM>(0, OnSelectedOccupationChanged);
            var list = new MBBindingList<SelectorItemVM>();
            foreach (var item in
                _occupations.Select(occupation =>
                {
                    switch (occupation)
                    {
                        case Occupation.GoodsTrader:
                        case Occupation.BannerBearer:
                            return new TextObject(occupation.ToString());
                        case Occupation.RuralNotable:
                            return GameTexts.FindText("str_rural_notable");
                        case Occupation.Artisan:
                        case Occupation.Preacher:
                        case Occupation.Headman:
                        case Occupation.GangLeader:
                            return GameTexts.FindText("str_charactertype_" + occupation.ToString().ToLower());
                        case Occupation.CaravanGuard:
                            return new TextObject("{=jxNe8lH2}Caravan Guard");
                    }
                    return GameTexts.FindText("str_occupation", occupation.ToString());
                }))
            {
                list.Add(new SelectorItemVM(item));
            }

            Occupations.ItemList = list;
            _characters = collection.GroupsInCultures.Values
                .SelectMany(groups => groups)
                .OfType<SPGroup>()
                .SelectMany(group => group.OccupationsInGroup.Values)
                .SelectMany(occupation => occupation.Characters.Values)
                .OfType<SPCharacter>()
                .Select(character => character.CharacterObject)
                .ToList();
            _factions = _characters
                .Where(character => character.HeroObject?.MapFaction != null)
                .Select(character => character.HeroObject.MapFaction)
                .Distinct()
                .OrderBy(faction => faction.Name?.ToString())
                .ToList();
            _clans = _characters
                .Where(character => character.HeroObject?.Clan != null)
                .Select(character => character.HeroObject.Clan)
                .Distinct()
                .OrderBy(clan => clan.Name?.ToString())
                .ToList();
            HeroFilters = new SelectorVM<SelectorItemVM>(
                new[]
                {
                    GameTexts.FindText("str_ebt_all"),
                    GameTexts.FindText("str_ebt_hero_filter", "Hero"),
                    GameTexts.FindText("str_ebt_hero_filter", "NonHero")
                },
                0,
                OnSimpleFilterChanged);
            Factions = new SelectorVM<SelectorItemVM>(
                0,
                OnSelectedFactionChanged);
            Clans = new SelectorVM<SelectorItemVM>(
                0,
                OnSimpleFilterChanged);
            RefreshFactions();
            RefreshClans();
        }

        public override void SelectedCultureAndGroupChanged(
            string factionCultureId,
            string clanCultureId,
            Group group,
            bool updateInstantly = true)
        {
            bool culturesChanged =
                _factionCultureId != factionCultureId
                || _clanCultureId != clanCultureId;
            _factionCultureId = factionCultureId;
            _clanCultureId = clanCultureId;
            _group = group;
            bool wasSuspended = _suspendFilterUpdates;
            if (culturesChanged)
            {
                _suspendFilterUpdates = true;
                RefreshFactions();
                RefreshClans();
                _suspendFilterUpdates = wasSuspended;
            }
            if (updateInstantly && !wasSuspended)
                UpdateCharacterList();
        }

        protected override void OnSetConfig(CharacterConfig config)
        {
            var spConfig = config as SPCharacterConfig;
            if (spConfig == null)
                return;
            SearchText = string.Empty;
            FactionSearchText = string.Empty;
            ClanSearchText = string.Empty;
            HeroFilters.SelectedIndex = RequiredHeroFilterIndex();
            CharacterObject character = spConfig.ActualCharacterObject;
            IFaction faction = character?.HeroObject?.MapFaction;
            Clan clan = character?.HeroObject?.Clan;
            Factions.SelectedIndex = faction == null
                ? 0
                : Math.Max(_factionsInSelection.IndexOf(faction) + 1, 0);
            RefreshClans();
            Clans.SelectedIndex = clan == null
                ? 0
                : Math.Max(_clansInSelection.IndexOf(clan) + 1, 0);
            Occupations.SelectedIndex = -1;
            Occupations.SelectedIndex =
                character == null ? 0 : (int)character.Occupation;
        }

        public override void ClearFilters(bool updateInstantly)
        {
            _suspendFilterUpdates = true;
            SearchText = string.Empty;
            FactionSearchText = string.Empty;
            ClanSearchText = string.Empty;
            HeroFilters.SelectedIndex = RequiredHeroFilterIndex();
            Factions.SelectedIndex = 0;
            Clans.SelectedIndex = 0;
            Occupations.SelectedIndex = 0;
            _suspendFilterUpdates = false;
            if (updateInstantly)
                UpdateCharacterList();
        }

        private void OnSelectedOccupationChanged(SelectorVM<SelectorItemVM> obj)
        {
            if (obj.SelectedItem != null && !_suspendFilterUpdates)
                UpdateCharacterList();
        }

        private void OnSelectedFactionChanged(
            SelectorVM<SelectorItemVM> selector)
        {
            if (_suspendFilterUpdates)
                return;

            RefreshClans();
            UpdateCharacterList();
        }

        private void OnSimpleFilterChanged(
            SelectorVM<SelectorItemVM> selector)
        {
            if (selector == HeroFilters)
            {
                OnPropertyChanged(nameof(AreHeroFiltersEnabled));
                OnPropertyChanged(nameof(HeroFiltersAlpha));
                _heroFiltersEnabledChanged?.Invoke(AreHeroFiltersEnabled);
            }
            if (!_suspendFilterUpdates)
                UpdateCharacterList();
        }

        private void UpdateCharacterList()
        {
            IEnumerable<List<Group>> groupsByCulture;
            if (CurrentHeroFilter() == HeroFilter.NonHero
                && _factionCultureId != null
                && Collection.GroupsInCultures.TryGetValue(
                    _factionCultureId,
                    out var cultureGroups))
            {
                groupsByCulture = new[] { cultureGroups };
            }
            else
            {
                groupsByCulture = Collection.GroupsInCultures.Values;
            }
            if (_group == null)
            {
                CharactersInCurrentGroup = groupsByCulture
                    .SelectMany(groups => groups.SelectMany(
                        GetCharactersInGroup))
                    .ToList();
            }
            else
            {
                CharactersInCurrentGroup = groupsByCulture
                    .SelectMany(groups => groups.Where(group =>
                            group.Info.FormationClass
                            == _group.Info.FormationClass)
                        .SelectMany(GetCharactersInGroup))
                    .ToList();
            }
            CharactersInCurrentGroup = CharactersInCurrentGroup
                .Where(MatchesFilters)
                .ToList();
            RefreshCharacterList();

        }

        private Occupation CurrentOccupation()
        {
            return (Occupation)Occupations.SelectedIndex;
        }

        private HeroFilter CurrentHeroFilter()
        {
            return _requiredHeroFilter
                ?? (HeroFilters == null
                ? HeroFilter.All
                : (HeroFilter)HeroFilters.SelectedIndex);
        }

        public void SetRequiredHeroFilter(bool? heroOnly)
        {
            _requiredHeroFilter = heroOnly.HasValue
                ? heroOnly.Value ? HeroFilter.Hero : HeroFilter.NonHero
                : (HeroFilter?)null;
            OnPropertyChanged(nameof(IsHeroFilterEnabled));
            OnPropertyChanged(nameof(HeroFilterAlpha));
            HeroFilters.SelectedIndex = RequiredHeroFilterIndex();
            UpdateCharacterList();
        }

        private int RequiredHeroFilterIndex()
        {
            return (int)(_requiredHeroFilter ?? HeroFilter.All);
        }

        private IEnumerable<Character> GetCharactersInGroup(Group group)
        {
            var spGroup = group as SPGroup;
            if (spGroup == null)
                return Enumerable.Empty<Character>();
            var occupation = CurrentOccupation();
            if (occupation == Occupation.NotAssigned)
                return spGroup.OccupationsInGroup.Values.SelectMany(c => c.Characters.Values);
            return spGroup.OccupationsInGroup.TryGetValue(occupation, out var characters)
                ? characters.Characters.Values
                : Enumerable.Empty<Character>();
        }

        private bool MatchesFilters(Character character)
        {
            var spCharacter = character as SPCharacter;
            CharacterObject characterObject = spCharacter?.CharacterObject;
            if (characterObject == null)
                return false;

            HeroFilter heroFilter = CurrentHeroFilter();
            if (heroFilter == HeroFilter.Hero && !characterObject.IsHero)
                return false;
            if (heroFilter == HeroFilter.NonHero && characterObject.IsHero)
                return false;

            if (_factionCultureId != null)
            {
                BasicCultureObject culture = characterObject.IsHero
                    ? characterObject.HeroObject?.MapFaction?.Culture
                    : characterObject.Culture;
                if (CultureId(culture) != _factionCultureId)
                    return false;
            }

            if (characterObject.IsHero
                && _clanCultureId != null
                && CultureId(characterObject.HeroObject?.Clan?.Culture)
                != _clanCultureId)
            {
                return false;
            }

            if (heroFilter != HeroFilter.NonHero)
            {
                IFaction faction = Factions?.SelectedIndex > 0
                    ? _factionsInSelection.ElementAtOrDefault(
                        Factions.SelectedIndex - 1)
                    : null;
                if (faction != null
                    && characterObject.HeroObject?.MapFaction != faction)
                    return false;

                if (characterObject.IsHero)
                {
                    Clan clan = Clans?.SelectedIndex > 0
                        ? _clansInSelection.ElementAtOrDefault(
                            Clans.SelectedIndex - 1)
                        : null;
                    if (clan != null
                        && characterObject.HeroObject?.Clan != clan)
                    {
                        return false;
                    }
                }
            }

            string search = SearchText.Trim();
            return string.IsNullOrEmpty(search)
                   || Contains(characterObject.Name?.ToString(), search)
                   || Contains(characterObject.StringId, search);
        }

        private void RefreshFactions()
        {
            IFaction selected = Factions?.SelectedIndex > 0
                ? _factionsInSelection.ElementAtOrDefault(
                    Factions.SelectedIndex - 1)
                : null;
            string search = FactionSearchText.Trim();
            _factionsInSelection.Clear();
            _factionsInSelection.AddRange(_factions.Where(faction =>
                (_factionCultureId == null
                 || CultureId(faction.Culture) == _factionCultureId)
                && (string.IsNullOrEmpty(search)
                    || Contains(faction.Name?.ToString(), search)
                    || Contains(faction.StringId, search))));
            RefreshSelector(
                Factions,
                _factionsInSelection.Select(faction => faction.Name),
                selected,
                OnSelectedFactionChanged);
        }

        private void RefreshClans()
        {
            Clan selected = Clans?.SelectedIndex > 0
                ? _clansInSelection.ElementAtOrDefault(Clans.SelectedIndex - 1)
                : null;
            string search = ClanSearchText.Trim();
            IFaction selectedFaction = Factions?.SelectedIndex > 0
                ? _factionsInSelection.ElementAtOrDefault(
                    Factions.SelectedIndex - 1)
                : null;
            _clansInSelection.Clear();
            _clansInSelection.AddRange(_clans.Where(clan =>
                (_clanCultureId == null
                 || CultureId(clan.Culture) == _clanCultureId)
                && (selectedFaction == null
                    || clan.MapFaction == selectedFaction)
                && (string.IsNullOrEmpty(search)
                    || Contains(clan.Name?.ToString(), search)
                    || Contains(clan.StringId, search))));
            RefreshSelector(
                Clans,
                _clansInSelection.Select(clan => clan.Name),
                selected,
                OnSimpleFilterChanged);
        }

        private void RefreshSelector(
            SelectorVM<SelectorItemVM> selector,
            IEnumerable<TextObject> names,
            object selected,
            Action<SelectorVM<SelectorItemVM>> onChanged)
        {
            if (selector == null)
                return;

            var items = new MBBindingList<SelectorItemVM>
            {
                new SelectorItemVM(GameTexts.FindText("str_ebt_all"))
            };
            foreach (TextObject name in names)
                items.Add(new SelectorItemVM(name));

            selector.SetOnChangeAction(null);
            selector.ItemList = items;
            selector.SelectedIndex = -1;
            int index = selected == null
                ? 0
                : selected is IFaction faction
                    ? _factionsInSelection.IndexOf(faction) + 1
                    : _clansInSelection.IndexOf((Clan)selected) + 1;
            selector.SelectedIndex = Math.Max(index, 0);
            selector.SetOnChangeAction(onChanged);
            onChanged?.Invoke(selector);
        }

        private static bool Contains(string value, string search)
        {
            return value?.IndexOf(
                       search,
                       StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private static string CultureId(BasicCultureObject culture)
        {
            return culture?.StringId ?? "null";
        }
    }
}
