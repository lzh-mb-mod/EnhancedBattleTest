using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest
{
    public class TroopGroup : ViewModel
    {
        private readonly TroopGroupConfig _config;
        private MBBindingList<TroopVM> _troops;

        [DataSourceProperty]
        public MBBindingList<TroopVM> Troops
        {
            get => _troops;
            set
            {
                if (_troops == value)
                    return;
                _troops = value;
                OnPropertyChanged(nameof(Troops));
            }
        }

        public TroopGroup(TroopGroupConfig config, bool isPlayerSide, BattleTypeConfig battleTypeConfig)
        {
            _config = config;
            Troops = new MBBindingList<TroopVM>();
            for (int i = 0; i < _config.Troops.Length; ++i)
            {
                GameTexts.SetVariable("TroopIndex", i);
                Troops.Add(new TroopVM(_config.Troops[i], GameTexts.FindText("str_ebt_troop_role", "Soldiers"),
                    isPlayerSide, battleTypeConfig));
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            foreach (var troopVm in Troops)
            {
                troopVm.RefreshValues();
            }
        }

        public bool IsValid()
        {
            foreach (var troopVm in _troops)
            {
                if (!troopVm.IsValid())
                    return false;
            }

            return true;
        }
    }
}
