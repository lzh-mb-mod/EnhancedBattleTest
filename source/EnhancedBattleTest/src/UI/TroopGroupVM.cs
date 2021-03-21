using EnhancedBattleTest.Config;
using EnhancedBattleTest.UI.Basic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class TroopGroupVM : ViewModel
    {
        private readonly TeamConfig _teamConfig;
        private readonly TroopGroupConfig _config;
        private readonly bool _isPlayerSide;
        private readonly BattleTypeConfig _battleTypeConfig;
        private MBBindingList<TroopVM> _troops;
        private bool _isGeneralTroopGroup;
        private bool _pushEnabled;
        private bool _popEnabled;

        [DataSourceProperty]
        public bool IsGeneralTroopGroup
        {
            get => _isGeneralTroopGroup;
            set
            {
                if (_isGeneralTroopGroup == value)
                    return;
                _isGeneralTroopGroup = value;
                OnPropertyChanged(nameof(IsGeneralTroopGroup));
            }
        }

        public TextVM TroopGroupName { get; }

        [DataSourceProperty]
        public MBBindingList<TroopVM> Troops
        {
            get => _troops;
            set
            {
                if (_troops == value)
                    return;
                _troops = value;
                OnPropertyChanged(nameof(Troops));
            }
        }

        public bool IsPlayerSide
        {
            set
            {
                foreach (var troop in Troops)
                {
                    troop.CharacterButton.IsPlayerSide = value;
                }
            }
        }

        public TroopGroupVM(TeamConfig teamConfig, TroopGroupConfig config, TextObject groupName, bool isGeneralTroopGroup, bool isPlayerSide, BattleTypeConfig battleTypeConfig)
        {
            _teamConfig = teamConfig;
            _config = config;
            _isPlayerSide = isPlayerSide;
            _battleTypeConfig = battleTypeConfig;
            Troops = new MBBindingList<TroopVM>();
            IsGeneralTroopGroup = isGeneralTroopGroup;
            TroopGroupName = new TextVM(groupName);
            foreach (var troopConfig in config.Troops)
            {
                Troops.Add(new TroopVM(teamConfig, troopConfig,
                    isPlayerSide, battleTypeConfig, isGeneralTroopGroup));
            }

            UpdateEnabled();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            foreach (var troopVm in Troops)
            {
                troopVm.RefreshValues();
            }
            UpdateEnabled();
        }

        public bool IsValid()
        {
            foreach (var troopVm in _troops)
            {
                if (!troopVm.IsValid())
                    return false;
            }

            return true;
        }

        public void PushTroop()
        {
            var newTroop = _config.Troops.Count == 0
                ? new TroopConfig(EnhancedBattleTestSubModule.IsMultiplayer)
                : new TroopConfig(_config.Troops[_config.Troops.Count - 1]);
            _config.Troops.Add(newTroop);
            Troops.Add(new TroopVM(_teamConfig, newTroop, _isPlayerSide, _battleTypeConfig, IsGeneralTroopGroup));
            UpdateEnabled();
        }

        [DataSourceProperty]
        public bool PushEnabled
        {
            get => _pushEnabled;
            set
            {
                if (_pushEnabled == value)
                    return;
                _pushEnabled = value;
                OnPropertyChanged(nameof(PushEnabled));
            }
        }


        public void PopTroop()
        {
            if (_config.Troops.Count > 0)
                _config.Troops.RemoveAt(_config.Troops.Count - 1);
            if (Troops.Count > 0)
                Troops.RemoveAt(Troops.Count - 1);
            UpdateEnabled();
        }

        [DataSourceProperty]
        public bool PopEnabled
        {
            get => _popEnabled;
            set
            {
                if (_popEnabled == value)
                    return;
                _popEnabled = value;
                OnPropertyChanged(nameof(PopEnabled));
            }
        }

        private void UpdateEnabled()
        {
            PushEnabled = Troops.Count < 2000;
            PopEnabled = Troops.Count > (IsGeneralTroopGroup ? 1 : 0);
        }
    }
}
