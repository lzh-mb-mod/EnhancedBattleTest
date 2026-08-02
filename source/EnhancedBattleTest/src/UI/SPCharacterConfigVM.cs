using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.SinglePlayer.Config;
using EnhancedBattleTest.SinglePlayer.Data;
using EnhancedBattleTest.UI.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class SPCharacterConfigVM : CharacterConfigVM
    {
        private static readonly string[] EncyclopediaSkillOrder =
        {
            "OneHanded", "TwoHanded", "Polearm", "Bow", "Crossbow",
            "Throwing", "Riding", "Athletics", "Crafting", "Scouting",
            "Tactics", "Roguery", "Charm", "Leadership", "Trade",
            "Steward", "Medicine", "Engineering", "Mariner", "Boatswain",
            "Shipmaster"
        };

        private PartyConfig _partyConfig;
        private SPCharacterConfig _config = new SPCharacterConfig();
        private bool _isAttacker;
        private BasicCharacterObject _preferredBannerCharacter;
        private bool _useSelectedCharacterForBanner;
        private bool _isGenderOverrideEnabled;
        private bool _isTierVisible;
        private bool _isEquipmentSetSelectorVisible;
        private string _equipmentSetText;
        public CharacterPreviewVM Character { get; }

        public TextVM MaleRatioText { get; }
        public TextVM FemaleRatioText { get; }

        public NumberVM<float> FemaleRatio { get; }

        public TextVM OpenEncyclopediaText { get; }
        public TextVM OverrideGenderText { get; }
        public TextVM LevelText { get; }
        public TextVM LevelValue { get; }
        public TextVM TierText { get; }
        public TextVM TierValue { get; }
        public TextVM GroupText { get; }
        public TextVM GroupValue { get; }
        public TextVM SkillsText { get; }
        public TextVM EquipmentText { get; }
        public MBBindingList<CharacterReviewItemVM> Skills { get; } =
            new MBBindingList<CharacterReviewItemVM>();
        public MBBindingList<CharacterEquipmentItemVM> LeftEquipment { get; } =
            new MBBindingList<CharacterEquipmentItemVM>();
        public MBBindingList<CharacterEquipmentItemVM> RightEquipment { get; } =
            new MBBindingList<CharacterEquipmentItemVM>();

        [DataSourceProperty]
        public bool IsEquipmentSetSelectorVisible
        {
            get => _isEquipmentSetSelectorVisible;
            private set
            {
                if (_isEquipmentSetSelectorVisible == value)
                    return;

                _isEquipmentSetSelectorVisible = value;
                OnPropertyChangedWithValue(
                    value,
                    nameof(IsEquipmentSetSelectorVisible));
            }
        }

        [DataSourceProperty]
        public string EquipmentSetText
        {
            get => _equipmentSetText;
            private set
            {
                if (_equipmentSetText == value)
                    return;

                _equipmentSetText = value;
                OnPropertyChangedWithValue(value, nameof(EquipmentSetText));
            }
        }

        [DataSourceProperty]
        public bool IsGenderOverrideEnabled
        {
            get => _isGenderOverrideEnabled;
            set
            {
                if (_isGenderOverrideEnabled == value)
                    return;

                _isGenderOverrideEnabled = value;
                _config.OverrideGender = value;
                OnPropertyChangedWithValue(
                    value,
                    nameof(IsGenderOverrideEnabled));
                SetCharacterToViewModel();
            }
        }

        [DataSourceProperty]
        public bool IsTierVisible
        {
            get => _isTierVisible;
            private set
            {
                if (_isTierVisible == value)
                    return;

                _isTierVisible = value;
                OnPropertyChangedWithValue(value, nameof(IsTierVisible));
            }
        }

        public void OpenEncyclopedia()
        {
            var link = _config.ActualCharacterObject?.EncyclopediaLink;
            if (link != null)
                Campaign.Current.EncyclopediaManager.GoToLink(_config.ActualCharacterObject.EncyclopediaLink);
            else
                Campaign.Current.EncyclopediaManager.GoToLink("LastPage", null);
        }

        public SPCharacterConfigVM()
        {
            Character = new CharacterPreviewVM(
                CharacterViewModel.StanceTypes.None,
                ZoomIn,
                ZoomOut);
            MaleRatioText = new TextVM(GameTexts.FindText("str_ebt_male_ratio"));
            FemaleRatioText = new TextVM(GameTexts.FindText("str_ebt_female_ratio"));
            OpenEncyclopediaText = new TextVM(GameTexts.FindText("str_ebt_open_encyclopedia"));
            OverrideGenderText =
                new TextVM(GameTexts.FindText("str_ebt_override_gender"));
            LevelText = new TextVM(GameTexts.FindText("str_ebt_level"));
            LevelValue = new TextVM(new TextObject("0"));
            TierText = new TextVM(GameTexts.FindText("str_party_troop_tier"));
            TierValue = new TextVM(new TextObject("0"));
            GroupText = new TextVM(GameTexts.FindText("str_ebt_group"));
            GroupValue = new TextVM(new TextObject(string.Empty));
            SkillsText = new TextVM(GameTexts.FindText("str_ebt_skills"));
            EquipmentText =
                new TextVM(GameTexts.FindText("str_ebt_equipment"));
            FemaleRatio = new NumberVM<float>(_config.FemaleRatio, 0, 1, false);
            FemaleRatio.OnValueChanged += femaleRatio =>
            {
                _config.FemaleRatio = femaleRatio;
                SetCharacterToViewModel();
            };
        }

        public override void SetConfig(
            PartyConfig partyConfig,
            CharacterConfig config,
            bool isAttacker,
            BasicCharacterObject preferredBannerCharacter,
            bool useSelectedCharacterForBanner)
        {
            if (!(config is SPCharacterConfig spConfig))
                return;
            _partyConfig = partyConfig;
            _config = spConfig;
            _isAttacker = isAttacker;
            _preferredBannerCharacter = preferredBannerCharacter;
            _useSelectedCharacterForBanner = useSelectedCharacterForBanner;
            IsGenderOverrideEnabled = _config.OverrideGender;
            FemaleRatio.Value = _config.FemaleRatio;
            SetCharacterToViewModel();
        }

        public override void SelectedCharacterChanged(Character character)
        {
            if (character == null)
                return;
            _config.CharacterId = character.StringId;
            _config.EquipmentSetIndex = 0;
            FemaleRatio.Value = _config.FemaleRatio = _config.CharacterObject.IsFemale ? 1 : 0;
            SetCharacterToViewModel();
        }

        public void ExecuteSelectPreviousEquipmentSet()
        {
            IReadOnlyList<Equipment> equipmentSets =
                _config.GetBattleEquipmentSets();
            if (equipmentSets.Count < 2)
                return;

            _config.EquipmentSetIndex--;
            if (_config.EquipmentSetIndex < 0)
                _config.EquipmentSetIndex = equipmentSets.Count - 1;
            SetCharacterToViewModel();
        }

        public void ExecuteSelectNextEquipmentSet()
        {
            IReadOnlyList<Equipment> equipmentSets =
                _config.GetBattleEquipmentSets();
            if (equipmentSets.Count < 2)
                return;

            _config.EquipmentSetIndex++;
            if (_config.EquipmentSetIndex >= equipmentSets.Count)
                _config.EquipmentSetIndex = 0;
            SetCharacterToViewModel();
        }

        protected override void OnPreviewZoomChanged(float value)
        {
            Character.PreviewZoom = value;
        }

        private void SetCharacterToViewModel()
        {
            if (!(_config.Character is SPCharacter mpCharacter))
                return;
            var characterObject = _config.CharacterObject;
            FillFrom(characterObject);
        }

        private void FillFrom(BasicCharacterObject character, int seed = -1)
        {
            if (_partyConfig != null)
            {
                BasicCharacterObject preferredCharacter =
                    _useSelectedCharacterForBanner
                        ? character
                        : _preferredBannerCharacter;
                _partyConfig.ResolveAppearance(
                    _isAttacker,
                    out Banner banner,
                    out var colors,
                    preferredCharacter);
                Character.ArmorColor1 = colors.Item1;
                Character.ArmorColor2 = colors.Item2;
                Character.BannerCodeText = banner.Serialize();
            }
            else
            {
                Character.ArmorColor1 = 0;
                Character.ArmorColor2 = 0;
                Character.BannerCodeText = "";
            }
            Character.CharStringId = character.StringId;
            bool isFemale = _config.OverrideGender
                ? _config.FemaleRatio > 0.5
                : character.IsFemale;
            Character.IsFemale = isFemale;
            Equipment equipment = ResolveEquipment(character);
            Character.EquipmentCode = equipment.CalculateEquipmentCode();
            Character.BodyProperties = null;
            Character.BodyProperties = FaceGen.GetRandomBodyProperties(character.Race, isFemale,
                character.GetBodyPropertiesMin(false), character.GetBodyPropertiesMax(),
                (int)equipment.HairCoverType, seed,
                character.BodyPropertyRange.HairTags,
                character.BodyPropertyRange.BeardTags,
                character.BodyPropertyRange.TattooTags,
                0f).ToString();
            Character.MountCreationKey =
                MountCreationKey.GetRandomMountKey(equipment[10].Item, Common.GetDJB2(character.StringId)).ToString();
            RefreshReview(character, equipment);
        }

        private Equipment ResolveEquipment(BasicCharacterObject character)
        {
            IReadOnlyList<Equipment> equipmentSets =
                _config.GetBattleEquipmentSets();
            IsEquipmentSetSelectorVisible = equipmentSets.Count > 1;

            int selectedIndex = _config.EquipmentSetIndex;
            if (selectedIndex < 0 || selectedIndex >= equipmentSets.Count)
                selectedIndex = _config.EquipmentSetIndex = 0;

            var text = new TextObject("{=vggt7exj}Set {CURINDEX}/{COUNT}");
            text.SetTextVariable("CURINDEX", selectedIndex + 1);
            text.SetTextVariable("COUNT", equipmentSets.Count);
            EquipmentSetText = text.ToString();
            return equipmentSets.ElementAtOrDefault(selectedIndex)
                   ?? character.Equipment;
        }

        private void RefreshReview(
            BasicCharacterObject character,
            Equipment equipment)
        {
            LevelValue.Text = character.Level.ToString();
            CharacterObject characterObject = character as CharacterObject;
            IsTierVisible = characterObject != null && !characterObject.IsHero;
            TierValue.Text = IsTierVisible
                ? characterObject.Tier.ToString()
                : string.Empty;
            GroupValue.Text =
                _config.Character?.GroupInfo?.Name?.ToString()
                ?? string.Empty;
            Skills.Clear();
            var reviewSkills =
                TaleWorlds.CampaignSystem.Extensions.Skills.All
                    .OrderBy(GetEncyclopediaSkillOrder)
                    .ThenBy(skill => skill.StringId)
                    .ToList();
            foreach (SkillObject skill in reviewSkills)
            {
                int value = character.GetSkillValue(skill);
                if (value > 0)
                {
                    Skills.Add(new CharacterReviewItemVM(skill, value));
                }
            }

            LeftEquipment.Clear();
            AddEquipment(
                LeftEquipment,
                equipment,
                EquipmentIndex.Head,
                EquipmentIndex.Cape,
                EquipmentIndex.Body,
                EquipmentIndex.Gloves,
                EquipmentIndex.Leg,
                EquipmentIndex.Horse,
                EquipmentIndex.HorseHarness);

            RightEquipment.Clear();
            AddEquipment(
                RightEquipment,
                equipment,
                EquipmentIndex.Weapon0,
                EquipmentIndex.Weapon1,
                EquipmentIndex.Weapon2,
                EquipmentIndex.Weapon3,
                EquipmentIndex.ExtraWeaponSlot);
        }

        private static void AddEquipment(
            MBBindingList<CharacterEquipmentItemVM> target,
            Equipment equipment,
            params EquipmentIndex[] indices)
        {
            foreach (EquipmentIndex index in indices)
            {
                var item =
                    new CharacterEquipmentItemVM(equipment[index].Item);
                if (index == EquipmentIndex.HorseHarness && item.HasItem)
                    item.Type = ItemObject.ItemTypeEnum.Horse.ToString();
                target.Add(item);
            }
        }

        private static int GetEncyclopediaSkillOrder(SkillObject skill)
        {
            int index = Array.FindIndex(
                EncyclopediaSkillOrder,
                id => id.Equals(
                    skill.StringId,
                    StringComparison.InvariantCultureIgnoreCase));
            return index >= 0 ? index : int.MaxValue;
        }
    }
}
