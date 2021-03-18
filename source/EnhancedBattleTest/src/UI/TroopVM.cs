using EnhancedBattleTest.Config;
using EnhancedBattleTest.UI.Basic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class TroopVM : ViewModel
    {
        public CharacterButtonVM CharacterButton { get; }

        public TextVM NumberText { get; }
        public NumberVM<int> Number { get; }

        public TextVM InvalidText { get; }

        public TroopVM(TeamConfig teamConfig, TroopConfig config, bool isPlayerSide, BattleTypeConfig battleTypeConfig)
        {
            CharacterButton = new CharacterButtonVM(teamConfig, config.Character, isPlayerSide, battleTypeConfig);
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
