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
    public class CharacterButtonVM : ViewModel
    {
        private readonly CharacterConfig _config;

        public TextVM CharacterRole { get; }
        public StringItemWithActionVM Name { get; }

        public CharacterButtonVM(CharacterConfig config, TextObject characterRole)
        {
            _config = config;
            CharacterRole = new TextVM(characterRole);
            Name = new StringItemWithActionVM(o =>
            {
                EnhancedBattleTestSubModule.Instance.SelectCharacter(new CharacterSelectionData(_config.Clone(),
                    characterConfig =>
                    {
                        _config.CopyFrom(characterConfig);
                    }, false));
            }, _config.Character.Name.ToString(), this);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            CharacterRole.RefreshValues();
            Name.ActionText = _config.Character.Name.ToString();
        }
    }
}
