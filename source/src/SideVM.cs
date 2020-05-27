using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class SideVM : ViewModel
    {
        private readonly TeamConfig _config;

        public TextVM Name { get; }

        public CharacterButtonVM General { get; }

        public TextVM EnableGeneralText { get; }

        public BoolVM EnableGeneral { get; }

        public TroopGroup TroopGroup { get; }

        public SideVM(TeamConfig config, TextObject name)
        {
            _config = config;
            Name = new TextVM(name);
            General = new CharacterButtonVM(_config.General,
                GameTexts.FindText("str_ebt_troop_role", "general"));
            EnableGeneralText = new TextVM(GameTexts.FindText("str_ebt_enable"));
            EnableGeneral = new BoolVM(_config.HasGeneral);
            EnableGeneral.OnValueChanged += value => _config.HasGeneral = value;

            TroopGroup = new TroopGroup(config.Troops);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            Name.RefreshValues();
            General.RefreshValues();
            TroopGroup.RefreshValues();
        }
    }
}
