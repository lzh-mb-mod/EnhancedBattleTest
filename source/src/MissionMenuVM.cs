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
        private Mission _mission;
        private SwitchTeamLogic _switchTeamLogic;
        private SwitchFreeCameraLogic _switchFreeCameraLogic;
        private PauseLogic _pauseLogic;
        private ResetMissionLogic _resetMissionLogic;

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

        public void SwitchTeam()
        {
            _switchTeamLogic?.SwapTeam();
            CloseMenu();
        }

        [DataSourceProperty] public bool SwitchTeamEnabled => _switchTeamLogic != null;

        public void SwitchFreeCamera()
        {
            _switchFreeCameraLogic?.SwitchCamera();
            CloseMenu();
        }

        [DataSourceProperty] public bool SwitchFreeCameraEnabled => _switchFreeCameraLogic != null;

        [DataSourceProperty]
        public bool DisableDying
        {
            get => this._config.disableDying;
            set
            {
                if (this._config.disableDying == value)
                    return;
                this._config.disableDying = value;
                _mission.GetMissionBehaviour<DisableDyingLogic>()?.SetDisableDying(DisableDying);
                this.OnPropertyChanged(nameof(DisableDying));
            }
        }

        public void TogglePause()
        {
            _pauseLogic?.TogglePause();
            CloseMenu();
        }

        [DataSourceProperty] public bool TogglePauseEnabled => this._pauseLogic != null;

        public void ResetMission()
        {
            _resetMissionLogic?.ResetMission();
            CloseMenu();
        }

        [DataSourceProperty] public bool ResetMissionEnabled => this._resetMissionLogic != null;

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
            this._mission = Mission.Current;
            this._switchTeamLogic = _mission.GetMissionBehaviour<SwitchTeamLogic>();
            this._switchFreeCameraLogic = _mission.GetMissionBehaviour<SwitchFreeCameraLogic>();
            this._pauseLogic = _mission.GetMissionBehaviour<PauseLogic>();
            this._resetMissionLogic = _mission.GetMissionBehaviour<ResetMissionLogic>();

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
