using EnhancedBattleTest.Config;
using EnhancedBattleTest.UI.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class SideVM : ViewModel
    {
        private readonly TeamConfig _config;
        private readonly BattleTypeConfig _battleTypeConfig;
        private readonly Func<IEnumerable<BasicCharacterObject>>
            _heroPlayerCharacters;
        private readonly Func<IEnumerable<BasicCharacterObject>> _partyHeroes;
        private bool _isPlayerSide;
        private bool _isTacticLevelOverrideEnabled;

        public TextVM Name { get; }
        public TextVM OverrideTacticLevelText { get; }
        public TextVM TacticText { get; }
        public TextVM AddAlliedPartyText { get; }
        public TextVM TotalTroopCountText { get; }
        public NumberVM<float> TacticLevel { get; }
        public PartyVM PrimaryParty { get; }
        public MBBindingList<PartyVM> AlliedParties { get; }

        [DataSourceProperty]
        public bool IsTacticLevelOverrideEnabled
        {
            get => _isTacticLevelOverrideEnabled;
            set
            {
                if (_isTacticLevelOverrideEnabled == value)
                    return;

                _isTacticLevelOverrideEnabled = value;
                _config.OverrideTacticLevel = value;
                OnPropertyChangedWithValue(
                    value,
                    nameof(IsTacticLevelOverrideEnabled));
            }
        }

        public bool IsPlayerSide
        {
            set
            {
                _isPlayerSide = value;
                Name.TextObject = value
                    ? new TextObject("{=BC7n6qxk}PLAYER")
                    : new TextObject("{=35IHscBa}ENEMY");
                PrimaryParty.IsPlayerSide = value;
                foreach (PartyVM party in AlliedParties)
                    party.IsPlayerSide = value;
                UpdateTotalTroopCount();
            }
        }

        public SideVM(
            TeamConfig config,
            bool isPlayerSide,
            BattleTypeConfig battleTypeConfig,
            Func<IEnumerable<BasicCharacterObject>> heroPlayerCharacters,
            Func<IEnumerable<BasicCharacterObject>> partyHeroes)
        {
            _config = config;
            _battleTypeConfig = battleTypeConfig;
            _heroPlayerCharacters = heroPlayerCharacters;
            _partyHeroes = partyHeroes;
            _isPlayerSide = isPlayerSide;
            Name = new TextVM(
                isPlayerSide
                    ? new TextObject("{=BC7n6qxk}PLAYER")
                    : new TextObject("{=35IHscBa}ENEMY"));
            OverrideTacticLevelText =
                new TextVM(GameTexts.FindText(
                    "str_ebt_override_tactic_level"));
            TacticText = new TextVM(GameTexts.FindText("str_ebt_tactic_level"));
            AddAlliedPartyText =
                new TextVM(GameTexts.FindText("str_ebt_add_allied_party"));
            TotalTroopCountText = new TextVM(new TextObject(string.Empty));
            TacticLevel = new NumberVM<float>(config.TacticLevel, 0, 100, true);
            TacticLevel.OnValueChanged += value =>
                _config.TacticLevel = (int)value;
            IsTacticLevelOverrideEnabled = _config.OverrideTacticLevel;
            PrimaryParty = new PartyVM(
                _config.PrimaryParty,
                GameTexts.FindText("str_ebt_primary_party"),
                isPlayerSide,
                battleTypeConfig,
                _config.PlayerCharacter,
                null,
                _heroPlayerCharacters,
                _partyHeroes,
                UpdateTotalTroopCount);
            AlliedParties = new MBBindingList<PartyVM>();
            foreach (PartyConfig party in _config.AlliedParties)
                AlliedParties.Add(CreateAlliedPartyVM(party));
            UpdateAlliedPartyNames();
            UpdateTotalTroopCount();
        }

        public void AddAlliedParty()
        {
            var config = new PartyConfig();
            _config.AlliedParties.Add(config);
            AlliedParties.Add(CreateAlliedPartyVM(config));
            UpdateAlliedPartyNames();
            UpdateTotalTroopCount();
        }

        public void SetPlayerType(PlayerType playerType)
        {
            PrimaryParty.SetPlayerType(playerType);
            UpdateTotalTroopCount();
        }

        public void RefreshPartyMoraleOverrideVisibility()
        {
            PrimaryParty.RefreshPartyMoraleOverrideVisibility();
            foreach (PartyVM party in AlliedParties)
                party.RefreshPartyMoraleOverrideVisibility();
        }

        public bool IsValid()
        {
            return PrimaryParty.IsValid()
                   && AlliedParties.All(party => party.IsValid());
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Name.RefreshValues();
            OverrideTacticLevelText.RefreshValues();
            TacticText.RefreshValues();
            AddAlliedPartyText.RefreshValues();
            TotalTroopCountText.RefreshValues();
            PrimaryParty.RefreshValues();
            foreach (PartyVM party in AlliedParties)
                party.RefreshValues();
        }

        private PartyVM CreateAlliedPartyVM(PartyConfig config)
        {
            return new PartyVM(
                config,
                new TextObject(),
                _isPlayerSide,
                _battleTypeConfig,
                null,
                RemoveAlliedParty,
                _heroPlayerCharacters,
                _partyHeroes,
                UpdateTotalTroopCount);
        }

        private void RemoveAlliedParty(PartyVM party)
        {
            int index = AlliedParties.IndexOf(party);
            if (index < 0)
                return;
            AlliedParties.RemoveAt(index);
            _config.AlliedParties.RemoveAt(index);
            UpdateAlliedPartyNames();
            UpdateTotalTroopCount();
        }

        private void UpdateAlliedPartyNames()
        {
            for (int i = 0; i < AlliedParties.Count; ++i)
            {
                TextObject name = GameTexts.FindText("str_ebt_allied_party");
                name.SetTextVariable("INDEX", i + 1);
                AlliedParties[i].SetName(name);
            }
        }

        private void UpdateTotalTroopCount()
        {
            int total = GetParties()
                .Sum(GetPartyTroopCount);
            if (_isPlayerSide
                && _battleTypeConfig.PlayerType != PlayerType.None
                && _config.PlayerCharacter?.CharacterObject
                    is CharacterObject playerCharacter
                && !ContainsCharacter(
                    _config.PrimaryParty,
                    playerCharacter))
            {
                total++;
            }
            TextObject text =
                GameTexts.FindText("str_ebt_total_troop_count");
            text.SetTextVariable("TROOP_COUNT", total);
            TotalTroopCountText.TextObject = text;
        }

        private static int GetPartyTroopCount(PartyConfig party)
        {
            int troopCount = party.Troops.Troops
                .Where(troop =>
                    troop?.Number > 0
                    && troop.Character?.CharacterObject
                        is CharacterObject)
                .Sum(troop => troop.Number);
            if (!party.HasHeroes)
                return troopCount;

            return troopCount + party.Heroes.Troops.Count(troop =>
                troop?.Character?.CharacterObject is CharacterObject);
        }

        private static bool ContainsCharacter(
            PartyConfig party,
            CharacterObject character)
        {
            return party.HasHeroes
                   && party.Heroes.Troops.Any(troop =>
                       troop?.Character?.CharacterObject == character)
                   || party.Troops.Troops.Any(troop =>
                       troop?.Number > 0
                       && troop.Character?.CharacterObject == character);
        }

        private IEnumerable<PartyConfig> GetParties()
        {
            yield return _config.PrimaryParty;
            foreach (PartyConfig party in _config.AlliedParties)
                yield return party;
        }
    }
}
