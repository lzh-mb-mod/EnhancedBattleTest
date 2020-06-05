using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class CharacterButtonVM : ViewModel
    {
        private CharacterConfig _config;
        private readonly BattleTypeConfig _battleTypeConfig;
        public bool IsPlayerSide { get; set; }
        private StringItemWithActionVM _name;

        public TextVM CharacterRole { get; }

        [DataSourceProperty]
        public StringItemWithActionVM Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public CharacterButtonVM(TeamConfig teamConfig, CharacterConfig config, TextObject characterRole, bool isPlayerSide, BattleTypeConfig battleTypeConfig)
        {
            _battleTypeConfig = battleTypeConfig;
            CharacterRole = new TextVM(characterRole);
            IsPlayerSide = isPlayerSide;
            SetConfig(teamConfig, config);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            CharacterRole.RefreshValues();
            Name.ActionText = _config.Character.Name.ToString();
        }

        private void SetConfig(TeamConfig teamConfig, CharacterConfig config)
        {
            _config = config;
            Name = new StringItemWithActionVM(
                o =>
                {
                    EnhancedBattleTestSubModule.Instance.SelectCharacter(new CharacterSelectionData(teamConfig, _config.Clone(),
                        IsPlayerSide == (_battleTypeConfig.PlayerSide == BattleSideEnum.Attacker),
                        characterConfig =>
                        {
                            _config.CopyFrom(characterConfig);
                            Name.ActionText = _config.Character.Name.ToString();
                        }, false));
                }, _config.Character.Name.ToString(), this);
        }
    }
}
