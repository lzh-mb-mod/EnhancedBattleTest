using EnhancedBattleTest.UI.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public sealed class PartySelectionData
    {
        public Action<MobileParty> SelectAction { get; }

        public PartySelectionData(Action<MobileParty> selectAction)
        {
            SelectAction = selectAction;
        }
    }

    public sealed class PartySelectionItemVM : ViewModel
    {
        private readonly MobileParty _party;
        private readonly Action<MobileParty> _select;

        public string Name { get; }
        public string Leader { get; }
        public string PartyType { get; }
        public string Faction { get; }
        public string MemberCount { get; }

        public PartySelectionItemVM(
            MobileParty party,
            Action<MobileParty> select)
        {
            _party = party;
            _select = select;
            Name = party.Name?.ToString() ?? party.StringId;
            Leader = party.LeaderHero?.Name?.ToString()
                     ?? GameTexts.FindText("str_ebt_no_leader").ToString();
            PartyType = PartySelectionVM.GetPartyTypeName(party).ToString();
            Faction = (party.MapFaction?.Name
                       ?? GameTexts.FindText("str_ebt_no_faction")).ToString();
            TextObject memberCount =
                GameTexts.FindText("str_ebt_party_member_count");
            memberCount.SetTextVariable(
                "MEMBER_COUNT",
                party.Party.NumberOfAllMembers);
            MemberCount = memberCount.ToString();
        }

        public void Select()
        {
            _select?.Invoke(_party);
        }

        public bool MatchesSearch(string search)
        {
            return string.IsNullOrEmpty(search)
                   || PartySelectionVM.Contains(Name, search)
                   || PartySelectionVM.Contains(Leader, search);
        }
    }

    public sealed class PartySelectionVM : ViewModel
    {
        private enum PartyTypeFilter
        {
            All,
            Lord,
            Caravan,
            Villager,
            Bandit,
            Militia,
            Garrison,
            Custom,
            Other
        }

        private readonly Action<PartySelectionData> _beginSelection;
        private readonly Action _endSelection;
        private readonly List<MobileParty> _parties;
        private readonly Dictionary<MobileParty, PartySelectionItemVM>
            _partyItems;
        private readonly List<IFaction> _factions;
        private readonly List<CultureObject> _cultures;
        private readonly List<Clan> _clans;
        private readonly List<IFaction> _factionsInSelection =
            new List<IFaction>();
        private readonly List<CultureObject> _culturesInSelection =
            new List<CultureObject>();
        private readonly List<Clan> _clansInSelection = new List<Clan>();
        private PartySelectionData _data;
        private string _partySearchText = string.Empty;
        private string _factionSearchText = string.Empty;
        private string _cultureSearchText = string.Empty;
        private string _clanSearchText = string.Empty;
        private bool _suppressFilterRefresh;
        private MBBindingList<PartySelectionItemVM> _displayedParties;

        public TextVM TitleText { get; }
        public TextVM PartyNameText { get; }
        public TextVM PartyTypeText { get; }
        public TextVM FactionText { get; }
        public TextVM CultureText { get; }
        public TextVM ClanText { get; }
        public TextVM CancelText { get; }
        public SelectorVM<SelectorItemVM> PartyTypes { get; }
        public SelectorVM<SelectorItemVM> Factions { get; }
        public SelectorVM<SelectorItemVM> Cultures { get; }
        public SelectorVM<SelectorItemVM> Clans { get; }
        [DataSourceProperty]
        public MBBindingList<PartySelectionItemVM> Parties
        {
            get => _displayedParties;
            private set
            {
                if (_displayedParties == value)
                    return;

                _displayedParties = value;
                OnPropertyChangedWithValue(value, nameof(Parties));
            }
        }

        [DataSourceProperty]
        public string PartySearchText
        {
            get => _partySearchText;
            set
            {
                value = value ?? string.Empty;
                if (_partySearchText == value)
                    return;

                _partySearchText = value;
                OnPropertyChangedWithValue(value, nameof(PartySearchText));
                if (!_suppressFilterRefresh)
                    RefreshParties();
            }
        }

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
                if (!_suppressFilterRefresh)
                    RefreshFactionsAndDependents();
            }
        }

        [DataSourceProperty]
        public string CultureSearchText
        {
            get => _cultureSearchText;
            set
            {
                value = value ?? string.Empty;
                if (_cultureSearchText == value)
                    return;
                _cultureSearchText = value;
                OnPropertyChangedWithValue(value, nameof(CultureSearchText));
                if (!_suppressFilterRefresh)
                    RefreshCulturesAndDependents();
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
                if (!_suppressFilterRefresh)
                    RefreshClansAndParties();
            }
        }

        public PartySelectionVM(
            Action<PartySelectionData> beginSelection,
            Action endSelection)
        {
            _beginSelection = beginSelection;
            _endSelection = endSelection;
            _parties = Campaign.Current.MobileParties
                .Where(party =>
                    party != null
                    && party.IsActive
                    && party.MemberRoster.Count > 0)
                .OrderBy(party => party.Name?.ToString())
                .ToList();
            _partyItems = _parties.ToDictionary(
                party => party,
                party => new PartySelectionItemVM(party, SelectParty));
            _factions = _parties
                .Select(party => party.MapFaction)
                .Where(faction => faction != null)
                .Distinct()
                .OrderBy(faction => faction.Name?.ToString())
                .ToList();
            _cultures = _parties
                .Select(party => party.MapFaction?.Culture)
                .Where(culture => culture != null)
                .Distinct()
                .OrderBy(culture => culture.Name?.ToString())
                .ToList();
            _clans = _parties
                .Select(party => party.ActualClan)
                .Where(clan => clan != null)
                .Distinct()
                .OrderBy(clan => clan.Name?.ToString())
                .ToList();

            TitleText =
                new TextVM(GameTexts.FindText("str_ebt_select_campaign_party"));
            PartyNameText =
                new TextVM(GameTexts.FindText("str_ebt_party_name"));
            PartyTypeText =
                new TextVM(GameTexts.FindText("str_ebt_party_type"));
            FactionText =
                new TextVM(GameTexts.FindText("str_ebt_faction"));
            CultureText =
                new TextVM(GameTexts.FindText("str_ebt_culture"));
            ClanText = new TextVM(GameTexts.FindText("str_ebt_clan"));
            CancelText = new TextVM(GameTexts.FindText("str_cancel"));
            Parties = new MBBindingList<PartySelectionItemVM>();
            PartyTypes = new SelectorVM<SelectorItemVM>(
                Enum.GetValues(typeof(PartyTypeFilter))
                    .Cast<PartyTypeFilter>()
                    .Select(GetPartyTypeFilterName),
                0,
                OnPartyFilterChanged);
            Factions = new SelectorVM<SelectorItemVM>(0, OnFactionChanged);
            Cultures = new SelectorVM<SelectorItemVM>(0, OnCultureChanged);
            Clans = new SelectorVM<SelectorItemVM>(0, OnPartyFilterChanged);
            RefreshCultures();
            RefreshFactions();
            RefreshClans();
            RefreshParties();

            EnhancedBattleTestSubModule.Instance.OnSelectParty += Open;
        }

        public override void OnFinalize()
        {
            EnhancedBattleTestSubModule.Instance.OnSelectParty -= Open;
            base.OnFinalize();
        }

        private void Open(PartySelectionData data)
        {
            _data = data;
            _suppressFilterRefresh = true;
            try
            {
                PartySearchText = string.Empty;
                FactionSearchText = string.Empty;
                CultureSearchText = string.Empty;
                ClanSearchText = string.Empty;
                PartyTypes.SelectedIndex = 0;
                Factions.SelectedIndex = 0;
                Cultures.SelectedIndex = 0;
                Clans.SelectedIndex = 0;
            }
            finally
            {
                _suppressFilterRefresh = false;
            }
            RefreshCulturesAndDependents();
            _beginSelection?.Invoke(data);
        }

        private void RefreshParties()
        {
            if (Parties == null)
                return;

            PartyTypeFilter type = PartyTypes?.SelectedIndex > 0
                ? (PartyTypeFilter)PartyTypes.SelectedIndex
                : PartyTypeFilter.All;
            IFaction faction = Factions?.SelectedIndex > 0
                ? _factionsInSelection[Factions.SelectedIndex - 1]
                : null;
            CultureObject culture = Cultures?.SelectedIndex > 0
                ? _culturesInSelection[Cultures.SelectedIndex - 1]
                : null;
            Clan clan = Clans?.SelectedIndex > 0
                ? _clansInSelection[Clans.SelectedIndex - 1]
                : null;
            string search = PartySearchText.Trim();

            var parties = new MBBindingList<PartySelectionItemVM>();
            foreach (MobileParty party in _parties.Where(party =>
                         MatchesType(party, type)
                         && (faction == null || party.MapFaction == faction)
                         && (culture == null
                             || party.MapFaction?.Culture == culture)
                         && (clan == null || party.ActualClan == clan)
                         && _partyItems[party].MatchesSearch(search)))
            {
                parties.Add(_partyItems[party]);
            }
            Parties = parties;
        }

        private void OnCultureChanged(SelectorVM<SelectorItemVM> selector)
        {
            if (!_suppressFilterRefresh)
                RefreshFactionsAndDependents();
        }

        private void OnFactionChanged(SelectorVM<SelectorItemVM> selector)
        {
            if (!_suppressFilterRefresh)
                RefreshClansAndParties();
        }

        private void OnPartyFilterChanged(SelectorVM<SelectorItemVM> selector)
        {
            if (!_suppressFilterRefresh)
                RefreshParties();
        }

        private void RefreshCulturesAndDependents()
        {
            RefreshCultures();
            RefreshFactionsAndDependents();
        }

        private void RefreshFactionsAndDependents()
        {
            RefreshFactions();
            RefreshClansAndParties();
        }

        private void RefreshClansAndParties()
        {
            RefreshClans();
            RefreshParties();
        }

        private void RefreshCultures()
        {
            CultureObject selected = Cultures?.SelectedIndex > 0
                ? _culturesInSelection.ElementAtOrDefault(
                    Cultures.SelectedIndex - 1)
                : null;
            string search = CultureSearchText.Trim();
            _culturesInSelection.Clear();
            _culturesInSelection.AddRange(_cultures.Where(culture =>
                string.IsNullOrEmpty(search)
                || Contains(culture.Name?.ToString(), search)
                || Contains(culture.StringId, search)));
            RefreshSelector(
                Cultures,
                _culturesInSelection.Select(culture => culture.Name),
                selected,
                OnCultureChanged);
        }

        private void RefreshFactions()
        {
            IFaction selected = Factions?.SelectedIndex > 0
                ? _factionsInSelection.ElementAtOrDefault(
                    Factions.SelectedIndex - 1)
                : null;
            CultureObject culture = Cultures?.SelectedIndex > 0
                ? _culturesInSelection[Cultures.SelectedIndex - 1]
                : null;
            string search = FactionSearchText.Trim();
            _factionsInSelection.Clear();
            _factionsInSelection.AddRange(_factions.Where(faction =>
                (culture == null || faction.Culture == culture)
                && (string.IsNullOrEmpty(search)
                    || Contains(faction.Name?.ToString(), search)
                    || Contains(faction.StringId, search))));
            RefreshSelector(
                Factions,
                _factionsInSelection.Select(faction => faction.Name),
                selected,
                OnFactionChanged);
        }

        private void RefreshClans()
        {
            Clan selected = Clans?.SelectedIndex > 0
                ? _clansInSelection.ElementAtOrDefault(
                    Clans.SelectedIndex - 1)
                : null;
            CultureObject culture = Cultures?.SelectedIndex > 0
                ? _culturesInSelection[Cultures.SelectedIndex - 1]
                : null;
            IFaction faction = Factions?.SelectedIndex > 0
                ? _factionsInSelection[Factions.SelectedIndex - 1]
                : null;
            string search = ClanSearchText.Trim();
            _clansInSelection.Clear();
            _clansInSelection.AddRange(_clans.Where(clan =>
                (culture == null || clan.Culture == culture)
                && (faction == null || clan.MapFaction == faction)
                && (string.IsNullOrEmpty(search)
                    || Contains(clan.Name?.ToString(), search)
                    || Contains(clan.StringId, search))));
            RefreshSelector(
                Clans,
                _clansInSelection.Select(clan => clan.Name),
                selected,
                OnPartyFilterChanged);
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
            int index = selected is IFaction faction
                ? _factionsInSelection.IndexOf(faction) + 1
                : selected is CultureObject culture
                    ? _culturesInSelection.IndexOf(culture) + 1
                    : selected is Clan clan
                        ? _clansInSelection.IndexOf(clan) + 1
                        : 0;
            selector.SelectedIndex = Math.Max(index, 0);
            selector.SetOnChangeAction(onChanged);
        }

        internal static bool Contains(string value, string search)
        {
            return value?.IndexOf(
                       search,
                       StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private static bool MatchesType(
            MobileParty party,
            PartyTypeFilter type)
        {
            switch (type)
            {
                case PartyTypeFilter.All:
                    return true;
                case PartyTypeFilter.Lord:
                    return party.IsLordParty;
                case PartyTypeFilter.Caravan:
                    return party.IsCaravan;
                case PartyTypeFilter.Villager:
                    return party.IsVillager;
                case PartyTypeFilter.Bandit:
                    return party.IsBandit;
                case PartyTypeFilter.Militia:
                    return party.IsMilitia;
                case PartyTypeFilter.Garrison:
                    return party.IsGarrison;
                case PartyTypeFilter.Custom:
                    return party.IsCustomParty;
                case PartyTypeFilter.Other:
                    return !party.IsLordParty
                           && !party.IsCaravan
                           && !party.IsVillager
                           && !party.IsBandit
                           && !party.IsMilitia
                           && !party.IsGarrison
                           && !party.IsCustomParty;
                default:
                    return false;
            }
        }

        private static TextObject GetPartyTypeFilterName(
            PartyTypeFilter type)
        {
            return type == PartyTypeFilter.All
                ? GameTexts.FindText("str_ebt_all")
                : GameTexts.FindText(
                    "str_ebt_party_type",
                    type.ToString());
        }

        internal static TextObject GetPartyTypeName(MobileParty party)
        {
            PartyTypeFilter type = Enum.GetValues(typeof(PartyTypeFilter))
                .Cast<PartyTypeFilter>()
                .Skip(1)
                .First(filter => MatchesType(party, filter));
            return GetPartyTypeFilterName(type);
        }

        private void SelectParty(MobileParty party)
        {
            Action<MobileParty> selectAction = _data?.SelectAction;
            Close();
            selectAction?.Invoke(party);
        }

        private void Close()
        {
            _endSelection?.Invoke();
        }
    }
}
