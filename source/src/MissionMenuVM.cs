using System;
using System.Collections.Generic;
using System.Text;
using Messages.FromBattleServer.ToBattleServerManager;
using TaleWorlds.Core;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

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
        private int _roundScale;
        private bool _isVisible;

        public NumericVM(string name, float initialValue, float min, float max, bool isDiscrete, Action<float> updateAction, int roundScale = 100, bool isVisible = true)
        {
            Name = name;
            _initialValue = initialValue;
            _min = min;
            _max = max;
            _optionValue = initialValue;
            _isDiscrete = isDiscrete;
            _updateAction = updateAction;
            _roundScale = roundScale;
            _isVisible = isVisible;
        }
        public string Name { get; }

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
                this._optionValue = MathF.Round(value * _roundScale) / (float)_roundScale;
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



        [DataSourceProperty]
        public bool IsVisible
        {
            get => this._isVisible;
            set
            {
                if (value == this._isVisible)
                    return;
                this._isVisible = value;
                this.OnPropertyChanged(nameof(IsVisible));
            }
        }
    }

    public class TacticOptionVM : ViewModel
    {
        public MissionMenuVM parent;
        public BattleSideEnum side;
        public TacticOptionEnum tacticOption;
        private bool _isSelected;

        [DataSourceProperty] public string Name { get; set; }

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
        private EnhancedMissionOrderUIHandler _orderUIHandler;

        private string _currentAIEnableType;
        private MBBindingList<TacticOptionVM> _attackerTacticOptions;
        private MBBindingList<TacticOptionVM> _defenderTacticOptions;

        private Action _closeMenu;
        public Func<BattleSideEnum, TacticOptionEnum, bool, bool> updateSelectedTactic;

        private SelectionOptionDataVM _playerFormation;

        public string EnableAIForString { get; } = GameTexts.FindText("str_enable_ai_for").ToString();
        public string AttackerTacticOptionString { get; } = GameTexts.FindText("str_attacker_tactic_option").ToString();
        public string DefenderTacticOptionString { get; } = GameTexts.FindText("str_defender_tactic_option").ToString();

        public string SwitchTeamString { get; } = GameTexts.FindText("str_switch_team").ToString();
        public string SwitchFreeCameraString { get; } = GameTexts.FindText("str_switch_free_camera").ToString();
        public string DisableDeathString { get; } = GameTexts.FindText("str_disable_death").ToString();
        public string ResetMissionString { get; } = GameTexts.FindText("str_reset_mission").ToString();
        public string TogglePauseString { get; } = GameTexts.FindText("str_toggle_pause").ToString();
        public string ResetSpeedString { get; } = GameTexts.FindText("str_reset_speed").ToString();

        public string UseRealisticBlockingString { get; } = GameTexts.FindText("str_em_use_realistic_blocking").ToString();

        public string ChangeCombatAIString { get; } = GameTexts.FindText("str_change_combat_ai").ToString();
        public string CombatAIString { get; } = GameTexts.FindText("str_combat_ai").ToString();

        public void PreviousAIEnableType()
        {
            _config.ToPreviousAIEnableType();
            Utility.ApplyTeamAIEnabled(_config);
            UpdateCurrentAIEnableType();
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
            UpdateCurrentAIEnableType();
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
        public SelectionOptionDataVM PlayerFormation
        {
            get => _playerFormation;
            set
            {
                if (_playerFormation == value)
                    return;
                _playerFormation = value;
                OnPropertyChanged(nameof(PlayerFormation));
            }
        }

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
            _missionSpeedLogic?.ResetSpeed();
        }

        [DataSourceProperty]
        public NumericVM SpeedFactor { get; }

        [DataSourceProperty]
        public bool UseRealisticBlocking
        {
            get => this._config.useRealisticBlocking;
            set
            {
                if (this._config.useRealisticBlocking == value)
                    return;
                this._config.useRealisticBlocking = value;
                ApplyUseRealisticBlocking();
                this.OnPropertyChanged(nameof(UseRealisticBlocking));
            }
        }

        [DataSourceProperty]
        public bool ChangeCombatAI
        {
            get => this._config.changeCombatAI;
            set
            {
                if (this._config.changeCombatAI == value)
                    return;
                this._config.changeCombatAI = value;
                this.CombatAI.IsVisible = value;
                ApplyCombatAI();
                this.OnPropertyChanged(nameof(ChangeCombatAI));
            }
        }

        [DataSourceProperty]
        public NumericVM CombatAI { get; }

        private void CloseMenu()
        {
            this._closeMenu?.Invoke();
        }

        public MissionMenuVM(BattleConfigBase config, Func<BattleSideEnum, TacticOptionEnum, bool, bool> updateSelectedTactic, Action closeMenu)
        {
            _config = config;
            UpdateCurrentAIEnableType();
            this.updateSelectedTactic = updateSelectedTactic;
            this._closeMenu = closeMenu;
            this._mission = Mission.Current;
            this._switchTeamLogic = _mission.GetMissionBehaviour<SwitchTeamLogic>();
            this._switchFreeCameraLogic = _mission.GetMissionBehaviour<SwitchFreeCameraLogic>();
            this._orderUIHandler = _mission.GetMissionBehaviour<EnhancedMissionOrderUIHandler>();
            this.PlayerFormation = new SelectionOptionDataVM(new SelectionOptionData(
                (int i) =>
                {
                    _config.playerFormation = i;
                    if (Mission.Current.MainAgent != null && Mission.Current.PlayerTeam != null)
                    {
                        var controller = Mission.Current.MainAgent.Controller;
                        Mission.Current.MainAgent.Controller = Agent.ControllerType.AI;
                        _orderUIHandler?.dataSource.RemoveTroops(Mission.Current.MainAgent);
                        Mission.Current.MainAgent.Formation =
                            Mission.Current.PlayerTeam.GetFormation((FormationClass)_config.playerFormation);
                        _orderUIHandler?.dataSource.AddTroops(Mission.Current.MainAgent);
                        Mission.Current.MainAgent.Controller = controller;
                    }
                }, () => _config.playerFormation,
                (int)FormationClass.NumberOfRegularFormations, new[]
                {
                    new SelectionItem(true, "str_troop_group_name", "0"),
                    new SelectionItem(true, "str_troop_group_name", "1"),
                    new SelectionItem(true, "str_troop_group_name", "2"),
                    new SelectionItem(true, "str_troop_group_name", "3"),
                    new SelectionItem(true, "str_troop_group_name", "4"),
                    new SelectionItem(true, "str_troop_group_name", "5"),
                    new SelectionItem(true, "str_troop_group_name", "6"),
                    new SelectionItem(true, "str_troop_group_name", "7"),
                }), GameTexts.FindText("str_player_formation"));
            this._missionSpeedLogic = _mission.GetMissionBehaviour<MissionSpeedLogic>();
            this._resetMissionLogic = _mission.GetMissionBehaviour<ResetMissionLogic>();
            this.SpeedFactor = new NumericVM(GameTexts.FindText("str_slow_motion_factor").ToString(),
                _mission.Scene.SlowMotionMode ? _mission.Scene.SlowMotionFactor : 1.0f, 0.01f, 2.0f, false,
                factor => { _missionSpeedLogic.SetSlowMotionFactor(factor); });

            this.ChangeCombatAI = this._config.changeCombatAI;
            this.CombatAI = new NumericVM(CombatAIString, _config.combatAI, 0, 100, true,
                combatAI =>
                {
                    this._config.combatAI = (int)combatAI;
                    ApplyCombatAI();
                }, 1, this._config.changeCombatAI);

            FillAttackerAvailableTactics();
            FillDefenderAvailableTactics();
        }

        private void UpdateCurrentAIEnableType()
        {
            this.CurrentAIEnableType = GameTexts.FindText("str_ai_enable_type", _config.aiEnableType.ToString()).ToString();
        }

        private void FillAttackerAvailableTactics()
        {
            var tactics = new MBBindingList<TacticOptionVM>();
            foreach (var tactic in _config.attackerTacticOptions)
            {
                tactics.Add(new TacticOptionVM { parent = this, side = BattleSideEnum.Attacker, IsSelected = tactic.isEnabled, tacticOption = tactic.tacticOption, Name = GameTexts.FindText("str_tactic_option", tactic.tacticOption.ToString()).ToString() });
            }

            this.AttackerAvailableTactics = tactics;
        }

        private void FillDefenderAvailableTactics()
        {
            var tactics = new MBBindingList<TacticOptionVM>();
            foreach (var tactic in _config.defenderTacticOptions)
            {
                tactics.Add(new TacticOptionVM { parent = this, side = BattleSideEnum.Defender, IsSelected = tactic.isEnabled, tacticOption = tactic.tacticOption, Name = GameTexts.FindText("str_tactic_option", tactic.tacticOption.ToString()).ToString() });
            }

            this.DefenderAvailableTactics = tactics;
        }

        private void ApplyCombatAI()
        {
            if (ChangeCombatAI)
            {
                foreach (var agent in _mission.Agents)
                {
                    AgentStatModel.SetAgentAIStat(agent, agent.AgentDrivenProperties, _config.combatAI);
                    agent.UpdateAgentProperties();
                }
            }
            else
            {
                foreach (var agent in _mission.Agents)
                {
                    MissionGameModels.Current.AgentStatCalculateModel.InitializeAgentStats(agent, agent.SpawnEquipment,
                        agent.AgentDrivenProperties, null);
                    agent.UpdateAgentProperties();
                }
            }
        }

        private void ApplyUseRealisticBlocking()
        {
            if (UseRealisticBlocking)
            {
                foreach (var agent in _mission.Agents)
                {
                    AgentStatModel.SetUseRealisticBlocking(agent.AgentDrivenProperties, true);
                    agent.UpdateAgentProperties();
                }
            }
            else
            {
                foreach (var agent in _mission.Agents)
                {
                    agent.AgentDrivenProperties.SetStat(DrivenProperty.UseRealisticBlocking, agent.Controller != Agent.ControllerType.Player ? 1f : 0.0f);
                    agent.UpdateAgentProperties();
                }
            }
        }
    }
}
