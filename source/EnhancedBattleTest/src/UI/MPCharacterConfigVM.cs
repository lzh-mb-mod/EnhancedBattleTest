using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.Multiplayer.Config;
using EnhancedBattleTest.Multiplayer.Data;
using EnhancedBattleTest.UI.Basic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using FaceGen = TaleWorlds.Core.FaceGen;

namespace EnhancedBattleTest.UI
{
    public class MPCharacterConfigVM : CharacterConfigVM
    {
        private MPCharacterConfig _config = new MPCharacterConfig();
        private bool _isAttacker;

        public bool IsMultiplayer => true;
        public bool IsSinglePlayer => false;

        public CharacterViewModel Character { get; } = new CharacterViewModel(CharacterViewModel.StanceTypes.None);

        public SelectorVM<SelectorItemVM> FirstPerks { get; }
        public SelectorVM<SelectorItemVM> SecondPerks { get; }


        public TextVM IsHeroText { get; }

        public TextVM MaleRatioText { get; }
        public TextVM FemaleRatioText { get; }
        public BoolVM IsHero { get; }
        public NumberVM<float> FemaleRatio { get; }

        public MPCharacterConfigVM()
        {
            FirstPerks = new SelectorVM<SelectorItemVM>(0, null);
            SecondPerks = new SelectorVM<SelectorItemVM>(0, null);

            IsHeroText = new TextVM(GameTexts.FindText("str_ebt_is_hero"));
            MaleRatioText = new TextVM(GameTexts.FindText("str_ebt_male_ratio"));
            FemaleRatioText = new TextVM(GameTexts.FindText("str_ebt_female_ratio"));
            IsHero = new BoolVM(_config.IsHero);
            FemaleRatio = new NumberVM<float>(_config.FemaleRatio, 0, 1, false);
            IsHero.OnValueChanged += b =>
            {
                _config.IsHero = b;
                SetCharacterToViewModel();
            };
            FemaleRatio.OnValueChanged += femaleRatio =>
            {
                _config.FemaleRatio = femaleRatio;
                SetCharacterToViewModel();
            };
        }

        public override void SetConfig(TeamConfig teamConfig, CharacterConfig config, bool isAttacker)
        {
            if (!(config is MPCharacterConfig mpConfig))
                return;
            _config = mpConfig;
            _isAttacker = isAttacker;
            FirstPerks.SelectedIndex = _config.SelectedFirstPerk;
            SecondPerks.SelectedIndex = _config.SelectedSecondPerk;
            IsHero.Value = _config.IsHero;
            FemaleRatio.Value = _config.FemaleRatio;
            SetCharacterToViewModel();
        }

        public override void SelectedCharacterChanged(Character character)
        {
            if (character == null)
                return;
            _config.CharacterId = character.StringId;
            _config.SelectedFirstPerk = 0;
            _config.SelectedSecondPerk = 0;
            _config.IsHero = IsHero.Value;
            _config.FemaleRatio = FemaleRatio.Value;
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
            var characterObject = _config.IsHero ? mpCharacter.HeroClass.HeroCharacter : mpCharacter.HeroClass.TroopCharacter;
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
            var equipment = Utility.GetNewEquipmentsForPerks(_config.HeroClass, _config.IsHero,
                _config.SelectedFirstPerk, _config.SelectedSecondPerk, _config.IsHero);
            Character.EquipmentCode = equipment.CalculateEquipmentCode();
            Character.BodyProperties = null;
            Character.BodyProperties = FaceGen.GetRandomBodyProperties(character.Race, _config.FemaleRatio > 0.5,
                character.GetBodyPropertiesMin(false), character.GetBodyPropertiesMax(),
                (int)equipment.HairCoverType, seed, character.HairTags, character.BeardTags,
                character.TattooTags).ToString();
            Character.MountCreationKey =
                MountCreationKey.GetRandomMountKey(equipment[10].Item, Common.GetDJB2(character.StringId)).ToString();
        }
    }
}
