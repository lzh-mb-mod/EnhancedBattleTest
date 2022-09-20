using System.Collections.Generic;
using System.Linq;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.Data.MissionData;
using EnhancedBattleTest.GameMode;
using EnhancedBattleTest.SinglePlayer;
using EnhancedBattleTest.UI.Basic;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle.SelectionItem;
using Campaign = TaleWorlds.CampaignSystem.Campaign;

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

        public TextVM SeasonText { get; }

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
            _config = BattleConfig.Deserialize(EnhancedBattleTestSubModule.IsMultiplayer);
            BattleConfig.Instance = _config;
            _scenes = _state.Scenes;

            TitleText = new TextVM(title);

            SwapTeamText = new TextVM(GameTexts.FindText("str_ebt_swap_team"));

            MapText = new TextVM(GameTexts.FindText("str_ebt_map"));

            SeasonText = new TextVM(GameTexts.FindText("str_ebt_season"));

            StartButtonText = new TextVM(GameTexts.FindText("str_start"));

            PlayerSide = new SideVM(_config.PlayerSideConfig, true,
                _config.BattleTypeConfig);
            EnemySide = new SideVM(_config.EnemySideConfig, false,
                _config.BattleTypeConfig);

            MapSelectionGroup = new MapSelectionGroupVM(_scenes);
            BattleTypeSelectionGroup = new BattleTypeSelectionGroup(_config.BattleTypeConfig, MapSelectionGroup, OnPlayerTypeChange);

            RecoverConfig();
            InitializeSiegeMachines();
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
            MapSelectionGroup.OverridesPlayerPosition = _config.MapConfig.OverridesPlayerPosition;
            MapSelectionGroup.IsSallyOutSelected = _config.MapConfig.IsSallyOutSelected;
            if (_config.MapConfig.OverridesPlayerPosition || Campaign.Current == null || MobileParty.MainParty == null)
            {
                MapSelectionGroup.MapSelection.SelectedIndex = MapSelectionGroup.MapSelection.ItemList.FindIndex(vm => vm.MapId == _config.MapConfig.SelectedMapId);
            }
            else
            {
                MapPatchData mapPatchAtPosition = Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position2D);
                MapSelectionGroup.MapSelection.SelectedIndex = MapSelectionGroup.MapSelection.ItemList.FindIndex(vm =>
                    vm.MapId == PlayerEncounter.GetBattleSceneForMapPatch(mapPatchAtPosition));
            }
            if (MapSelectionGroup.SelectedMap == null)
            {
                MapSelectionGroup.MapSelection.SelectedIndex = 0;
            }
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

            switch (_config.MapConfig.Season)
            {
                case "summer":
                    MapSelectionGroup.SeasonSelection.SelectedIndex = 0;
                    break;
                case "fall":
                    MapSelectionGroup.SeasonSelection.SelectedIndex = 1;
                    break;
                case "winter":
                    MapSelectionGroup.SeasonSelection.SelectedIndex = 2;
                    break;
                case "spring":
                    MapSelectionGroup.SeasonSelection.SelectedIndex = 3;
                    break;
            }

            MapSelectionGroup.TimeOfDaySelection.SelectedIndex = MapSelectionGroup.TimeOfDaySelection.ItemList.IndexOf(
                MapSelectionGroup.TimeOfDaySelection.ItemList.FirstOrDefault(vm =>
                    vm.TimeOfDay == _config.MapConfig.TimeOfDay) ??
                MapSelectionGroup.TimeOfDaySelection.ItemList.FirstOrDefault());

            MapSelectionGroup.RainDensitySelection.SelectedIndex = MapSelectionGroup.RainDensitySelection.ItemList.IndexOf(
                MapSelectionGroup.RainDensitySelection.ItemList.FirstOrDefault(vm =>
                    vm.RainDensity == _config.MapConfig.RainDensity) ??
                MapSelectionGroup.RainDensitySelection.ItemList.FirstOrDefault());

            MapSelectionGroup.FogDensitySelection.SelectedIndex = MapSelectionGroup.FogDensitySelection.ItemList.IndexOf(
                MapSelectionGroup.FogDensitySelection.ItemList.FirstOrDefault(vm =>
                    vm.FogDensity == _config.MapConfig.FogDensity) ??
                MapSelectionGroup.FogDensitySelection.ItemList.FirstOrDefault());
        }

        public void ExecuteSwapTeam()
        {
            (_config.PlayerSideConfig, _config.EnemySideConfig) = (_config.EnemySideConfig, _config.PlayerSideConfig);
            BattleTypeSelectionGroup.SwapSide();
            (PlayerSide, EnemySide) = (EnemySide, PlayerSide);
            PlayerSide.IsPlayerSide = true;
            EnemySide.IsPlayerSide = false;
        }

        public void ExecuteBack()
        {
            ApplyConfig();
            _config.Serialize(EnhancedBattleTestSubModule.IsMultiplayer);
            _config = null;
            GameStateManager.Current.PopState();
        }

        public void ExecuteStart()
        {
            if (!IsValid())
                return;
            if (!ApplyConfig())
                return;

            var mapItem = GetMap();
            if (mapItem == null)
                return;
            _config.Serialize(EnhancedBattleTestSubModule.IsMultiplayer);            
            EnhancedBattleTestPartyController.BattleConfig = _config;
            //EnhancedBattleTestMissions.OpenMission(_config, mapItem.MapId);
            BattleStarter.Start(_config, mapItem.MapId);
        }

        private bool ApplyConfig()
        {
            _config.MapConfig.SelectedMapId = MapSelectionGroup.SelectedMap.MapId;
            _config.MapConfig.OverridesPlayerPosition = MapSelectionGroup.OverridesPlayerPosition;
            _config.MapConfig.IsSallyOutSelected = MapSelectionGroup.IsSallyOutSelected;
            if (MapSelectionGroup.SceneLevelSelection.SelectedItem != null)
                _config.MapConfig.SceneLevel = MapSelectionGroup.SceneLevelSelection.SelectedItem.Level;
            if (MapSelectionGroup.WallHitpointSelection.SelectedItem != null)
                _config.MapConfig.BreachedWallCount = MapSelectionGroup.WallHitpointSelection.SelectedItem.BreachedWallCount;
            if (MapSelectionGroup.SeasonSelection.SelectedItem != null)
                _config.MapConfig.Season = MapSelectionGroup.SelectedSeasonId;
            if (MapSelectionGroup.TimeOfDaySelection.SelectedItem != null)
                _config.MapConfig.TimeOfDay = MapSelectionGroup.SelectedTimeOfDay;
            if (MapSelectionGroup.RainDensitySelection.SelectedItem != null)
                _config.MapConfig.RainDensity = MapSelectionGroup.SelectedRainDensity;
            if (MapSelectionGroup.FogDensitySelection.SelectedItem != null)
                _config.MapConfig.FogDensity = MapSelectionGroup.SelectedFogDensity;

            _config.SiegeMachineConfig.AttackerMeleeMachines =
                AttackerMeleeMachines.Select(vm => vm.MachineID).ToList();
            _config.SiegeMachineConfig.AttackerRangedMachines =
                AttackerRangedMachines.Select(vm => vm.MachineID).ToList();
            _config.SiegeMachineConfig.DefenderMachines =
                DefenderMachines.Select(vm => vm.MachineID).ToList();
            if (_config.BattleTypeConfig.BattleType == BattleType.Siege &&
                !_config.PlayerSideConfig.Teams[0].HasGeneral || _config.PlayerSideConfig.Teams[0].Generals.Troops.Count == 0)
            {
                Utility.DisplayLocalizedText("str_ebt_siege_no_player");
                return false;
            }

            return true;
        }

        private MapItemVM GetMap()
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

            return selectedMap;
        }

        private void OnPlayerTypeChange(bool isCommander)
        {

        }

        private void InitializeSiegeMachines()
        {
            AttackerMeleeMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < MAX_ATTACKER_MELEE_MACHINE_COUNT; ++index)
                AttackerMeleeMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(_config.SiegeMachineConfig.AttackerMeleeMachines.ElementAtOrDefault(index)),
                    OnMeleeMachineSelection, OnResetMachineSelection));
            AttackerRangedMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < MAX_ATTACKER_RANGED_MACHINE_COUNT; ++index)
                AttackerRangedMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(_config.SiegeMachineConfig.AttackerRangedMachines.ElementAtOrDefault(index)),
                    OnAttackerRangedMachineSelection, OnResetMachineSelection));
            DefenderMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < MAX_DEFENDER_MACHINE_COUNT; ++index)
                DefenderMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(_config.SiegeMachineConfig.DefenderMachines.ElementAtOrDefault(index)),
                    OnDefenderRangedMachineSelection, OnResetMachineSelection));
            SetDefaultSiegeMachines();
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
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=MVOWsP48}Select a Melee Machine").ToString(), string.Empty, inquiryElements, false, 1, GameTexts.FindText("str_done").ToString(), "", selectedElements => selectedSlot.SetMachineType(selectedElements.First().Identifier as SiegeEngineType), null));
        }

        private void OnAttackerRangedMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>
            {
                new InquiryElement(null, GameTexts.FindText("str_empty").ToString(), null)
            };
            foreach (SiegeEngineType attackerRangedMachine in GetAllAttackerRangedMachines())
                inquiryElements.Add(new InquiryElement(attackerRangedMachine, attackerRangedMachine.Name.ToString(), null));
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=SLZzfNPr}Select a Ranged Machine").ToString(), string.Empty, inquiryElements, false, 1, GameTexts.FindText("str_done").ToString(), "", selectedElements => selectedSlot.SetMachineType(selectedElements[0].Identifier as SiegeEngineType), null));
        }

        private void OnDefenderRangedMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>
            {
                new InquiryElement(null, GameTexts.FindText("str_empty").ToString(), null)
            };
            foreach (SiegeEngineType defenderRangedMachine in GetAllDefenderRangedMachines())
                inquiryElements.Add(new InquiryElement(defenderRangedMachine, defenderRangedMachine.Name.ToString(), null));
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=SLZzfNPr}Select a Ranged Machine").ToString(), string.Empty, inquiryElements, false, 1, GameTexts.FindText("str_done").ToString(), "", selectedElements => selectedSlot.SetMachineType(selectedElements[0].Identifier as SiegeEngineType), null));
        }
        private void OnResetMachineSelection(CustomBattleSiegeMachineVM selectedSlot) => selectedSlot.SetMachineType((SiegeEngineType)null);
    }
}
