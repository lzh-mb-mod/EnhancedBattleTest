using EnhancedBattleTest.Config;
using EnhancedBattleTest.UI.Basic;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class BattleTypeSelectionGroup : ViewModel
    {
        private readonly BattleTypeConfig _config;
        private readonly MapSelectionGroupVM _mapSelectionGroup;
        private readonly Action<bool> _onPlayerTypeChange;
        private SelectorVM<SelectorItemVM> _battleTypeSelection;
        private SelectorVM<SelectorItemVM> _playerTypeSelection;
        private SelectorVM<SelectorItemVM> _playerSideSelection;
        private SelectorVM<SelectorItemVM> _equipmentModifierTypeSelection;

        public TextVM BattleTypeText { get; }

        public TextVM PlayerTypeText { get; }

        public TextVM PlayerSideText { get; }
        public TextVM EquipmentModifierTypeText { get; }

        public BattleTypeSelectionGroup(
          BattleTypeConfig config,
          MapSelectionGroupVM mapSelectionGroup,
          Action<bool> onPlayerTypeChange)
        {
            _config = config;
            _mapSelectionGroup = mapSelectionGroup;
            _onPlayerTypeChange = onPlayerTypeChange;

            BattleTypeText = new TextVM(GameTexts.FindText("str_ebt_battle_type"));
            PlayerTypeText = new TextVM(GameTexts.FindText("str_ebt_player_type"));
            PlayerSideText = new TextVM(GameTexts.FindText("str_ebt_player_side"));
            EquipmentModifierTypeText = new TextVM(GameTexts.FindText("str_ebt_equipment_modifier_type"));
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            BattleTypeSelection = new SelectorVM<SelectorItemVM>(new List<TextObject>()
            {
                GameTexts.FindText("str_ebt_battle_type", "Field"),
                GameTexts.FindText("str_ebt_battle_type", "Siege"),
                GameTexts.FindText("str_ebt_battle_type", "Village"),
            }, (int)_config.BattleType, OnBattleTypeSelection);
            PlayerTypeSelection = new SelectorVM<SelectorItemVM>(new List<TextObject>()
            {
                GameTexts.FindText("str_ebt_player_type", "Commander"),
                GameTexts.FindText("str_ebt_player_type", "Sergeant"),
            }, (int)_config.PlayerType, OnPlayerTypeSelectionChange);
            PlayerSideSelection = new SelectorVM<SelectorItemVM>(new List<TextObject>()
            {
                GameTexts.FindText("str_ebt_side", "Defender"),
                GameTexts.FindText("str_ebt_side", "Attacker"),
            }, _config.PlayerSide == BattleSideEnum.Defender ? 0 : 1, OnPlayerSideChanged);
            EquipmentModifierTypeSelection = new SelectorVM<SelectorItemVM>(new List<TextObject>()
            {
                GameTexts.FindText("str_ebt_modifier_type", EquipmentModifierType.Random.ToString()),
                GameTexts.FindText("str_ebt_modifier_type", EquipmentModifierType.Average.ToString()),
                GameTexts.FindText("str_ebt_modifier_type", EquipmentModifierType.None.ToString()),
            }, (int)_config.EquipmentModifierType, OnEquipmentModifierType);
            BattleTypeText.RefreshValues();
            PlayerTypeText.RefreshValues();
            PlayerSideText.RefreshValues();
            EquipmentModifierTypeText.RefreshValues();
        }

        public void RandomizeAll()
        {
            this.BattleTypeSelection.ExecuteRandomize();
            this.PlayerTypeSelection.ExecuteRandomize();
            this.PlayerSideSelection.ExecuteRandomize();
        }

        private void OnBattleTypeSelection(SelectorVM<SelectorItemVM> selector)
        {
            _config.BattleType = (BattleType)selector.SelectedIndex;
            _mapSelectionGroup.OnGameTypeChange((BattleType)selector.SelectedIndex);
        }

        private void OnPlayerTypeSelectionChange(SelectorVM<SelectorItemVM> selector)
        {
            _config.PlayerType = (PlayerType)selector.SelectedIndex;
            _onPlayerTypeChange(selector.SelectedIndex == 0);
        }

        private void OnPlayerSideChanged(SelectorVM<SelectorItemVM> selector)
        {
            _config.PlayerSide = selector.SelectedIndex == 0 ? BattleSideEnum.Defender : BattleSideEnum.Attacker;
        }

        private void OnEquipmentModifierType(SelectorVM<SelectorItemVM> selector)
        {
            _config.EquipmentModifierType = (EquipmentModifierType)selector.SelectedIndex;
        }

        public BattleType GetCurrentBattleType()
        {
            return (BattleType)BattleTypeSelection.SelectedIndex;
        }

        public PlayerType GetCurrentPlayerType()
        {
            return (PlayerType)PlayerTypeSelection.SelectedIndex;
        }

        public BattleSideEnum GetCurrentPlayerSide()
        {
            return PlayerSideSelection.SelectedIndex == 0 ? BattleSideEnum.Attacker : BattleSideEnum.Defender;
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> BattleTypeSelection
        {
            get => _battleTypeSelection;
            set
            {
                if (_battleTypeSelection == value)
                    return;
                _battleTypeSelection = value;
                OnPropertyChanged(nameof(BattleTypeSelection));
            }
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> PlayerTypeSelection
        {
            get => _playerTypeSelection;
            set
            {
                if (_playerTypeSelection == value)
                    return;
                _playerTypeSelection = value;
                OnPropertyChanged(nameof(PlayerTypeSelection));
            }
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> PlayerSideSelection
        {
            get => _playerSideSelection;
            set
            {
                if (_playerSideSelection == value)
                    return;
                _playerSideSelection = value;
                OnPropertyChanged(nameof(PlayerSideSelection));
            }
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> EquipmentModifierTypeSelection
        {
            get => _equipmentModifierTypeSelection;
            set
            {
                if (_equipmentModifierTypeSelection == value)
                    return;
                _equipmentModifierTypeSelection = value;
                OnPropertyChanged(nameof(EquipmentModifierTypeSelection));
            }
        }

        public void SwapSide()
        {
            if (PlayerSideSelection.SelectedIndex == 0)
                PlayerSideSelection.SelectedIndex = 1;
            else if (PlayerSideSelection.SelectedIndex == 1)
                PlayerSideSelection.SelectedIndex = 0;
        }
    }
}
