using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace EnhancedBattleTest
{
    public class BoolVM : ViewModel
    {
        private bool _value;

        [DataSourceProperty]
        public bool Value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;
                _value = value;
                OnPropertyChanged(nameof(Value));
                OnValueChanged?.Invoke(_value);
            }
        }

        public event Action<bool> OnValueChanged;
        public BoolVM(bool value)
        {
            Value = value;
        }
    }

}
