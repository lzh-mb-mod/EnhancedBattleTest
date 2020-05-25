using System;
using System.IO;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace EnhancedBattleTest
{
    public class EnhancedFreeBattleConfig : BattleConfigBase<EnhancedFreeBattleConfig>
    {
        public class SceneInfo
        {
            public string name;
            public int soldiersPerRow = 20;
            public Vec2 formationPosition;
            public Vec2 formationDirection;
            public float distance = 50;
            public float skyBrightness = -1;
            public float rainDensity = -1;
        }

        private static EnhancedFreeBattleConfig _instance;

        public SceneInfo[] sceneList;
        public int sceneIndex;
        public float soldierXInterval, soldierYInterval;

        public bool makeGruntVoice = true;

        public bool hasBoundary;

        protected static Version BinaryVersion => new Version(1, 10);

        protected void UpgradeToCurrentVersion()
        {
            switch (ConfigVersion?.ToString())
            {
                default:
                    Utility.DisplayLocalizedText("str_config_incompatible");
                    ResetToDefault();
                    Serialize();
                    break;
                case "1.10":
                    break;
            }
        }

        [XmlIgnore]
        public override int SoldiersPerRow
        { 
            get => sceneList[sceneIndex].soldiersPerRow;
            set => sceneList[sceneIndex].soldiersPerRow = value;
        }

        [XmlIgnore]
        public override Vec2 FormationPosition
        {
            get => sceneList[sceneIndex].formationPosition;
            set => sceneList[sceneIndex].formationPosition = value;
        }

        [XmlIgnore]
        public override Vec2 FormationDirection
        {
            get => sceneList[sceneIndex].formationDirection;
            set => sceneList[sceneIndex].formationDirection = value;
        }

        [XmlIgnore]
        public override float SkyBrightness
        {
            get => sceneList[sceneIndex].skyBrightness;
            set => sceneList[sceneIndex].skyBrightness = value;
        }

        [XmlIgnore]
        public override float RainDensity
        {
            get => sceneList[sceneIndex].rainDensity;
            set => sceneList[sceneIndex].rainDensity = value;
        }

        [XmlIgnore]
        public override float Distance
        {
            get => sceneList[sceneIndex].distance;
            set => sceneList[sceneIndex].distance = value;
        }

        [XmlIgnore]
        public override string SceneName => sceneList[sceneIndex].name;

        public EnhancedFreeBattleConfig()
            : base(BattleType.FieldBattle)
        { }
        private static EnhancedFreeBattleConfig CreateDefault()
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
                new SceneInfo{name = "mp_skirmish_map_001a", formationPosition = new Vec2(200, 200), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_tdm_map_001", formationPosition = new Vec2(385, 570), formationDirection = new Vec2(0.7f, -0.3f).Normalized()},
                new SceneInfo{name = "mp_tdm_map_001_spring", formationPosition = new Vec2(385, 570), formationDirection = new Vec2(0.7f, -0.3f).Normalized()},
                new SceneInfo{name = "mp_tdm_map_004", formationPosition = new Vec2(300,325), formationDirection = new Vec2(-1,0).Normalized()},
                new SceneInfo{name = "mp_duel_001", formationPosition = new Vec2(567, 600), formationDirection = new Vec2(0, 10)},
                new SceneInfo{name = "mp_duel_001_winter", formationPosition = new Vec2(567, 600), formationDirection = new Vec2(0, 1)},
                //  new SceneInfo{name = "mp_duel_002", formationPosition = new Vec2(567, 600), formationDirection = new Vec2(0, 10)},
                new SceneInfo{name = "mp_ruins_2", formationPosition = new Vec2(514, 470), formationDirection = new Vec2(1, 0)},
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
                new SceneInfo{name = "mp_sergeant_map_001", formationPosition = new Vec2(250, 500), formationDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_sergeant_map_005", formationPosition = new Vec2(330, 439), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_sergeant_map_007", formationPosition = new Vec2(330, 439), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_sergeant_map_008", formationPosition = new Vec2(471,505), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_sergeant_map_009", formationPosition = new Vec2(530,503), formationDirection = new Vec2(0,1)},
                new SceneInfo{name = "mp_sergeant_map_010", formationPosition = new Vec2(391,376), formationDirection = new Vec2(0,1)},
                new SceneInfo{name = "mp_sergeant_map_011", formationPosition = new Vec2(485,364), formationDirection = new Vec2(0.4f,0.6f)},
                new SceneInfo{name = "mp_sergeant_map_011s", formationPosition = new Vec2(485,364), formationDirection = new Vec2(0.4f,0.6f)},
                new SceneInfo{name = "mp_sergeant_map_012", formationPosition = new Vec2(493,312), formationDirection = new Vec2(0.2f, 0.8f).Normalized()},
                new SceneInfo{name = "mp_sergeant_map_013", formationPosition = new Vec2(580,576), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_sergeant_map_vlandia_01", formationPosition = new Vec2(485,364), formationDirection = new Vec2(0.4f,0.6f)},
                //new SceneInfo{name = "mp_siege_map_001", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_siege_map_002", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_siege_map_003", formationPosition = new Vec2(461,634), formationDirection = new Vec2(0.55f,0.45f).Normalized(), distance = 180},
                new SceneInfo{name = "mp_siege_map_004", formationPosition = new Vec2(502,472), formationDirection = new Vec2(0.23f,0.77f).Normalized(), distance = 200},
                new SceneInfo{name = "mp_siege_map_004_bat", formationPosition = new Vec2(502,472), formationDirection = new Vec2(0.23f,0.77f).Normalized(), distance = 200},
                new SceneInfo{name = "mp_siege_map_004_rs", formationPosition = new Vec2(502,472), formationDirection = new Vec2(0.23f,0.77f).Normalized(), distance = 200},
                new SceneInfo{name = "mp_siege_map_005", formationPosition = new Vec2(424, 320), formationDirection = new Vec2(0,1), distance = 220},
                new SceneInfo{name = "mp_siege_map_007_battania", formationPosition = new Vec2(614,612), formationDirection = new Vec2(0.65f, 0.35f), distance = 140},
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
                new SceneInfo{name = "mp_skirmish_map_002_winter", formationPosition = new Vec2(415,490), formationDirection = new Vec2(0.3f, 0.7f).Normalized(), soldiersPerRow = 10},
                new SceneInfo{name = "mp_skirmish_map_002f", formationPosition = new Vec2(415,490), formationDirection = new Vec2(0.3f, 0.7f).Normalized(), soldiersPerRow = 10},
                new SceneInfo{name = "mp_skirmish_map_003_skinc", formationPosition = new Vec2(650,675), formationDirection = new Vec2(-0.7f,0.3f).Normalized(), soldiersPerRow = 10},
                new SceneInfo{name = "mp_skirmish_map_004", formationPosition = new Vec2(320,288), formationDirection = new Vec2(0,1), soldiersPerRow = 10},
                new SceneInfo{name = "mp_skirmish_map_005", formationPosition = new Vec2(477,496), formationDirection = new Vec2(1, 0), soldiersPerRow = 10},
                new SceneInfo{name = "mp_skirmish_map_006", formationPosition = new Vec2(480,561), formationDirection = new Vec2(1, 0), skyBrightness = 0},
                new SceneInfo{name = "mp_skirmish_map_006_nowater", formationPosition = new Vec2(480,561), formationDirection = new Vec2(1, 0), skyBrightness = 0},
                new SceneInfo{name = "mp_skirmish_map_007", formationPosition = new Vec2(190,154), formationDirection = new Vec2(0, 1)},
                new SceneInfo{name = "mp_skirmish_map_007_winter", formationPosition = new Vec2(190,154), formationDirection = new Vec2(0, 1)},
                //new SceneInfo{name = "mp_skirmish_map_008", formationPosition = new Vec2(495,500), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_skirmish_map_008_skin", formationPosition = new Vec2(495,500), formationDirection = new Vec2(1, 0)}, 
                //new SceneInfo{name = "mp_skirmish_map_009", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_skirmish_map_010", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_skirmish_map_013", formationPosition = new Vec2(250, 500), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_skirmish_map_battania_02", formationPosition = new Vec2(360,186), formationDirection = new Vec2(0.6f,-0.4f), soldiersPerRow = 10},
                new SceneInfo{name = "mp_skirmish_map_battania_03", formationPosition = new Vec2(360,186), formationDirection = new Vec2(0.6f,-0.4f), soldiersPerRow = 10},
            };
            int defaultIndex = 0;
            var p = new EnhancedFreeBattleConfig
            {
                ConfigVersion = BinaryVersion.ToString(2),
                sceneList = list,
                sceneIndex = defaultIndex,
                playerClass = new ClassInfo { classStringId = "mp_light_cavalry_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0 },
                SpawnPlayer = true,
                enemyClass = new ClassInfo { classStringId = "mp_light_cavalry_battania", selectedFirstPerk = 0, selectedSecondPerk = 0 },
                SpawnEnemyCommander = true,
                playerTroops = new ClassInfo[3]
                {
                    new ClassInfo { classStringId = "mp_shock_infantry_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                    new ClassInfo { classStringId = "mp_light_infantry_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                    new ClassInfo { classStringId = "mp_heavy_infantry_vlandia", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                },
                enemyTroops = new ClassInfo[3]
                {
                    new ClassInfo { classStringId = "mp_shock_infantry_battania", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                    new ClassInfo { classStringId = "mp_light_infantry_battania", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                    new ClassInfo { classStringId = "mp_heavy_infantry_battania", selectedFirstPerk = 0, selectedSecondPerk = 0, troopCount = 20 },
                },
                soldierXInterval = 1.5f,
                soldierYInterval = 1f,
                hasBoundary = true,
                disableDeath = false,
                changeCombatAI = false,
                combatAI = 100,
            };
            return p;
        }

        public static EnhancedFreeBattleConfig Get()
        {
            if (_instance == null)
            {
                _instance = CreateDefault();
                _instance.SyncWithSave();
            }

            return _instance;
        }

        public override bool Validate() {
            return base.Validate()
                && this.sceneIndex >= 0 && this.sceneIndex < this.sceneList.Length
                && this.Distance > 0
                && soldierXInterval > 0
                && soldierYInterval > 0
                && SoldiersPerRow > 0
                && FormationDirection.Length > 0;
        }

        public override bool Serialize()
        {
            try
            {
                EnsureSaveDirectory();
                XmlSerializer serializer = new XmlSerializer(typeof(EnhancedFreeBattleConfig));
                using (TextWriter writer = new StreamWriter(SaveName))
                {
                    serializer.Serialize(writer, this);
                }
                Utility.DisplayLocalizedText("str_saved_config");
                return true;
            }
            catch (Exception e)
            {
                Utility.DisplayLocalizedText("str_save_config_failed");
                Utility.DisplayLocalizedText("str_exception_caught");
                Utility.DisplayMessage(e.ToString());
                Console.WriteLine(e);
            }

            return false;
        }

        public override bool Deserialize()
        {
            try
            {
                EnsureSaveDirectory();
                XmlSerializer deserializer = new XmlSerializer(typeof(EnhancedFreeBattleConfig));
                using (TextReader reader = new StreamReader(SaveName))
                {
                    var config = (EnhancedFreeBattleConfig)deserializer.Deserialize(reader);
                    this.CopyFrom(config);
                }
                Utility.DisplayLocalizedText("str_loaded_config");
                UpgradeToCurrentVersion();
                return true;
            }
            catch (Exception e)
            {
                Utility.DisplayLocalizedText("str_load_config_failed");
                Utility.DisplayLocalizedText("str_exception_caught");
                Utility.DisplayMessage(e.ToString());
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


        protected override void CopyFrom(EnhancedFreeBattleConfig other)
        {
            base.CopyFrom(other);

            if (other.sceneList != null)
                this.sceneList = other.sceneList;
            this.sceneIndex = other.sceneIndex;
            this.soldierXInterval = other.soldierXInterval;
            this.soldierYInterval = other.soldierYInterval;
            this.makeGruntVoice = other.makeGruntVoice;
            this.hasBoundary = other.hasBoundary;
        }
        protected override string SaveName => SavePath + nameof(EnhancedFreeBattleConfig) +".xml";
        protected override string[] OldNames { get; } = { SavePath + "Param.xml", SavePath + "EnhancedTestBattleConfig.xml" };
    }
}