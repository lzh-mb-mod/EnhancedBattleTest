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
        private EnhancedBattleTestParams currentParams;
        private Action<EnhancedBattleTestParams> startAction;
        private Action<EnhancedBattleTestParams> backAction;
        private CharacterSelectionView _selectionView;

        private List<MultiplayerClassDivisions.MPHeroClass> _allMpHeroClasses;
        private Dictionary<string, Dictionary<string, List<MultiplayerClassDivisions.MPHeroClass>>>_allMpHeroClassesMap;

        private string _selectedMapName;
        private int _selectedSceneIndex;
        private string _playerSoldierCount, _enemySoldierCount;
        private string _distance;
        private string _soldierXInterval, _soldierYInterval;
        private string _soldiersPerRow;
        private string _playerName, _playerTroopName, _enemyTroopName;
        private string _formationPosition;
        private string _formationDirection;
        private string _skyBrightness;

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
                if (value < 0 || value >= EnhancedBattleTestParams.SceneList.Length || value == this._selectedSceneIndex)
                    return;
                this._selectedSceneIndex = value;
                EnhancedBattleTestParams.SceneInfo info = EnhancedBattleTestParams.SceneList[value];
                this.SelectedMapName = info.name;
                this.FormationPosition = Vec2ToString(info.defaultPosition);
                this.FormationDirection = Vec2ToString(info.defaultDirection);
                this.SkyBrightness = info.defaultBrightness.ToString();
                this.SoldiersPerRow = info.defaultSoldiersPerRow.ToString();
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

        public string RainDensity => this.currentParams.rainDensity.ToString();

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
            : base()
        {
            this._selectionView = selectionView;
            this.currentParams = EnhancedBattleTestParams.Get();

            this.SelectedMapName = currentParams.SceneName;
            this.SelectedSceneIndex = currentParams.sceneIndex; 
            this._playerSoldierCount = currentParams.playerSoldierCount.ToString();
            this._enemySoldierCount = currentParams.enemySoldierCount.ToString();
            this._distance = currentParams.distance.ToString();
            this._soldierXInterval = currentParams.soldierXInterval.ToString();
            this._soldierYInterval = currentParams.soldierYInterval.ToString();
            this._soldiersPerRow = currentParams.soldiersPerRow.ToString();
            this.FormationPosition = Vec2ToString(currentParams.formationPosition);
            this.FormationDirection = Vec2ToString(currentParams.formationDirection);
            this.SkyBrightness = currentParams.skyBrightness.ToString();

            this.PlayerName = this.PlayerHeroClass.HeroName.ToString();
            this.PlayerTroopName = this.PlayerTroopHeroClass.TroopName.ToString();
            this.EnemyTroopName = this.EnemyTroopHeroClass.TroopName.ToString();
            this.startAction = startAction;
            this.backAction = backAction;
            
            this._allMpHeroClassesMap = GetHeroClassesMap();
            this._allMpHeroClasses = GetHeroClasses().ToList();
            if (this.PlayerHeroClass == null) this.PlayerHeroClass = this._allMpHeroClasses[0];
            if (this.PlayerTroopHeroClass == null) this.PlayerTroopHeroClass = this._allMpHeroClasses[0];
            if (this.EnemyTroopHeroClass == null) this.EnemyTroopHeroClass = this._allMpHeroClasses[0];
        }

        private void PreviousMap()
        {
            if (this.SelectedSceneIndex == 0)
                return;
            this.SelectedSceneIndex--;
        }
        private void NextMap()
        {
            if (this.SelectedSceneIndex + 1 >= EnhancedBattleTestParams.SceneList.Length)
                return;
            this.SelectedSceneIndex++;
        }

        private void StartEnhancedBattleTest()
        {
            try
            {
                currentParams.sceneIndex = this.SelectedSceneIndex;
                currentParams.playerSoldierCount = System.Convert.ToInt32(this.PlayerSoldierCount);
                currentParams.enemySoldierCount = System.Convert.ToInt32(this.EnemySoldierCount);
                currentParams.distance = System.Convert.ToSingle(this.Distance);
                currentParams.soldierXInterval = System.Convert.ToSingle(this.SoldierXInterval);
                currentParams.soldierYInterval = System.Convert.ToSingle(this.SoldierYInterval);
                currentParams.soldiersPerRow = System.Convert.ToInt32(this.SoldiersPerRow);
                currentParams.formationPosition = StringToVec2(this.FormationPosition);
                currentParams.formationDirection = StringToVec2(this.FormationDirection).Normalized();
                currentParams.skyBrightness = System.Convert.ToSingle(this.SkyBrightness);
                EnhancedBattleTestParams.SceneInfo info = EnhancedBattleTestParams.SceneList[this.SelectedSceneIndex];
                info.defaultPosition = currentParams.formationPosition;
                info.defaultDirection = currentParams.formationDirection;
                info.defaultBrightness = currentParams.skyBrightness;
                info.defaultSoldiersPerRow = currentParams.soldiersPerRow;
            }
            catch
            {
                return;
            }
            if (!currentParams.validate()) {
                return;
            }
            ModuleLogger.Writer.WriteLine("StartEnhancedBattleTest");

            this.startAction(currentParams);
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