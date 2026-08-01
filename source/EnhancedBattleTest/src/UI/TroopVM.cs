using EnhancedBattleTest.Config;
using EnhancedBattleTest.UI.Basic;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.UI
{
    public class TroopVM : ViewModel
    {
        public CharacterButtonVM CharacterButton { get; }

        public TextVM NumberText { get; }
        public NumberVM<int> Number { get; }

        public TextVM InvalidText { get; }

        public bool IsGeneralTroop { get; }
        public bool CanMoveUp { get; }
        public bool CanMoveDown { get; }
        public bool CanRemove { get; }
        public TextVM InsertText { get; }
        public TextVM RemoveText { get; }
        public TextVM MoveUpText { get; }
        public TextVM MoveDownText { get; }

        private readonly Action _insertAfter;
        private readonly Action _remove;
        private readonly Action _moveUp;
        private readonly Action _moveDown;

        public TroopVM(
            PartyConfig partyConfig,
            TroopConfig config,
            bool isPlayerSide,
            BattleTypeConfig battleTypeConfig,
            bool isGeneralTroop = false,
            Action onCharacterChanged = null,
            Func<BasicCharacterObject> preferredBannerCharacter = null,
            Func<bool> useSelectedCharacterForBanner = null,
            Action insertAfter = null,
            Action remove = null,
            Action moveUp = null,
            Action moveDown = null,
            bool canMoveUp = false,
            bool canMoveDown = false,
            bool canRemove = true)
        {
            _insertAfter = insertAfter;
            _remove = remove;
            _moveUp = moveUp;
            _moveDown = moveDown;
            CanMoveUp = canMoveUp;
            CanMoveDown = canMoveDown;
            CanRemove = canRemove;
            CharacterButton = new CharacterButtonVM(
                partyConfig,
                config.Character,
                isPlayerSide,
                battleTypeConfig,
                onCharacterChanged,
                preferredBannerCharacter,
                useSelectedCharacterForBanner,
                isGeneralTroop);
            NumberText = new TextVM(GameTexts.FindText("str_ebt_number"));
            Number = new NumberVM<int>(config.Number, 0, 5000, true);
            Number.OnValueChanged += number =>
            {
                config.Number = number;
                onCharacterChanged?.Invoke();
            };
            InvalidText = new TextVM(GameTexts.FindText("str_ebt_invalid"));
            InsertText = new TextVM(GameTexts.FindText("str_ebt_insert_after"));
            RemoveText = new TextVM(GameTexts.FindText("str_ebt_remove"));
            MoveUpText = new TextVM(GameTexts.FindText("str_ebt_move_up"));
            MoveDownText = new TextVM(GameTexts.FindText("str_ebt_move_down"));
            IsGeneralTroop = isGeneralTroop;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            CharacterButton.RefreshValues();
            NumberText.RefreshValues();
            Number.RefreshValues();
            InvalidText.RefreshValues();
            InsertText.RefreshValues();
            RemoveText.RefreshValues();
            MoveUpText.RefreshValues();
            MoveDownText.RefreshValues();
        }

        public bool IsValid()
        {
            return !Number.IsIllegal;
        }

        public void InsertAfter()
        {
            _insertAfter?.Invoke();
        }

        public void SelectCharacter(Action cancelAction = null)
        {
            CharacterButton.SelectCharacter(cancelAction);
        }

        public void Remove()
        {
            _remove?.Invoke();
        }

        public void MoveUp()
        {
            _moveUp?.Invoke();
        }

        public void MoveDown()
        {
            _moveDown?.Invoke();
        }
    }
}
