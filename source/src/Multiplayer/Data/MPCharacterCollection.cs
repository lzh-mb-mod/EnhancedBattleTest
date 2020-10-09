using System.Collections.Generic;
using EnhancedBattleTest.Data;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Multiplayer.Data
{
    public class MPCharacterCollection : CharacterCollection
    {
        public override List<string> Cultures { get; } = new List<string>();

        public override Dictionary<string, List<Group>> GroupsInCultures { get; } =
            new Dictionary<string, List<Group>>();

        public override bool IsMultiplayer => true;

        public override void Initialize()
        {
            Debug.Assert(MultiplayerClassDivisions.AvailableCultures != null, "Available Cultures should not be null");
            if (MultiplayerClassDivisions.AvailableCultures == null)
                return;
            Cultures.Clear();
            foreach (var eachCulture in MultiplayerClassDivisions.AvailableCultures)
            {
                Cultures.Add(eachCulture.StringId);
                var groupsInCurrentCulture = new List<Group>();
                GroupsInCultures.Add(eachCulture.StringId, groupsInCurrentCulture);
                foreach (var mpHeroClass in MultiplayerClassDivisions.GetMPHeroClasses(eachCulture))
                {
                    var group =
                        groupsInCurrentCulture.Find(g => g.Info.StringId == mpHeroClass.ClassGroup.StringId) as MPGroup;
                    if (group == null)
                    {
                        var newGroup = new MPGroup(mpHeroClass.ClassGroup,
                            mpHeroClass.HeroCharacter.DefaultFormationClass);
                        groupsInCurrentCulture.Add(newGroup);
                        groupsInCurrentCulture.Sort((lhs, rhs) => lhs.Info.FormationClass - rhs.Info.FormationClass);
                        group = newGroup;
                    }

                    group.CharactersInGroup.Add(mpHeroClass.StringId, new MPCharacter(mpHeroClass, group.Info));
                }
            }
        }
    }
}
