using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TaleWorlds.Library;

namespace EnhancedBattleTest
{
    public class NumberVM<T> : ViewModel where T : struct,
        IComparable,
        IComparable<T>,
        IConvertible,
        IEquatable<T>,
        IFormattable
    {
        private readonly T _maxValue;
        private T _number;
        private string _text;
        private bool _isIllegal;

        [DataSourceProperty]
        public T Number
        {
            get => _number;
            set
            {
                if (_number.Equals(value))
                    return;
                _number = value;
                Text = _number.ToString();
                OnPropertyChanged(nameof(Number));
                OnNumberChanged?.Invoke(_number);
            }
        }

        [DataSourceProperty]
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;
                _text = value;
                OnPropertyChanged(nameof(Text));
                UpdateNumber();
            }
        }

        [DataSourceProperty]
        public bool IsIllegal
        {
            get => _isIllegal;
            set
            {
                if (_isIllegal == value)
                    return;
                _isIllegal = value;
                OnPropertyChanged(nameof(IsIllegal));
            }
        }

        public event Action<T> OnNumberChanged;

        public NumberVM(T initialValue, T maxValue)
        {
            _maxValue = maxValue;
            Number = initialValue;
            Text = Number.ToString();
        }

        private void UpdateNumber()
        {
            try
            {
                var number = (T)System.Convert.ChangeType(Text, typeof(T));
                if (number.CompareTo(_maxValue) > 0)
                {
                    IsIllegal = true;
                    return;
                }

                Number = number;
                IsIllegal = false;
            }
            catch
            {
                IsIllegal = true;
            }
        }
    }
}
