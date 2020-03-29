using System;
using System.Collections.Generic;
using System.Text;
using Messages.FromBattleServer.ToBattleServerManager;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class NumericVM : ViewModel
    {

        private readonly float _initialValue;
        private float _min;
        private float _max;
        private float _optionValue;
        private bool _isDiscrete;
        private Action<float> _updateAction;

        public NumericVM(float initialValue, float min, float max, bool isDiscrete, Action<float> updateAction)
        {
            _initialValue = initialValue;
            _min = min;
            _max = max;
            _optionValue = initialValue;
            _isDiscrete = isDiscrete;
            _updateAction = updateAction;
        }

        [DataSourceProperty]
        public float Min
        {
            get => this._min;
            set
            {
                if (Math.Abs(value - this._min) < 0.01f)
                    return;
                this._min = value;
                this.OnPropertyChanged(nameof(Min));
            }
        }

        [DataSourceProperty]
        public float Max
        {
            get => this._max;
            set
            {
                if (Math.Abs(value - this._max) < 0.01f)
                    return;
                this._max = value;
                this.OnPropertyChanged(nameof(Max));
            }
        }

        [DataSourceProperty]
        public float OptionValue
        {
            get => this._optionValue;
            set
            {
                if (Math.Abs((double)value - (double)this._optionValue) < 0.01f)
                    return;
                this._optionValue = MathF.Round(value * 100) / 100f;
                this.OnPropertyChanged(nameof(OptionValue));
                this.OnPropertyChanged(nameof(OptionValueAsString));
                this._updateAction(OptionValue);
            }
        }

        [DataSourceProperty]
        public bool IsDiscrete
        {
            get => this._isDiscrete;
            set
            {
                if (value == this._isDiscrete)
                    return;
                this._isDiscrete = value;
                this.OnPropertyChanged(nameof(IsDiscrete));
            }
        }

        [DataSourceProperty]
        public string OptionValueAsString => !this.IsDiscrete ? this._optionValue.ToString("F") : ((int)this._optionValue).ToString();
    }

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
        private MissionSpeedLogic _missionSpeedLogic;
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
        public bool DisableDeath
        {
            get => this._config.disableDeath;
            set
            {
                if (this._config.disableDeath == value)
                    return;
                this._config.disableDeath = value;
                _mission.GetMissionBehaviour<DisableDeathLogic>()?.SetDisableDeath(DisableDeath);
                this.OnPropertyChanged(nameof(DisableDeath));
            }
        }

        public void ResetMission()
        {
            _resetMissionLogic?.ResetMission();
            CloseMenu();
        }

        [DataSourceProperty] public bool ResetMissionEnabled => this._resetMissionLogic != null;

        public void TogglePause()
        {
            _missionSpeedLogic?.TogglePause();
            CloseMenu();
        }

        [DataSourceProperty] public bool AdjustSpeedEnabled => this._missionSpeedLogic != null;

        public void ResetSpeed()
        {
            SpeedFactor.OptionValue = 1.0f;
            _missionSpeedLogic.ResetSpeed();
        }

        [DataSourceProperty]
        public NumericVM SpeedFactor { get;}

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
            this._missionSpeedLogic = _mission.GetMissionBehaviour<MissionSpeedLogic>();
            this._resetMissionLogic = _mission.GetMissionBehaviour<ResetMissionLogic>();
            this.SpeedFactor = new NumericVM(_mission.Scene.SlowMotionMode ? _mission.Scene.SlowMotionFactor : 1.0f, 0.01f, 2.0f, false, factor =>
                {
                    _missionSpeedLogic.SetSlowMotionFactor(factor);
                });

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
