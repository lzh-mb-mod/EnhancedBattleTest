
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Modbed
{
    public class EnhancedBattleTestParams
    {
        public class SceneInfo
        {
            public string name;
            public Vec2 defaultPosition;
            public Vec2 defaultDirection;
            public float defaultBrightness = -1;
            public int defaultSoldiersPerRow = 20;
        }
        public static SceneInfo[] SceneList { get; private set; }
        private static EnhancedBattleTestParams _instance;

        public string SceneName => SceneList[sceneIndex].name;

        public int sceneIndex;
        public int playerSoldierCount, enemySoldierCount;
        public float distance;
        public float soldierXInterval, soldierYInterval;
        public int soldiersPerRow;
        public Vec2 formationPosition;
        public Vec2 formationDirection;
        public float skyBrightness;
        public float rainDensity;
        public string playerHeroClassStringId;
        public int playerSelectedPerk;
        public string playerTroopHeroClassStringId;
        public int playerTroopSelectedPerk;
        public string enemyTroopHeroClassStringId;
        public int enemyTroopSelectedPerk;
        public bool useFreeCamera;

        public MultiplayerClassDivisions.MPHeroClass PlayerHeroClass
        {
            get => MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(playerHeroClassStringId);
            set => playerHeroClassStringId = value.StringId;
        }
        public MultiplayerClassDivisions.MPHeroClass PlayerTroopHeroClass
        {
            get => MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(playerTroopHeroClassStringId);
            set => playerTroopHeroClassStringId = value.StringId;
        }
        public MultiplayerClassDivisions.MPHeroClass EnemyTroopHeroClass
        {
            get => MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(enemyTroopHeroClassStringId);
            set => enemyTroopHeroClassStringId = value.StringId;
        }

        private static EnhancedBattleTestParams CreateDefault()
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
            SceneList = new[]
            {
                //"mp_skirmish_map_001a",
                //"mp_tdm_map_001",
                //"mp_duel_001",
                //"mp_duel_001_winter",
                //"mp_duel_002",
                //"mp_ruins_2",
                new SceneInfo{name = "mp_skirmish_map_001a", defaultPosition = new Vec2(100, 100), defaultDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_tdm_map_001", defaultPosition = new Vec2(385, 570), defaultDirection = new Vec2(0.7f, -0.3f).Normalized()},
                new SceneInfo{name = "mp_duel_001", defaultPosition = new Vec2(567, 600), defaultDirection = new Vec2(0, 10)},
                new SceneInfo{name = "mp_duel_001_winter", defaultPosition = new Vec2(567, 600), defaultDirection = new Vec2(0, 1)},
                new SceneInfo{name = "mp_ruins_2", defaultPosition = new Vec2(514, 470), defaultDirection = new Vec2(1, 0)},
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
                new SceneInfo{name = "mp_sergeant_map_001", defaultPosition = new Vec2(250, 500), defaultDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_sergeant_map_005", defaultPosition = new Vec2(330, 439), defaultDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_sergeant_map_007", defaultPosition = new Vec2(330, 439), defaultDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_sergeant_map_008", defaultPosition = new Vec2(471,505), defaultDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_sergeant_map_009", defaultPosition = new Vec2(530,503), defaultDirection = new Vec2(0,1)},
                //new SceneInfo{name = "mp_sergeant_map_010", defaultPosition = new Vec2(391,376), defaultDirection = new Vec2(0,1)},
                new SceneInfo{name = "mp_sergeant_map_011", defaultPosition = new Vec2(485,364), defaultDirection = new Vec2(0.4f,0.6f)},
                new SceneInfo{name = "mp_sergeant_map_011s", defaultPosition = new Vec2(485,364), defaultDirection = new Vec2(0.4f,0.6f)},
                //new SceneInfo{name = "mp_sergeant_map_012", defaultPosition = new Vec2(580,576), defaultDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_sergeant_map_013", defaultPosition = new Vec2(580,576), defaultDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_sergeant_map_vlandia_01", defaultPosition = new Vec2(485,364), defaultDirection = new Vec2(0.4f,0.6f), defaultBrightness = 500},
                //new SceneInfo{name = "mp_siege_map_001", defaultPosition = new Vec2(100, 100), defaultDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_siege_map_002", defaultPosition = new Vec2(100, 100), defaultDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_siege_map_003", defaultPosition = new Vec2(100, 100), defaultDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_siege_map_004", defaultPosition = new Vec2(100, 100), defaultDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_siege_map_005", defaultPosition = new Vec2(100, 100), defaultDirection = new Vec2(1, 0)},
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
                new SceneInfo{name = "mp_skirmish_map_002f", defaultPosition = new Vec2(415,490), defaultDirection = new Vec2(0.3f, 0.7f).Normalized(), defaultSoldiersPerRow = 10},
                new SceneInfo{name = "mp_skirmish_map_002_winter", defaultPosition = new Vec2(415,490), defaultDirection = new Vec2(0.3f, 0.7f), defaultSoldiersPerRow = 10},
                new SceneInfo{name = "mp_skirmish_map_004", defaultPosition = new Vec2(320,288), defaultDirection = new Vec2(0,1), defaultSoldiersPerRow = 10},
                new SceneInfo{name = "mp_skirmish_map_005", defaultPosition = new Vec2(477,496), defaultDirection = new Vec2(1, 0), defaultSoldiersPerRow = 10},
                new SceneInfo{name = "mp_skirmish_map_006", defaultPosition = new Vec2(480,561), defaultDirection = new Vec2(1, 0), defaultBrightness = 1},
                new SceneInfo{name = "mp_skirmish_map_007", defaultPosition = new Vec2(192,185), defaultDirection = new Vec2(0, 1)},
                new SceneInfo{name = "mp_skirmish_map_007_winter", defaultPosition = new Vec2(192,185), defaultDirection = new Vec2(0, 1)},
                //new SceneInfo{name = "mp_skirmish_map_008", defaultPosition = new Vec2(100, 100), defaultDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_skirmish_map_009", defaultPosition = new Vec2(100, 100), defaultDirection = new Vec2(1, 0)},
                //new SceneInfo{name = "mp_skirmish_map_010", defaultPosition = new Vec2(100, 100), defaultDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_skirmish_map_013", defaultPosition = new Vec2(250, 500), defaultDirection = new Vec2(1, 0)},
                new SceneInfo{name = "mp_skirmish_map_battania_02", defaultPosition = new Vec2(360,186), defaultDirection = new Vec2(0.6f,-0.4f), defaultSoldiersPerRow = 10},
                new SceneInfo{name = "mp_skirmish_map_battania_03", defaultPosition = new Vec2(360,186), defaultDirection = new Vec2(0.6f,-0.4f), defaultSoldiersPerRow = 10},
            };
            int index = SceneList.FindIndex(sceneInfo => sceneInfo.name == "mp_skirmish_map_001a");
            var p = new EnhancedBattleTestParams
            {
                sceneIndex = index,
                playerSoldierCount = 20,
                enemySoldierCount = 20,
                distance = 50,
                soldierXInterval = 1.5f,
                soldierYInterval = 1f,
                soldiersPerRow = SceneList[index].defaultSoldiersPerRow,
                formationPosition = SceneList[index].defaultPosition,
                formationDirection = SceneList[index].defaultDirection,
                skyBrightness = SceneList[index].defaultBrightness,
                rainDensity = 0,
                PlayerHeroClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("mp_light_cavalry_vlandia"),
                playerSelectedPerk = 0,
                PlayerTroopHeroClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("mp_heavy_infantry_vlandia"),
                playerTroopSelectedPerk = 0,
                EnemyTroopHeroClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("mp_shock_infantry_vlandia"),
                enemyTroopSelectedPerk = 0,
                useFreeCamera = false
            };
            return p;
        }

        public static EnhancedBattleTestParams Get()
        {
            if (_instance == null)
                _instance = CreateDefault();
            return _instance;
        }

        public bool validate() {
            return this.playerSoldierCount >= 0
                && this.enemySoldierCount >= 0
                && this.distance > 0
                && soldierXInterval > 0
                && soldierYInterval > 0
                && soldiersPerRow > 0
                && formationDirection.Length > 0
                && PlayerHeroClass != null
                && PlayerTroopHeroClass != null
                && EnemyTroopHeroClass != null
            ;
        }

        public EnhancedBattleTestParams Copy()
        {
            return new EnhancedBattleTestParams
            {
                sceneIndex = this.sceneIndex,
                playerSoldierCount = this.playerSoldierCount,
                enemySoldierCount = this.enemySoldierCount,
                distance = this.distance,
                soldierXInterval = this.soldierXInterval,
                soldierYInterval = this.soldierYInterval,
                soldiersPerRow = this.soldiersPerRow,
                formationPosition = this.formationPosition,
                formationDirection = this.formationDirection,
                skyBrightness = this.skyBrightness,
                rainDensity = this.rainDensity,
                PlayerHeroClass = this.PlayerHeroClass,
                playerSelectedPerk = this.playerSelectedPerk,
                PlayerTroopHeroClass = this.PlayerTroopHeroClass,
                playerTroopSelectedPerk = this.playerTroopSelectedPerk,
                EnemyTroopHeroClass = this.EnemyTroopHeroClass,
                enemyTroopSelectedPerk = this.enemyTroopSelectedPerk,
                useFreeCamera = this.useFreeCamera,
            };
        }
    }
}