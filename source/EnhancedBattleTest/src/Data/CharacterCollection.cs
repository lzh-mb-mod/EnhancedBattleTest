using System.Collections.Generic;
using EnhancedBattleTest.SinglePlayer.Data;

namespace EnhancedBattleTest.Data
{

    public abstract class CharacterCollection
    {
        public abstract List<string> Cultures { get; }
        public abstract Dictionary<string, List<Group>> GroupsInCultures { get; }

        public abstract void Initialize();

        public static CharacterCollection Create()
        {
            return new SPCharacterCollection();
        }
    }
}
