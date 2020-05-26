using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;

namespace EnhancedBattleTest
{
    public class BattleTypeSelectionGroup : ViewModel
    {
        private readonly BattleTypeConfig _config;
        private readonly MapSelectionGroup _mapSelectionGroup;
        private readonly Action<bool> _onPlayerTypeChange;
        private SelectorVM<SelectorItemVM> _battleTypeSelection;
        private SelectorVM<SelectorItemVM> _playerTypeSelection;
        private SelectorVM<SelectorItemVM> _playerSideSelection;

        public TextVM BattleTypeText { get; }

        public TextVM PlayerTypeText { get; }

        public TextVM PlayerSideText { get; }

        public BattleTypeSelectionGroup(
          BattleTypeConfig config,
          MapSelectionGroup mapSelectionGroup,
          Action<bool> onPlayerTypeChange)
        {
            _config = config;
            _mapSelectionGroup = mapSelectionGroup;
            _onPlayerTypeChange = onPlayerTypeChange;

            BattleTypeText = new TextVM(GameTexts.FindText("str_ebt_battle_type"));
            PlayerTypeText = new TextVM(GameTexts.FindText("str_ebt_player_type"));
            PlayerSideText = new TextVM(GameTexts.FindText("str_ebt_player_side"));
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            BattleTypeSelection = new SelectorVM<SelectorItemVM>(new List<string>()
            {
                GameTexts.FindText("str_ebt_battle_type", "Field").ToString(),
                GameTexts.FindText("str_ebt_battle_type", "Siege").ToString(),
                GameTexts.FindText("str_ebt_battle_type", "Village").ToString(),
            }, (int)_config.BattleType, OnBattleTypeSelection);
            PlayerTypeSelection = new SelectorVM<SelectorItemVM>(new List<string>()
            {
                GameTexts.FindText("str_ebt_player_type", "Commander").ToString(),
                GameTexts.FindText("str_ebt_player_type", "Sergeant").ToString(),
            }, (int)_config.PlayerType, OnPlayerTypeSelectionChange);
            this.PlayerSideSelection = new SelectorVM<SelectorItemVM>(new List<string>()
            {
                GameTexts.FindText("str_ebt_side", "Attacker").ToString(),
                GameTexts.FindText("str_ebt_side", "Defender").ToString(),
            }, _config.PlayerSide == BattleSideEnum.Defender ? 1 : 0, null);
            BattleTypeText.RefreshValues();
            PlayerTypeText.RefreshValues();
            PlayerSideText.RefreshValues();
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
            switch (selector.SelectedIndex)
            {
                case (int) BattleType.Field:
                    _mapSelectionGroup.SetSearchMode(MapSelectionGroup.SearchMode.Battle);
                    break;
                case (int) BattleType.Siege:
                    _mapSelectionGroup.SetSearchMode(MapSelectionGroup.SearchMode.Siege);
                    break;
                case (int) BattleType.Village:
                    _mapSelectionGroup.SetSearchMode(MapSelectionGroup.SearchMode.Village);
                    break;
            }
        }

        private void OnPlayerTypeSelectionChange(SelectorVM<SelectorItemVM> selector)
        {
            _config.PlayerType = (PlayerType)selector.SelectedIndex;
            _onPlayerTypeChange(selector.SelectedIndex == 0);
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
    }
}
