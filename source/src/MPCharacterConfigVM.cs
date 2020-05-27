using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class MPCharacterConfigVM : CharacterConfigVM
    {
        private MPCharacterConfig _config = new MPCharacterConfig();

        public CharacterViewModel Character { get; } = new CharacterViewModel(CharacterViewModel.StanceTypes.None);

        public bool IsMultiplayer => false;
        public bool IsSinglePlayer => true;

        public SelectorVM<SelectorItemVM> FirstPerks { get; }
        public SelectorVM<SelectorItemVM> SecondPerks { get; }

        public MPCharacterConfigVM()
        {
            FirstPerks = new SelectorVM<SelectorItemVM>(0, null);
            SecondPerks = new SelectorVM<SelectorItemVM>(0, null);
        }

        public override void SetConfig(CharacterConfig config)
        {
            _config = config as MPCharacterConfig;
        }

        public override void SelectedCharacterChanged(Character character)
        {
            if (character == null)
                return;
            _config.CharacterId = character.StringId;
            _config.SelectedFirstPerk = 0;
            _config.SelectedSecondPerk = 0;
            SetPerks();
        }

        private void SetPerks()
        {
            if (!(_config.Character is MPCharacter mpCharacter))
                return;
            FirstPerks.Refresh(mpCharacter.HeroClass.GetAllAvailablePerksForListIndex(0).Select(perk => perk.Name), 0,
                vm =>
                {
                    if (vm.SelectedIndex < 0)
                        return;
                    _config.SelectedFirstPerk = vm.SelectedIndex;
                    SetCharacterToViewModel();
                });
            SecondPerks.Refresh(mpCharacter.HeroClass.GetAllAvailablePerksForListIndex(1).Select(perk => perk.Name), 0,
                vm =>
                {
                    if (vm.SelectedIndex < 0)
                        return;
                    _config.SelectedSecondPerk = vm.SelectedIndex;
                    SetCharacterToViewModel();
                });
        }

        private void SetCharacterToViewModel()
        {
            if (!(_config.Character is MPCharacter mpCharacter))
                return;
            var characterObject = mpCharacter.HeroClass.HeroCharacter;
            Character.FillFrom(characterObject);
            var equipment = Utility.GetNewEquipmentsForPerks(_config, true);
            Character.EquipmentCode = equipment.CalculateEquipmentCode();
        }
    }
}
