using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class SideVM : ViewModel
    {
        private TeamConfig _config;
        private ImageIdentifierVM _banner;

        public TextVM Name { get; }

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

        public TextVM TacticText { get; }
        public NumberVM<float> TacticLevel { get; }

        public CharacterButtonVM General { get; }

        public TextVM EnableGeneralText { get; }

        public BoolVM EnableGeneral { get; }

        public TroopGroup TroopGroup { get; }

        public bool IsPlayerSide
        {
            set
            {
                Name.TextObject = value ? new TextObject("{=BC7n6qxk}PLAYER") : new TextObject("{=35IHscBa}ENEMY");
                General.IsPlayerSide = value;
                TroopGroup.IsPlayerSide = value;
            }
        }

        public SideVM(TeamConfig config, bool isPlayerSide, BattleTypeConfig battleTypeConfig)
        {
            Name = new TextVM(isPlayerSide ? new TextObject("{=BC7n6qxk}PLAYER") : new TextObject("{=35IHscBa}ENEMY"));
            _config = config;
            Banner = new ImageIdentifierVM(BannerCode.CreateFrom(_config.BannerKey), true);
            TacticText = new TextVM(GameTexts.FindText("str_ebt_tactic_level"));
            TacticLevel = new NumberVM<float>(config.TacticLevel, 0, 50, true);
            TacticLevel.OnValueChanged += f => _config.TacticLevel = (int)f;
            General = new CharacterButtonVM(_config, _config.General,
                GameTexts.FindText("str_ebt_troop_role", "general"), isPlayerSide, battleTypeConfig);
            EnableGeneralText = new TextVM(GameTexts.FindText("str_ebt_enable"));
            EnableGeneral = new BoolVM(_config.HasGeneral);
            EnableGeneral.OnValueChanged += OnEnableGeneralChanged;

            TroopGroup = new TroopGroup(config, config.Troops, isPlayerSide, battleTypeConfig);
        }

        private void OnEnableGeneralChanged(bool value)
        {
            _config.HasGeneral = value;
        }

        public bool IsValid()
        {
            return TroopGroup.IsValid();
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
            General.RefreshValues();
            TroopGroup.RefreshValues();
        }
    }
}
