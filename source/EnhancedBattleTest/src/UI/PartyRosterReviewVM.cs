using EnhancedBattleTest.UI.Basic;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public sealed class PartyRosterReviewData
    {
        public MobileParty Party { get; }
        public Action<MobileParty, bool> ImportAction { get; }

        public PartyRosterReviewData(
            MobileParty party,
            Action<MobileParty, bool> importAction)
        {
            Party = party;
            ImportAction = importAction;
        }
    }

    public sealed class PartyRosterReviewItemVM : ViewModel
    {
        public string Name { get; }
        public string Number { get; }

        public PartyRosterReviewItemVM(TroopRosterElement element)
        {
            Name = element.Character.Name?.ToString()
                   ?? element.Character.StringId;
            Number = element.Number.ToString();
        }
    }

    public sealed class PartyRosterReviewVM : ViewModel
    {
        private readonly Action<PartyRosterReviewData> _beginReview;
        private readonly Action _endReview;
        private PartyRosterReviewData _data;

        public TextVM TitleText { get; }
        public TextVM SummaryText { get; }
        public TextVM AddText { get; }
        public TextVM ReplaceText { get; }
        public TextVM CancelText { get; }
        public MBBindingList<PartyRosterReviewItemVM> Members { get; } =
            new MBBindingList<PartyRosterReviewItemVM>();

        public PartyRosterReviewVM(
            Action<PartyRosterReviewData> beginReview,
            Action endReview)
        {
            _beginReview = beginReview;
            _endReview = endReview;
            TitleText = new TextVM(
                GameTexts.FindText("str_ebt_review_campaign_party"));
            SummaryText = new TextVM(new TextObject(string.Empty));
            AddText = new TextVM(
                GameTexts.FindText("str_ebt_add_campaign_party_members"));
            ReplaceText = new TextVM(
                GameTexts.FindText("str_ebt_replace_campaign_party_members"));
            CancelText = new TextVM(GameTexts.FindText("str_cancel"));
            EnhancedBattleTestSubModule.Instance.OnReviewParty += Open;
        }

        public override void OnFinalize()
        {
            EnhancedBattleTestSubModule.Instance.OnReviewParty -= Open;
            base.OnFinalize();
        }

        private void Open(PartyRosterReviewData data)
        {
            _data = data;
            TextObject summary =
                GameTexts.FindText("str_ebt_campaign_party_review_summary");
            summary.SetTextVariable("PARTY_NAME", data.Party.Name);
            summary.SetTextVariable(
                "MEMBER_COUNT",
                data.Party.Party.NumberOfAllMembers);
            SummaryText.TextObject = summary;
            Members.Clear();
            foreach (TroopRosterElement element in data.Party.MemberRoster
                         .GetTroopRoster()
                         .Where(element =>
                             element.Number > 0
                             && element.Character != null)
                         .OrderByDescending(element =>
                             element.Character.IsHero)
                         .ThenBy(element =>
                             element.Character.Name?.ToString()))
            {
                Members.Add(new PartyRosterReviewItemVM(element));
            }
            _beginReview?.Invoke(data);
        }

        private void Add()
        {
            Complete(false);
        }

        private void Replace()
        {
            Complete(true);
        }

        private void Complete(bool replace)
        {
            PartyRosterReviewData data = _data;
            Close();
            data?.ImportAction?.Invoke(data.Party, replace);
        }

        private void Close()
        {
            _endReview?.Invoke();
        }
    }
}
