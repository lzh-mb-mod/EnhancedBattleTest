using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{

    public abstract class CharacterCollection
    {
        public abstract List<BasicCultureObject> Cultures { get; }
        public abstract Dictionary<string, List<Group>> GroupsInCultures { get; }

        public abstract void Initialize();

        public abstract bool IsMultiplayer { get; }

        public static CharacterCollection Create(bool isMultiplayer)
        {
            return isMultiplayer ? (CharacterCollection)new MPCharacterCollection() : new SPCharacterCollection();
        }
    }
}
