using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using Debug = System.Diagnostics.Debug;

namespace EnhancedBattleTest
{
    public class CustomBattleConfigVM : BattleConfigVMBase<CustomBattleConfig>
    {
        private Action<CustomBattleConfig> startAction;
        private Action<CustomBattleConfig> backAction;
        
        private int _selectedSceneIndex;
        private string _skyBrightness;
        private string _rainDensity;

        private string _selectedMapName;

        [DataSourceProperty]
        public string SelectedMapName
        {
            get => this._selectedMapName;
            set
            {
                this._selectedMapName = value;
                this.OnPropertyChanged(nameof(SelectedMapName));
            }
        }

        public int SelectedSceneIndex
        {
            get => this._selectedSceneIndex;
            set
            {
                if (value < 0 || value >= CurrentConfig.sceneList.Length || value == this._selectedSceneIndex)
                    return;

                this.CurrentConfig.sceneIndex = value;
                this._selectedSceneIndex = value;
                UpdateSceneContent();
            }
        }

        [DataSourceProperty]
        public string SkyBrightness
        {
            get => this._skyBrightness;
            set
            {
                if (this._skyBrightness == value)
                    return;
                this._skyBrightness = value;
                this.OnPropertyChanged(nameof(SkyBrightness));
            }
        }

        [DataSourceProperty]
        public string RainDensity
        {
            get => _rainDensity;
            set
            {
                if (this._rainDensity == value)
                    return;
                this._rainDensity = value;
                this.OnPropertyChanged(nameof(RainDensity));
            }
        }

        public bool UseFreeCamera
        {
            get => this.CurrentConfig.useFreeCamera;
            set => this.CurrentConfig.useFreeCamera = value;
        }

        public CustomBattleConfigVM(CharacterSelectionView selectionView, Action<CustomBattleConfig> startAction,
            Action<CustomBattleConfig> backAction)
            : base(selectionView, CustomBattleConfig.Get())
        {
            InitializeContent();

            this.startAction = startAction;
            this.backAction = backAction;
        }

        private void PreviousMap()
        {
            if (this.SelectedSceneIndex == 0)
                return;
            this.SelectedSceneIndex--;
        }
        private void NextMap()
        {
            if (this.SelectedSceneIndex + 1 >= CurrentConfig.sceneList.Length)
                return;
            this.SelectedSceneIndex++;
        }

        private new void SelectPlayerCharacter()
        {
            base.SelectPlayerCharacter();
        }

        private new void SelectPlayerTroopCharacter()
        {
            base.SelectPlayerTroopCharacter();
        }

        private new void SelectEnemyTroopCharacter()
        {
            base.SelectEnemyTroopCharacter();
        }

        private void Start()
        {
            if (SaveConfig() != SaveParamResult.success)
                return;
            ModuleLogger.Writer.WriteLine("StartBattle");
            this.startAction(CurrentConfig);
        }

        private void Save()
        {
            SaveConfig();
        }

        private void LoadConfig()
        {
            this.CurrentConfig.ReloadSavedConfig();
            this.InitializeContent();
            Utility.DisplayMessage("Reset successfully");
        }

        private void GoBack()
        {
            backAction(this.CurrentConfig);
        }

        private void InitializeContent()
        {
            this._selectedSceneIndex = CurrentConfig.sceneIndex;
            UpdateSceneContent();
        }

        private void UpdateSceneContent()
        {
            this.SelectedMapName = CurrentConfig.SceneName;
            this.SkyBrightness = CurrentConfig.SkyBrightness.ToString();
            this.RainDensity = CurrentConfig.RainDensity.ToString();
        }

        protected override void ApplyConfig()
        {
            base.ApplyConfig();
            CurrentConfig.sceneIndex = this.SelectedSceneIndex;
            CurrentConfig.SkyBrightness = System.Convert.ToSingle(this.SkyBrightness);
            CurrentConfig.RainDensity = System.Convert.ToSingle(this.RainDensity);
            
        }
    }

}