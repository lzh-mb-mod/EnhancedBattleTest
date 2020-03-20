using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace EnhancedBattleTest
{
    public class EnhancedSiegeBattleConfig : BattleConfigBase<EnhancedSiegeBattleConfig>
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

        private static EnhancedSiegeBattleConfig _instance;

        public SceneInfo[] sceneList;
        public int sceneIndex;
        public float soldierXInterval, soldierYInterval;

        public bool charge;

        public bool hasBoundary;

        protected static Version BinaryVersion => new Version(1, 9);

        protected void UpgradeToCurrentVersion()
        {
            switch (ConfigVersion?.ToString())
            {
                default:
                    Utility.DisplayMessage("Config version not compatible.\nReset config.");
                    ResetToDefault();
                    Serialize();
                    break;
                case "1.9":
                    break;
            }
        }

        [XmlIgnore]
        public int SoldiersPerRow
        {
            get => sceneList[sceneIndex].soldiersPerRow;
            set => sceneList[sceneIndex].soldiersPerRow = value;
        }

        [XmlIgnore]
        public Vec2 FormationPosition
        {
            get => sceneList[sceneIndex].formationPosition;
            set => sceneList[sceneIndex].formationPosition = value;
        }

        [XmlIgnore]
        public Vec2 FormationDirection
        {
            get => sceneList[sceneIndex].formationDirection;
            set => sceneList[sceneIndex].formationDirection = value;
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
        public float Distance
        {
            get => sceneList[sceneIndex].distance;
            set => sceneList[sceneIndex].distance = value;
        }

        [XmlIgnore]
        public string SceneName => sceneList[sceneIndex].name;

        [XmlIgnore]
        public bool IsSiegeMission => SceneName.Contains("siege");

        public EnhancedSiegeBattleConfig()
            : base(BattleType.SiegeBattle)
        { }

        private static EnhancedSiegeBattleConfig CreateDefault()
        {
            SceneInfo[] list = new[]
            {
                //new SceneInfo{name = "mp_siege_map_001", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_siege_map_002", formationPosition = new Vec2(100, 100), formationDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_siege_map_003", formationPosition = new Vec2(461,634), formationDirection = new Vec2(0.55f,0.45f).Normalized(), distance = 180},
                //new SceneInfo{name = "mp_siege_map_004", formationPosition = new Vec2(502,472), formationDirection = new Vec2(0.23f,0.77f).Normalized(), distance = 200},
                //new SceneInfo{name = "mp_siege_map_004_bat", formationPosition = new Vec2(502,472), formationDirection = new Vec2(0.23f,0.77f).Normalized(), distance = 200},
                //new SceneInfo{name = "mp_siege_map_004_rs", formationPosition = new Vec2(470,413), formationDirection = new Vec2(0, 1)},
                new SceneInfo{name = "mp_siege_map_005", formationPosition = new Vec2(424, 320), formationDirection = new Vec2(0,1), distance = 220},
                //new SceneInfo{name = "mp_siege_map_006", formationPosition = new Vec2(424, 320), formationDirection = new Vec2(0,1), distance = 220},
                new SceneInfo{name = "mp_siege_map_007_battania", formationPosition = new Vec2(614,612), formationDirection = new Vec2(0.65f, 0.35f), distance = 140},
            };
            int defaultIndex = 0;
            var p = new EnhancedSiegeBattleConfig
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
                charge = false,
                hasBoundary = true,
                disableDying = false,
                changeCombatAI = false,
                combatAI = 100,
            };
            return p;
        }

        public static EnhancedSiegeBattleConfig Get()
        {
            if (_instance == null)
            {
                _instance = new EnhancedSiegeBattleConfig();
                _instance.SyncWithSave();
            }

            return _instance;
        }

        public override bool Validate()
        {
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
                XmlSerializer serializer = new XmlSerializer(typeof(EnhancedSiegeBattleConfig));
                using (TextWriter writer = new StreamWriter(SaveName))
                {
                    serializer.Serialize(writer, this);
                }
                Utility.DisplayMessage("Config saved.");
                return true;
            }
            catch (Exception e)
            {
                Utility.DisplayMessage("Error: Saving config failed.");
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
                XmlSerializer deserializer = new XmlSerializer(typeof(EnhancedSiegeBattleConfig));
                using (TextReader reader = new StreamReader(SaveName))
                {
                    var config = (EnhancedSiegeBattleConfig)deserializer.Deserialize(reader);
                    this.CopyFrom(config);
                }
                Utility.DisplayMessage("Config loaded.");
                UpgradeToCurrentVersion();
                return true;
            }
            catch (Exception e)
            {
                Utility.DisplayMessage("Error: Loading config failed.");
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


        protected override void CopyFrom(EnhancedSiegeBattleConfig other)
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
            this.soldierXInterval = other.soldierXInterval;
            this.soldierYInterval = other.soldierYInterval;
            this.charge = other.charge;
            this.hasBoundary = other.hasBoundary;
        }
        protected override string SaveName => SavePath + nameof(EnhancedSiegeBattleConfig) + ".xml";
        protected override string[] OldNames { get; } = {};
    }
}
