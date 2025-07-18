using EnhancedBattleTest.Config;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Library;

namespace EnhancedBattleTest.UI
{
    public class CharacterButtonVM : ViewModel
    {
        private CharacterConfig _config;
        private readonly BattleTypeConfig _battleTypeConfig;
        public bool IsPlayerSide { get; set; }
        private StringItemWithActionVM _name;

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

        public CharacterButtonVM(TeamConfig teamConfig, CharacterConfig config, bool isPlayerSide, BattleTypeConfig battleTypeConfig)
        {
            _battleTypeConfig = battleTypeConfig;
            IsPlayerSide = isPlayerSide;
            SetConfig(teamConfig, config);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
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
