using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using Debug = System.Diagnostics.Debug;

namespace Modbed
{
    public class EnhancedBattleTestVM : ViewModel
    {
        private enum SaveParamResult
        {
            success, failed, notValid
        }
        private EnhancedBattleTestParams currentParams;
        private Action<EnhancedBattleTestParams> startAction;
        private Action<EnhancedBattleTestParams> backAction;
        private CharacterSelectionView _selectionView;

        private List<MultiplayerClassDivisions.MPHeroClass> _allMpHeroClasses;
        private Dictionary<string, Dictionary<string, List<MultiplayerClassDivisions.MPHeroClass>>>_allMpHeroClassesMap;

        private int _selectedSceneIndex;
        private string _playerSoldierCount, _enemySoldierCount;
        private string _distance;
        private string _soldierXInterval, _soldierYInterval;
        private string _soldiersPerRow;
        private string _formationPosition;
        private string _formationDirection;
        private string _skyBrightness;
        private string _rainDensity;

        private string _selectedMapName;
        private string _playerName, _playerTroopName, _enemyTroopName;

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
                if (value < 0 || value >= currentParams.sceneList.Length || value == this._selectedSceneIndex)
                    return;

                this.currentParams.sceneIndex = value;
                this._selectedSceneIndex = value;
                UpdateSceneContent();
            }
        }

        [DataSourceProperty]
        public string PlayerSoldierCount
        {
            get => this._playerSoldierCount;
            set
            {
                if (value == this._playerSoldierCount)
                    return;
                this._playerSoldierCount = value;
                this.OnPropertyChanged(nameof(PlayerSoldierCount));
            }
        }

        [DataSourceProperty]
        public string EnemySoldierCount
        {
            get => this._enemySoldierCount;
            set
            {
                if (value == this._enemySoldierCount)
                    return;
                this._enemySoldierCount = value;
                this.OnPropertyChanged(nameof(EnemySoldierCount));
            }
        }

        [DataSourceProperty]
        public string Distance
        {
            get => this._distance;
            set
            {
                if (value == this._distance)
                    return;
                this._distance = value;
                this.OnPropertyChanged(nameof(Distance));
            }
        }

        [DataSourceProperty]
        public string SoldierXInterval
        {
            get => this._soldierXInterval;
            set
            {
                if (value == this._soldierXInterval)
                    return;
                this._soldierXInterval = value;
                this.OnPropertyChanged(nameof(SoldierXInterval));
            }
        }

        [DataSourceProperty]
        public string SoldierYInterval
        {
            get => this._soldierYInterval;
            set
            {
                if (value == this._soldierYInterval)
                    return;
                this._soldierYInterval = value;
                this.OnPropertyChanged(nameof(SoldierYInterval));
            }
        }
    
        [DataSourceProperty]
        public string SoldiersPerRow
        {
            get => this._soldiersPerRow;
            set
            {
                if (value == this._soldiersPerRow)
                    return;
                this._soldiersPerRow = value;
                this.OnPropertyChanged(nameof(SoldiersPerRow));
            }
        }

        [DataSourceProperty]
        public string PlayerName
        {
            get => this._playerName;
            set
            {
                if (this._playerName == value)
                    return;
                this._playerName = value;
                this.OnPropertyChanged(nameof(PlayerName));
            }
        }

        public MultiplayerClassDivisions.MPHeroClass PlayerHeroClass
        {
            get => this.currentParams.PlayerHeroClass;
            set
            {
                this.currentParams.PlayerHeroClass = value;
                this.PlayerName = value?.HeroName.ToString();
            }
        }

        [DataSourceProperty]
        public string PlayerTroopName
        {
            get => this._playerTroopName;
            set
            {
                if (this._playerTroopName == value)
                    return;
                this._playerTroopName = value;
                this.OnPropertyChanged(nameof(PlayerTroopName));
            }
        }

        public MultiplayerClassDivisions.MPHeroClass PlayerTroopHeroClass
        {
            get => this.currentParams.PlayerTroopHeroClass;
            set
            {
                this.currentParams.PlayerTroopHeroClass = value;
                this.PlayerTroopName = value?.TroopName.ToString();
            }
        }

        [DataSourceProperty]
        public string EnemyTroopName
        {
            get => this._enemyTroopName;
            set
            {
                if (this._enemyTroopName == value)
                    return;
                this._enemyTroopName = value;
                this.OnPropertyChanged(nameof(EnemyTroopName));
            }
        }

        public MultiplayerClassDivisions.MPHeroClass EnemyTroopHeroClass
        {
            get => this.currentParams.EnemyTroopHeroClass;
            set
            {
                this.currentParams.EnemyTroopHeroClass = value;
                this.EnemyTroopName = value?.TroopName.ToString();
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

        [DataSourceProperty]
        public string FormationPosition
        {
            get => this._formationPosition;
            set
            {
                if (this._formationPosition == value)
                    return;
                this._formationPosition = value;
                this.OnPropertyChanged(nameof(FormationPosition));
            }
        }

        public string FormationDirection
        {
            get => this._formationDirection;
            set
            {
                if (this._formationDirection == value)
                    return;
                this._formationDirection = value;
                this.OnPropertyChanged(nameof(FormationDirection));
            }
        }

        public bool UseFreeCamera
        {
            get => this.currentParams.useFreeCamera;
            set => this.currentParams.useFreeCamera = value;
        }

        public EnhancedBattleTestVM(CharacterSelectionView selectionView, Action<EnhancedBattleTestParams> startAction,
            Action<EnhancedBattleTestParams> backAction)
        {
            this._selectionView = selectionView;
            this.currentParams = EnhancedBattleTestParams.Get();

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
            if (this.SelectedSceneIndex + 1 >= currentParams.sceneList.Length)
                return;
            this.SelectedSceneIndex++;
        }

        private void StartEnhancedBattleTest()
        {
            if (SaveConfig() != SaveParamResult.success)
                return;
            ModuleLogger.Writer.WriteLine("StartEnhancedBattleTest");
            this.startAction(currentParams);
        }

        private void Save()
        {
            SaveConfig();
        }

        private void ResetToDefault()
        {
            this.currentParams.ResetToDefault();
            this.InitializeContent();
            EnhancedBattleTestUtility.DisplayMessage("Reset successfully");
        }

        private void GoBack()
        {
            backAction(this.currentParams);
        }

        private void SelectPlayerCharacter() {
            ModuleLogger.Log("SelectPlayerCharacter");
            var p = new CharacterSelectionParams {
                allMpHeroClassMap = this._allMpHeroClassesMap,
                isTroop = false,
                selectedHeroClass = this.PlayerHeroClass,
                selectedPerk = this.currentParams.playerSelectedPerk,
                selectAction = (heroClass, perkIndex) =>
                {
                    this.PlayerHeroClass = heroClass;
                    this.currentParams.playerSelectedPerk = perkIndex;
                    this._selectionView.OnClose();
                }
            };
            _selectionView.Open(p);
        }

        private void SelectPlayerSoldierCharacter() {
            ModuleLogger.Log("SelectPlayerSoldierCharacter");
            var p = new CharacterSelectionParams {
                allMpHeroClassMap = this._allMpHeroClassesMap,
                isTroop = true,
                selectedHeroClass = this.PlayerTroopHeroClass,
                selectedPerk = this.currentParams.playerTroopSelectedPerk,
                selectAction = (heroClass, perkIndex) =>
                {
                    this.PlayerTroopHeroClass = heroClass;
                    this.currentParams.playerTroopSelectedPerk = perkIndex;
                    this._selectionView.OnClose();
                }
            };
            _selectionView.Open(p);
        }

        private void SelectEnemySoldierCharacter() {
            ModuleLogger.Log("SelectPlayerCharacter");
            var p = new CharacterSelectionParams {
                allMpHeroClassMap = this._allMpHeroClassesMap,
                isTroop = true,
                selectedHeroClass = this.EnemyTroopHeroClass,
                selectedPerk = this.currentParams.enemyTroopSelectedPerk,
                selectAction = (heroClass, perkIndex) =>
                {
                    this.EnemyTroopHeroClass = heroClass;
                    this.currentParams.enemyTroopSelectedPerk = perkIndex;
                    this._selectionView.OnClose();
                }
            };
            _selectionView.Open(p);
        }

        private void InitializeContent()
        {
            this._selectedSceneIndex = currentParams.sceneIndex;
            UpdateSceneContent();

            this.PlayerSoldierCount = currentParams.playerSoldierCount.ToString();
            this.EnemySoldierCount = currentParams.enemySoldierCount.ToString();
            this.Distance = currentParams.distance.ToString();
            this.SoldierXInterval = currentParams.soldierXInterval.ToString();
            this.SoldierXInterval = currentParams.soldierYInterval.ToString();

            this._allMpHeroClassesMap = GetHeroClassesMap();
            this._allMpHeroClasses = GetHeroClasses().ToList();
            if (this.PlayerHeroClass == null) this.PlayerHeroClass = this._allMpHeroClasses[0];
            if (this.PlayerTroopHeroClass == null) this.PlayerTroopHeroClass = this._allMpHeroClasses[0];
            if (this.EnemyTroopHeroClass == null) this.EnemyTroopHeroClass = this._allMpHeroClasses[0];

            this.PlayerName = this.PlayerHeroClass.HeroName.ToString();
            this.PlayerTroopName = this.PlayerTroopHeroClass.TroopName.ToString();
            this.EnemyTroopName = this.EnemyTroopHeroClass.TroopName.ToString();
        }

        private void UpdateSceneContent()
        {
            this.SelectedMapName = currentParams.SceneName;
            this.SoldiersPerRow = currentParams.SoldiersPerRow.ToString();
            this.FormationPosition = Vec2ToString(currentParams.FormationPosition);
            this.FormationDirection = Vec2ToString(currentParams.FormationDirection);
            this.SkyBrightness = currentParams.SkyBrightness.ToString();
            this.RainDensity = currentParams.RainDensity.ToString();
        }

        private SaveParamResult SaveConfig()
        {
            try
            {
                ApplyConfig();
            }
            catch
            {
                EnhancedBattleTestUtility.DisplayMessage("Content is illegal.");
                return SaveParamResult.notValid;
            }

            if (!currentParams.validate())
            {
                EnhancedBattleTestUtility.DisplayMessage("Content is out of range.");
                return SaveParamResult.notValid;
            }
            currentParams.Serialize();
            return SaveParamResult.success;
        }

        private void ApplyConfig()
        {
            currentParams.sceneIndex = this.SelectedSceneIndex;
            currentParams.SoldiersPerRow = System.Convert.ToInt32(this.SoldiersPerRow);
            currentParams.FormationPosition = StringToVec2(this.FormationPosition);
            currentParams.FormationDirection = StringToVec2(this.FormationDirection).Normalized();
            currentParams.SkyBrightness = System.Convert.ToSingle(this.SkyBrightness);
            currentParams.RainDensity = System.Convert.ToSingle(this.RainDensity);

            currentParams.playerSoldierCount = System.Convert.ToInt32(this.PlayerSoldierCount);
            currentParams.enemySoldierCount = System.Convert.ToInt32(this.EnemySoldierCount);
            currentParams.distance = System.Convert.ToSingle(this.Distance);
            currentParams.soldierXInterval = System.Convert.ToSingle(this.SoldierXInterval);
            currentParams.soldierYInterval = System.Convert.ToSingle(this.SoldierYInterval);
        }

        private List<MultiplayerClassDivisions.MPHeroClass> GetHeroClasses()
        {
            return MultiplayerClassDivisions.GetMPHeroClasses().ToList();
        }

        private Dictionary<string, Dictionary<string, List<MultiplayerClassDivisions.MPHeroClass>>>
            GetHeroClassesMap()
        {
            if (MultiplayerClassDivisions.AvailableCultures == null)
                MultiplayerClassDivisions.Initialize();
            Debug.Assert(MultiplayerClassDivisions.AvailableCultures != null, "Available Cultures should not be null");
            var heroesInCulture =
                new Dictionary<string, Dictionary<string, List<MultiplayerClassDivisions.MPHeroClass>>>();
            foreach (var eachCulture in MultiplayerClassDivisions.AvailableCultures)
            {
                var heroesInGroup = new Dictionary<string, List<MultiplayerClassDivisions.MPHeroClass>>();
                foreach (var mpHeroClass in MultiplayerClassDivisions.GetMPHeroClasses(eachCulture))
                {
                    List<MultiplayerClassDivisions.MPHeroClass> heroList = null;
                    if (!heroesInGroup.TryGetValue(mpHeroClass.ClassGroup.StringId, out heroList))
                    {
                        heroesInGroup[mpHeroClass.ClassGroup.StringId] = heroList = new List<MultiplayerClassDivisions.MPHeroClass>();
                    }

                    heroList.Add(mpHeroClass);
                }

                heroesInCulture.Add(eachCulture.StringId, heroesInGroup);
            }

            return heroesInCulture;
        }

        private string Vec2ToString(Vec2 vec2)
        {
            return $"{vec2.x},{vec2.y}";
        }

        private Vec2 StringToVec2(string str)
        {
            var posParts = str.Split(',');
            return new Vec2(System.Convert.ToSingle(posParts[0]), System.Convert.ToSingle(posParts[1]));
        }
    }
    
}