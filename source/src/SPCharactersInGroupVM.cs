using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest
{
    public class SPCharactersInGroupVM : CharactersInGroupVM
    {
        private readonly List<Occupation> _occupations = new List<Occupation>();
        private BasicCultureObject _culture;
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
                        case Occupation.BannerBearer:
                            return new TextObject(occupation.ToString());
                        case Occupation.Outlaw:
                            return GameTexts.FindText("str_outlaw");
                        case Occupation.RuralNotable:
                            return GameTexts.FindText("str_rural_notable");
                        case Occupation.Artisan:
                        case Occupation.Preacher:
                        case Occupation.Headman:
                        case Occupation.GangLeader:
                            return GameTexts.FindText("str_charactertype_" + occupation.ToString().ToLower());
                        case Occupation.Judge:
                            return new TextObject("{=ZRkceJx3}Judge");
                        case Occupation.CaravanGuard:
                            return new TextObject("{=jxNe8lH2}Caravan Guard");
                    }
                    return GameTexts.FindText("str_occupation", occupation.ToString()) ;
                }))
            {
                list.Add(new SelectorItemVM(item));
            }

            Occupations.ItemList = list;
        }

        public override void SelectedCultureAndGroupChanged(BasicCultureObject culture, Group group, bool updateInstantly = true)
        {
            _culture = culture;
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
            Occupations.SelectedIndex = (int)spConfig.ActualCharacterObject.Occupation + 1;
        }

        private void OnSelectedOccupationChanged(SelectorVM<SelectorItemVM> obj)
        {
            if (obj.SelectedItem != null)
                UpdateCharacterList();
        }

        private void UpdateCharacterList()
        {
            if (_culture == null)
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
                CharactersInCurrentGroup = Collection.GroupsInCultures[_culture.StringId]
                    .SelectMany(GetCharactersInGroup).ToList();
            }
            else
            {
                CharactersInCurrentGroup = Collection.GroupsInCultures[_culture.StringId]
                    .Where(g => g.Info.FormationClass == _group.Info.FormationClass)
                    .SelectMany(GetCharactersInGroup).ToList();
            }
            RefreshCharacterList();

        }

        private Occupation CurrentOccupation()
        {
            return (Occupation)Occupations.SelectedIndex - 1;
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
