using EnhancedBattleTest.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.SinglePlayer.Data
{
    public class SPCharacter : Character
    {

        public CharacterObject CharacterObject;
        public override string StringId => CharacterObject.StringId;
        public override TextObject Name
        {
            get
            {
                TextObject name = CharacterObject.Name;
                return TextObject.IsNullOrEmpty(name) ? new TextObject(StringId) : name;
            }
        }
        public override BasicCultureObject Culture => SPCharacterCollection.GetCharacterCulture(CharacterObject);
        public override GroupInfo GroupInfo { get; }

        public Occupation Occupation => CharacterObject.Occupation;

        public SPCharacter(CharacterObject characterObject, GroupInfo groupInfo)
        {
            CharacterObject = characterObject;
            GroupInfo = groupInfo;
        }
    }
}
