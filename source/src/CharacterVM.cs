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
    public class CharacterVM : ViewModel
    {
        private BasicCharacterObject _character;
        private CharacterConfig _config;

        public TextVM CharacterRole { get; }
        public StringItemWithActionVM Name { get; }
        public BasicCharacterObject Character
        {
            get => _character;
            set
            {
                _character = value;
                _config.CharacterId = _character.StringId;
                Name.ActionText = _character.Name.ToString();
                Name.Identifier = this;
            }
        }

        public CharacterVM(CharacterConfig config, TextObject characterRole)
        {
            _config = config;
            CharacterRole = new TextVM(characterRole);
            Name = new StringItemWithActionVM(o => { }, "", this);
            Character = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(config.CharacterId);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            CharacterRole.RefreshValues();
            Name.ActionText = _character.Name.ToString();
        }
    }
}
