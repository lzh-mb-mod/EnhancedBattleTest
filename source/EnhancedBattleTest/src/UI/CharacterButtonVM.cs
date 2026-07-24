using EnhancedBattleTest.Config;
using System;
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

        public CharacterButtonVM(
            PartyConfig partyConfig,
            CharacterConfig config,
            bool isPlayerSide,
            BattleTypeConfig battleTypeConfig,
            Action onCharacterChanged = null)
        {
            _battleTypeConfig = battleTypeConfig;
            IsPlayerSide = isPlayerSide;
            SetConfig(partyConfig, config, onCharacterChanged);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Name.ActionText = _config.Character.Name.ToString();
        }

        private void SetConfig(
            PartyConfig partyConfig,
            CharacterConfig config,
            Action onCharacterChanged)
        {
            _config = config;
            Name = new StringItemWithActionVM(
                o =>
                {
                    EnhancedBattleTestSubModule.Instance.SelectCharacter(new CharacterSelectionData(partyConfig, _config.Clone(),
                        IsPlayerSide == (_battleTypeConfig.PlayerSide == BattleSideEnum.Attacker),
                        characterConfig =>
                        {
                            _config.CopyFrom(characterConfig);
                            Name.ActionText = _config.Character.Name.ToString();
                            onCharacterChanged?.Invoke();
                        }, false));
                }, _config.Character.Name.ToString(), this);
        }
    }
}
