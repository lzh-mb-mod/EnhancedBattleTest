using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Debug = System.Diagnostics.Debug;

namespace EnhancedBattleTest
{
    public abstract class BattleConfigVMBase<T>: ViewModel where T : BattleConfigBase<T>
    {
        protected enum SaveParamResult
        {
            success, failed, notValid
        }

        struct TroopInfo
        {
            public string name;
            public string count;
        }
        // ViewModel does not allow property to be virtual or abstract.
        // Because there would be two property with the same name, which will cause exception thrown in constructor of ViewModel
        protected T CurrentConfig { get; set; }
        private CharacterSelectionView _selectionView;
        private List<MultiplayerClassDivisions.MPHeroClass> _allMpHeroClasses;
        private Dictionary<string, Dictionary<string, List<MultiplayerClassDivisions.MPHeroClass>>> _allMpHeroClassesMap;
        
        private string _playerName, _enemyName;
        private TroopInfo[] _playerTroopInfos, _enemyTroopInfos;


        [DataSourceProperty]
        public string PlayerTroopCount1
        {
            get => this._playerTroopInfos[0].count;
            set
            {
                if (value == this._playerTroopInfos[0].count)
                    return;
                this._playerTroopInfos[0].count = value;
                this.OnPropertyChanged(nameof(PlayerTroopCount1));
            }
        }
        [DataSourceProperty]
        public string PlayerTroopCount2
        {
            get => this._playerTroopInfos[1].count;
            set
            {
                if (value == this._playerTroopInfos[1].count)
                    return;
                this._playerTroopInfos[1].count = value;
                this.OnPropertyChanged(nameof(PlayerTroopCount2));
            }
        }
        [DataSourceProperty]
        public string PlayerTroopCount3
        {
            get => this._playerTroopInfos[2].count;
            set
            {
                if (value == this._playerTroopInfos[2].count)
                    return;
                this._playerTroopInfos[2].count = value;
                this.OnPropertyChanged(nameof(PlayerTroopCount3));
            }
        }

        void UpdatePlayerSoldierCount()
        {
            PlayerTroopCount1 = CurrentConfig.playerTroops[0].troopCount.ToString();
            PlayerTroopCount2 = CurrentConfig.playerTroops[1].troopCount.ToString();
            PlayerTroopCount3 = CurrentConfig.playerTroops[2].troopCount.ToString();
        }

        [DataSourceProperty]
        public string EnemyTroopCount1
        {
            get => this._enemyTroopInfos[0].count;
            set
            {
                if (value == this._enemyTroopInfos[0].count)
                    return;
                this._enemyTroopInfos[0].count = value;
                this.OnPropertyChanged(nameof(EnemyTroopCount1));
            }
        }
        [DataSourceProperty]
        public string EnemyTroopCount2
        {
            get => this._enemyTroopInfos[1].count;
            set
            {
                if (value == this._enemyTroopInfos[1].count)
                    return;
                this._enemyTroopInfos[1].count = value;
                this.OnPropertyChanged(nameof(EnemyTroopCount2));
            }
        }
        [DataSourceProperty]
        public string EnemyTroopCount3
        {
            get => this._enemyTroopInfos[2].count;
            set
            {
                if (value == this._enemyTroopInfos[2].count)
                    return;
                this._enemyTroopInfos[2].count = value;
                this.OnPropertyChanged(nameof(EnemyTroopCount3));
            }
        }

        void UpdateEnemySoldierCount()
        {
            EnemyTroopCount1 = CurrentConfig.enemyTroops[0].troopCount.ToString();
            EnemyTroopCount2 = CurrentConfig.enemyTroops[1].troopCount.ToString();
            EnemyTroopCount3 = CurrentConfig.enemyTroops[2].troopCount.ToString();
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
            get => this.CurrentConfig.PlayerHeroClass;
            set
            {
                this.CurrentConfig.PlayerHeroClass = value;
                this.PlayerName = value?.HeroName.ToString();
            }
        }

        [DataSourceProperty]
        public string EnemyName
        {
            get => this._enemyName;
            set
            {
                if (this._enemyName == value)
                    return;
                this._enemyName = value;
                this.OnPropertyChanged(nameof(EnemyName));
            }
        }

        public MultiplayerClassDivisions.MPHeroClass EnemyHeroClass
        {
            get => this.CurrentConfig.EnemyHeroClass;
            set
            {
                this.CurrentConfig.EnemyHeroClass = value;
                this.EnemyName = value?.HeroName.ToString();
            }
        }

        public bool SpawnEnemyCommander
        {
            get => this.CurrentConfig.SpawnEnemyCommander;
            set
            {
                this.CurrentConfig.SpawnEnemyCommander = value;
                this.OnPropertyChanged(nameof(SpawnEnemyCommander));
            }
        }

        [DataSourceProperty]
        public string PlayerTroopName1
        {
            get => this._playerTroopInfos[0].name;
            set
            {
                if (this._playerTroopInfos[0].name == value)
                    return;
                this._playerTroopInfos[0].name = value;
                this.OnPropertyChanged(nameof(PlayerTroopName1));
            }
        }

        [DataSourceProperty]
        public string PlayerTroopName2
        {
            get => this._playerTroopInfos[1].name;
            set
            {
                if (this._playerTroopInfos[1].name == value)
                    return;
                this._playerTroopInfos[1].name = value;
                this.OnPropertyChanged(nameof(PlayerTroopName2));
            }
        }

        [DataSourceProperty]
        public string PlayerTroopName3
        {
            get => this._playerTroopInfos[2].name;
            set
            {
                if (this._playerTroopInfos[2].name == value)
                    return;
                this._playerTroopInfos[2].name = value;
                this.OnPropertyChanged(nameof(PlayerTroopName3));
            }
        }

        private void UpdatePlayerTroopName()
        {
            this.PlayerTroopName1 = this.PlayerTroopHeroClass1.TroopName.ToString();
            this.PlayerTroopName2 = this.PlayerTroopHeroClass2.TroopName.ToString();
            this.PlayerTroopName3 = this.PlayerTroopHeroClass3.TroopName.ToString();
        }

        public MultiplayerClassDivisions.MPHeroClass PlayerTroopHeroClass1
        {
            get => this.CurrentConfig.GetPlayerTroopHeroClass(0);
            set
            {
                this.CurrentConfig.SetPlayerTroopHeroClass(0, value);
                this.PlayerTroopName1 = value?.TroopName.ToString();
            }
        }
        public MultiplayerClassDivisions.MPHeroClass PlayerTroopHeroClass2
        {
            get => this.CurrentConfig.GetPlayerTroopHeroClass(1);
            set
            {
                this.CurrentConfig.SetPlayerTroopHeroClass(1, value);
                this.PlayerTroopName2 = value?.TroopName.ToString();
            }
        }
        public MultiplayerClassDivisions.MPHeroClass PlayerTroopHeroClass3
        {
            get => this.CurrentConfig.GetPlayerTroopHeroClass(2);
            set
            {
                this.CurrentConfig.SetPlayerTroopHeroClass(2, value);
                this.PlayerTroopName3 = value?.TroopName.ToString();
            }
        }

        [DataSourceProperty]
        public string EnemyTroopName1
        {
            get => this._enemyTroopInfos[0].name;
            set
            {
                if (this._enemyTroopInfos[0].name == value)
                    return;
                this._enemyTroopInfos[0].name = value;
                this.OnPropertyChanged(nameof(EnemyTroopName1));
            }
        }

        [DataSourceProperty]
        public string EnemyTroopName2
        {
            get => this._enemyTroopInfos[1].name;
            set
            {
                if (this._enemyTroopInfos[1].name == value)
                    return;
                this._enemyTroopInfos[1].name = value;
                this.OnPropertyChanged(nameof(EnemyTroopName2));
            }
        }

        [DataSourceProperty]
        public string EnemyTroopName3
        {
            get => this._enemyTroopInfos[2].name;
            set
            {
                if (this._enemyTroopInfos[2].name == value)
                    return;
                this._enemyTroopInfos[2].name = value;
                this.OnPropertyChanged(nameof(EnemyTroopName3));
            }
        }

        private void UpdateEnemyTroopName()
        {
            this.EnemyTroopName1 = EnemyTroopHeroClass1.TroopName.ToString();
            this.EnemyTroopName2 = EnemyTroopHeroClass2.TroopName.ToString();
            this.EnemyTroopName3 = EnemyTroopHeroClass3.TroopName.ToString();
        }

        public MultiplayerClassDivisions.MPHeroClass EnemyTroopHeroClass1
        {
            get => this.CurrentConfig.GetEnemyTroopHeroClass(0);
            set
            {
                this.CurrentConfig.SetEnemyTroopHeroClass(0, value);
                this.EnemyTroopName1 = value?.TroopName.ToString();
            }
        }

        public MultiplayerClassDivisions.MPHeroClass EnemyTroopHeroClass2
        {
            get => this.CurrentConfig.GetEnemyTroopHeroClass(1);
            set
            {
                this.CurrentConfig.SetEnemyTroopHeroClass(1, value);
                this.EnemyTroopName2 = value?.TroopName.ToString();
            }
        }

        public MultiplayerClassDivisions.MPHeroClass EnemyTroopHeroClass3
        {
            get => this.CurrentConfig.GetEnemyTroopHeroClass(2);
            set
            {
                this.CurrentConfig.SetEnemyTroopHeroClass(2, value);
                this.EnemyTroopName3 = value?.TroopName.ToString();
            }
        }

        protected BattleConfigVMBase(CharacterSelectionView selectionView, T currentConfig)
        {
            this._selectionView = selectionView;
            this.CurrentConfig = currentConfig;
            this._playerTroopInfos = new TroopInfo[3];
            this._enemyTroopInfos = new TroopInfo[3];
            InitializeCharactersContent();
        }

        private void InitializeCharactersContent()
        {
            UpdatePlayerSoldierCount();
            UpdateEnemySoldierCount();

            this._allMpHeroClassesMap = GetHeroClassesMap();
            this._allMpHeroClasses = GetHeroClasses().ToList();

            if (this.PlayerHeroClass == null) this.PlayerHeroClass = this._allMpHeroClasses[0];
            if (this.EnemyHeroClass == null) this.EnemyHeroClass = this._allMpHeroClasses[0];
            if (this.PlayerTroopHeroClass1 == null) this.PlayerTroopHeroClass1 = this._allMpHeroClasses[0];
            if (this.PlayerTroopHeroClass2 == null) this.PlayerTroopHeroClass2 = this._allMpHeroClasses[0];
            if (this.PlayerTroopHeroClass3 == null) this.PlayerTroopHeroClass3 = this._allMpHeroClasses[0];
            if (this.EnemyTroopHeroClass1 == null) this.EnemyTroopHeroClass1 = this._allMpHeroClasses[0];
            if (this.EnemyTroopHeroClass2 == null) this.EnemyTroopHeroClass2 = this._allMpHeroClasses[0];
            if (this.EnemyTroopHeroClass3 == null) this.EnemyTroopHeroClass3 = this._allMpHeroClasses[0];

            this.PlayerName = this.PlayerHeroClass.HeroName.ToString();
            this.EnemyName = this.EnemyHeroClass.HeroName.ToString();
            UpdatePlayerTroopName();
            UpdateEnemyTroopName();
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
            if (MultiplayerClassDivisions.AvailableCultures != null)
                foreach (var eachCulture in MultiplayerClassDivisions.AvailableCultures)
                {
                    var heroesInGroup = new Dictionary<string, List<MultiplayerClassDivisions.MPHeroClass>>();
                    foreach (var mpHeroClass in MultiplayerClassDivisions.GetMPHeroClasses(eachCulture))
                    {
                        List<MultiplayerClassDivisions.MPHeroClass> heroList = null;
                        if (!heroesInGroup.TryGetValue(mpHeroClass.ClassGroup.StringId, out heroList))
                        {
                            heroesInGroup[mpHeroClass.ClassGroup.StringId] =
                                heroList = new List<MultiplayerClassDivisions.MPHeroClass>();
                        }

                        heroList.Add(mpHeroClass);
                    }

                    heroesInCulture.Add(eachCulture.StringId, heroesInGroup);
                }

            return heroesInCulture;
        }

        protected void SelectPlayerCharacter()
        {
            ModuleLogger.Log("SelectPlayerCharacter");
            _selectionView.Open(CharacterSelectionParams.CharacterSelectionParamsFor(this._allMpHeroClassesMap,
                this.CurrentConfig.playerClass, false, (param) =>
                {
                    this.PlayerHeroClass = param.selectedHeroClass;
                    this.CurrentConfig.playerClass.selectedFirstPerk = param.selectedFirstPerk;
                    this.CurrentConfig.playerClass.selectedSecondPerk = param.selectedSecondPerk;
                    _selectionView.OnClose();
                }));
        }

        protected void SelectEnemyCharacter()
        {
            ModuleLogger.Log("SelectEnemyCharacter");
            _selectionView.Open(CharacterSelectionParams.CharacterSelectionParamsFor(this._allMpHeroClassesMap,
                this.CurrentConfig.playerClass, false, (param) =>
                {
                    this.EnemyHeroClass = param.selectedHeroClass;
                    this.CurrentConfig.enemyClass.selectedFirstPerk = param.selectedFirstPerk;
                    this.CurrentConfig.enemyClass.selectedSecondPerk = param.selectedSecondPerk;
                    _selectionView.OnClose();
                }));
        }

        protected void SelectPlayerTroopCharacter1()
        {
            ModuleLogger.Log("SelectPlayerTroopCharacter1");
            _selectionView.Open(CharacterSelectionParams.CharacterSelectionParamsFor(this._allMpHeroClassesMap,
                this.CurrentConfig.playerTroops[0], true, (param) =>
                {
                    this.PlayerTroopHeroClass1 = param.selectedHeroClass;
                    this.CurrentConfig.playerTroops[0].selectedFirstPerk = param.selectedFirstPerk;
                    this.CurrentConfig.playerTroops[0].selectedSecondPerk = param.selectedSecondPerk;
                    _selectionView.OnClose();
                }));
        }

        protected void SelectPlayerTroopCharacter2()
        {
            ModuleLogger.Log("SelectPlayerTroopCharacter2");
            _selectionView.Open(CharacterSelectionParams.CharacterSelectionParamsFor(this._allMpHeroClassesMap,
                this.CurrentConfig.playerTroops[1], true, (param) =>
                {
                    this.PlayerTroopHeroClass2 = param.selectedHeroClass;
                    this.CurrentConfig.playerTroops[1].selectedFirstPerk = param.selectedFirstPerk;
                    this.CurrentConfig.playerTroops[1].selectedSecondPerk = param.selectedSecondPerk;
                    _selectionView.OnClose();
                }));
        }

        protected void SelectPlayerTroopCharacter3()
        {
            ModuleLogger.Log("SelectPlayerTroopCharacter3");
            _selectionView.Open(CharacterSelectionParams.CharacterSelectionParamsFor(this._allMpHeroClassesMap,
                this.CurrentConfig.playerTroops[2], true, (param) =>
                {
                    this.PlayerTroopHeroClass3 = param.selectedHeroClass;
                    this.CurrentConfig.playerTroops[2].selectedFirstPerk = param.selectedFirstPerk;
                    this.CurrentConfig.playerTroops[2].selectedSecondPerk = param.selectedSecondPerk;
                    _selectionView.OnClose();
                }));
        }

        protected void SelectEnemyTroopCharacter1()
        {
            ModuleLogger.Log("SelectEnemyTroopCharacter1");
            _selectionView.Open(CharacterSelectionParams.CharacterSelectionParamsFor(this._allMpHeroClassesMap,
                this.CurrentConfig.enemyTroops[0],
                true, (param) =>
                {
                    this.EnemyTroopHeroClass1 = param.selectedHeroClass;
                    this.CurrentConfig.enemyTroops[0].selectedFirstPerk = param.selectedFirstPerk;
                    this.CurrentConfig.enemyTroops[0].selectedSecondPerk = param.selectedSecondPerk;
                    _selectionView.OnClose();
                }));
        }

        protected void SelectEnemyTroopCharacter2()
        {
            ModuleLogger.Log("SelectEnemyTroopCharacter2");
            _selectionView.Open(CharacterSelectionParams.CharacterSelectionParamsFor(this._allMpHeroClassesMap,
                this.CurrentConfig.enemyTroops[1],
                true, (param) =>
                {
                    this.EnemyTroopHeroClass2 = param.selectedHeroClass;
                    this.CurrentConfig.enemyTroops[1].selectedFirstPerk = param.selectedFirstPerk;
                    this.CurrentConfig.enemyTroops[1].selectedSecondPerk = param.selectedSecondPerk;
                    _selectionView.OnClose();
                }));
        }

        protected void SelectEnemyTroopCharacter3()
        {
            ModuleLogger.Log("SelectEnemyTroopCharacter3");
            _selectionView.Open(CharacterSelectionParams.CharacterSelectionParamsFor(this._allMpHeroClassesMap,
                this.CurrentConfig.enemyTroops[2],
                true, (param) =>
                {
                    this.EnemyTroopHeroClass3 = param.selectedHeroClass;
                    this.CurrentConfig.enemyTroops[2].selectedFirstPerk = param.selectedFirstPerk;
                    this.CurrentConfig.enemyTroops[2].selectedSecondPerk = param.selectedSecondPerk;
                    _selectionView.OnClose();
                }));
        }
        protected SaveParamResult SaveConfig()
        {
            try
            {
                ApplyConfig();
            }
            catch
            {
                Utility.DisplayMessage("Content is illegal.");
                return SaveParamResult.notValid;
            }

            if (!CurrentConfig.Validate())
            {
                Utility.DisplayMessage("Content is out of range.");
                return SaveParamResult.notValid;
            }
            CurrentConfig.Serialize();
            return SaveParamResult.success;
        }

        protected virtual void ApplyConfig()
        {
            CurrentConfig.playerTroops.Zip(this._playerTroopInfos,
                (classInfo, troopInfo) => classInfo.troopCount = System.Convert.ToInt32(troopInfo.count));
            CurrentConfig.enemyTroops.Zip(this._enemyTroopInfos,
                (classInfo, troopInfo) => classInfo.troopCount = System.Convert.ToInt32(troopInfo.count));
        }
    }
}