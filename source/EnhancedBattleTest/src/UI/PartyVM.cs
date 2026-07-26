using EnhancedBattleTest.BannerEditor;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.UI.Basic;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class PartyVM : ViewModel
    {
        private readonly PartyConfig _config;
        private readonly BattleTypeConfig _battleTypeConfig;
        private readonly Action<PartyVM> _remove;
        private readonly CharacterConfig _playerCharacterConfig;
        private bool _isPlayerSide;
        private ImageIdentifierVM _banner;
        private bool _isBannerEditorEnabled;
        private bool _canConfigureArmy;
        private bool _isPlayerCharacterVisible;

        public TextVM Name { get; }
        public TextVM EnableGeneralText { get; }
        public TextVM CustomBannerText { get; }
        public TextVM InArmyText { get; }
        public TextVM PlayerCharacterText { get; }
        public TextVM RemovePartyText { get; }
        public BoolVM EnableGeneral { get; }
        public BoolVM UseCustomBanner { get; }
        public BoolVM InArmy { get; }
        public CharacterButtonVM PlayerCharacter { get; }
        public TroopGroupVM Generals { get; }
        public TroopGroupVM Troops { get; }
        public bool CanRemove => _remove != null;
        public bool ShouldShowBanner => true;

        [DataSourceProperty]
        public bool CanConfigureArmy
        {
            get => _canConfigureArmy;
            private set
            {
                if (_canConfigureArmy == value)
                    return;
                _canConfigureArmy = value;
                OnPropertyChanged(nameof(CanConfigureArmy));
            }
        }

        [DataSourceProperty]
        public bool IsPlayerCharacterVisible
        {
            get => _isPlayerCharacterVisible;
            private set
            {
                if (_isPlayerCharacterVisible == value)
                    return;
                _isPlayerCharacterVisible = value;
                OnPropertyChanged(nameof(IsPlayerCharacterVisible));
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Banner
        {
            get => _banner;
            private set
            {
                if (_banner == value)
                    return;
                _banner = value;
                OnPropertyChanged(nameof(Banner));
            }
        }

        [DataSourceProperty]
        public bool IsBannerEditorEnabled
        {
            get => _isBannerEditorEnabled;
            private set
            {
                if (_isBannerEditorEnabled == value)
                    return;
                _isBannerEditorEnabled = value;
                OnPropertyChanged(nameof(IsBannerEditorEnabled));
            }
        }

        public bool IsPlayerSide
        {
            set
            {
                _isPlayerSide = value;
                Generals.IsPlayerSide = value;
                Troops.IsPlayerSide = value;
                if (PlayerCharacter != null)
                    PlayerCharacter.IsPlayerSide = value;
                UpdateConditionalControls();
                RefreshBanner();
            }
        }

        public PartyVM(
            PartyConfig config,
            TextObject name,
            bool isPlayerSide,
            BattleTypeConfig battleTypeConfig,
            CharacterConfig playerCharacterConfig = null,
            Action<PartyVM> remove = null)
        {
            _config = config;
            _battleTypeConfig = battleTypeConfig;
            _playerCharacterConfig = playerCharacterConfig;
            _remove = remove;
            _isPlayerSide = isPlayerSide;
            Name = new TextVM(name);
            EnableGeneralText = new TextVM(GameTexts.FindText("str_ebt_enable"));
            CustomBannerText = new TextVM(GameTexts.FindText("str_ebt_custom_banner"));
            InArmyText = new TextVM(GameTexts.FindText("str_ebt_in_army"));
            PlayerCharacterText =
                new TextVM(GameTexts.FindText("str_ebt_player_character"));
            RemovePartyText = new TextVM(GameTexts.FindText("str_ebt_remove_allied_party"));
            EnableGeneral = new BoolVM(_config.HasGeneral);
            EnableGeneral.OnValueChanged += value =>
            {
                _config.HasGeneral = value;
                RefreshBanner();
            };
            UseCustomBanner = new BoolVM(_config.UseCustomBanner);
            UseCustomBanner.OnValueChanged += value =>
            {
                if (value && !_config.UseCustomBanner)
                    _config.BannerKey = ResolveBanner().Serialize();
                _config.UseCustomBanner = value;
                IsBannerEditorEnabled = value;
                RefreshBanner();
            };
            InArmy = new BoolVM(_config.IsInArmy);
            InArmy.OnValueChanged += value => _config.IsInArmy = value;
            if (_playerCharacterConfig != null)
            {
                PlayerCharacter = new CharacterButtonVM(
                    _config,
                    _playerCharacterConfig,
                    isPlayerSide,
                    battleTypeConfig,
                    RefreshBanner,
                    GetPreferredBannerCharacter,
                    () => GetPreferredBannerCharacter()
                          == _playerCharacterConfig.CharacterObject);
            }
            Generals = new TroopGroupVM(
                _config,
                _config.Generals,
                GameTexts.FindText("str_ebt_generals"),
                true,
                isPlayerSide,
                battleTypeConfig,
                RefreshBanner,
                GetPreferredBannerCharacter);
            Troops = new TroopGroupVM(
                _config,
                _config.Troops,
                GameTexts.FindText("str_ebt_troops"),
                false,
                isPlayerSide,
                battleTypeConfig,
                RefreshBanner,
                GetPreferredBannerCharacter);
            IsBannerEditorEnabled = _config.UseCustomBanner;
            UpdateConditionalControls();
            RefreshBanner();
        }

        public void SetName(TextObject name)
        {
            Name.TextObject = name;
        }

        public void RemoveParty()
        {
            _remove?.Invoke(this);
        }

        public void SetPlayerType(PlayerType playerType)
        {
            IsPlayerCharacterVisible =
                _isPlayerSide && _playerCharacterConfig != null;
            RefreshBanner();
            Generals.RefreshValues();
            Troops.RefreshValues();
        }

        public void EditBanner()
        {
            if (!IsBannerEditorEnabled)
                return;

            BannerEditorState.Config = _config;
            BannerEditorState.PreferredCharacter =
                GetPreferredBannerCharacter();
            BannerEditorState.OnDone = RefreshBanner;
            Game.Current.GameStateManager.PushState(
                Game.Current.GameStateManager.CreateState<BannerEditorState>());
        }

        public bool IsValid()
        {
            return Generals.IsValid() && Troops.IsValid();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Name.RefreshValues();
            EnableGeneralText.RefreshValues();
            CustomBannerText.RefreshValues();
            InArmyText.RefreshValues();
            PlayerCharacterText.RefreshValues();
            RemovePartyText.RefreshValues();
            PlayerCharacter?.RefreshValues();
            Generals.RefreshValues();
            Troops.RefreshValues();
            RefreshBanner();
        }

        private void RefreshBanner()
        {
            Banner = new ImageIdentifierVM(
                BannerCode.CreateFrom(
                    ResolveBanner().Serialize()),
                true);
        }

        private Banner ResolveBanner()
        {
            return _config.ResolveBanner(
                IsAttacker(),
                GetPreferredBannerCharacter());
        }

        private BasicCharacterObject GetPreferredBannerCharacter()
        {
            if (!_isPlayerSide || _playerCharacterConfig == null)
                return null;

            BasicCharacterObject playerCharacter =
                _playerCharacterConfig.CharacterObject;
            if (_battleTypeConfig.PlayerType == PlayerType.Commander)
                return playerCharacter;

            return _config.GetFirstGeneralCharacter(playerCharacter)
                   ?? playerCharacter;
        }

        private bool IsAttacker()
        {
            return _isPlayerSide
                   == (_battleTypeConfig.PlayerSide == BattleSideEnum.Attacker);
        }

        private void UpdateConditionalControls()
        {
            CanConfigureArmy = _isPlayerSide;
            SetPlayerType(_battleTypeConfig.PlayerType);
        }
    }
}
