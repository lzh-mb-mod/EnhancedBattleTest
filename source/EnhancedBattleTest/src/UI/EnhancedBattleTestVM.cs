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
        private bool _hasConfirmedCampaignStateWarning;

        private MBBindingList<CustomBattleSiegeMachineVM> _attackerMeleeMachines;
        private MBBindingList<CustomBattleSiegeMachineVM> _attackerRangedMachines;
        private MBBindingList<CustomBattleSiegeMachineVM> _defenderMachines;

        private bool _isAttackerCustomMachineSelectionEnabled;
        private bool _isDefenderCustomMachineSelectionEnabled;
        private SideVM _playerSide;
        private SideVM _enemySide;
        private BattleTypeSelectionGroup _battleTypeSelectionGroup;

        public TextVM TitleText { get; }

        public TextVM SwapTeamText { get; }

        public TextVM MapText { get; }

        public TextVM StartButtonText { get; }
        public TextVM SaveConfigurationText { get; }
        public TextVM LoadConfigurationText { get; }

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
        [DataSourceProperty]
        public BattleTypeSelectionGroup BattleTypeSelectionGroup
        {
            get => _battleTypeSelectionGroup;
            private set
            {
                if (_battleTypeSelectionGroup == value)
                    return;
                _battleTypeSelectionGroup = value;
                OnPropertyChanged(nameof(BattleTypeSelectionGroup));
            }
        }
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
            _config = BattleConfig.Deserialize(
                out bool recoveredConfiguration);
            BattleConfig.Instance = _config;
            _scenes = _state.Scenes;

            TitleText = new TextVM(title);

            SwapTeamText = new TextVM(GameTexts.FindText("str_ebt_swap_team"));

            MapText = new TextVM(GameTexts.FindText("str_ebt_map"));


            StartButtonText = new TextVM(GameTexts.FindText("str_start"));
            SaveConfigurationText =
                new TextVM(GameTexts.FindText("str_ebt_save_configuration"));
            LoadConfigurationText =
                new TextVM(GameTexts.FindText("str_ebt_load_configuration"));

            PlayerSide = CreateSide(
                _config,
                _config.PlayerTeamConfig,
                true);
            EnemySide = CreateSide(
                _config,
                _config.EnemyTeamConfig,
                false);

            MapSelectionGroup = new MapSelectionGroupVM(_scenes);
            BattleTypeSelectionGroup = new BattleTypeSelectionGroup(_config.BattleTypeConfig, MapSelectionGroup, OnPlayerTypeChange);

            InitializeSiegeMachines(_config);
            SetDefaultSiegeMachines();
            RecoverConfig(_config, MapSelectionGroup);
            if (recoveredConfiguration)
            {
                Utility.DisplayLocalizedText(
                    "str_ebt_current_configuration_corrupt");
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            TitleText.RefreshValues();
            PlayerSide.RefreshValues();
            EnemySide.RefreshValues();
            BattleTypeSelectionGroup.RefreshValues();
            MapSelectionGroup.RefreshValues();
            SaveConfigurationText.RefreshValues();
            LoadConfigurationText.RefreshValues();
        }


        public void SetActiveState(bool isActive)
        {
        }

        public bool IsValid()
        {
            return PlayerSide.IsValid() && EnemySide.IsValid();
        }

        private static void RecoverConfig(
            BattleConfig config,
            MapSelectionGroupVM mapSelectionGroup)
        {
            mapSelectionGroup.SelectedMapId = config.MapConfig.MapId;
            //if (MapSelectionGroup.SearchText.IsStringNoneOrEmpty())
            //{
            //    MapSelectionGroup.SearchText = new TextObject("{=7i1vmgQ9}Select a Map").ToString();
            //}
            mapSelectionGroup.SceneLevelSelection.SelectedIndex = Math.Max(
                Math.Min(
                    config.MapConfig.SceneLevel - 1,
                    mapSelectionGroup.SceneLevelSelection.ItemList.Count - 1),
                0);
            switch (config.MapConfig.BreachedWallCount)
            {
                case 0:
                    mapSelectionGroup.WallHitpointSelection.SelectedIndex = 0;
                    break;
                case 1:
                    mapSelectionGroup.WallHitpointSelection.SelectedIndex = 1;
                    break;
                case 2:
                    mapSelectionGroup.WallHitpointSelection.SelectedIndex = 2;
                    break;
            }

            config.MapConfig.DayOfYear = GetConfiguredDayOfYear(config);
            mapSelectionGroup.DayOfYear.Value = config.MapConfig.DayOfYear;

            mapSelectionGroup.TimeOfDay.Value = config.MapConfig.TimeOfDay;
            float fogDensity = config.MapConfig.FogDensity;
            string weather = GetConfiguredWeather(
                config,
                config.MapConfig.Weather,
                config.MapConfig.RainDensity,
                ref fogDensity);
            mapSelectionGroup.SetWeather(weather);
            mapSelectionGroup.ImproveExposure =
                config.MapConfig.ImproveExposure;
            mapSelectionGroup.CanUseLowAltitudeAtmosphere =
                config.MapConfig.CanUseLowAltitudeAtmosphere
                || config.MapConfig.Weather == "low_altitude";
            mapSelectionGroup.SetFogDensity(fogDensity);
        }

        private static string GetConfiguredWeather(
            BattleConfig config,
            string weather,
            float rainDensity,
            ref float fogDensity)
        {
            switch (weather)
            {
                case "overcast":
                case "light_rain":
                case "heavy_rain":
                case "rain_storm":
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
                    return "rain_storm";
                case "low_altitude":
                    return "clear";
            }

            if (rainDensity <= 0f)
                return "clear";

            bool isWinter =
                AtmosphereModel.GetSeasonIndex(config.MapConfig.DayOfYear)
                == (int)CampaignTime.Seasons.Winter;
            if (isWinter)
                return rainDensity < 0.75f ? "snowy" : "blizzard";

            return rainDensity < 0.75f ? "light_rain" : "heavy_rain";
        }

        private static int GetConfiguredDayOfYear(BattleConfig config)
        {
            if (config.MapConfig.DayOfYear >= 1
                && config.MapConfig.DayOfYear <= CampaignTime.DaysInYear)
                return config.MapConfig.DayOfYear;

            switch (config.MapConfig.Season)
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
            _config = null;
            bool showCampaignStateWarning =
                EnhancedBattleTestSaveGuard.ConsumePostBattleWarning();
            Game.Current.GameStateManager.PopState();
            if (showCampaignStateWarning)
                EnhancedBattleTestSaveGuard.ShowCampaignStateWarning(false);
        }

        public void ExecuteSaveConfiguration()
        {
            if (!ApplyConfig())
                return;

            if (!BattleConfig.TryGetSavedConfigurationNames(
                    out IReadOnlyList<string> names))
            {
                Utility.DisplayLocalizedText(
                    "str_ebt_configuration_list_failed");
                return;
            }
            if (names.Count == 0)
            {
                ShowNewConfigurationInquiry(names);
                return;
            }

            List<InquiryElement> configurations = new List<InquiryElement>
            {
                new InquiryElement(
                    null,
                    GameTexts.FindText(
                        "str_ebt_save_as_new_configuration").ToString(),
                    null)
            };
            configurations.AddRange(
                names.Select(name => new InquiryElement(name, name, null)));
            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    GameTexts.FindText(
                        "str_ebt_save_configuration").ToString(),
                    GameTexts.FindText(
                        "str_ebt_save_configuration_destination_description")
                        .ToString(),
                    configurations,
                    true,
                    1,
                    1,
                    GameTexts.FindText("str_done").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    selected =>
                    {
                        string name = selected.FirstOrDefault()?.Identifier
                            as string;
                        if (name == null)
                            ShowNewConfigurationInquiry(names);
                        else
                            ShowOverwriteConfigurationInquiry(name);
                    },
                    null));
        }

        private void ShowNewConfigurationInquiry(
            IReadOnlyList<string> existingNames)
        {
            InformationManager.ShowTextInquiry(
                new TextInquiryData(
                    GameTexts.FindText(
                        "str_ebt_save_configuration").ToString(),
                    GameTexts.FindText(
                        "str_ebt_save_configuration_description").ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_done").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    name =>
                    {
                        string trimmedName = name.Trim();
                        SaveConfiguration(trimmedName);
                    },
                    null,
                    false,
                    name =>
                    {
                        Tuple<bool, string> validation =
                            BattleConfig.ValidateConfigurationName(name);
                        if (!validation.Item1)
                            return validation;

                        string trimmedName = name.Trim();
                        if (existingNames.Any(existingName =>
                                string.Equals(
                                    existingName,
                                    trimmedName,
                                    StringComparison.OrdinalIgnoreCase)))
                        {
                            return Tuple.Create(
                                false,
                                GameTexts.FindText(
                                    "str_ebt_configuration_name_exists")
                                .ToString());
                        }

                        return validation;
                    }));
        }

        private void ShowOverwriteConfigurationInquiry(string name)
        {
            TextObject description = GameTexts.FindText(
                "str_ebt_overwrite_configuration_description");
            description.SetTextVariable("CONFIGURATION_NAME", name);
            InformationManager.ShowInquiry(
                new InquiryData(
                    GameTexts.FindText(
                        "str_ebt_overwrite_configuration").ToString(),
                    description.ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_continue").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    () => SaveConfiguration(name),
                    null),
                false,
                false);
        }

        private void SaveConfiguration(string name)
        {
            if (!_config.Serialize(name))
            {
                Utility.DisplayLocalizedText(
                    "str_ebt_configuration_save_failed");
                return;
            }
            if (!_config.Serialize())
            {
                Utility.DisplayLocalizedText(
                    "str_ebt_current_configuration_save_failed");
                return;
            }

            TextObject message = GameTexts.FindText(
                "str_ebt_configuration_saved");
            message.SetTextVariable("CONFIGURATION_NAME", name);
            Utility.DisplayMessage(message.ToString());
        }

        public void ExecuteLoadConfiguration()
        {
            if (!BattleConfig.TryGetSavedConfigurationNames(
                    out IReadOnlyList<string> names))
            {
                Utility.DisplayLocalizedText(
                    "str_ebt_configuration_list_failed");
                return;
            }
            if (names.Count == 0)
            {
                Utility.DisplayLocalizedText(
                    "str_ebt_no_saved_configuration");
                return;
            }

            List<InquiryElement> configurations = names
                .Select(name => new InquiryElement(name, name, null))
                .ToList();
            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    GameTexts.FindText(
                        "str_ebt_load_configuration").ToString(),
                    GameTexts.FindText(
                        "str_ebt_load_configuration_description").ToString(),
                    configurations,
                    true,
                    1,
                    1,
                    GameTexts.FindText("str_done").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    selected =>
                    {
                        string name = selected.FirstOrDefault()?.Identifier
                            as string;
                        if (string.IsNullOrEmpty(name)
                            || !BattleConfig.TryDeserialize(
                                name,
                                out BattleConfig loadedConfig))
                        {
                            Utility.DisplayLocalizedText(
                                "str_ebt_configuration_load_failed");
                            return;
                        }

                        if (!TryLoadConfiguration(loadedConfig))
                        {
                            Utility.DisplayLocalizedText(
                                "str_ebt_configuration_load_failed");
                            return;
                        }
                        TextObject message = GameTexts.FindText(
                            "str_ebt_configuration_loaded");
                        message.SetTextVariable(
                            "CONFIGURATION_NAME",
                            name);
                        Utility.DisplayMessage(message.ToString());
                    },
                    null));
        }

        public void ExecuteStart()
        {
            if (!IsValid())
                return;
            if (_config.BattleTypeConfig.BattleType == BattleType.Siege
                && _config.BattleTypeConfig.PlayerType == PlayerType.None)
            {
                Utility.DisplayLocalizedText(
                    "str_ebt_siege_player_character_required");
                return;
            }
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

            if (_hasConfirmedCampaignStateWarning)
            {
                OpenMission(sceneData);
                return;
            }
            EnhancedBattleTestSaveGuard.ShowCampaignStateWarning(
                true,
                () =>
                {
                    _hasConfirmedCampaignStateWarning = true;
                    OpenMission(sceneData);
                });
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
                party => party?.HasHeroes == true
                         && party.Heroes.Troops.Any(
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
            _config.MapConfig.ImproveExposure =
                MapSelectionGroup.ImproveExposure;
            _config.MapConfig.CanUseLowAltitudeAtmosphere =
                MapSelectionGroup.CanUseLowAltitudeAtmosphere;

            _config.SiegeMachineConfig.AttackerMeleeMachines =
                AttackerMeleeMachines.Select(vm => vm.MachineID).ToList();
            _config.SiegeMachineConfig.AttackerRangedMachines =
                AttackerRangedMachines.Select(vm => vm.MachineID).ToList();
            _config.SiegeMachineConfig.DefenderMachines =
                DefenderMachines.Select(vm => vm.MachineID).ToList();
            _config.NormalizeCharacterGroups();
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

        private static SideVM CreateSide(
            BattleConfig config,
            TeamConfig team,
            bool isPlayerSide)
        {
            return new SideVM(
                team,
                isPlayerSide,
                config.BattleTypeConfig,
                () => GetHeroPlayerCharacters(config),
                () => GetPartyHeroes(config));
        }

        private static IEnumerable<BasicCharacterObject>
            GetHeroPlayerCharacters(BattleConfig config)
        {
            if (config.BattleTypeConfig.PlayerType == PlayerType.None)
                return Enumerable.Empty<BasicCharacterObject>();

            return new[]
                {
                    config.PlayerTeamConfig?.PlayerCharacter?.CharacterObject,
                    config.EnemyTeamConfig?.PlayerCharacter?.CharacterObject
                }
                .Where(character =>
                    character is CharacterObject characterObject
                    && characterObject.IsHero)
                .Distinct();
        }

        private static IEnumerable<BasicCharacterObject> GetPartyHeroes(
            BattleConfig config)
        {
            return GetParties(config.PlayerTeamConfig)
                .Concat(GetParties(config.EnemyTeamConfig))
                .SelectMany(party => party.Heroes.Troops)
                .Select(troop => troop?.Character?.CharacterObject)
                .Where(character =>
                    character is CharacterObject characterObject
                    && characterObject.IsHero)
                .Distinct();
        }

        private static IEnumerable<PartyConfig> GetParties(TeamConfig team)
        {
            if (team?.PrimaryParty != null)
                yield return team.PrimaryParty;
            if (team?.AlliedParties == null)
                yield break;
            foreach (PartyConfig party in team.AlliedParties)
            {
                if (party != null)
                    yield return party;
            }
        }

        private void OnPlayerTypeChange(PlayerType playerType)
        {
            PlayerSide.SetPlayerType(playerType);
        }

        private bool TryLoadConfiguration(BattleConfig config)
        {
            try
            {
                SideVM playerSide = CreateSide(
                    config,
                    config.PlayerTeamConfig,
                    true);
                SideVM enemySide = CreateSide(
                    config,
                    config.EnemyTeamConfig,
                    false);
                var battleTypeSelectionGroup =
                    new BattleTypeSelectionGroup(
                        config.BattleTypeConfig,
                        MapSelectionGroup,
                        OnPlayerTypeChange);
                CreateSiegeMachines(
                    config,
                    out MBBindingList<CustomBattleSiegeMachineVM>
                        attackerMeleeMachines,
                    out MBBindingList<CustomBattleSiegeMachineVM>
                        attackerRangedMachines,
                    out MBBindingList<CustomBattleSiegeMachineVM>
                        defenderMachines);
                var validationMapSelectionGroup =
                    new MapSelectionGroupVM(_scenes);
                validationMapSelectionGroup.OnGameTypeChange(
                    config.BattleTypeConfig.BattleType);
                RecoverConfig(config, validationMapSelectionGroup);

                _config = config;
                BattleConfig.Instance = config;
                PlayerSide = playerSide;
                EnemySide = enemySide;
                BattleTypeSelectionGroup = battleTypeSelectionGroup;
                AttackerMeleeMachines = attackerMeleeMachines;
                AttackerRangedMachines = attackerRangedMachines;
                DefenderMachines = defenderMachines;
                MapSelectionGroup.OnGameTypeChange(
                    config.BattleTypeConfig.BattleType);
                RecoverConfig(config, MapSelectionGroup);
                playerSide.SetPlayerType(config.BattleTypeConfig.PlayerType);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private void InitializeSiegeMachines(BattleConfig config)
        {
            CreateSiegeMachines(
                config,
                out MBBindingList<CustomBattleSiegeMachineVM>
                    attackerMeleeMachines,
                out MBBindingList<CustomBattleSiegeMachineVM>
                    attackerRangedMachines,
                out MBBindingList<CustomBattleSiegeMachineVM>
                    defenderMachines);
            AttackerMeleeMachines = attackerMeleeMachines;
            AttackerRangedMachines = attackerRangedMachines;
            DefenderMachines = defenderMachines;
        }

        private void CreateSiegeMachines(
            BattleConfig config,
            out MBBindingList<CustomBattleSiegeMachineVM>
                attackerMeleeMachines,
            out MBBindingList<CustomBattleSiegeMachineVM>
                attackerRangedMachines,
            out MBBindingList<CustomBattleSiegeMachineVM> defenderMachines)
        {
            attackerMeleeMachines =
                new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < MAX_ATTACKER_MELEE_MACHINE_COUNT; ++index)
                attackerMeleeMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(config.SiegeMachineConfig.AttackerMeleeMachines.ElementAtOrDefault(index)),
                    OnMeleeMachineSelection, OnResetSelection));
            attackerRangedMachines =
                new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < MAX_ATTACKER_RANGED_MACHINE_COUNT; ++index)
                attackerRangedMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(config.SiegeMachineConfig.AttackerRangedMachines.ElementAtOrDefault(index)),
                    OnAttackerRangedMachineSelection, OnResetSelection));
            defenderMachines =
                new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < MAX_DEFENDER_MACHINE_COUNT; ++index)
                defenderMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(config.SiegeMachineConfig.DefenderMachines.ElementAtOrDefault(index)),
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
