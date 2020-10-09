using System.Collections.Generic;
using EnhancedBattleTest.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest.SinglePlayer.Data
{
    public class SPCharacterCollection : CharacterCollection
    {
        public override List<string> Cultures { get; } = new List<string>();
        public override Dictionary<string, List<Group>> GroupsInCultures { get; } = new Dictionary<string, List<Group>>();
        public override bool IsMultiplayer => false;

        public override void Initialize()
        {
            Cultures.Clear();

            foreach (var basicCharacterObject in TaleWorlds.Core.Game.Current.ObjectManager.GetObjectTypeList<BasicCharacterObject>())
            {
                if (basicCharacterObject is CharacterObject characterObject)
                {
                    if (characterObject.IsTemplate || characterObject.IsChildTemplate)
                        continue;
                    var culture = characterObject.Culture;
                    var cultureId = culture?.StringId ?? "null";
                    if (!Cultures.Contains(cultureId))
                        Cultures.Add(cultureId);
                    if (!GroupsInCultures.TryGetValue(cultureId, out var groupsInCurrentCulture))
                    {
                        GroupsInCultures[cultureId] = groupsInCurrentCulture = new List<Group>();
                    }

                    var formationClass = characterObject.DefaultFormationClass;
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
}
