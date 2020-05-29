using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace EnhancedBattleTest
{
    public class MPCharactersInGroupVM : CharactersInGroupVM
    {
        private BasicCultureObject _culture;
        private Group _group;
        public MPCharactersInGroupVM(CharacterCollection collection) : base(collection)
        {
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
            UpdateCharacterList();
        }

        private void UpdateCharacterList()
        {
            if (_culture == null)
            {
                if (_group == null)
                {
                    CharactersInCurrentGroup = Collection.GroupsInCultures.Values
                        .SelectMany(groups => groups.SelectMany(g => (g as MPGroup)?.CharactersInGroup.Values))
                        .ToList();
                }
                else
                {
                    CharactersInCurrentGroup = Collection.GroupsInCultures.Values.SelectMany(groups =>
                        groups.Where(g => g.Info.FormationClass == _group.Info.FormationClass)
                            .SelectMany(g => (g as MPGroup)?.CharactersInGroup.Values)).ToList();
                }
            }
            else if (_group == null)
            {
                CharactersInCurrentGroup = Collection.GroupsInCultures[_culture.StringId]
                    .SelectMany(g => (g as MPGroup)?.CharactersInGroup.Values).ToList();
            }
            else
            {
                if (!(_group is MPGroup mpGroup))
                    return;
                CharactersInCurrentGroup = mpGroup.CharactersInGroup.Values.ToList();
            }
            RefreshCharacterList();
        }
    }
}
