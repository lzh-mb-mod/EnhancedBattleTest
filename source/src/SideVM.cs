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
        private readonly TeamConfig _config;
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

        public CharacterButtonVM General { get; }

        public TextVM EnableGeneralText { get; }

        public BoolVM EnableGeneral { get; }

        public TroopGroup TroopGroup { get; }

        public SideVM(TeamConfig config, TextObject name, bool isPlayerSide, BattleTypeConfig battleTypeConfig)
        {
            _config = config;
            Name = new TextVM(name);
            Banner = new ImageIdentifierVM(BannerCode.CreateFrom(_config.BannerKey));
            General = new CharacterButtonVM(_config, _config.General,
                GameTexts.FindText("str_ebt_troop_role", "general"), isPlayerSide, battleTypeConfig);
            EnableGeneralText = new TextVM(GameTexts.FindText("str_ebt_enable"));
            EnableGeneral = new BoolVM(_config.HasGeneral);
            EnableGeneral.OnValueChanged += value => _config.HasGeneral = value;

            TroopGroup = new TroopGroup(config, config.Troops, isPlayerSide, battleTypeConfig);
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
