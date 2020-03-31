using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Debug = System.Diagnostics.Debug;

namespace EnhancedBattleTest
{
    public abstract class BattleConfigVMBase<T> : ViewModel where T : BattleConfigBase<T>
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
        private MissionMenuView _missionMenuView;
        private List<MultiplayerClassDivisions.MPHeroClass> _allMpHeroClasses;
        private Dictionary<string, Dictionary<string, List<MultiplayerClassDivisions.MPHeroClass>>> _allMpHeroClassesMap;

        private string _playerName, _enemyName;
        private TroopInfo[] _playerTroopInfos, _enemyTroopInfos;
        private string _combatAI;

        public MultiplayerClassDivisions.MPHeroClass PlayerHeroClass
        {
            get => this.CurrentConfig.PlayerHeroClass;
            set
            {
                this.CurrentConfig.PlayerHeroClass = value;
                this.PlayerName = value?.HeroName.ToString();
            }
        }

        public string ControlTroopTipString { get; } = GameTexts.FindText("str_control_troop_tip").ToString();
        public string SwitchTeamTipString { get; } = GameTexts.FindText("str_switch_team_tip").ToString();
        public string SwitchFreeCameraTipString { get; } = GameTexts.FindText("str_switch_free_camera_tip").ToString();
        public string DisableDeathTipString { get; } = GameTexts.FindText("str_disable_death_tip").ToString();
        public string ResetMissionTipString { get; } = GameTexts.FindText("str_reset_mission_tip").ToString();
        public string MoreOptionTipString { get; } = GameTexts.FindText("str_more_option_tip").ToString();
        public string PauseTipString { get; } = GameTexts.FindText("str_pause_tip").ToString();
        public string LeaveMissionTip { get; } = GameTexts.FindText("str_leave_mission_tip").ToString();
        public string ReadPositionTip { get; } = GameTexts.FindText("str_read_position_tip").ToString();
        public string TeleportTip { get; } = GameTexts.FindText("str_teleport_tip").ToString();

        public string PlayerCharacterString { get; } = GameTexts.FindText("str_player_character").ToString();
        public string SpawnPlayerString { get; } = GameTexts.FindText("str_spawn_player").ToString();

        public string IsPlayerAttackerString { get; } = GameTexts.FindText("str_is_player_attacker").ToString();

        public string PlayerTroop1String { get; } = GameTexts.FindText("str_player_troop_1").ToString();
        public string PlayerTroop2String { get; } = GameTexts.FindText("str_player_troop_2").ToString();
        public string PlayerTroop3String { get; } = GameTexts.FindText("str_player_troop_3").ToString();

        public string PlayerTroop1CountString { get; } = GameTexts.FindText("str_player_troop_1_count").ToString();
        public string PlayerTroop2CountString { get; } = GameTexts.FindText("str_player_troop_2_count").ToString();
        public string PlayerTroop3CountString { get; } = GameTexts.FindText("str_player_troop_3_count").ToString();

        public string EnemyTroop1String { get; } = GameTexts.FindText("str_enemy_troop_1").ToString();
        public string EnemyTroop2String { get; } = GameTexts.FindText("str_enemy_troop_2").ToString();
        public string EnemyTroop3String { get; } = GameTexts.FindText("str_enemy_troop_3").ToString();

        public string EnemyTroop1CountString { get; } = GameTexts.FindText("str_enemy_troop_1_count").ToString();
        public string EnemyTroop2CountString { get; } = GameTexts.FindText("str_enemy_troop_2_count").ToString();
        public string EnemyTroop3CountString { get; } = GameTexts.FindText("str_enemy_troop_3_count").ToString();

        public string EnemyCommanderString { get; } = GameTexts.FindText("str_enemy_commander").ToString();
        public string SpawnEnemyCommanderString { get; } = GameTexts.FindText("str_spawn_enemy_commander").ToString();

        public string SoldiersPerRowString { get; } = GameTexts.FindText("str_soldiers_per_row").ToString();
        public string FormationPositionString { get; } = GameTexts.FindText("str_formation_position").ToString();
        public string FormationDirectionString { get; } = GameTexts.FindText("str_formation_direction").ToString();
        public string DistanceString { get; } = GameTexts.FindText("str_distance").ToString();
        public string SkyBrightnessString { get; } = GameTexts.FindText("str_sky_brightness").ToString();
        public string RainDensityString { get; } = GameTexts.FindText("str_rain_density").ToString();

        public string MoreOptionsString { get; } = GameTexts.FindText("str_more_options").ToString();
        public string NoFriendlyBannerString { get; } = GameTexts.FindText("str_no_friendly_banner").ToString();
        public string NoKillNotificationString { get; } = GameTexts.FindText("str_no_kill_notification").ToString();


        public string MakeGruntVoiceString { get; } = GameTexts.FindText("str_make_grunt_voice").ToString();
        public string HasBoundaryString { get; } = GameTexts.FindText("str_has_boundary").ToString();
        public string ChangeCombatAIString { get; } = GameTexts.FindText("str_change_combat_ai").ToString();
        public string CombatAIString { get; } = GameTexts.FindText("str_combat_ai").ToString();

        public string SaveAndStartString { get; } = GameTexts.FindText("str_save_and_start").ToString();
        public string SaveString { get; } = GameTexts.FindText("str_save_config").ToString();
        public string LoadConfigString { get; } = GameTexts.FindText("str_load_config").ToString();
        public string ExitString { get; } = GameTexts.FindText("str_exit").ToString();

        [DataSourceProperty]
        public bool SpawnPlayer
        {
            get => this.CurrentConfig.SpawnPlayer;
            set
            {
                this.CurrentConfig.SpawnPlayer = value;
                this.OnPropertyChanged(nameof(SpawnPlayer));
            }
        }

        [DataSourceProperty]
        public bool IsPlayerAttacker
        {
            get => this.CurrentConfig.isPlayerAttacker;
            set
            {
                if (this.CurrentConfig.isPlayerAttacker == value)
                    return;
                this.CurrentConfig.isPlayerAttacker = value;
                this.OnPropertyChanged(nameof(IsPlayerAttacker));
            }
        }

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

        [DataSourceProperty]
        public bool UseRealisticBlocking
        {
            get => this.CurrentConfig.useRealisticBlocking;
            set
            {
                if (this.CurrentConfig.useRealisticBlocking == value)
                    return;
                this.CurrentConfig.useRealisticBlocking = value;
                this.OnPropertyChanged(nameof(UseRealisticBlocking));
            }
        }

        [DataSourceProperty]
        public bool NoAgentLabel
        {
            get => this.CurrentConfig.noAgentLabel;
            set
            {
                if (this.CurrentConfig.noAgentLabel == value)
                    return;
                this.CurrentConfig.noAgentLabel = value;
                this.OnPropertyChanged(nameof(NoAgentLabel));
            }
        }

        [DataSourceProperty]
        public bool NoKillNotification
        {
            get => this.CurrentConfig.noKillNotification;
            set
            {
                if (this.CurrentConfig.noKillNotification == value)
                    return;
                this.CurrentConfig.noKillNotification = value;
                this.OnPropertyChanged(nameof(NoKillNotification));
            }
        }

        [DataSourceProperty]
        public bool ChangeCombatAI
        {
            get => this.CurrentConfig.changeCombatAI;
            set
            {
                if (this.CurrentConfig.changeCombatAI == value)
                    return;
                this.CurrentConfig.changeCombatAI = value;
                this.OnPropertyChanged(nameof(ChangeCombatAI));
            }
        }

        [DataSourceProperty]
        public string CombatAI
        {
            get => this._combatAI;
            set
            {
                if (this._combatAI == value)
                    return;
                this._combatAI = value;
                this.OnPropertyChanged(nameof(CombatAI));
            }
        }

        protected BattleConfigVMBase(CharacterSelectionView selectionView, MissionMenuView missionMenuView, T currentConfig)
        {
            this._selectionView = selectionView;
            this._missionMenuView = missionMenuView;
            this.CurrentConfig = currentConfig;
            this._playerTroopInfos = new TroopInfo[3];
            this._enemyTroopInfos = new TroopInfo[3];

            InitializeContent();
        }

        private void InitializeContent()
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

            this.ChangeCombatAI = this.CurrentConfig.changeCombatAI;
            this.CombatAI = this.CurrentConfig.combatAI.ToString();
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
            _selectionView.Open(CharacterSelectionParams.CharacterSelectionParamsFor(this._allMpHeroClassesMap,
                this.CurrentConfig.enemyClass, false, (param) =>
                {
                    this.EnemyHeroClass = param.selectedHeroClass;
                    this.CurrentConfig.enemyClass.selectedFirstPerk = param.selectedFirstPerk;
                    this.CurrentConfig.enemyClass.selectedSecondPerk = param.selectedSecondPerk;
                    _selectionView.OnClose();
                }));
        }

        protected void SelectPlayerTroopCharacter1()
        {
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

        protected void OpenMissionMenu()
        {
            this._missionMenuView.ActivateMenu();
        }

        protected SaveParamResult SaveConfig()
        {
            try
            {
                ApplyConfig();
            }
            catch
            {
                Utility.DisplayLocalizedText("str_content_illegal");
                return SaveParamResult.notValid;
            }

            if (!CurrentConfig.Validate())
            {
                Utility.DisplayLocalizedText("str_content_outofrange");
                return SaveParamResult.notValid;
            }
            CurrentConfig.Serialize();
            return SaveParamResult.success;
        }

        protected virtual void ApplyConfig()
        {
            for (int i = 0; i < CurrentConfig.playerTroops.Length; ++i)
            {
                CurrentConfig.playerTroops[i].troopCount = System.Convert.ToInt32(this._playerTroopInfos[i].count);
            }
            for (int i = 0; i < CurrentConfig.enemyTroops.Length; ++i)
            {
                CurrentConfig.enemyTroops[i].troopCount = System.Convert.ToInt32(this._enemyTroopInfos[i].count);
            }

            CurrentConfig.combatAI = System.Convert.ToInt32(this._combatAI);
        }
    }
}