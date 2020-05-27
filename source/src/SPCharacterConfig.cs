using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class SPCharacterConfig : CharacterConfig
    {
        [XmlIgnore]
        public string _characterId;

        public string CharacterId
        {
            get => _characterId;
            set
            {
                _characterId = value;
                CharacterObject = Game.Current.ObjectManager.GetObject<CharacterObject>(_characterId);
                Character = new SPCharacter(CharacterObject, new SPGroup(CharacterObject.CurrentFormationClass).Info);

            }
        }

        [XmlIgnore]
        public override Character Character { get; protected set; }

        [XmlIgnore]
        public CharacterObject CharacterObject;

        public override CharacterConfig Clone()
        {
            return new SPCharacterConfig()
            {
                CharacterId = CharacterId,
            };
        }

        public override void CopyFrom(CharacterConfig other)
        {
            var spOther = other as SPCharacterConfig;
            if (spOther == null)
                return;
            CharacterId = spOther.CharacterId;
        }

        public SPCharacterConfig()
        {
            CharacterId = Game.Current.ObjectManager.GetObjectTypeList<CharacterObject>().First().StringId; ;
        }
    }
}
