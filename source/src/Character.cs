using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public abstract class Character
    {
        public abstract string StringId { get; }
        public abstract TextObject Name { get; }

        public abstract BasicCultureObject Culture { get; }
        public abstract GroupInfo GroupInfo { get; }
    }
}
