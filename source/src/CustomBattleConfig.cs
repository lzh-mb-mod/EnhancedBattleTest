using System;
using System.IO;
using System.Xml.Serialization;


namespace EnhancedBattleTest
{
    public class CustomBattleConfig : BattleConfigBase<CustomBattleConfig>
    {
        public class SceneInfo
        {
            public string name;
            public float skyBrightness = -1;
            public float rainDensity = -1;
        }

        private static CustomBattleConfig _instance;


        public SceneInfo[] sceneList;
        public int sceneIndex;

        protected static Version BinaryVersion => new Version(1, 0);

        protected void UpgradeToCurrentVersion()
        {
            switch (ConfigVersion?.ToString())
            {
                case "1.0": break;
                default:
                    Utility.DisplayMessage("Config version not compatible.\nReset config.");
                    ResetToDefault();
                    Serialize();
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
        
        public bool useFreeCamera;

        [XmlIgnore]
        public string SceneName => sceneList[sceneIndex].name;

        private static CustomBattleConfig CreateDefault()
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
                //"mp_tdm_map_001",
                //"mp_duel_001",
                //"mp_duel_001_winter",
                //"mp_duel_002",
                //"mp_ruins_2",
                new SceneInfo{name = "mp_skirmish_map_001a"},
                new SceneInfo{name = "mp_tdm_map_001"},
                new SceneInfo{name = "mp_duel_001"},
                new SceneInfo{name = "mp_duel_001_winter"},
                new SceneInfo{name = "mp_ruins_2"},
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
                //new SceneInfo{name = "mp_sergeant_map_005", formationPosition = new Vec2(330, 439), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_sergeant_map_007"},
                new SceneInfo{name = "mp_sergeant_map_008"},
                new SceneInfo{name = "mp_sergeant_map_009"},
                //new SceneInfo{name = "mp_sergeant_map_010", formationPosition = new Vec2(391,376), formationDirection = new Vec2(0,1)},
                new SceneInfo{name = "mp_sergeant_map_011"},
                new SceneInfo{name = "mp_sergeant_map_011s"},
                //new SceneInfo{name = "mp_sergeant_map_012", formationPosition = new Vec2(580,576), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_sergeant_map_013"},
                new SceneInfo{name = "mp_sergeant_map_vlandia_01"},
                //new SceneInfo{name = "mp_siege_map_001", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_siege_map_002", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_siege_map_003", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_siege_map_004", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_siege_map_005", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                //"mp_skirmish_map_002f",
                //"mp_skirmish_map_002_winter",
                //"mp_skirmish_map_004",
                //"mp_skirmish_map_005",
                //"mp_skirmish_map_006",
                //"mp_skirmish_map_007",
                //"mp_skirmish_map_007_winter",
                //"mp_skirmish_map_008",
                //"mp_skirmish_map_009",
                //"mp_skirmish_map_010",
                //"mp_skirmish_map_013",
                //"mp_skirmish_map_battania_02",
                //"mp_skirmish_map_battania_03"
                new SceneInfo{name = "mp_skirmish_map_002f"},
                new SceneInfo{name = "mp_skirmish_map_002_winter"},
                new SceneInfo{name = "mp_skirmish_map_004"},
                new SceneInfo{name = "mp_skirmish_map_005"},
                new SceneInfo{name = "mp_skirmish_map_006"},
                new SceneInfo{name = "mp_skirmish_map_007"},
                new SceneInfo{name = "mp_skirmish_map_007_winter"},
                //new SceneInfo{name = "mp_skirmish_map_008", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_skirmish_map_009", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_skirmish_map_010", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_skirmish_map_013"},
                new SceneInfo{name = "mp_skirmish_map_battania_02"},
                new SceneInfo{name = "mp_skirmish_map_battania_03"},
            };
            int defaultIndex = 0;
            var p = new CustomBattleConfig
            {
                ConfigVersion = BinaryVersion.ToString(2),
                sceneList = list,
                sceneIndex = defaultIndex,
                playerSoldierCount = 20,
                enemySoldierCount = 20,
                playerClass = new ClassInfo { classStringId = "mp_light_cavalry_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0 },
                playerTroopClass = new ClassInfo { classStringId = "mp_heavy_infantry_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0 },
                enemyTroopClass = new ClassInfo { classStringId = "mp_shock_infantry_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0 },
                useFreeCamera = false
            };
            return p;
        }

        public static CustomBattleConfig Get()
        {
            if (_instance == null)
            {
                _instance = new CustomBattleConfig();
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
                XmlSerializer serializer = new XmlSerializer(typeof(CustomBattleConfig));
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
                Console.WriteLine(e);
            }

            return false;
        }

        public override bool Deserialize()
        {
            try
            {
                EnsureSaveDirectory();
                XmlSerializer deserializer = new XmlSerializer(typeof(CustomBattleConfig));
                using (TextReader reader = new StreamReader(SaveName))
                {
                    var config = (CustomBattleConfig)deserializer.Deserialize(reader);
                    this.CopyFrom(config);
                }
                Utility.DisplayMessage("Load config succeeded.");
                UpgradeToCurrentVersion();
                return true;
            }
            catch (Exception e)
            {
                Utility.DisplayMessage("Error: Load config failed.");
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

        protected override void CopyFrom(CustomBattleConfig other)
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
            this.useFreeCamera = other.useFreeCamera;
        }

        protected override string SaveName => SavePath + "CustomBattleConfig.xml";
        protected override string[] OldNames { get; } = { };
    }
}