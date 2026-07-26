using EnhancedBattleTest.UI.Basic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public sealed class CharacterReviewItemVM : ViewModel
    {
        public TextVM Name { get; }
        public TextVM Value { get; }
        public string SkillId { get; }
        public int SkillValue { get; }
        public BasicTooltipViewModel Hint { get; }

        public CharacterReviewItemVM(SkillObject skill, int value)
        {
            Name = new TextVM(skill.Name);
            Value = new TextVM(new TextObject(value.ToString()));
            SkillId = skill.StringId;
            SkillValue = value;
            Hint = new BasicTooltipViewModel(() =>
            {
                GameTexts.SetVariable("STR1", skill.Name);
                GameTexts.SetVariable("STR2", skill.Description);
                return GameTexts.FindText(
                    "str_string_newline_string").ToString();
            });
        }
    }
}
