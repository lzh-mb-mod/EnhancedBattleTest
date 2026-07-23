using System.Xml.Serialization;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.SinglePlayer.Config;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Config
{
    [XmlInclude(typeof(SPCharacterConfig))]
    public abstract class CharacterConfig
    {
        [XmlIgnore]
        public abstract Character Character { get; protected set; }

        public abstract BasicCharacterObject CharacterObject { get; }

        public abstract CharacterConfig Clone();
        public abstract void CopyFrom(CharacterConfig other);

        public static CharacterConfig Create()
        {
            return new SPCharacterConfig();
        }

        public static CharacterConfig Create(string id, float femaleRatio = 0)
        {
            return new SPCharacterConfig(id, femaleRatio);
        }
    }
}
