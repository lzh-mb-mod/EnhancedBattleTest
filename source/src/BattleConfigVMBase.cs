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
        // ViewModel does not allow property to be virtual or abstract.
        // Because there would be two property with the same name, which will cause exception thrown in constructor of ViewModel
        protected T CurrentConfig { get; set; }
        private CharacterSelectionView _selectionView;
        private List<MultiplayerClassDivisions.MPHeroClass> _allMpHeroClasses;
        private Dictionary<string, Dictionary<string, List<MultiplayerClassDivisions.MPHeroClass>>> _allMpHeroClassesMap;

        private string _playerSoldierCount, _enemySoldierCount;
        private string _playerName, _playerTroopName, _enemyTroopName;


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
            get => this.CurrentConfig.PlayerTroopHeroClass;
            set
            {
                this.CurrentConfig.PlayerTroopHeroClass = value;
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
            get => this.CurrentConfig.EnemyTroopHeroClass;
            set
            {
                this.CurrentConfig.EnemyTroopHeroClass = value;
                this.EnemyTroopName = value?.TroopName.ToString();
            }
        }

        public BattleConfigVMBase(CharacterSelectionView selectionView, T currentConfig)
        {
            this._selectionView = selectionView;
            this.CurrentConfig = currentConfig;
            InitializeCharactersContent();
        }

        private void InitializeCharactersContent()
        {
            this.PlayerSoldierCount = CurrentConfig.playerSoldierCount.ToString();
            this.EnemySoldierCount = CurrentConfig.enemySoldierCount.ToString();

            this._allMpHeroClassesMap = GetHeroClassesMap();
            this._allMpHeroClasses = GetHeroClasses().ToList();

            if (this.PlayerHeroClass == null) this.PlayerHeroClass = this._allMpHeroClasses[0];
            if (this.PlayerTroopHeroClass == null) this.PlayerTroopHeroClass = this._allMpHeroClasses[0];
            if (this.EnemyTroopHeroClass == null) this.EnemyTroopHeroClass = this._allMpHeroClasses[0];

            this.PlayerName = this.PlayerHeroClass.HeroName.ToString();
            this.PlayerTroopName = this.PlayerTroopHeroClass.TroopName.ToString();
            this.EnemyTroopName = this.EnemyTroopHeroClass.TroopName.ToString();
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

        protected void SelectPlayerTroopCharacter()
        {
            ModuleLogger.Log("SelectPlayerTroopCharacter");
            _selectionView.Open(CharacterSelectionParams.CharacterSelectionParamsFor(this._allMpHeroClassesMap,
                this.CurrentConfig.playerTroopClass, true, (param) =>
                {
                    this.PlayerTroopHeroClass = param.selectedHeroClass;
                    this.CurrentConfig.playerTroopClass.selectedFirstPerk = param.selectedFirstPerk;
                    this.CurrentConfig.playerTroopClass.selectedSecondPerk = param.selectedSecondPerk;
                    _selectionView.OnClose();
                }));
        }

        protected void SelectEnemyTroopCharacter()
        {
            ModuleLogger.Log("SelectEnemyTroopCharacter");
            _selectionView.Open(CharacterSelectionParams.CharacterSelectionParamsFor(this._allMpHeroClassesMap,
                this.CurrentConfig.enemyTroopClass,
                true, (param) =>
                {
                    this.EnemyTroopHeroClass = param.selectedHeroClass;
                    this.CurrentConfig.enemyTroopClass.selectedFirstPerk = param.selectedFirstPerk;
                    this.CurrentConfig.enemyTroopClass.selectedSecondPerk = param.selectedSecondPerk;
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
            CurrentConfig.playerSoldierCount = System.Convert.ToInt32(this.PlayerSoldierCount);
            CurrentConfig.enemySoldierCount = System.Convert.ToInt32(this.EnemySoldierCount);
        }
    }
}