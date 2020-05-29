using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public class SPCharacterCollection : CharacterCollection
    {
        public override List<BasicCultureObject> Cultures { get; } = new List<BasicCultureObject>();
        public override Dictionary<string, List<Group>> GroupsInCultures { get; } = new Dictionary<string, List<Group>>();
        public override bool IsMultiplayer => false;

        public override void Initialize()
        {
            Cultures.Clear();

            foreach (var characterObject in Game.Current.ObjectManager.GetObjectTypeList<CharacterObject>())
            {
                if (characterObject.IsTemplate || characterObject.IsChildTemplate)
                    continue;
                var culture = characterObject.Culture;
                if (!GroupsInCultures.TryGetValue(culture.StringId, out var groupsInCurrentCulture))
                {
                    GroupsInCultures[culture.StringId] = groupsInCurrentCulture = new List<Group>();
                    Cultures.Add(culture);
                }

                var formationClass = characterObject.CurrentFormationClass;
                var group = groupsInCurrentCulture.Find(g => g.Info.FormationClass == formationClass) as SPGroup;
                if (group == null)
                {
                    var newGroup = new SPGroup(formationClass);
                    groupsInCurrentCulture.Add(newGroup);
                    groupsInCurrentCulture.Sort((lhs, rhs) => lhs.Info.FormationClass - rhs.Info.FormationClass);
                    group = newGroup;
                }

                var occupation = characterObject.Occupation;
                if (!group.OccupationsInGroup.TryGetValue(occupation, out var charactersInOccupation))
                {
                    group.OccupationsInGroup[occupation] = charactersInOccupation = new CharactersInOccupation();
                }

                charactersInOccupation.Characters.Add(characterObject.StringId,
                    new SPCharacter(characterObject, group.Info));
            }
        }
    }
}
