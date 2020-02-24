using System;
using System.IO;
using System.Xml.Serialization;


namespace EnhancedBattleTest
{
    public class EnhancedCustomBattleConfig : BattleConfigBase<EnhancedCustomBattleConfig>
    {
        public class SceneInfo
        {
            public string name;
            public float skyBrightness = -1;
            public float rainDensity = -1;
        }

        private static EnhancedCustomBattleConfig _instance;


        public SceneInfo[] sceneList;

        public int sceneIndex;

        protected static Version BinaryVersion => new Version(1, 3);

        protected void UpgradeToCurrentVersion()
        {
            switch (ConfigVersion?.ToString())
            {
                case "1.0":
                case "1.1":
                case "1.2":
                default:
                    Utility.DisplayMessage("Config version not compatible.\nReset config.");
                    ResetToDefault();
                    Serialize();
                    break;
                case "1.3":
                    break;
            }
        }
        [XmlIgnore]
        public float SkyBrightness
        {
            get => sceneList[sceneIndex].skyBrightness;
            set => sceneList[sceneIndex].skyBrightness = value;
        }

        [XmlIgnore]
        public float RainDensity
        {
            get => sceneList[sceneIndex].rainDensity;
            set => sceneList[sceneIndex].rainDensity = value;
        }

        [XmlIgnore]
        public string SceneName => sceneList[sceneIndex].name;

        private static EnhancedCustomBattleConfig CreateDefault()
        {
            //string sceneIndex = "mp_skirmish_map_001a";
            //string sceneIndex = "mp_sergeant_map_001";
            // string sceneIndex = "mp_test_bora";
            // string sceneIndex = "battle_test";
            // string sceneIndex = "mp_duel_001_winter";
            // string sceneIndex = "mp_sergeant_map_001";
            // string sceneIndex = "mp_tdm_map_001";
            // string sceneIndex = "scn_world_map";
            // string sceneIndex = "mp_compact";
            SceneInfo[] list = new[]
            {
                //"mp_skirmish_map_001a",
                //"mp_sergeant_map_001"
                //"mp_sergeant_map_005",
                //"mp_sergeant_map_007",
                //"mp_sergeant_map_008",
                //"mp_sergeant_map_009",
                //"mp_sergeant_map_010",
                //"mp_sergeant_map_011",
                //"mp_sergeant_map_011s",
                //"mp_sergeant_map_012",
                //"mp_sergeant_map_013",
                //"mp_sergeant_map_vlandia_01",
                //"mp_siege_map_001",
                //"mp_siege_map_002",
                //"mp_siege_map_003",
                //"mp_siege_map_004",
                //"mp_siege_map_005",
                new SceneInfo{name = "mp_sergeant_map_001"},
                //new SceneInfo{name = "mp_sergeant_map_005"},
                new SceneInfo{name = "mp_sergeant_map_007"},
                new SceneInfo{name = "mp_sergeant_map_008"},
                new SceneInfo{name = "mp_sergeant_map_009"},
                new SceneInfo{name = "mp_sergeant_map_010"},
                new SceneInfo{name = "mp_sergeant_map_011"},
                new SceneInfo{name = "mp_sergeant_map_011s"},
                new SceneInfo{name = "mp_sergeant_map_012"},
                new SceneInfo{name = "mp_sergeant_map_013"},
                new SceneInfo{name = "mp_sergeant_map_vlandia_01"},
            };
            int defaultIndex = 0;
            var p = new EnhancedCustomBattleConfig
            {
                ConfigVersion = BinaryVersion.ToString(2),
                sceneList = list,
                sceneIndex = defaultIndex,
                playerClass = new ClassInfo { classStringId = "mp_light_cavalry_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0},
                enemyClass = new ClassInfo { classStringId = "mp_light_cavalry_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0 },
                spawnEnemyCommander = true,
                playerTroops = new ClassInfo[3]
                {
                    new ClassInfo { classStringId = "mp_shock_infantry_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                    new ClassInfo { classStringId = "mp_heavy_ranged_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                    new ClassInfo { classStringId = "mp_heavy_infantry_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                },
                enemyTroops = new ClassInfo[3]
                {
                    new ClassInfo { classStringId = "mp_shock_infantry_battania", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                    new ClassInfo { classStringId = "mp_heavy_ranged_battania", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                    new ClassInfo { classStringId = "mp_light_infantry_battania", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                },
                useFreeCamera = false,
            };
            return p;
        }

        public static EnhancedCustomBattleConfig Get()
        {
            if (_instance == null)
            {
                _instance = new EnhancedCustomBattleConfig();
                _instance.SyncWithSave();
            }
            return _instance;
        }

        public override bool Validate()
        {
            return base.Validate()
                && this.sceneIndex >= 0 && this.sceneIndex < this.sceneList.Length;
        }

        public override bool Serialize()
        {
            try
            {
                EnsureSaveDirectory();
                XmlSerializer serializer = new XmlSerializer(typeof(EnhancedCustomBattleConfig));
                using (TextWriter writer = new StreamWriter(SaveName))
                {
                    serializer.Serialize(writer, this);
                }
                Utility.DisplayMessage("Save config succeeded.");
                return true;
            }
            catch (Exception e)
            {
                Utility.DisplayMessage("Error: Save config failed.");
                Utility.DisplayMessage("Exception caught: " + e.ToString());
                Console.WriteLine(e);
            }

            return false;
        }

        public override bool Deserialize()
        {
            try
            {
                EnsureSaveDirectory();
                XmlSerializer deserializer = new XmlSerializer(typeof(EnhancedCustomBattleConfig));
                using (TextReader reader = new StreamReader(SaveName))
                {
                    var config = (EnhancedCustomBattleConfig)deserializer.Deserialize(reader);
                    this.CopyFrom(config);
                }
                Utility.DisplayMessage("Load config succeeded.");
                UpgradeToCurrentVersion();
                return true;
            }
            catch (Exception e)
            {
                Utility.DisplayMessage("Error: Load config failed.");
                Utility.DisplayMessage("Exception caught: " + e.ToString());
                Console.WriteLine(e);
            }

            return false;
        }

        public override void ReloadSavedConfig()
        {
            var loadedConfig = CreateDefault();
            if (loadedConfig.Deserialize())
                CopyFrom(loadedConfig);
        }

        public override void ResetToDefault()
        {
            CopyFrom(CreateDefault());
        }

        protected override void CopyFrom(EnhancedCustomBattleConfig other)
        {
            //public SceneInfo[] sceneList { get; set; }
            //public int sceneIndex;
            //public int playerSoldierCount, enemySoldierCount;
            //public float distance;
            //public float soldierXInterval, soldierYInterval;
            ////public int soldiersPerRow;
            ////public Vec2 formationPosition;
            ////public Vec2 formationDirection;
            ////public float skyBrightness;
            ////public float rainDensity;
            //[XmlElement("PlayerStringId")]
            //public string playerHeroClassStringId;
            //public int playerSelectedPerk;
            //[XmlElement("PlayerTroopStringId")]
            //public string playerTroopHeroClassStringId;
            //public int playerTroopSelectedPerk;
            //[XmlElement("EnemyTroopStringId")]
            //public string enemyTroopHeroClassStringId;
            //public int enemyTroopSelectedPerk;
            //public bool useFreeCamera;
            base.CopyFrom(other);

            if (other.sceneList != null)
                this.sceneList = other.sceneList;
            this.sceneIndex = other.sceneIndex;
        }

        protected override string SaveName => SavePath + nameof(EnhancedCustomBattleConfig) + ".xml";
        protected override string[] OldNames { get; } = { };
    }
}