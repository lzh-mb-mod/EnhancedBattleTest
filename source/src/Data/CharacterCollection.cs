using System.Collections.Generic;
using EnhancedBattleTest.Multiplayer.Data;
using EnhancedBattleTest.SinglePlayer.Data;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Data
{

    public abstract class CharacterCollection
    {
        public abstract List<string> Cultures { get; }
        public abstract Dictionary<string, List<Group>> GroupsInCultures { get; }

        public abstract void Initialize();

        public abstract bool IsMultiplayer { get; }

        public static CharacterCollection Create(bool isMultiplayer)
        {
            return isMultiplayer ? (CharacterCollection)new MPCharacterCollection() : new SPCharacterCollection();
        }
    }
}
