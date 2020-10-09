using System.Linq;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.Multiplayer.Data;

namespace EnhancedBattleTest.UI
{
    public class MPCharactersInGroupVM : CharactersInGroupVM
    {
        private string _cultureId;
        private Group _group;
        public MPCharactersInGroupVM(CharacterCollection collection) : base(collection)
        {
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
            UpdateCharacterList();
        }

        private void UpdateCharacterList()
        {
            if (_cultureId == null)
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
                CharactersInCurrentGroup = Collection.GroupsInCultures[_cultureId]
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
