using System;
using System.Globalization;
using TaleWorlds.Library;

namespace EnhancedBattleTest.UI.Basic
{
    public class NumberVM<T> : ViewModel where T : struct,
        IComparable,
        IComparable<T>,
        IConvertible,
        IEquatable<T>,
        IFormattable
    {
        private T _value;
        private string _text;
        private bool _isIllegal;
        private bool _isDiscrete;

        public T Min { get; }

        public T Max { get; }

        [DataSourceProperty]
        public T Value
        {
            get => _value;
            set
            {
                if (_value.Equals(value))
                    return;
                if (IsDiscrete)
                {
                    _value = value;
                    Text = _value.ToString();
                }
                else
                {
                    var oldValue = _value.ToDouble(CultureInfo.CurrentCulture);
                    var newValue = value.ToDouble(CultureInfo.CurrentCulture);
                    if (Math.Abs(oldValue - newValue) < 0.01)
                        return;
                    _value = value;
                    Text = newValue.ToString("F");
                }

                OnPropertyChanged(nameof(Value));
                OnValueChanged?.Invoke(_value);
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
        public bool IsDiscrete
        {
            get => this._isDiscrete;
            set
            {
                if (value == this._isDiscrete)
                    return;
                this._isDiscrete = value;
                this.OnPropertyChanged(nameof(IsDiscrete));
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

        public event Action<T> OnValueChanged;

        public NumberVM(T initialValue, T min, T max, bool isDiscrete)
        {
            IsDiscrete = isDiscrete;
            Min = min;
            Max = max;
            Value = initialValue;
            Text = IsDiscrete ? _value.ToString() : _value.ToDouble(CultureInfo.CurrentCulture).ToString("F");
        }

        private void UpdateNumber()
        {
            try
            {
                var number = (T)Convert.ChangeType(Text, typeof(T));
                if (number.CompareTo(Min) < 0 || number.CompareTo(Max) > 0)
                {
                    IsIllegal = true;
                    return;
                }

                Value = number;
                IsIllegal = false;
            }
            catch
            {
                IsIllegal = true;
            }
        }
    }
}
