using System.Xml.Serialization;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.Multiplayer.Config;
using EnhancedBattleTest.SinglePlayer.Config;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Config
{
    [XmlInclude(typeof(MPCharacterConfig))]
    [XmlInclude(typeof(SPCharacterConfig))]
    public abstract class CharacterConfig
    {
        [XmlIgnore]
        public abstract Character Character { get; protected set; }

        public abstract BasicCharacterObject CharacterObject { get; }

        public abstract CharacterConfig Clone();
        public abstract void CopyFrom(CharacterConfig other);

        public static CharacterConfig Create(bool isMultiplayer)
        {
            if (isMultiplayer)
            {
                return new MPCharacterConfig();
            }
            else
            {
                return new SPCharacterConfig();
            }
        }

        public static CharacterConfig Create(bool isMultiplayer, string id, float femaleRatio = 0)
        {
            if (isMultiplayer)
            {
                return new MPCharacterConfig(id, femaleRatio);
            }
            else
            {
                return new SPCharacterConfig(id, femaleRatio);
            }
        }
    }
}
