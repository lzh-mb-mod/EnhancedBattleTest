using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.SinglePlayer.Config;
using EnhancedBattleTest.SinglePlayer.Data;
using EnhancedBattleTest.UI.Basic;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class SPCharactersInGroupVM : CharactersInGroupVM
    {
        private readonly List<Occupation> _occupations = new List<Occupation>();
        private string _cultureId;
        private Group _group;
        public TextVM OccupationText { get; }
        public SelectorVM<SelectorItemVM> Occupations { get; }

        public SPCharactersInGroupVM(CharacterCollection collection) : base(collection)
        {
            OccupationText = new TextVM(new TextObject("{=GZxFIeiJ}Occupation"));
            for (Occupation occupation = Occupation.NotAssigned;
                occupation < Occupation.NumberOfOccupations;
                ++occupation)
            {
                _occupations.Add(occupation);
            }

            Occupations = new SelectorVM<SelectorItemVM>(0, OnSelectedOccupationChanged);
            var list = new MBBindingList<SelectorItemVM>();
            foreach (var item in
                _occupations.Select(occupation =>
                {
                    switch (occupation)
                    {
                        case Occupation.GoodsTrader:
                            return new TextObject("{=thbcgVVO}Trader");
                        //case Occupation.Outlaw:
                        //    return GameTexts.FindText("str_outlaw");
                        case Occupation.Artisan:
                        case Occupation.Preacher:
                        case Occupation.Headman:
                        case Occupation.GangLeader:
                        case Occupation.RuralNotable:
                            return GameTexts.FindText("str_charactertype_" + occupation.ToString().ToLower());
                        //case Occupation.Judge:
                        //    return new TextObject("{=ZRkceJx3}Judge");
                        case Occupation.CaravanGuard:
                        case Occupation.BannerBearer:
                            return new TextObject(occupation.ToString());
                    }
                    return GameTexts.FindText("str_occupation", occupation.ToString());
                }))
            {
                list.Add(new SelectorItemVM(item));
            }

            Occupations.ItemList = list;
        }

        public override void SelectedCultureAndGroupChanged(string cultureId, Group group, bool updateInstantly = true)
        {
            _cultureId = cultureId;
            _group = group;
            if (updateInstantly)
                UpdateCharacterList();
        }

        protected override void OnSetConfig(CharacterConfig config)
        {
            var spConfig = config as SPCharacterConfig;
            if (spConfig == null)
                return;
            Occupations.SelectedIndex = -1;
            Occupations.SelectedIndex = (int)spConfig.ActualCharacterObject.Occupation;
        }

        private void OnSelectedOccupationChanged(SelectorVM<SelectorItemVM> obj)
        {
            if (obj.SelectedItem != null)
                UpdateCharacterList();
        }

        private void UpdateCharacterList()
        {
            if (_cultureId == null)
            {
                if (_group == null)
                {
                    CharactersInCurrentGroup = Collection.GroupsInCultures.Values
                        .SelectMany(groups => groups.SelectMany(GetCharactersInGroup))
                        .ToList();
                }
                else
                {
                    CharactersInCurrentGroup = Collection.GroupsInCultures.Values
                        .SelectMany(groups => groups.Where(g => g.Info.FormationClass == _group.Info.FormationClass)
                            .SelectMany(GetCharactersInGroup)).ToList();
                }
            }
            else if (_group == null)
            {
                CharactersInCurrentGroup = Collection.GroupsInCultures[_cultureId]
                    .SelectMany(GetCharactersInGroup).ToList();
            }
            else
            {
                CharactersInCurrentGroup = Collection.GroupsInCultures[_cultureId]
                    .Where(g => g.Info.FormationClass == _group.Info.FormationClass)
                    .SelectMany(GetCharactersInGroup).ToList();
            }
            RefreshCharacterList();

        }

        private Occupation CurrentOccupation()
        {
            return (Occupation)Occupations.SelectedIndex;
        }

        private IEnumerable<Character> GetCharactersInGroup(Group group)
        {
            var spGroup = group as SPGroup;
            if (spGroup == null)
                return Enumerable.Empty<Character>();
            var occupation = CurrentOccupation();
            if (occupation == Occupation.NotAssigned)
                return spGroup.OccupationsInGroup.Values.SelectMany(c => c.Characters.Values);
            return spGroup.OccupationsInGroup.TryGetValue(occupation, out var characters)
                ? characters.Characters.Values
                : Enumerable.Empty<Character>();
        }
    }
}
