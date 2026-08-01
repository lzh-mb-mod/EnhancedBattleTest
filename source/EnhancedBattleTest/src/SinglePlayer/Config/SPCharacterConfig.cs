using System.Linq;
using System.Xml.Serialization;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.SinglePlayer.Data;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest.SinglePlayer.Config
{
    public class SPCharacterConfig : CharacterConfig
    {
        [XmlIgnore]
        private string _characterId;
        public bool OverrideGender;
        public float FemaleRatio;
        public int EquipmentSetIndex;

        public string CharacterId
        {
            get => _characterId;
            set
            {
                if (value == null)
                    return;
                _characterId = value;
                ActualCharacterObject = null;
                Character = null;
                var characterObject = TaleWorlds.Core.Game.Current.ObjectManager.GetObject<CharacterObject>(value);
                if (characterObject == null)
                    return;
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
            OverrideGender = spOther.OverrideGender;
            FemaleRatio = spOther.FemaleRatio;
            EquipmentSetIndex = spOther.EquipmentSetIndex;
        }

        public IReadOnlyList<Equipment> GetBattleEquipmentSets()
        {
            return GetBattleEquipmentSets(ActualCharacterObject);
        }

        public static IReadOnlyList<Equipment> GetBattleEquipmentSets(
            CharacterObject character)
        {
            var result = new List<Equipment>();
            Equipment heroEquipment = character?.HeroObject?.BattleEquipment;
            if (heroEquipment != null)
                result.Add(heroEquipment);

            if (character != null)
            {
                foreach (Equipment equipment in character.BattleEquipments)
                {
                    if (equipment != null
                        && !result.Any(existing =>
                            existing.IsEquipmentEqualTo(equipment)))
                    {
                        result.Add(equipment);
                    }
                }
            }

            if (result.Count == 0 && character?.Equipment != null)
                result.Add(character.Equipment);
            return result;
        }

        public Equipment GetSelectedEquipment()
        {
            IReadOnlyList<Equipment> equipmentSets =
                GetBattleEquipmentSets();
            return EquipmentSetIndex >= 0
                   && EquipmentSetIndex < equipmentSets.Count
                ? equipmentSets[EquipmentSetIndex]
                : null;
        }

        public SPCharacterConfig()
        {
            CharacterId = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>().First().BasicTroop.StringId;
        }

        public SPCharacterConfig(string id, float femaleRatio = 0)
        {
            CharacterId = id;
            FemaleRatio = femaleRatio;
        }
    }
}
