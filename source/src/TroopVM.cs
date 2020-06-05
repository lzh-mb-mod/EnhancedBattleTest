using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class TroopVM : ViewModel
    {
        private TroopConfig _config;

        public CharacterButtonVM CharacterButton { get; }

        public TextVM NumberText { get; }
        public NumberVM<int> Number { get; }

        public TextVM InvalidText { get; }

        public TroopVM(TeamConfig teamConfig, TroopConfig config, TextObject troopRole, bool isPlayerSide, BattleTypeConfig battleTypeConfig)
        {
            _config = config;
            CharacterButton = new CharacterButtonVM(teamConfig, _config.Character, troopRole, isPlayerSide, battleTypeConfig);
            NumberText = new TextVM(GameTexts.FindText("str_ebt_number"));
            Number = new NumberVM<int>(config.Number, 0, 5000, true);
            Number.OnValueChanged += number => config.Number = number;
            InvalidText = new TextVM(GameTexts.FindText("str_ebt_invalid"));
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            CharacterButton.RefreshValues();
            NumberText.RefreshValues();
            Number.RefreshValues();
            InvalidText.RefreshValues();
        }

        public bool IsValid()
        {
            return !Number.IsIllegal;
        }
    }
}
