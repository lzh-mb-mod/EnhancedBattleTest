using EnhancedBattleTest.BannerEditor;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.UI.Basic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class TeamVM : ViewModel
    {
        private int _index;
        private readonly TeamConfig _config;
        private ImageIdentifierVM _banner;
        private bool _isBannerVisible;
        public TextVM Name { get; }

        public TextVM CustomBannerText { get; }

        public BoolVM CustomBanner { get; }

        [DataSourceProperty]
        public ImageIdentifierVM Banner
        {
            get => _banner;
            set
            {
                if (_banner == value)
                    return;
                _banner = value;
                OnPropertyChanged(nameof(Banner));
            }
        }

        public bool ShouldShowBanner => !EnhancedBattleTestSubModule.IsMultiplayer;

        public TextVM CustomTacticLevelText { get; }

        public BoolVM CustomTacticLevel { get; }

        public TextVM TacticText { get; }
        public NumberVM<float> TacticLevel { get; }

        public TroopGroupVM Generals { get; }

        public TextVM EnableGeneralText { get; }

        public BoolVM EnableGeneral { get; }
        public TroopGroupVM Troops { get; }

        public bool IsPlayerSide
        {
            set
            {
                Name.TextObject = GameTexts.FindText("str_ebt_party_name").SetTextVariable("PARTY_TEXT", value
                    ? new TextObject("{=sSJSTe5p}Player Party")
                    : new TextObject("{=0xC75dN6}Enemy Party")).SetTextVariable("INDEX", _index);
                Generals.IsPlayerSide = value;
                Troops.IsPlayerSide = value;
            }
        }

        public TeamVM(TeamConfig teamConfig, bool isPlayerSide, int index, BattleTypeConfig battleTypeConfig)
        {
            _index = index;
            _config = teamConfig;
            Name = new TextVM(GameTexts.FindText("str_ebt_party_name").SetTextVariable("PARTY_TEXT", isPlayerSide
                ? new TextObject("{=sSJSTe5p}Player Party")
                : new TextObject("{=0xC75dN6}Enemy Party")).SetTextVariable("INDEX", _index));
            CustomBannerText = new TextVM(GameTexts.FindText("str_ebt_custom_banner"));
            IsBannerVisible = teamConfig.CustomBanner;
            CustomBanner = new BoolVM(teamConfig.CustomBanner);
            CustomBanner.OnValueChanged += b => _config.CustomBanner = IsBannerVisible = b;
            Banner = new ImageIdentifierVM(BannerCode.CreateFrom(_config.BannerKey), true);
            CustomTacticLevelText = new TextVM(GameTexts.FindText("str_ebt_custom_tactic_level"));
            CustomTacticLevel = new BoolVM(teamConfig.CustomTacticLevel);
            CustomTacticLevel.OnValueChanged += b => _config.CustomTacticLevel = b;
            TacticText = new TextVM(GameTexts.FindText("str_ebt_tactic_level"));
            TacticLevel = new NumberVM<float>(_config.TacticLevel, 0, 100, true);
            TacticLevel.OnValueChanged += f => _config.TacticLevel = (int)f;
            Generals = new TroopGroupVM(_config, _config.Generals, GameTexts.FindText("str_ebt_troop_role", "General"),
                true, isPlayerSide, battleTypeConfig);
            EnableGeneralText = new TextVM(GameTexts.FindText("str_ebt_enable"));
            EnableGeneral = new BoolVM(_config.HasGeneral);
            EnableGeneral.OnValueChanged += OnEnableGeneralChanged;

            Troops = new TroopGroupVM(_config, _config.Troops, GameTexts.FindText("str_ebt_troop_role", "Soldiers"),
                false, isPlayerSide, battleTypeConfig);
        }

        private void OnEnableGeneralChanged(bool value)
        {
            _config.HasGeneral = value;
        }
        public bool IsValid()
        {
            return Generals.IsValid() && Troops.IsValid();
        }

        public void EditBanner()
        {
            BannerEditorState.Config = _config;
            BannerEditorState.OnDone = () => Banner = new ImageIdentifierVM(BannerCode.CreateFrom(_config.BannerKey), true);
            Game.Current.GameStateManager.PushState(Game.Current.GameStateManager.CreateState<BannerEditorState>());
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            Name.RefreshValues();
            Generals.RefreshValues();
            Troops.RefreshValues();
        }

        [DataSourceProperty]
        public bool IsBannerVisible
        {
            get => _isBannerVisible;
            set
            {
                if (_isBannerVisible == value)
                    return;
                _isBannerVisible = value;
                OnPropertyChanged(nameof(IsBannerVisible));
            }
        }
    }
}
