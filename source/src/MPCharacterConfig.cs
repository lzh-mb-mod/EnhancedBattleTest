using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class MPCharacterConfig : CharacterConfig
    {
        [XmlIgnore]
        private string _characterId;
        public int SelectedFirstPerk;
        public int SelectedSecondPerk;
        public bool IsHero;
        public bool IsFemale;

        public string CharacterId
        {
            get => _characterId;
            set
            {
                _characterId = value;
                HeroClass = Game.Current.ObjectManager.GetObject<MultiplayerClassDivisions.MPHeroClass>(_characterId);
                Character = new MPCharacter(HeroClass, new MPGroup(HeroClass.ClassGroup).Info);
            }
        }

        [XmlIgnore]
        public override Character Character { get; protected set; }

        [XmlIgnore]
        public BasicCharacterObject CharacterObject => IsHero ? HeroClass.HeroCharacter : HeroClass.TroopCharacter;

        [XmlIgnore]
        public MultiplayerClassDivisions.MPHeroClass HeroClass;

        public override CharacterConfig Clone()
        {
            var result = new MPCharacterConfig();
            result.CopyFrom(this);
            return result;
        }

        public override void CopyFrom(CharacterConfig other)
        {
            var mpOther = other as MPCharacterConfig;
            if (mpOther == null)
                return;
            CharacterId = mpOther.CharacterId;
            SelectedFirstPerk = mpOther.SelectedFirstPerk;
            SelectedSecondPerk = mpOther.SelectedSecondPerk;
            IsHero = mpOther.IsHero;
            IsFemale = mpOther.IsFemale;
        }

        public MPCharacterConfig()
        {
            CharacterId = Game.Current.ObjectManager.GetObjectTypeList<MultiplayerClassDivisions.MPHeroClass>().First().StringId;
        }

    }
}
