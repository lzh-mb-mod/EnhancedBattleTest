using EnhancedBattleTest.Data;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Multiplayer.Data
{
    public class MPCharacter : Character
    {
        public MultiplayerClassDivisions.MPHeroClass HeroClass { get; }

        public override string StringId => HeroClass.StringId;

        public override TextObject Name => HeroClass.HeroName;
        public override BasicCultureObject Culture => HeroClass.Culture;
        public override GroupInfo GroupInfo { get; }

        public MPCharacter(MultiplayerClassDivisions.MPHeroClass heroClass, GroupInfo groupInfo)
        {
            HeroClass = heroClass;
            GroupInfo = groupInfo;
        }
    }
}
