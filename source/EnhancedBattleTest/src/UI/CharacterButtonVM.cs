using EnhancedBattleTest.Config;
using System;
using System.Collections.Generic;
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
            bool? heroOnly = null,
            Func<IEnumerable<BasicCharacterObject>> unavailableHeroes = null)
        {
            _battleTypeConfig = battleTypeConfig;
            IsPlayerSide = isPlayerSide;
            SetConfig(
                partyConfig,
                config,
                onCharacterChanged,
                preferredBannerCharacter,
                useSelectedCharacterForBanner,
                heroOnly,
                unavailableHeroes);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Name.ActionText = GetCharacterName();
        }

        private void SetConfig(
            PartyConfig partyConfig,
            CharacterConfig config,
            Action onCharacterChanged,
            Func<BasicCharacterObject> preferredBannerCharacter,
            Func<bool> useSelectedCharacterForBanner,
            bool? heroOnly,
            Func<IEnumerable<BasicCharacterObject>> unavailableHeroes)
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
                            Name.ActionText = GetCharacterName();
                            onCharacterChanged?.Invoke();
                        },
                        false,
                        preferredBannerCharacter?.Invoke(),
                        useSelectedCharacterForBanner?.Invoke() == true,
                        heroOnly,
                        unavailableHeroes?.Invoke(),
                        cancelAction));
            Name = new StringItemWithActionVM(
                o => SelectCharacter(),
                GetCharacterName(),
                this);
        }

        private string GetCharacterName() =>
            (_config.Character?.Name ?? GameTexts.FindText("str_ebt_select_character")).ToString();

        public void SelectCharacter(Action cancelAction = null)
        {
            _selectCharacter?.Invoke(cancelAction);
        }
    }
}
