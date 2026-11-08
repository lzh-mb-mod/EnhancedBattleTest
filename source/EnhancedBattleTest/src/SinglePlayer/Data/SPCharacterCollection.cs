using System.Collections.Generic;
using System.Linq;
using EnhancedBattleTest.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace EnhancedBattleTest.SinglePlayer.Data
{
    public class SPCharacterCollection : CharacterCollection
    {
        public override Dictionary<string, List<Group>> GroupsInCultures { get; } =
            new Dictionary<string, List<Group>>();

        internal static bool IsSelectable(CharacterObject character) =>
            character != null && !character.IsTemplate && !character.IsChildTemplate
            && !string.IsNullOrWhiteSpace(character.StringId);

        // CharacterObject.Culture casts non-hero cultures to CultureObject in v1.2.12.
        internal static BasicCultureObject GetCharacterCulture(CharacterObject character) =>
            character?.HeroObject?.Culture ?? ((BasicCharacterObject)character)?.Culture;

        internal static IEnumerable<CharacterObject> GetCampaignCharacters() =>
            Game.Current?.ObjectManager?.GetObjectTypeList<BasicCharacterObject>()
                ?.OfType<CharacterObject>().Where(IsSelectable)
            ?? Enumerable.Empty<CharacterObject>();

        internal static CharacterObject FindDefaultTroop(
            BasicCultureObject preferredCulture = null,
            CharacterObject otherSideTroop = null)
        {
            CharacterObject basicTroop = (preferredCulture as CultureObject)?.BasicTroop;
            if (IsSelectable(basicTroop) && !basicTroop.IsHero && basicTroop != otherSideTroop)
                return basicTroop;

            return GetCampaignCharacters()
                .Where(character => !character.IsHero)
                .OrderBy(character => character == otherSideTroop)
                .ThenByDescending(character => preferredCulture != null
                    ? GetCharacterCulture(character) == preferredCulture
                    : otherSideTroop != null && GetCharacterCulture(character) != GetCharacterCulture(otherSideTroop))
                .FirstOrDefault();
        }

        public override void Initialize()
        {
            Cultures.Clear();
            GroupsInCultures.Clear();

            foreach (CharacterObject character in GetCampaignCharacters())
            {
                string cultureId = RegisterCulture(GetCharacterCulture(character));
                if (character.IsHero)
                {
                    RegisterCulture(character.HeroObject?.MapFaction?.Culture);
                    RegisterCulture(character.HeroObject?.Clan?.Culture);
                }
                if (!GroupsInCultures.TryGetValue(cultureId, out var groups))
                {
                    GroupsInCultures[cultureId] = groups = new List<Group>();
                }

                var formationClass = character.DefaultFormationClass;
                var group = groups.Find(g => g.Info.FormationClass == formationClass) as SPGroup;
                if (group == null)
                {
                    group = new SPGroup(formationClass);
                    groups.Add(group);
                    groups.Sort((lhs, rhs) => lhs.Info.FormationClass - rhs.Info.FormationClass);
                }

                var occupation = character.Occupation;
                if (!group.OccupationsInGroup.TryGetValue(occupation, out var characters))
                {
                    group.OccupationsInGroup[occupation] = characters = new CharactersInOccupation();
                }

                characters.Characters[character.StringId] = new SPCharacter(character, group.Info);
            }
        }
    }
}
