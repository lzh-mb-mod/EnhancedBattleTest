using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace EnhancedBattleTest
{
    public class SPCharacterConfigVM : CharacterConfigVM
    {
        private SPCharacterConfig _config = new SPCharacterConfig();
        private bool _isAttacker;
        public bool IsMultiplayer => false;
        public bool IsSingleplayer => true;

        public CharacterViewModel Character { get; } = new CharacterViewModel(CharacterViewModel.StanceTypes.None);

        public TextVM FemaleRatioText { get; }

        public NumberVM<float> FemaleRatio { get; }

        public SPCharacterConfigVM()
        {
            FemaleRatioText = new TextVM(GameTexts.FindText("str_ebt_female_ratio"));
            FemaleRatio = new NumberVM<float>(_config.FemaleRatio, 0, 1, false);
            FemaleRatio.OnValueChanged += femaleRatio =>
            {
                _config.FemaleRatio = femaleRatio;
                SetCharacterToViewModel();
            };
        }

        public override void SetConfig(CharacterConfig config, bool isAttacker)
        {
            if (!(config is SPCharacterConfig spConfig))
                return;
            _config = spConfig;
            _isAttacker = isAttacker;
            FemaleRatio.Value = _config.FemaleRatio;
            SetCharacterToViewModel();
        }

        public override void SelectedCharacterChanged(Character character)
        {
            if (character == null)
                return;
            _config.CharacterId = character.StringId;
            FemaleRatio.Value = _config.FemaleRatio = _config.CharacterObject.IsFemale ? 1 : 0;
            SetCharacterToViewModel();
        }

        private void SetCharacterToViewModel()
        {
            if (!(_config.Character is SPCharacter mpCharacter))
                return;
            var characterObject = _config.CharacterObject;
            FillFrom(_isAttacker, characterObject);
        }

        private void FillFrom(bool isAttacker, BasicCharacterObject character, int seed = -1)
        {
            if (character.Culture != null)
            {
                Character.ArmorColor1 = Utility.ClothingColor1(character.Culture, isAttacker);
                Character.ArmorColor2 = Utility.ClothingColor2(character.Culture, isAttacker);
                Character.BannerCodeText = Utility.BannerFor(character.Culture, isAttacker).Serialize();
            }
            else
            {
                Character.ArmorColor1 = 0;
                Character.ArmorColor2 = 0;
                Character.BannerCodeText = "";
            }
            Character.CharStringId = character.StringId;
            Character.IsFemale = _config.FemaleRatio > 0.5;
            var equipment = character.Equipment;
            Character.EquipmentCode = equipment.CalculateEquipmentCode();
            Character.BodyProperties = null;
            Character.BodyProperties = FaceGen.GetRandomBodyProperties(_config.FemaleRatio > 0.5,
                character.GetBodyPropertiesMin(false), character.GetBodyPropertiesMax(),
                (int)equipment.HairCoverType, seed, character.HairTags, character.BeardTags,
                character.TattooTags).ToString();
            Character.MountCreationKey =
                MountCreationKey.GetRandomMountKey(equipment[10].Item, Common.GetDJB2(character.StringId));
        }
    }
}
