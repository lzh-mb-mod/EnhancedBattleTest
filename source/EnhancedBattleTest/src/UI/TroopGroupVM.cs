using EnhancedBattleTest.Config;
using EnhancedBattleTest.UI.Basic;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class TroopGroupVM : ViewModel
    {
        private readonly PartyConfig _partyConfig;
        private readonly TroopGroupConfig _config;
        private readonly bool _isPlayerSide;
        private readonly BattleTypeConfig _battleTypeConfig;
        private readonly Action _onCharacterChanged;
        private readonly Func<BasicCharacterObject> _preferredBannerCharacter;
        private MBBindingList<TroopVM> _troops;
        private bool _isGeneralTroopGroup;
        private bool _pushEnabled;
        private bool _popEnabled;
        private bool _isContentVisible = true;

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
        public bool IsContentVisible
        {
            get => _isContentVisible;
            private set
            {
                if (_isContentVisible == value)
                    return;
                _isContentVisible = value;
                OnPropertyChanged(nameof(IsContentVisible));
            }
        }

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

        public TroopGroupVM(
            PartyConfig partyConfig,
            TroopGroupConfig config,
            TextObject groupName,
            bool isGeneralTroopGroup,
            bool isPlayerSide,
            BattleTypeConfig battleTypeConfig,
            Action onCharacterChanged = null,
            Func<BasicCharacterObject> preferredBannerCharacter = null)
        {
            _partyConfig = partyConfig;
            _config = config;
            _isPlayerSide = isPlayerSide;
            _battleTypeConfig = battleTypeConfig;
            _onCharacterChanged = onCharacterChanged;
            _preferredBannerCharacter = preferredBannerCharacter;
            Troops = new MBBindingList<TroopVM>();
            IsGeneralTroopGroup = isGeneralTroopGroup;
            TroopGroupName = new TextVM(groupName);
            Reload();
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

        public void Reload()
        {
            var troops = new MBBindingList<TroopVM>();
            for (int i = 0; i < _config.Troops.Count; ++i)
            {
                int index = i;
                TroopConfig troopConfig = _config.Troops[index];
                TroopConfig capturedConfig = troopConfig;
                troops.Add(new TroopVM(
                    _partyConfig,
                    capturedConfig,
                    _isPlayerSide,
                    _battleTypeConfig,
                    IsGeneralTroopGroup,
                    _onCharacterChanged,
                    _preferredBannerCharacter,
                    () => IsBannerCharacter(capturedConfig.Character),
                    () => InsertAfter(index),
                    () => RemoveAt(index),
                    () => Move(index, index - 1),
                    () => Move(index, index + 1),
                    index > 0,
                    index < _config.Troops.Count - 1,
                    !IsGeneralTroopGroup || _config.Troops.Count > 1));
            }

            Troops = troops;
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

        public void SetContentVisible(bool value)
        {
            IsContentVisible = value;
        }

        public void PushTroop()
        {
            var newTroop = _config.Troops.Count == 0
                ? new TroopConfig()
                : new TroopConfig(_config.Troops[_config.Troops.Count - 1]);
            _config.Troops.Add(newTroop);
            Reload();
            _onCharacterChanged?.Invoke();
        }

        public void InsertFirst()
        {
            var newTroop = _config.Troops.Count == 0
                ? new TroopConfig()
                : new TroopConfig(_config.Troops[0]);
            _config.Troops.Insert(0, newTroop);
            Reload();
            _onCharacterChanged?.Invoke();
        }

        public void RemoveFirst()
        {
            if (_config.Troops.Count > (IsGeneralTroopGroup ? 1 : 0))
                _config.Troops.RemoveAt(0);
            Reload();
            _onCharacterChanged?.Invoke();
        }

        private void InsertAfter(int index)
        {
            if (index < 0 || index >= _config.Troops.Count)
                return;

            _config.Troops.Insert(
                index + 1,
                new TroopConfig(_config.Troops[index]));
            Reload();
            _onCharacterChanged?.Invoke();
        }

        private void RemoveAt(int index)
        {
            if (index < 0 || index >= _config.Troops.Count)
                return;

            _config.Troops.RemoveAt(index);
            Reload();
            _onCharacterChanged?.Invoke();
        }

        private void Move(int from, int to)
        {
            if (from < 0 || from >= _config.Troops.Count
                || to < 0 || to >= _config.Troops.Count)
                return;

            TroopConfig troop = _config.Troops[from];
            _config.Troops.RemoveAt(from);
            _config.Troops.Insert(to, troop);
            Reload();
            _onCharacterChanged?.Invoke();
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
            Reload();
            _onCharacterChanged?.Invoke();
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

        private bool IsBannerCharacter(CharacterConfig character)
        {
            BasicCharacterObject preferredCharacter =
                _preferredBannerCharacter?.Invoke();
            return preferredCharacter != null
                ? preferredCharacter == character?.CharacterObject
                : ReferenceEquals(
                    _partyConfig.GetBannerCharacterConfig(),
                    character);
        }
    }
}
