using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class MPCharacterCollection : CharacterCollection
    {
        public override List<BasicCultureObject> Cultures { get; } = new List<BasicCultureObject>();

        public override Dictionary<string, List<Group>> GroupsInCultures { get; } =
            new Dictionary<string, List<Group>>();

        public override bool isMultiplayer => true;

        public void Initialize()
        {
            Debug.Assert(MultiplayerClassDivisions.AvailableCultures != null, "Available Cultures should not be null");
            if (MultiplayerClassDivisions.AvailableCultures == null)
                return;
            Cultures.Clear();
            foreach (var eachCulture in MultiplayerClassDivisions.AvailableCultures)
            {
                Cultures.Add(eachCulture);
                var groupsInCurrentCulture = new List<Group>();
                GroupsInCultures.Add(eachCulture.StringId, groupsInCurrentCulture);
                foreach (var mpHeroClass in MultiplayerClassDivisions.GetMPHeroClasses(eachCulture))
                {
                    Group group = groupsInCurrentCulture.Find(g => g.Info.StringId == mpHeroClass.ClassGroup.StringId);
                    if (group == null)
                    {
                        var newGroup = new MPGroup(mpHeroClass.ClassGroup);
                        groupsInCurrentCulture.Add(newGroup);
                        newGroup.CharactersInGroup.Add(mpHeroClass.StringId, new MPCharacter(mpHeroClass, newGroup.Info));
                    }
                    else
                        group.CharactersInGroup.Add(mpHeroClass.StringId, new MPCharacter(mpHeroClass, group.Info));
                }
            }
        }
    }
}
