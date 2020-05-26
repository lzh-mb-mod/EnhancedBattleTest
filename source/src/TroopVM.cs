using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class TroopVM : ViewModel
    {
        private TroopConfig _config;

        public CharacterVM Character { get; }

        public TextVM NumberText { get; }
        public NumberVM<int> Number { get; }

        public TextVM InvalidText { get; }

        public TroopVM(TroopConfig config, TextObject troopRole)
        {
            _config = config;
            Character = new CharacterVM(_config.Character, troopRole);
            NumberText = new TextVM(GameTexts.FindText("str_ebt_number"));
            Number = new NumberVM<int>(config.Number, 5000);
            Number.OnNumberChanged += number => config.Number = number;
            InvalidText = new TextVM(GameTexts.FindText("str_ebt_invalid"));
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            Character.RefreshValues();
            NumberText.RefreshValues();
            Number.RefreshValues();
            InvalidText.RefreshValues();
        }
    }
}
