using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class TacticOptionVM : ViewModel
    {
        public MissionMenuVM parent;
        public BattleSideEnum side;
        public TacticOptionEnum tacticOption;
        private bool _isSelected;

        [DataSourceProperty]
        public string Name => tacticOption.ToString();

        [DataSourceProperty]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected)
                    return;
                _isSelected = value;
                this.OnPropertyChanged(nameof(IsSelected));
            }
        }

        private void OnSelect()
        {
            bool willEnable = !IsSelected;
            bool result = parent.updateSelectedTactic?.Invoke(side, tacticOption, willEnable) ?? false;
            if (result)
                IsSelected = willEnable;
        }
    }
    public class MissionMenuVM : ViewModel
    {
        private BattleConfigBase _config;

        private string _currentAIEnableType;
        private MBBindingList<TacticOptionVM> _attackerTacticOptions;
        private MBBindingList<TacticOptionVM> _defenderTacticOptions;

        private Action _closeMenu;
        public Func<BattleSideEnum, TacticOptionEnum, bool, bool> updateSelectedTactic;

        public void PreviousAIEnableType()
        {
            _config.ToPreviousAIEnableType();
            Utility.ApplyTeamAIEnabled(_config);
            CurrentAIEnableType = _config.aiEnableType.ToString();
        }

        [DataSourceProperty]
        public string CurrentAIEnableType
        {
            get => _currentAIEnableType;
            set
            {
                if (value == this._currentAIEnableType)
                    return;
                this._currentAIEnableType = value;
                this.OnPropertyChanged(nameof(CurrentAIEnableType));
            }
        }

        public void NextAIEnableType()
        {
            _config.ToNextAIEnableType();
            Utility.ApplyTeamAIEnabled(_config);
            CurrentAIEnableType = _config.aiEnableType.ToString();
        }

        [DataSourceProperty]
        public MBBindingList<TacticOptionVM> AttackerAvailableTactics
        {
            get => _attackerTacticOptions;
            set
            {
                if (value == _attackerTacticOptions)
                    return;
                this._attackerTacticOptions = value;
                this.OnPropertyChanged(nameof(AttackerAvailableTactics));
            }
        }

        [DataSourceProperty]
        public MBBindingList<TacticOptionVM> DefenderAvailableTactics
        {
            get => _defenderTacticOptions;
            set
            {
                if (value == _defenderTacticOptions)
                    return;
                this._defenderTacticOptions = value;
                this.OnPropertyChanged(nameof(DefenderAvailableTactics));
            }
        }

        [DataSourceProperty]
        public bool DisableDying
        {
            get => this._config.disableDying;
            set
            {
                if (this._config.disableDying == value)
                    return;
                this._config.disableDying = value;
                Utility.CurrentMission().GetMissionBehaviour<TrainingLogic>()?.SetDisableDying(DisableDying);
                this.OnPropertyChanged(nameof(DisableDying));
            }
        }

        private void CloseMenu()
        {
            this._closeMenu?.Invoke();
        }

        public MissionMenuVM(BattleConfigBase config, Func<BattleSideEnum, TacticOptionEnum, bool, bool> updateSelectedTactic, Action closeMenu)
        {
            _config = config;
            this.CurrentAIEnableType = config.aiEnableType.ToString();
            this.updateSelectedTactic = updateSelectedTactic;
            this._closeMenu = closeMenu;

            FillAttackerAvailableTactics();
            FillDefenderAvailableTactics();
        }

        private void FillAttackerAvailableTactics()
        {
            var tactics = new MBBindingList<TacticOptionVM>();
            foreach (var tactic in _config.attackerTacticOptions)
            {
                tactics.Add(new TacticOptionVM { parent = this, side = BattleSideEnum.Attacker, IsSelected = tactic.isEnabled, tacticOption = tactic.tacticOption });
            }

            this.AttackerAvailableTactics = tactics;
        }

        private void FillDefenderAvailableTactics()
        {
            var tactics = new MBBindingList<TacticOptionVM>();
            foreach (var tactic in _config.defenderTacticOptions)
            {
                tactics.Add(new TacticOptionVM { parent = this, side = BattleSideEnum.Defender, IsSelected = tactic.isEnabled, tacticOption = tactic.tacticOption });
            }

            this.DefenderAvailableTactics = tactics;
        }
    }
}
