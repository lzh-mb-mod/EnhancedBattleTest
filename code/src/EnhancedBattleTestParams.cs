
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Modbed
{
    public class EnhancedBattleTestParams
    {
        public string scene;
        public int playerSoldierCount, enemySoldierCount;
        public float distance;
        public float soldierXInterval, soldierYInterval;
        public int soldiersPerRow;
        public Vec2 formationPosition;
        public string FormationPositionString
        {
            get => $"{this.formationPosition.x},{this.formationPosition.y}";
            set
            {
                var posParts = value.Split(',');
                formationPosition = new Vec2(System.Convert.ToSingle(posParts[0]), System.Convert.ToSingle(posParts[1]));
            }
        }
        public Vec2 formationDirection;
        public string FormationDirectionString => $"{this.formationDirection.x},{this.formationDirection.y}";
        public float skyBrightness;
        public float rainDensity;
        public string playerHeroClassStringId;
        public int playerSelectedPerk;
        public string playerTroopHeroClassStringId;
        public int playerTroopSelectedPerk;
        public string enemyTroopHeroClassStringId;
        public int enemyTroopSelectedPerk;
        public bool useFreeCamera;
        private static EnhancedBattleTestParams _instance;

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

        private static EnhancedBattleTestParams CreateDefault() {
            var p = new EnhancedBattleTestParams();
            p.scene = "mp_skirmish_map_001a";
            p.playerSoldierCount = 20;
            p.enemySoldierCount = 20;
            // p.playerSoldierCount = 50;
            // p.enemySoldierCount = 1;
            p.distance = 50;
            p.soldierXInterval = 5f;
            p.soldierYInterval = 3f;
            p.soldiersPerRow = 100;
            // p.soldiersPerRow = 10;
            p.formationPosition = new Vec2(250, 500);
            p.formationDirection = new Vec2(1, 0);
            p.skyBrightness = -1;
            p.rainDensity = 0;
            p.PlayerHeroClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("mp_light_cavalry_vlandia");
            p.playerSelectedPerk = 0;
            p.PlayerTroopHeroClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("mp_heavy_infantry_vlandia");
            p.playerTroopSelectedPerk = 0;
            p.EnemyTroopHeroClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("mp_shock_infantry_vlandia");
            p.enemyTroopSelectedPerk = 0;
            p.useFreeCamera = false;
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
                scene = (string)this.scene.Clone(),
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