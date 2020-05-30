using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.ObjectSystem;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestVM : ViewModel
    {
        private readonly EnhancedBattleTestState _state;
        private BattleConfig _config;
        private readonly List<SceneData> _scenes;

        private MBBindingList<CustomBattleSiegeMachineVM> _attackerMeleeMachines;
        private MBBindingList<CustomBattleSiegeMachineVM> _attackerRangedMachines;
        private MBBindingList<CustomBattleSiegeMachineVM> _defenderMachines;

        private bool _isAttackerCustomMachineSelectionEnabled;
        private bool _isDefenderCustomMachineSelectionEnabled;

        public TextVM TitleText { get; }

        public TextVM MapText { get; }

        public TextVM SeasonText { get; }

        public TextVM StartButtonText { get; }

        public SideVM PlayerSide { get; }
        public SideVM EnemySide { get; }
        public MapSelectionGroup MapSelectionGroup { get; }
        public BattleTypeSelectionGroup BattleTypeSelectionGroup { get; }

        [DataSourceProperty]
        public MBBindingList<CustomBattleSiegeMachineVM> AttackerMeleeMachines
        {
            get => this._attackerMeleeMachines;
            set
            {
                if (value == this._attackerMeleeMachines)
                    return;
                this._attackerMeleeMachines = value;
                this.OnPropertyChanged(nameof(AttackerMeleeMachines));
            }
        }

        [DataSourceProperty]
        public MBBindingList<CustomBattleSiegeMachineVM> AttackerRangedMachines
        {
            get => this._attackerRangedMachines;
            set
            {
                if (value == this._attackerRangedMachines)
                    return;
                this._attackerRangedMachines = value;
                this.OnPropertyChanged(nameof(AttackerRangedMachines));
            }
        }

        [DataSourceProperty]
        public MBBindingList<CustomBattleSiegeMachineVM> DefenderMachines
        {
            get => this._defenderMachines;
            set
            {
                if (value == this._defenderMachines)
                    return;
                this._defenderMachines = value;
                this.OnPropertyChanged(nameof(DefenderMachines));
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
            _scenes = _state.Scenes;

            TitleText = new TextVM(title);

            MapText = new TextVM(GameTexts.FindText("str_ebt_map"));

            SeasonText = new TextVM(GameTexts.FindText("str_ebt_season"));

            StartButtonText = new TextVM(GameTexts.FindText("str_start"));

            PlayerSide = new SideVM(_config.PlayerTeamConfig, new TextObject("{=BC7n6qxk}PLAYER"), true,
                _config.BattleTypeConfig);
            EnemySide = new SideVM(_config.EnemyTeamConfig, new TextObject("{=35IHscBa}ENEMY"), false,
                _config.BattleTypeConfig);

            MapSelectionGroup = new MapSelectionGroup("",
                _scenes.Select(sceneData =>
                    new MapSelectionElement(sceneData.Name.ToString(), sceneData.IsSiegeMap,
                        sceneData.IsVillageMap)).ToList());
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
            MapSelectionGroup.SearchText = _config.MapConfig.MapNameSearchText;
            MapSelectionGroup.SceneLevelSelection.SelectedIndex = _config.MapConfig.SceneLevel - 1;
            switch (_config.MapConfig.WallHitPoint)
            {
                case 0:
                    MapSelectionGroup.WallHitpointSelection.SelectedIndex = 0;
                    break;
                case 50:
                    MapSelectionGroup.WallHitpointSelection.SelectedIndex = 1;
                    break;
                case 100:
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
        }

        public void ExecuteBack()
        {
            _config = null;
            Game.Current.GameStateManager.PopState();
        }

        public void ExecuteStart()
        {
            if (!IsValid())
                return;

            _config.MapConfig.MapNameSearchText = MapSelectionGroup.SearchText;
            if (MapSelectionGroup.SceneLevelSelection.SelectedItem != null)
                _config.MapConfig.SceneLevel = int.Parse(MapSelectionGroup.SceneLevelSelection.SelectedItem.StringItem);
            if (MapSelectionGroup.WallHitpointSelection.SelectedItem != null)
                _config.MapConfig.WallHitPoint = int.Parse(MapSelectionGroup.WallHitpointSelection.SelectedItem.StringItem);
            if (MapSelectionGroup.SeasonSelection.SelectedItem != null)
                _config.MapConfig.Season = MapSelectionGroup.SeasonSelection.SelectedItem.StringItem.ToLower();

            MapSelectionElement selectedMap;
            MapSelectionElement mapWithName = MapSelectionGroup.GetMapWithName(MapSelectionGroup.SearchText);
            if (mapWithName != null)
                selectedMap = mapWithName;
            else
            {
                MapSelectionGroup.ExecuteSelectRandomMap();
                selectedMap = MapSelectionGroup.SelectedMap;
                if (selectedMap == null)
                {
                    Utility.DisplayLocalizedText("str_ebt_no_map");
                    return;
                }

                // Keep search text not changed.
                MapSelectionGroup.SearchText = _config.MapConfig.MapNameSearchText;
            }

            SceneData sceneData = _scenes.First(data => data.Name.ToString() == selectedMap.MapName);

            _config.SiegeMachineConfig.AttackerMeleeMachines =
                AttackerMeleeMachines.Select(vm => vm.MachineID).ToList();
            _config.SiegeMachineConfig.AttackerRangedMachines =
                AttackerRangedMachines.Select(vm => vm.MachineID).ToList();
            _config.SiegeMachineConfig.DefenderMachines =
                DefenderMachines.Select(vm => vm.MachineID).ToList();

            _config.Serialize(EnhancedBattleTestSubModule.IsMultiplayer);
            GameTexts.SetVariable("MapName", sceneData.Name);
            Utility.DisplayLocalizedText("str_ebt_current_map");
            EnhancedBattleTestMissions.OpenMission(_config, sceneData.Id);
        }

        private void OnPlayerTypeChange(bool isCommander)
        {

        }

        private void InitializeSiegeMachines()
        {
            AttackerMeleeMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < 3; ++index)
                AttackerMeleeMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(_config.SiegeMachineConfig.AttackerMeleeMachines.ElementAtOrDefault(index)),
                    OnMeleeMachineSelection));
            AttackerRangedMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < 4; ++index)
                AttackerRangedMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(_config.SiegeMachineConfig.AttackerRangedMachines.ElementAtOrDefault(index)),
                    OnAttackerRangedMachineSelection));
            DefenderMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (var index = 0; index < 4; ++index)
                DefenderMachines.Add(new CustomBattleSiegeMachineVM(
                    Utility.GetSiegeEngineType(_config.SiegeMachineConfig.DefenderMachines.ElementAtOrDefault(index)),
                    OnDefenderRangedMachineSelection));
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
            get => this._isAttackerCustomMachineSelectionEnabled;
            set
            {
                if (value == this._isAttackerCustomMachineSelectionEnabled)
                    return;
                this._isAttackerCustomMachineSelectionEnabled = value;
                this.OnPropertyChanged(nameof(IsAttackerCustomMachineSelectionEnabled));
            }
        }

        [DataSourceProperty]
        public bool IsDefenderCustomMachineSelectionEnabled
        {
            get => this._isDefenderCustomMachineSelectionEnabled;
            set
            {
                if (value == this._isDefenderCustomMachineSelectionEnabled)
                    return;
                this._isDefenderCustomMachineSelectionEnabled = value;
                this.OnPropertyChanged(nameof(IsDefenderCustomMachineSelectionEnabled));
            }
        }

        private void OnMeleeMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>
            {
                new InquiryElement((object) null, "Empty", (ImageIdentifier) null)
            };
            foreach (SiegeEngineType attackerMeleeMachine in this.GetAllAttackerMeleeMachines())
                inquiryElements.Add(new InquiryElement((object)attackerMeleeMachine, attackerMeleeMachine.Name.ToString(), (ImageIdentifier)null));
            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=MVOWsP48}Select a Melee Machine", (Dictionary<string, TextObject>)null).ToString(), string.Empty, inquiryElements, false, true, GameTexts.FindText("str_done", (string)null).ToString(), "", (Action<List<InquiryElement>>)(selectedElements => selectedSlot.SetMachineType(selectedElements.First<InquiryElement>().Identifier as SiegeEngineType)), (Action<List<InquiryElement>>)null, ""), false);
        }

        private void OnAttackerRangedMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>
            {
                new InquiryElement((object) null, "Empty", (ImageIdentifier) null)
            };
            foreach (SiegeEngineType attackerRangedMachine in this.GetAllAttackerRangedMachines())
                inquiryElements.Add(new InquiryElement((object)attackerRangedMachine, attackerRangedMachine.Name.ToString(), (ImageIdentifier)null));
            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=SLZzfNPr}Select a Ranged Machine", (Dictionary<string, TextObject>)null).ToString(), string.Empty, inquiryElements, false, true, GameTexts.FindText("str_done", (string)null).ToString(), "", (Action<List<InquiryElement>>)(selectedElements => selectedSlot.SetMachineType(selectedElements[0].Identifier as SiegeEngineType)), (Action<List<InquiryElement>>)null, ""), false);
        }

        private void OnDefenderRangedMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>
            {
                new InquiryElement((object) null, "Empty", (ImageIdentifier) null)
            };
            foreach (SiegeEngineType defenderRangedMachine in this.GetAllDefenderRangedMachines())
                inquiryElements.Add(new InquiryElement((object)defenderRangedMachine, defenderRangedMachine.Name.ToString(), (ImageIdentifier)null));
            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=SLZzfNPr}Select a Ranged Machine", (Dictionary<string, TextObject>)null).ToString(), string.Empty, inquiryElements, false, true, GameTexts.FindText("str_done", (string)null).ToString(), "", (Action<List<InquiryElement>>)(selectedElements => selectedSlot.SetMachineType(selectedElements[0].Identifier as SiegeEngineType)), (Action<List<InquiryElement>>)null, ""), false);
        }
    }
}
