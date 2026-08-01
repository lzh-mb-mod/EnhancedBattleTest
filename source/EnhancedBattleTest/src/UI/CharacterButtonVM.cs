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
        private Action<Action> _selectCharacter;
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
            Action onCharacterChanged = null,
            Func<BasicCharacterObject> preferredBannerCharacter = null,
            Func<bool> useSelectedCharacterForBanner = null,
            bool? heroOnly = null)
        {
            _battleTypeConfig = battleTypeConfig;
            IsPlayerSide = isPlayerSide;
            SetConfig(
                partyConfig,
                config,
                onCharacterChanged,
                preferredBannerCharacter,
                useSelectedCharacterForBanner,
                heroOnly);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Name.ActionText = _config.Character.Name.ToString();
        }

        private void SetConfig(
            PartyConfig partyConfig,
            CharacterConfig config,
            Action onCharacterChanged,
            Func<BasicCharacterObject> preferredBannerCharacter,
            Func<bool> useSelectedCharacterForBanner,
            bool? heroOnly)
        {
            _config = config;
            _selectCharacter = cancelAction =>
                EnhancedBattleTestSubModule.Instance.SelectCharacter(
                    new CharacterSelectionData(
                        partyConfig,
                        _config.Clone(),
                        IsPlayerSide
                        == (_battleTypeConfig.PlayerSide
                            == BattleSideEnum.Attacker),
                        characterConfig =>
                        {
                            _config.CopyFrom(characterConfig);
                            Name.ActionText =
                                _config.Character.Name.ToString();
                            onCharacterChanged?.Invoke();
                        },
                        false,
                        preferredBannerCharacter?.Invoke(),
                        useSelectedCharacterForBanner?.Invoke() == true,
                        heroOnly,
                        cancelAction));
            Name = new StringItemWithActionVM(
                o => SelectCharacter(),
                _config.Character.Name.ToString(),
                this);
        }

        public void SelectCharacter(Action cancelAction = null)
        {
            _selectCharacter?.Invoke(cancelAction);
        }
    }
}
