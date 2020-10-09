using System.Linq;
using System.Xml.Serialization;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.SinglePlayer.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest.SinglePlayer.Config
{
    public class SPCharacterConfig : CharacterConfig
    {
        [XmlIgnore]
        private string _characterId;
        public float FemaleRatio;

        public string CharacterId
        {
            get => _characterId;
            set
            {
                if (value == null)
                    return;
                var characterObject = TaleWorlds.Core.Game.Current.ObjectManager.GetObject<CharacterObject>(value);
                if (characterObject == null)
                    return;
                _characterId = value;
                ActualCharacterObject = characterObject;
                Character = new SPCharacter(ActualCharacterObject,
                    new SPGroup(CharacterObject.DefaultFormationClass).Info);
            }
        }

        [XmlIgnore]
        public override Character Character { get; protected set; }

        [XmlIgnore] public override BasicCharacterObject CharacterObject => ActualCharacterObject;

        [XmlIgnore] public CharacterObject ActualCharacterObject;

        public override CharacterConfig Clone()
        {
            var result = new SPCharacterConfig();
            result.CopyFrom(this);
            return result;
        }

        public override void CopyFrom(CharacterConfig other)
        {
            var spOther = other as SPCharacterConfig;
            if (spOther == null)
                return;
            CharacterId = spOther.CharacterId;
            FemaleRatio = spOther.FemaleRatio;
        }

        public SPCharacterConfig()
        {
            CharacterId = TaleWorlds.Core.Game.Current.ObjectManager.GetObjectTypeList<CharacterObject>().First().StringId;
        }

        public SPCharacterConfig(string id, float femaleRatio = 0)
        {
            CharacterId = id;
            FemaleRatio = femaleRatio;
        }
    }
}
