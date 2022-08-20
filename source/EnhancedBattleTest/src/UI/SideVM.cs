using EnhancedBattleTest.Config;
using EnhancedBattleTest.UI.Basic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.UI
{
    public class SideVM : ViewModel
    {
        private readonly SideConfig _config;
        private bool _isPlayerSide;
        private bool _isRemoveTeamEnabled;
        private bool _isAddTeamEnabled;
        private BattleTypeConfig _battleTypeConfig;

        public TextVM Name { get; }

        public MBBindingList<TeamVM> Teams { get; }

        public bool IsPlayerSide
        {
            get => _isPlayerSide;
            set
            {
                _isPlayerSide = value;
                Name.TextObject = value ? new TextObject("{=BC7n6qxk}PLAYER") : new TextObject("{=35IHscBa}ENEMY");
                foreach (var team in Teams)
                {
                    team.IsPlayerSide = value;
                }
            }
        }

        public bool IsAddTeamEnabled
        {
            get => _isAddTeamEnabled;
            set
            {
                if (_isAddTeamEnabled == value)
                    return;
                _isAddTeamEnabled = value;
                OnPropertyChanged(nameof(IsAddTeamEnabled));
            }
        }

        public bool IsRemoveTeamEnabled
        {
            get => _isRemoveTeamEnabled;
            set
            {
                if (_isRemoveTeamEnabled == value)
                    return;
                _isRemoveTeamEnabled = value;
                OnPropertyChanged(nameof(IsRemoveTeamEnabled));
            }
        }

        public SideVM(SideConfig sideConfig, bool isPlayerSide, BattleTypeConfig battleTypeConfig)
        {
            Name = new TextVM(isPlayerSide ? new TextObject("{=BC7n6qxk}PLAYER") : new TextObject("{=35IHscBa}ENEMY"));
            _config = sideConfig;
            _battleTypeConfig = battleTypeConfig;

            Teams = new MBBindingList<TeamVM>();
            IsPlayerSide = isPlayerSide;
            for (int i = 0; i < _config.Teams.Count; ++i)
            {
                Teams.Add(new TeamVM(sideConfig.Teams[i], isPlayerSide, i, battleTypeConfig));
            }
            TeamCountChanged();
        }

        public bool IsValid()
        {
            return Teams.All(team => team.IsValid());
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            Name.RefreshValues();
            foreach (var team in Teams)
            {
                team.RefreshValues();
            }
        }

        public void AddTeam()
        {
            var newConfig = new TeamConfig();
            _config.Teams.Add(newConfig);
            Teams.Add(new TeamVM(newConfig, IsPlayerSide, _config.Teams.Count - 1, _battleTypeConfig));
            TeamCountChanged();
        }

        public void RemoveTeam()
        {
            _config.Teams.RemoveAt(_config.Teams.Count - 1);
            Teams.RemoveAt(Teams.Count - 1);
            TeamCountChanged();
        }

        private void TeamCountChanged()
        {
            IsAddTeamEnabled = Teams.Count < 10;
            IsRemoveTeamEnabled = Teams.Count > 1;
        }
    }
}
