using System;

namespace EnhancedBattleTest.Config
{
    public class MapConfig
    {
        public bool OverridesPlayerPosition = false;
        public bool IsSallyOutSelected = false;
        public string SelectedMapId = String.Empty;
        public int BreachedWallCount = 0;
        public int SceneLevel = 1;
        public string Season = String.Empty;
        public int TimeOfDay;
        public float RainDensity;
        public float FogDensity;
    }
}
