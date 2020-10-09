using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI.Basic
{
    public class TextVM : ViewModel
    {
        private TextObject _textObject;
        private string _text;

        public TextObject TextObject
        {
            get => _textObject;
            set
            {
                _textObject = value;
                Text = _textObject.ToString();
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
            }
        }

        public TextVM(TextObject text)
        {
            TextObject = text;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            TextObject = TextObject;
        }
    }
}
