using EnhancedBattleTest.Config;
using EnhancedBattleTest.Data;
using EnhancedBattleTest.UI.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle.SelectionItem;

namespace EnhancedBattleTest.UI
{
    public class MapSelectionGroupVM : ViewModel
    {
        private bool _isCurrentMapSiege;
        private bool _isSallyOutSelected;
        private SelectorVM<MapItemVM> _mapSelection;
        private SelectorVM<SceneLevelItemVM> _sceneLevelSelection;
        private SelectorVM<WallHitpointItemVM> _wallHitpointSelection;
        private SelectorVM<WeatherItemVM> _weatherSelection;
        private bool _isDefaultFogDensity;
        //private MBBindingList<MapItemVM> _mapSearchResults;
        private string _titleText;

        private string _mapText;
        private string _dayOfYearText;
        private string _timeOfDayText;
        private string _rainDensityText;
        private string _fogDensityText;
        private string _fogDefaultText;
        private string _sceneLevelText;
        private string _wallHitpointsText;
        private string _attackerSiegeMachinesText;
        private string _defenderSiegeMachinesText;
        private string _sallyoutText;
        private readonly List<SceneData> _scenes;

        public int SelectedWallBreachedCount { get; private set; }

        public int SelectedSceneLevel { get; private set; }

        public float SelectedTimeOfDay => TimeOfDay.Value;

        public int SelectedDayOfYear => (int)DayOfYear.Value;

        public string SelectedWeatherId { get; private set; }

        public float SelectedFogDensity =>
            IsDefaultFogDensity ? -1f : FogDensity.Value;

        public NumberVM<float> TimeOfDay { get; }

        public NumberVM<float> DayOfYear { get; }

        public NumberVM<float> FogDensity { get; }

        public string SelectedMapId
        {
            get
            {
               return SelectedMap?.MapId ?? "";
            }
            set
            {
                var index = Math.Max(MapSelection.ItemList.FindIndex(x => x.MapId == value), 0);
                MapSelection.SelectedIndex = index;
            }
        }

        public MapItemVM SelectedMap { get; private set; }

        private readonly List<MapItemVM> _battleMaps;

        private readonly List<MapItemVM> _villageMaps;

        private readonly List<MapItemVM> _siegeMaps;

        private List<MapItemVM> _availableMaps;

        public MapSelectionGroupVM(List<SceneData> scenes)
        {
            _scenes = scenes;
            //MapSearchResults = new MBBindingList<MapItemVM>();
            _battleMaps = new List<MapItemVM>();
            _villageMaps = new List<MapItemVM>();
            _siegeMaps = new List<MapItemVM>();
            MapSelection = new SelectorVM<MapItemVM>(0, new Action<SelectorVM<MapItemVM>>(this.OnMapSelection));
            WallHitpointSelection = new SelectorVM<WallHitpointItemVM>(0, OnWallHitpointSelection);
            SceneLevelSelection = new SelectorVM<SceneLevelItemVM>(0, OnSceneLevelSelection);
            WeatherSelection =
                new SelectorVM<WeatherItemVM>(0, OnWeatherSelection);
            DayOfYear = new NumberVM<float>(1f, 1f, CampaignTime.DaysInYear, true);
            TimeOfDay = new NumberVM<float>(6f, 0f, 24f, false);
            FogDensity = new NumberVM<float>(1f, 0f, 64f, false);
            IsDefaultFogDensity = true;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            PrepareMapLists();
            TitleText = new TextObject("{=customgametitle}Map").ToString();
            MapText = new TextObject("{=customgamemapname}Map").ToString();
            DayOfYearText =
                GameTexts.FindText("str_ebt_day_of_year").ToString();
            TimeOfDayText = new TextObject("{=DszSWnc3}Time of Day").ToString();
            RainDensityText =
                GameTexts.FindText("str_ebt_weather").ToString();
            FogDensityText = GameTexts.FindText("str_ebt_fog_density").ToString();
            FogDefaultText =
                GameTexts.FindText("str_ebt_density_default").ToString();
            SceneLevelText = new TextObject("{=0s52GQJt}Scene Level").ToString();
            WallHitpointsText = new TextObject("{=4IuXGSdc}Wall Hitpoints").ToString();
            AttackerSiegeMachinesText = new TextObject("{=AmfIfeIc}Choose Attacker Siege Machines").ToString();
            DefenderSiegeMachinesText = new TextObject("{=UoiSWe87}Choose Defender Siege Machines").ToString();
            SalloutText = new TextObject("{=EcKMGoFv}Sallyout").ToString();
            MapSelection.ItemList.Clear();
            WallHitpointSelection.ItemList.Clear();
            SceneLevelSelection.ItemList.Clear();
            WeatherSelection.ItemList.Clear();
            foreach (Tuple<string, int> wallHitpoint in CustomBattleData.WallHitpoints)
                WallHitpointSelection.AddItem(new WallHitpointItemVM(wallHitpoint.Item1, wallHitpoint.Item2));
            foreach (int sceneLevel in CustomBattleData.SceneLevels)
                SceneLevelSelection.AddItem(new SceneLevelItemVM(sceneLevel));
            AddWeatherItem("clear", "str_ebt_weather_clear");
            AddWeatherItem("light_rain", "str_ebt_weather_light_rain");
            AddWeatherItem("heavy_rain", "str_ebt_weather_heavy_rain");
            AddWeatherItem("snowy", "str_ebt_weather_snowy");
            AddWeatherItem("blizzard", "str_ebt_weather_blizzard");
            WallHitpointSelection.SelectedIndex = 0;
            SceneLevelSelection.SelectedIndex = 0;
            WeatherSelection.SelectedIndex = 0;
        }

        private void AddWeatherItem(string id, string textId)
        {
            WeatherSelection.AddItem(
                new WeatherItemVM(id, GameTexts.FindText(textId)));
        }

        public void ExecuteSallyOutChange()
        {
            IsSallyOutSelected = !IsSallyOutSelected;
        }

        private void PrepareMapLists()
        {
            _battleMaps.Clear();
            _villageMaps.Clear();
            _siegeMaps.Clear();
            foreach (var sceneData in _scenes)
            {
                MapItemVM mapItemVm = new MapItemVM(
                    sceneData.Name.ToString(),
                    sceneData.SceneID,
                    null);
                if (sceneData.IsVillageMap)
                    _villageMaps.Add(mapItemVm);
                else if (sceneData.IsSiegeMap)
                    _siegeMaps.Add(mapItemVm);
                else if (!sceneData.IsLordsHallMap)
                    _battleMaps.Add(mapItemVm);
            }
            Comparer<MapItemVM> comparer = Comparer<MapItemVM>.Create((x, y) => -x.MapName.CompareTo(y.MapName));
            _battleMaps.Sort(comparer);
            _villageMaps.Sort(comparer);
            _siegeMaps.Sort(comparer);
        }


        private void OnMapSelection(SelectorVM<MapItemVM> selector)
        {
            SelectedMap = selector.SelectedItem;
            //SearchText = selector.SelectedItem.MapName;
        }

        private void OnWallHitpointSelection(SelectorVM<WallHitpointItemVM> selector)
        {
            SelectedWallBreachedCount = selector.SelectedItem.BreachedWallCount;
        }

        private void OnSceneLevelSelection(SelectorVM<SceneLevelItemVM> selector)
        {
            SelectedSceneLevel = selector.SelectedItem.Level;
        }

        private void OnWeatherSelection(SelectorVM<WeatherItemVM> selector)
        {
            SelectedWeatherId = selector.SelectedItem.WeatherId;
        }

        public void SetWeather(string weatherId)
        {
            WeatherSelection.SelectedIndex = Math.Max(
                WeatherSelection.ItemList.FindIndex(
                    item => item.WeatherId == weatherId),
                0);
        }

        public void SetFogDensity(float density)
        {
            IsDefaultFogDensity = density < 0f;
            if (!IsDefaultFogDensity)
                FogDensity.Value = density;
        }

        public void OnGameTypeChange(BattleType gameType)
        {
            //MapSearchResults.Clear();
            MapSelection.ItemList.Clear();
            //SelectedMap = null;
            switch (gameType)
            {
                case BattleType.Battle:
                    IsCurrentMapSiege = false;
                    _availableMaps = _battleMaps;
                    break;
                case BattleType.Village:
                    IsCurrentMapSiege = false;
                    _availableMaps = _villageMaps;
                    break;
                case BattleType.Siege:
                    IsCurrentMapSiege = true;
                    _availableMaps = _siegeMaps;
                    break;
            }
            foreach (MapItemVM availableMap in _availableMaps)
            {
                //MapSearchResults.Add(availableMap);
                MapSelection.AddItem(availableMap);
            }

            MapSelection.SelectedIndex = 0;
            //if (_availableMaps.Count == 0)
            //{
            //    Utility.DisplayLocalizedText("str_ebt_no_map");
            //}
            //else
            //{
            //    //_searchText = new TextObject("{=7i1vmgQ9}Select a Map").ToString();
            //    _searchText = "";
            //    OnPropertyChanged(nameof(SearchText));
            //}
        }

        public void RandomizeAll()
        {
            //MBBindingList<MapItemVM> mapSearchResults = MapSearchResults;
            // ISSUE: explicit non-virtual call
            //if (mapSearchResults != null && mapSearchResults.Count > 0)
            //{
            //    SearchText = "";
            //    SelectedMap = MapSearchResults[MBRandom.RandomInt(MapSearchResults.Count)];
            //}
            MapSelection.ExecuteRandomize();
            SceneLevelSelection.ExecuteRandomize();
            WallHitpointSelection.ExecuteRandomize();
            WeatherSelection.ExecuteRandomize();
            DayOfYear.Value = MBRandom.RandomInt(
                1,
                CampaignTime.DaysInYear + 1);
            TimeOfDay.Value = MBRandom.RandomFloat * 24f;
            IsDefaultFogDensity = false;
            FogDensity.Value = MBRandom.RandomFloat * 64f;
        }

        public void RandomizeMap()
        {
            //MBBindingList<MapItemVM> mapSearchResults = MapSearchResults;
            //// ISSUE: explicit non-virtual call
            //if (mapSearchResults != null && mapSearchResults.Count > 0)
            //{
            //    SelectedMap = MapSearchResults[MBRandom.RandomInt(MapSearchResults.Count)];
            //    SearchText = SelectedMap.MapName;
            //}
            MapSelection.ExecuteRandomize();
        }

        //private void RefreshSearch(bool isAppending)
        //{
        //    if (isAppending)
        //    {
        //        foreach (MapItemVM mapItemVm in MapSearchResults.ToList())
        //        {
        //            if (mapItemVm.MapName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) < 0)
        //                MapSearchResults.Remove(mapItemVm);
        //            else
        //                mapItemVm.UpdateSearchedText(_searchText);
        //        }
        //    }
        //    else
        //    {
        //        MapSearchResults.Clear();
        //        foreach (MapItemVM availableMap in _availableMaps)
        //        {
        //            MapItemVM map = availableMap;
        //            if (map.MapName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0 && MapSearchResults.All(m => m.MapName != map.MapName))
        //                MapSearchResults.Add(map);
        //        }
        //        _availableMaps.ForEach(m => m.UpdateSearchedText(_searchText));
        //    }
        //}



        [DataSourceProperty]
        public SelectorVM<MapItemVM> MapSelection
        {
            get => this._mapSelection;
            set
            {
                if (value == this._mapSelection)
                    return;
                this._mapSelection = value;
                this.OnPropertyChangedWithValue<SelectorVM<MapItemVM>>(value, nameof(MapSelection));
            }
        }

        //[DataSourceProperty]
        //public MBBindingList<MapItemVM> MapSearchResults
        //{
        //    get => _mapSearchResults;
        //    set
        //    {
        //        if (value == _mapSearchResults)
        //            return;
        //        _mapSearchResults = value;
        //        OnPropertyChanged(nameof(MapSearchResults));
        //    }
        //}

        [DataSourceProperty]
        public SelectorVM<SceneLevelItemVM> SceneLevelSelection
        {
            get => _sceneLevelSelection;
            set
            {
                if (value == _sceneLevelSelection)
                    return;
                _sceneLevelSelection = value;
                OnPropertyChanged(nameof(SceneLevelSelection));
            }
        }

        [DataSourceProperty]
        public SelectorVM<WallHitpointItemVM> WallHitpointSelection
        {
            get => _wallHitpointSelection;
            set
            {
                if (value == _wallHitpointSelection)
                    return;
                _wallHitpointSelection = value;
                OnPropertyChanged(nameof(WallHitpointSelection));
            }
        }

        [DataSourceProperty]
        public SelectorVM<WeatherItemVM> WeatherSelection
        {
            get => _weatherSelection;
            set
            {
                if (value == _weatherSelection)
                    return;
                _weatherSelection = value;
                OnPropertyChangedWithValue(value, nameof(WeatherSelection));
            }
        }

        [DataSourceProperty]
        public bool IsDefaultFogDensity
        {
            get => _isDefaultFogDensity;
            set
            {
                if (value == _isDefaultFogDensity)
                    return;
                _isDefaultFogDensity = value;
                FogDensity.IsEnabled = !value;
                OnPropertyChanged(nameof(IsDefaultFogDensity));
            }
        }

        [DataSourceProperty]
        public bool IsCurrentMapSiege
        {
            get => _isCurrentMapSiege;
            set
            {
                if (value == _isCurrentMapSiege)
                    return;
                _isCurrentMapSiege = value;
                OnPropertyChanged(nameof(IsCurrentMapSiege));
            }
        }

        [DataSourceProperty]
        public bool IsSallyOutSelected
        {
            get => _isSallyOutSelected;
            set
            {
                if (value == _isSallyOutSelected)
                    return;
                _isSallyOutSelected = value;
                OnPropertyChanged(nameof(IsSallyOutSelected));
            }
        }

        //[DataSourceProperty]
        //public string SearchText
        //{
        //    get => _searchText;
        //    set
        //    {
        //        if (value == _searchText)
        //            return;
        //        bool isAppending = true;
        //        if (!string.IsNullOrEmpty(_searchText))
        //            isAppending = value.ToLower().Contains(_searchText.ToLower());
        //        _searchText = value;
        //        RefreshSearch(isAppending);
        //        OnPropertyChanged(nameof(SearchText));
        //    }
        //}

        [DataSourceProperty]
        public string TitleText
        {
            get => _titleText;
            set
            {
                if (value == _titleText)
                    return;
                _titleText = value;
                OnPropertyChanged(nameof(TitleText));
            }
        }

        [DataSourceProperty]
        public string MapText
        {
            get
            {
                return _mapText;
            }
            set
            {
                if (value != _mapText)
                {
                    _mapText = value;
                    OnPropertyChangedWithValue(value, "MapText");
                }
            }
        }

        [DataSourceProperty]
        public string DayOfYearText
        {
            get => _dayOfYearText;
            set
            {
                if (value == _dayOfYearText)
                    return;
                _dayOfYearText = value;
                OnPropertyChanged(nameof(DayOfYearText));
            }
        }

        [DataSourceProperty]
        public string TimeOfDayText
        {
            get => _timeOfDayText;
            set
            {
                if (value == _timeOfDayText)
                    return;
                _timeOfDayText = value;
                OnPropertyChangedWithValue(value, nameof(TimeOfDayText));
            }
        }

        [DataSourceProperty]
        public string RainDensityText
        {
            get => _rainDensityText;
            set
            {
                if (value == _rainDensityText)
                    return;
                _rainDensityText = value;
                OnPropertyChangedWithValue(value, nameof(RainDensityText));
            }
        }

        [DataSourceProperty]
        public string FogDensityText
        {
            get => _fogDensityText;
            set
            {
                if (value == _fogDensityText)
                    return;
                _fogDensityText = value;
                OnPropertyChangedWithValue(value, nameof(FogDensityText));
            }
        }

        [DataSourceProperty]
        public string FogDefaultText
        {
            get => _fogDefaultText;
            set
            {
                if (value == _fogDefaultText)
                    return;
                _fogDefaultText = value;
                OnPropertyChangedWithValue(value, nameof(FogDefaultText));
            }
        }

        [DataSourceProperty]
        public string SceneLevelText
        {
            get => _sceneLevelText;
            set
            {
                if (value == _sceneLevelText)
                    return;
                _sceneLevelText = value;
                OnPropertyChanged(nameof(SceneLevelText));
            }
        }

        [DataSourceProperty]
        public string WallHitpointsText
        {
            get => _wallHitpointsText;
            set
            {
                if (value == _wallHitpointsText)
                    return;
                _wallHitpointsText = value;
                OnPropertyChanged(nameof(WallHitpointsText));
            }
        }

        [DataSourceProperty]
        public string AttackerSiegeMachinesText
        {
            get => _attackerSiegeMachinesText;
            set
            {
                if (value == _attackerSiegeMachinesText)
                    return;
                _attackerSiegeMachinesText = value;
                OnPropertyChanged(nameof(AttackerSiegeMachinesText));
            }
        }

        [DataSourceProperty]
        public string DefenderSiegeMachinesText
        {
            get => _defenderSiegeMachinesText;
            set
            {
                if (value == _defenderSiegeMachinesText)
                    return;
                _defenderSiegeMachinesText = value;
                OnPropertyChanged(nameof(DefenderSiegeMachinesText));
            }
        }

        [DataSourceProperty]
        public string SalloutText
        {
            get => _sallyoutText;
            set
            {
                if (value == _sallyoutText)
                    return;
                _sallyoutText = value;
                OnPropertyChanged(nameof(SalloutText));
            }
        }
    }

    public sealed class WeatherItemVM : SelectorItemVM
    {
        public string WeatherId { get; }

        public WeatherItemVM(string weatherId, TextObject name)
            : base(name)
        {
            WeatherId = weatherId;
        }
    }

}
