using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.SinglePlayer.Config;
using EnhancedBattleTest.SinglePlayer.Data;
using EnhancedBattleTest.UI.Basic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace EnhancedBattleTest.UI
{
    public class SPCharacterConfigVM : CharacterConfigVM
    {
        private TeamConfig _teamConfig;
        private SPCharacterConfig _config = new SPCharacterConfig();
        private bool _isAttacker;
        public bool IsMultiplayer => false;
        public bool IsSingleplayer => true;

        public CharacterViewModel Character { get; } = new CharacterViewModel(CharacterViewModel.StanceTypes.None);

        public TextVM MaleRatioText { get; }
        public TextVM FemaleRatioText { get; }

        public NumberVM<float> FemaleRatio { get; }

        public TextVM OpenEncyclopediaText { get; }

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
            MaleRatioText = new TextVM(GameTexts.FindText("str_ebt_male_ratio"));
            FemaleRatioText = new TextVM(GameTexts.FindText("str_ebt_female_ratio"));
            OpenEncyclopediaText = new TextVM(GameTexts.FindText("str_ebt_open_encyclopedia"));
            FemaleRatio = new NumberVM<float>(_config.FemaleRatio, 0, 1, false);
            FemaleRatio.OnValueChanged += femaleRatio =>
            {
                _config.FemaleRatio = femaleRatio;
                SetCharacterToViewModel();
            };
        }

        public override void SetConfig(TeamConfig teamConfig, CharacterConfig config, bool isAttacker)
        {
            if (!(config is SPCharacterConfig spConfig))
                return;
            _teamConfig = teamConfig;
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
            FillFrom(characterObject);
        }

        private void FillFrom(BasicCharacterObject character, int seed = -1)
        {
            if (_teamConfig != null)
            {
                var banner = _teamConfig.GetPreviewBanner();
                Character.BannerCodeText = banner.Serialize();
                Character.ArmorColor1 = banner.BannerDataList.Count > 0
                    ? BannerManager.GetColor(banner.BannerDataList[0].ColorId)
                    : uint.MaxValue;
                Character.ArmorColor2 = banner.BannerDataList.Count > 1
                    ? BannerManager.GetColor(banner.BannerDataList[1].ColorId)
                    : uint.MaxValue;
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
            Character.BodyProperties = FaceGen.GetRandomBodyProperties(character.Race, _config.FemaleRatio > 0.5,
                character.GetBodyPropertiesMin(false), character.GetBodyPropertiesMax(),
                (int)equipment.HairCoverType, seed, character.HairTags, character.BeardTags,
                character.TattooTags).ToString();
            Character.MountCreationKey =
                MountCreationKey.GetRandomMountKey(equipment[10].Item, Common.GetDJB2(character.StringId)).ToString();
        }
    }
}
