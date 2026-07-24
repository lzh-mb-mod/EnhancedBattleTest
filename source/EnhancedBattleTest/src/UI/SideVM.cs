using EnhancedBattleTest.Config;
using EnhancedBattleTest.UI.Basic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class SideVM : ViewModel
    {
        private readonly TeamConfig _config;
        private readonly BattleTypeConfig _battleTypeConfig;
        private bool _isPlayerSide;

        public TextVM Name { get; }
        public TextVM TacticText { get; }
        public TextVM AddAlliedPartyText { get; }
        public NumberVM<float> TacticLevel { get; }
        public PartyVM PrimaryParty { get; }
        public MBBindingList<PartyVM> AlliedParties { get; }

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
            }
        }

        public SideVM(
            TeamConfig config,
            bool isPlayerSide,
            BattleTypeConfig battleTypeConfig)
        {
            _config = config;
            _battleTypeConfig = battleTypeConfig;
            _isPlayerSide = isPlayerSide;
            Name = new TextVM(
                isPlayerSide
                    ? new TextObject("{=BC7n6qxk}PLAYER")
                    : new TextObject("{=35IHscBa}ENEMY"));
            TacticText = new TextVM(GameTexts.FindText("str_ebt_tactic_level"));
            AddAlliedPartyText =
                new TextVM(GameTexts.FindText("str_ebt_add_allied_party"));
            TacticLevel = new NumberVM<float>(config.TacticLevel, 0, 100, true);
            TacticLevel.OnValueChanged += value =>
                _config.TacticLevel = (int)value;
            PrimaryParty = new PartyVM(
                _config.PrimaryParty,
                GameTexts.FindText("str_ebt_primary_party"),
                isPlayerSide,
                battleTypeConfig,
                _config.PlayerCharacter);
            AlliedParties = new MBBindingList<PartyVM>();
            foreach (PartyConfig party in _config.AlliedParties)
                AlliedParties.Add(CreateAlliedPartyVM(party));
            UpdateAlliedPartyNames();
        }

        public void AddAlliedParty()
        {
            var config = new PartyConfig();
            _config.AlliedParties.Add(config);
            AlliedParties.Add(CreateAlliedPartyVM(config));
            UpdateAlliedPartyNames();
        }

        public void SetPlayerType(PlayerType playerType)
        {
            PrimaryParty.SetPlayerType(playerType);
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
            TacticText.RefreshValues();
            AddAlliedPartyText.RefreshValues();
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
                RemoveAlliedParty);
        }

        private void RemoveAlliedParty(PartyVM party)
        {
            int index = AlliedParties.IndexOf(party);
            if (index < 0)
                return;
            AlliedParties.RemoveAt(index);
            _config.AlliedParties.RemoveAt(index);
            UpdateAlliedPartyNames();
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
    }
}
