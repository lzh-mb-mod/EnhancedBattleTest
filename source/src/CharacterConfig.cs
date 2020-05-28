using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    [XmlInclude(typeof(MPCharacterConfig))]
    [XmlInclude(typeof(SPCharacterConfig))]
    public abstract class CharacterConfig
    {
        [XmlIgnore]
        public abstract Character Character { get; protected set; }

        public abstract CharacterConfig Clone();
        public abstract void CopyFrom(CharacterConfig other);

        public static CharacterConfig Create(bool isMultiplayer)
        {
            return isMultiplayer ? (CharacterConfig) new MPCharacterConfig() : new SPCharacterConfig();
        }
    }
}
