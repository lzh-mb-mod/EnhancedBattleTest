using EnhancedBattleTest.BannerEditor;
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

        public TroopGroupVM Generals { get; }

        public TextVM EnableGeneralText { get; }

        public BoolVM EnableGeneral { get; }

        public MBBindingList<TroopGroupVM> TroopGroups { get; }

        public bool IsPlayerSide
        {
            set
            {
                Name.TextObject = value ? new TextObject("{=BC7n6qxk}PLAYER") : new TextObject("{=35IHscBa}ENEMY");
                Generals.IsPlayerSide = value;
                foreach (var troopGroup in TroopGroups)
                {
                    troopGroup.IsPlayerSide = value;
                }
            }
        }

        public SideVM(TeamConfig config, bool isPlayerSide, BattleTypeConfig battleTypeConfig)
        {
            Name = new TextVM(isPlayerSide ? new TextObject("{=BC7n6qxk}PLAYER") : new TextObject("{=35IHscBa}ENEMY"));
            _config = config;
            Banner = new ImageIdentifierVM(BannerCode.CreateFrom(_config.BannerKey), true);
            TacticText = new TextVM(GameTexts.FindText("str_ebt_tactic_level"));
            TacticLevel = new NumberVM<float>(config.TacticLevel, 0, 100, true);
            TacticLevel.OnValueChanged += f => _config.TacticLevel = (int)f;
            Generals = new TroopGroupVM(_config, _config.Generals, GameTexts.FindText("str_ebt_troop_role", "General"),
                true, isPlayerSide, battleTypeConfig);
            EnableGeneralText = new TextVM(GameTexts.FindText("str_ebt_enable"));
            EnableGeneral = new BoolVM(_config.HasGeneral);
            EnableGeneral.OnValueChanged += OnEnableGeneralChanged;

            TroopGroups = new MBBindingList<TroopGroupVM>();
            for (int i = 0; i < _config.TroopGroups.Length; ++i)
            {
                TroopGroups.Add(new TroopGroupVM(config, _config.TroopGroups[i],
                    GameTexts.FindText("str_ebt_troop_role", "Soldiers").SetTextVariable("TroopIndex", i + 1), false,
                    isPlayerSide, battleTypeConfig));
            }
        }

        private void OnEnableGeneralChanged(bool value)
        {
            _config.HasGeneral = value;
        }

        public bool IsValid()
        {
            return Generals.IsValid() && TroopGroups.All(troopGroup => troopGroup.IsValid());
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
            foreach (var troopGroupVm in TroopGroups)
            {
                troopGroupVm.RefreshValues();
            }
        }
    }
}
