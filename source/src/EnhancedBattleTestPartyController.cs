using TaleWorlds.CampaignSystem;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestPartyController
    {
        public static BattleConfig BattleConfig;

        public static MobileParty PlayerParty;
        public static MobileParty EnemyParty;


        public static void Initialize()
        {
            PlayerParty = MobileParty.Create("enhanced_battle_test_player_party");
            EnemyParty = MobileParty.Create("enemy_battle_test_enemy_party");
        }

        public static void OnGameEnd()
        {
            PlayerParty.RemoveParty();
            PlayerParty = null;
            EnemyParty.RemoveParty();
            EnemyParty = null;
        }
    }
}
