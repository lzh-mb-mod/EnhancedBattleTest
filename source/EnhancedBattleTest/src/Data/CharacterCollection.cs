using System.Collections.Generic;
using EnhancedBattleTest.SinglePlayer.Data;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Data
{

    public abstract class CharacterCollection
    {
        internal const string NoCultureId = "null";
        public Dictionary<string, BasicCultureObject> Cultures { get; } =
            new Dictionary<string, BasicCultureObject>();
        public abstract Dictionary<string, List<Group>> GroupsInCultures { get; }

        public abstract void Initialize();

        internal static string GetCultureId(BasicCultureObject culture) =>
            string.IsNullOrEmpty(culture?.StringId) ? NoCultureId : culture.StringId;

        protected string RegisterCulture(BasicCultureObject culture)
        {
            string id = GetCultureId(culture);
            if (!Cultures.ContainsKey(id))
                Cultures.Add(id, culture);
            return id;
        }

        public static CharacterCollection Create()
        {
            return new SPCharacterCollection();
        }
    }
}
