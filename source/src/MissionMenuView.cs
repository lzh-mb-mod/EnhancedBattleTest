using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace EnhancedBattleTest
{
    public class MissionMenuView : MissionView
    {
        private MissionMenuVM _dataSource;
        private GauntletLayer _gauntletLayer;
        private GauntletMovie _movie;
        private BattleConfigBase _config;

        public delegate void OnMissionMenuViewDelegate();

        public event OnMissionMenuViewDelegate OnMissionMenuViewActivated;
        public event OnMissionMenuViewDelegate OnMissionMenuViewDeactivated;


        public bool IsActivated { get; set; }

        public MissionMenuView(BattleConfigBase config)
        {
            this._config = config;
            this.ViewOrderPriorty = 24;
        }

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            this._gauntletLayer = null;
            this._dataSource?.OnFinalize();
            this._dataSource = null;
            this._movie = null;
            this._config = null;
        }

        public void ToggleMenu()
        {
            if (IsActivated)
                DeactivateMenu();
            else
                ActivateMenu();
        }

        public void ActivateMenu()
        {
            IsActivated = true;
            this._dataSource = new MissionMenuVM(_config, (side, tacticOption, isEnabled) =>
            {
                if (side == BattleSideEnum.None || side == BattleSideEnum.NumSides)
                    return false;
                if (_config.battleType == BattleType.SiegeBattle)
                {
                    Utility.DisplayMessage("Tactic option cannot be changed in siege battle mode");
                    return false;
                }
                var tacticArray = side == BattleSideEnum.Attacker
                    ? _config.attackerTacticOptions
                    : _config.defenderTacticOptions;
                int count = tacticArray.Sum(info =>
                    info.tacticOption == tacticOption ? (isEnabled ? 1 : 0) : info.isEnabled ? 1 : 0);
                if (count == 0)
                {
                    Utility.DisplayMessage("There should be at least one tactic");
                    return false;
                }
                tacticArray.First(info => info.tacticOption == tacticOption).isEnabled = isEnabled;
                var team = side == BattleSideEnum.Attacker ? Mission.AttackerTeam : Mission.DefenderTeam;
                if (team == null)
                    return true;
                if (isEnabled)
                    TacticOptionHelper.AddTacticComponent(team, tacticOption, true);
                else
                    TacticOptionHelper.RemoveTacticComponent(team, tacticOption, true);
                return true;
            }, this.DeactivateMenu);
            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty) { IsFocusLayer = true };
            this._gauntletLayer.InputRestrictions.SetInputRestrictions();
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            this._movie = this._gauntletLayer.LoadMovie(nameof(MissionMenuView), _dataSource);
            this.MissionScreen.AddLayer(this._gauntletLayer);
            ScreenManager.TrySetFocus(this._gauntletLayer);
            Utility.PauseGame();
            this.OnMissionMenuViewActivated?.Invoke();
        }

        public void DeactivateMenu()
        {
            IsActivated = false;
            this._dataSource = null;
            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
            this.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._movie = null;
            this._gauntletLayer = null;
            Utility.UnpauseGame();
            this.OnMissionMenuViewDeactivated?.Invoke();
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (IsActivated)
            {
                if (this._gauntletLayer.Input.IsKeyReleased(InputKey.RightMouseButton) ||
                    this._gauntletLayer.Input.IsKeyReleased(InputKey.Numpad8) ||
                    this._gauntletLayer.Input.IsHotKeyReleased("Exit"))
                    DeactivateMenu();
            }
            else if (this.Input.IsKeyReleased(InputKey.Numpad8))
                ActivateMenu();
        }
    }
}
