using EnhancedBattleTest.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace EnhancedBattleTest.Data
{
    public class EnhancedBattleTestPartyController
    {
        public static BattleConfig BattleConfig;

        public static MobileParty PlayerParty;
        public static MobileParty EnemyParty;


        public static void Initialize()
        {
            PlayerParty = MBObjectManager.Instance.CreateObject<MobileParty>("enhanced_battle_test_player_party");
            PlayerParty.SetCustomName(new TextObject("{=sSJSTe5p}Player Party"));
            EnemyParty = MBObjectManager.Instance.CreateObject<MobileParty>("enhanced_battle_test_enemy_party");
            EnemyParty.SetCustomName(new TextObject("{=0xC75dN6}Enemy Party"));
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
