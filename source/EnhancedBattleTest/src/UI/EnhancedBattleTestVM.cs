using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.Data.MissionData;
using EnhancedBattleTest.GameMode;
using EnhancedBattleTest.UI.Basic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;

namespace EnhancedBattleTest.UI
{
    public class EnhancedBattleTestVM : ViewModel
    {
        private const int MAX_ATTACKER_MELEE_MACHINE_COUNT = 3;
        private const int MAX_ATTACKER_RANGED_MACHINE_COUNT = 4;
        private const int MAX_DEFENDER_MACHINE_COUNT = 4;

        private readonly EnhancedBattleTestState _state;
        private BattleConfig _config;
        private readonly List<SceneData> _scenes;
        private bool _hasShownCampaignStateWarning;

        private MBBindingList<CustomBattleSiegeMachineVM> _attackerMeleeMachines;
        private MBBindingList<CustomBattleSiegeMachineVM> _attackerRangedMachines;
        private MBBindingList<CustomBattleSiegeMachineVM> _defenderMachines;

        private bool _isAttackerCustomMachineSelectionEnabled;
        private bool _isDefenderCustomMachineSelectionEnabled;
        private SideVM _playerSide;
        private SideVM _enemySide;

        public TextVM TitleText { get; }

        public TextVM SwapTeamText { get; }

        public TextVM MapText { get; }

        public TextVM StartButtonText { get; }

        [DataSourceProperty]
        public SideVM PlayerSide
        {
            get => _playerSide;
            set
            {
                if (_playerSide == value)
                    return;
                _playerSide = value;
                OnPropertyChanged(nameof(PlayerSide));
            }
        }

        [DataSourceProperty]
        public SideVM EnemySide
        {
            get => _enemySide;
            set
            {
                if (_enemySide == value)
                    return;
                _enemySide = value;
                OnPropertyChanged(nameof(EnemySide));
            }
        }
        public BattleTypeSelectionGroup BattleTypeSelectionGroup { get; }
        public MapSelectionGroupVM MapSelectionGroup { get; }

        [DataSourceProperty]
        public MBBindingList<CustomBattleSiegeMachineVM> AttackerMeleeMachines
        {
            get => _attackerMeleeMachines;
            set
            {
                if (value == _attackerMeleeMachines)
                    return;
                _attackerMeleeMachines = value;
                OnPropertyChanged(nameof(AttackerMeleeMachines));
            }
        }

        [DataSourceProperty]
        public MBBindingList<CustomBattleSiegeMachineVM> AttackerRangedMachines
        {
            get => _attackerRangedMachines;
            set
            {
                if (value == _attackerRangedMachines)
                    return;
                _attackerRangedMachines = value;
                OnPropertyChanged(nameof(AttackerRangedMachines));
            }
        }

        [DataSourceProperty]
        public MBBindingList<CustomBattleSiegeMachineVM> DefenderMachines
        {
            get => _defenderMachines;
            set
            {
                if (value == _defenderMachines)
                    return;
                _defenderMachines = value;
                OnPropertyChanged(nameof(DefenderMachines));
            }
        }

        private IEnumerable<SiegeEngineType> GetAllDefenderRangedMachines()
        {
            yield return DefaultSiegeEngineTypes.Ballista;
            yield return DefaultSiegeEngineTypes.FireBallista;
            yield return DefaultSiegeEngineTypes.Catapult;
            yield return DefaultSiegeEngineTypes.FireCatapult;
        }

        private IEnumerable<SiegeEngineType> GetAllAttackerRangedMachines()
        {
            yield return DefaultSiegeEngineTypes.Ballista;
            yield return DefaultSiegeEngineTypes.FireBallista;
            yield return DefaultSiegeEngineTypes.Onager;
            yield return DefaultSiegeEngineTypes.FireOnager;
            yield return DefaultSiegeEngineTypes.Trebuchet;
        }

        private IEnumerable<SiegeEngineType> GetAllAttackerMeleeMachines()
        {
            yield return DefaultSiegeEngineTypes.Ram;
            yield return DefaultSiegeEngineTypes.SiegeTower;
        }

        private static SiegeEngineType GetSiegeWeaponType(SiegeEngineType siegeWeaponType)
        {
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ladder)
                return DefaultSiegeEngineTypes.Ladder;
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ballista)
                return DefaultSiegeEngineTypes.Ballista;
            if (siegeWeaponType == DefaultSiegeEngineTypes.FireBallista)
                return DefaultSiegeEngineTypes.FireBallista;
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ram || siegeWeaponType == DefaultSiegeEngineTypes.ImprovedRam)
                return DefaultSiegeEngineTypes.Ram;
            if (siegeWeaponType == DefaultSiegeEngineTypes.SiegeTower)
                return DefaultSiegeEngineTypes.SiegeTower;
            if (siegeWeaponType == DefaultSiegeEngineTypes.Onager || siegeWeaponType == DefaultSiegeEngineTypes.Catapult)
                return DefaultSiegeEngineTypes.Onager;
            if (siegeWeaponType == DefaultSiegeEngineTypes.FireOnager || siegeWeaponType == DefaultSiegeEngineTypes.FireCatapult)
                return DefaultSiegeEngineTypes.FireOnager;
            return siegeWeaponType == DefaultSiegeEngineTypes.Trebuchet || siegeWeaponType == DefaultSiegeEngineTypes.Bricole ? DefaultSiegeEngineTypes.Trebuchet : siegeWeaponType;
        }

        public EnhancedBattleTestVM(EnhancedBattleTestState state, TextObject title)
        {
            _state = state;
            _config = BattleConfig.Deserialize();
            BattleConfig.Instance = _config;
            _scenes = _state.Scenes;

            TitleText = new TextVM(title);

            SwapTeamText = new TextVM(GameTexts.FindText("str_ebt_swap_team"));

            MapText = new TextVM(GameTexts.FindText("str_ebt_map"));


            StartButtonText = new TextVM(GameTexts.FindText("str_start"));

            PlayerSide = new SideVM(_config.PlayerTeamConfig, true,
                _config.BattleTypeConfig);
            EnemySide = new SideVM(_config.EnemyTeamConfig, false,
                _config.BattleTypeConfig);

            MapSelectionGroup = new MapSelectionGroupVM(_scenes);
            BattleTypeSelectionGroup = new BattleTypeSelectionGroup(_config.BattleTypeConfig, MapSelectionGroup, OnPlayerTypeChange);

            InitializeSiegeMachines();
            SetDefaultSiegeMachines();
            RecoverConfig();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            TitleText.RefreshValues();
            PlayerSide.RefreshValues();
            EnemySide.RefreshValues();
            BattleTypeSelectionGroup.RefreshValues();
            MapSelectionGroup.RefreshValues();
        }


        public void SetActiveState(bool isActive)
        {
        }

        public bool IsValid()
        {
            return PlayerSide.IsValid() && EnemySide.IsValid();
        }

        private void RecoverConfig()
        {
            MapSelectionGroup.SelectedMapId = _config.MapConfig.MapId;
            //if (MapSelectionGroup.SearchText.IsStringNoneOrEmpty())
            //{
            //    MapSelectionGroup.SearchText = new TextObject("{=7i1vmgQ9}Select a Map").ToString();
            //}
            MapSelectionGroup.SceneLevelSelection.SelectedIndex = _config.MapConfig.SceneLevel - 1;
            switch (_config.MapConfig.BreachedWallCount)
            {
                case 0:
                    MapSelectionGroup.WallHitpointSelection.SelectedIndex = 0;
                    break;
                case 1:
                    MapSelectionGroup.WallHitpointSelection.SelectedIndex = 1;
                    break;
                case 2:
                    MapSelectionGroup.WallHitpointSelection.SelectedIndex = 2;
                    break;
            }

            _config.MapConfig.DayOfYear = GetConfiguredDayOfYear();
            MapSelectionGroup.DayOfYear.Value = _config.MapConfig.DayOfYear;

            MapSelectionGroup.TimeOfDay.Value = _config.MapConfig.TimeOfDay;
            float fogDensity = _config.MapConfig.FogDensity;
            string weather = GetConfiguredWeather(
                _config.MapConfig.Weather,
                _config.MapConfig.RainDensity,
                ref fogDensity);
            MapSelectionGroup.SetWeather(weather);
            MapSelectionGroup.SetFogDensity(fogDensity);
        }

        private string GetConfiguredWeather(
            string weather,
            float rainDensity,
            ref float fogDensity)
        {
            switch (weather)
            {
                case "light_rain":
                case "heavy_rain":
                case "snowy":
                case "blizzard":
                    return weather;
                case "rain_light":
                    return "light_rain";
                case "rain":
                case "rain_heavy":
                    return "heavy_rain";
                case "snow_light":
                case "snow":
                    return "snowy";
                case "snow_heavy":
                    return "blizzard";
                case "fog_light":
                    fogDensity = 8f;
                    return "clear";
                case "fog_heavy":
                    fogDensity = 32f;
                    return "clear";
                case "rainstorm":
                    fogDensity = 16f;
                    return "heavy_rain";
            }

            if (rainDensity <= 0f)
                return "clear";

            bool isWinter =
                AtmosphereModel.GetSeasonIndex(_config.MapConfig.DayOfYear)
                == (int)CampaignTime.Seasons.Winter;
            if (isWinter)
                return rainDensity < 0.75f ? "snowy" : "blizzard";

            return rainDensity < 0.75f ? "light_rain" : "heavy_rain";
        }

        private int GetConfiguredDayOfYear()
        {
            if (_config.MapConfig.DayOfYear >= 1
                && _config.MapConfig.DayOfYear <= CampaignTime.DaysInYear)
                return _config.MapConfig.DayOfYear;

            switch (_config.MapConfig.Season)
            {
                case "summer":
                    return 32;
                case "fall":
                    return 53;
                case "winter":
                    return 74;
                case "spring":
                    return 11;
                default:
                    return Campaign.Current != null
                        ? CampaignTime.Now.GetDayOfYear + 1
                        : 1;
            }
        }

        public void ExecuteSwapTeam()
        {
            {
                var tmp = _config.PlayerTeamConfig;
                _config.PlayerTeamConfig = _config.EnemyTeamConfig;
                _config.EnemyTeamConfig = tmp;
            }
            BattleTypeSelectionGroup.SwapSide();
            {
                var tmp = PlayerSide;
                PlayerSide = EnemySide;
                EnemySide = tmp;
            }
            PlayerSide.IsPlayerSide = true;
            EnemySide.IsPlayerSide = false;
        }

        public void ExecuteBack()
        {
            ApplyConfig();
            _config.Serialize();
            _config = null;
            bool showCampaignStateWarning =
                EnhancedBattleTestSaveGuard.ConsumePostBattleWarning();
            Game.Current.GameStateManager.PopState();
            if (showCampaignStateWarning)
                EnhancedBattleTestSaveGuard.ShowCampaignStateWarning(false);
        }

        public void ExecuteStart()
        {
            if (!IsValid())
                return;
            if (!HasAvailableSergeantGeneral())
            {
                Utility.DisplayLocalizedText(
                    "str_ebt_sergeant_general_required");
                return;
            }
            if (!ApplyConfig())
                return;

            var sceneData = GetMap();
            if (sceneData == null)
                return;
            _config.Serialize();

            if (_hasShownCampaignStateWarning)
            {
                OpenMission(sceneData);
                return;
            }

            _hasShownCampaignStateWarning = true;
            EnhancedBattleTestSaveGuard.ShowCampaignStateWarning(
                true,
                () => OpenMission(sceneData));
        }

        private bool HasAvailableSergeantGeneral()
        {
            if (_config.BattleTypeConfig.PlayerType != PlayerType.Sergeant)
                return true;

            TeamConfig team = _config.PlayerTeamConfig;
            BasicCharacterObject playerCharacter =
                team.PlayerCharacter?.CharacterObject;
            IEnumerable<PartyConfig> parties =
                new[] { team.PrimaryParty };
            if (team.PrimaryParty.IsInArmy)
            {
                parties = parties.Concat(
                    team.AlliedParties.Where(party => party.IsInArmy));
            }

            return parties.Any(
                party => party?.HasGeneral == true
                         && party.Generals.Troops.Any(
                             troop =>
                                 troop?.Character?.CharacterObject != null
                                 && troop.Character.CharacterObject
                                 != playerCharacter));
        }

        private void OpenMission(SceneData sceneData)
        {
            GameTexts.SetVariable("MapName", sceneData.Name);
            Utility.DisplayLocalizedText("str_ebt_current_map");
            EnhancedBattleTestMissions.OpenMission(
                _config,
                sceneData.SceneID,
                sceneData.Terrain);
        }

        private bool ApplyConfig()
        {
            if (MapSelectionGroup.SelectedMap != null)
            {
                _config.MapConfig.MapId = MapSelectionGroup.SelectedMapId;
            }
            if (MapSelectionGroup.SceneLevelSelection.SelectedItem != null)
                _config.MapConfig.SceneLevel = MapSelectionGroup.SceneLevelSelection.SelectedItem.Level;
            if (MapSelectionGroup.WallHitpointSelection.SelectedItem != null)
                _config.MapConfig.BreachedWallCount = MapSelectionGroup.WallHitpointSelection.SelectedItem.BreachedWallCount;
            _config.MapConfig.DayOfYear = MapSelectionGroup.SelectedDayOfYear;
            _config.MapConfig.Season = string.Empty;
            _config.MapConfig.TimeOfDay = MapSelectionGroup.SelectedTimeOfDay;
            _config.MapConfig.Weather = MapSelectionGroup.SelectedWeatherId;
            _config.MapConfig.RainDensity = 0f;
            _config.MapConfig.FogDensity =
                MapSelectionGroup.SelectedFogDensity;

            _config.SiegeMachineConfig.AttackerMeleeMachines =
                AttackerMeleeMachines.Select(vm => vm.MachineID).ToList();
            _config.SiegeMachineConfig.AttackerRangedMachines =
                AttackerRangedMachines.Select(vm => vm.MachineID).ToList();
            _config.SiegeMachineConfig.DefenderMachines =
                DefenderMachines.Select(vm => vm.MachineID).ToList();
            return true;
        }

        private SceneData GetMap()
        {
            var selectedMap = MapSelectionGroup.SelectedMap;
            if (selectedMap == null)
            {
                MapSelectionGroup.MapSelection.ExecuteRandomize();
                selectedMap = MapSelectionGroup.SelectedMap;
                if (selectedMap == null)
                {
                    Utility.DisplayLocalizedText("str_ebt_no_map");
                    return null;
                }

            }
            return _scenes.First(data => data.Name.ToString() == selectedMap.MapName);
        }

        private void OnPlayerTypeChange(bool isCommander)
        {
            PlayerSide.SetPlayerType(
                isCommander ? PlayerType.Commander : PlayerType.Sergeant);
        }

        private void InitializeSiegeMachines()
        {
            AttackerMeleeMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < MAX_ATTACKER_MELEE_MACHINE_COUNT; ++index)
                AttackerMeleeMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(_config.SiegeMachineConfig.AttackerMeleeMachines.ElementAtOrDefault(index)),
                    OnMeleeMachineSelection, OnResetSelection));
            AttackerRangedMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < MAX_ATTACKER_RANGED_MACHINE_COUNT; ++index)
                AttackerRangedMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(_config.SiegeMachineConfig.AttackerRangedMachines.ElementAtOrDefault(index)),
                    OnAttackerRangedMachineSelection, OnResetSelection));
            DefenderMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < MAX_DEFENDER_MACHINE_COUNT; ++index)
                DefenderMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(_config.SiegeMachineConfig.DefenderMachines.ElementAtOrDefault(index)),
                    OnDefenderRangedMachineSelection, OnResetSelection));
        }
        private void SetDefaultSiegeMachines()
        {
            this.AttackerMeleeMachines[0].SetMachineType(DefaultSiegeEngineTypes.SiegeTower);
            this.AttackerMeleeMachines[1].SetMachineType(DefaultSiegeEngineTypes.Ram);
            this.AttackerMeleeMachines[2].SetMachineType(DefaultSiegeEngineTypes.SiegeTower);
            this.AttackerRangedMachines[0].SetMachineType(DefaultSiegeEngineTypes.Trebuchet);
            this.AttackerRangedMachines[1].SetMachineType(DefaultSiegeEngineTypes.Onager);
            this.AttackerRangedMachines[2].SetMachineType(DefaultSiegeEngineTypes.Onager);
            this.AttackerRangedMachines[3].SetMachineType(DefaultSiegeEngineTypes.FireBallista);
            this.DefenderMachines[0].SetMachineType(DefaultSiegeEngineTypes.FireCatapult);
            this.DefenderMachines[1].SetMachineType(DefaultSiegeEngineTypes.FireCatapult);
            this.DefenderMachines[2].SetMachineType(DefaultSiegeEngineTypes.Catapult);
            this.DefenderMachines[3].SetMachineType(DefaultSiegeEngineTypes.FireBallista);
        }

        private void OnResetSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            selectedSlot.SetMachineType((SiegeEngineType)null);
        }

        private void ExecuteDoneDefenderCustomMachineSelection()
        {
            IsDefenderCustomMachineSelectionEnabled = false;
        }

        private void ExecuteDoneAttackerCustomMachineSelection()
        {
            IsAttackerCustomMachineSelectionEnabled = false;
        }

        [DataSourceProperty]
        public bool IsAttackerCustomMachineSelectionEnabled
        {
            get => _isAttackerCustomMachineSelectionEnabled;
            set
            {
                if (value == _isAttackerCustomMachineSelectionEnabled)
                    return;
                _isAttackerCustomMachineSelectionEnabled = value;
                OnPropertyChanged(nameof(IsAttackerCustomMachineSelectionEnabled));
            }
        }

        [DataSourceProperty]
        public bool IsDefenderCustomMachineSelectionEnabled
        {
            get => _isDefenderCustomMachineSelectionEnabled;
            set
            {
                if (value == _isDefenderCustomMachineSelectionEnabled)
                    return;
                _isDefenderCustomMachineSelectionEnabled = value;
                OnPropertyChanged(nameof(IsDefenderCustomMachineSelectionEnabled));
            }
        }

        private void OnMeleeMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>
            {
                new InquiryElement(null, GameTexts.FindText("str_empty").ToString(), null)
            };
            foreach (SiegeEngineType attackerMeleeMachine in GetAllAttackerMeleeMachines())
                inquiryElements.Add(new InquiryElement(attackerMeleeMachine, attackerMeleeMachine.Name.ToString(), null));
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=MVOWsP48}Select a Melee Machine").ToString(), string.Empty, inquiryElements, false, 1, 1, GameTexts.FindText("str_done").ToString(), "", selectedElements => selectedSlot.SetMachineType(selectedElements.First().Identifier as SiegeEngineType), null));
        }

        private void OnAttackerRangedMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>
            {
                new InquiryElement(null, GameTexts.FindText("str_empty").ToString(), null)
            };
            foreach (SiegeEngineType attackerRangedMachine in GetAllAttackerRangedMachines())
                inquiryElements.Add(new InquiryElement(attackerRangedMachine, attackerRangedMachine.Name.ToString(), null));
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=SLZzfNPr}Select a Ranged Machine").ToString(), string.Empty, inquiryElements, false, 1, 1, GameTexts.FindText("str_done").ToString(), "", selectedElements => selectedSlot.SetMachineType(selectedElements[0].Identifier as SiegeEngineType), null));
        }

        private void OnDefenderRangedMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>
            {
                new InquiryElement(null, GameTexts.FindText("str_empty").ToString(), null)
            };
            foreach (SiegeEngineType defenderRangedMachine in GetAllDefenderRangedMachines())
                inquiryElements.Add(new InquiryElement(defenderRangedMachine, defenderRangedMachine.Name.ToString(), null));
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=SLZzfNPr}Select a Ranged Machine").ToString(), string.Empty, inquiryElements, false, 1, 1, GameTexts.FindText("str_done").ToString(), "", selectedElements => selectedSlot.SetMachineType(selectedElements[0].Identifier as SiegeEngineType), null));
        }
    }
}
