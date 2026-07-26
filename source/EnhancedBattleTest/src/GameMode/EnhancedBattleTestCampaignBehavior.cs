using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace EnhancedBattleTest.GameMode
{
    public sealed class EnhancedBattleTestCampaignBehavior : CampaignBehaviorBase
    {
        public const string MenuId = "enhanced_battle_test_menu";
        private const string CampMenuId = "camp_menu";

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        public static bool CanOpen()
        {
            return Campaign.Current != null
                   && GameStateManager.Current.ActiveState is MapState mapState
                   && !mapState.AtMenu
                   && MobileParty.MainParty != null
                   && MobileParty.MainParty.MapEvent == null
                   && MobileParty.MainParty.SiegeEvent == null
                   && MobileParty.MainParty.CurrentSettlement == null;
        }

        public static string GetMainPartyMenuId()
        {
            return Campaign.Current?.GameMenuManager.GetGameMenu(CampMenuId) != null
                ? CampMenuId
                : MenuId;
        }

        private static void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenu(
                MenuId,
                new TextObject("{=EnhancedBattleTest_campaign_menu_desc}Prepare a custom battle test.").ToString(),
                args =>
                {
                    string backgroundMesh =
                        MobileParty.MainParty?.MapFaction?.Culture?.EncounterBackgroundMesh;
                    args.MenuContext.SetBackgroundMeshName(
                        string.IsNullOrEmpty(backgroundMesh)
                            ? "wait_fallback"
                            : backgroundMesh);
                    MobileParty.MainParty?.SetMoveModeHold();
                });

            AddConfigureBattleOption(campaignGameStarter, MenuId);
            if (Campaign.Current.GameMenuManager.GetGameMenu(CampMenuId) != null)
                AddConfigureBattleOption(campaignGameStarter, CampMenuId);

            campaignGameStarter.AddGameMenuOption(
                MenuId,
                "enhanced_battle_test_leave",
                new TextObject("{=3sRdGQou}Leave").ToString(),
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                args => GameMenu.ExitToLast(),
                true);
        }

        private static void AddConfigureBattleOption(
            CampaignGameStarter campaignGameStarter,
            string menuId)
        {
            campaignGameStarter.AddGameMenuOption(
                menuId,
                "enhanced_battle_test_start",
                new TextObject("{=EnhancedBattleTest_campaign_menu_start}Configure Battle").ToString(),
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return true;
                },
                args => EnhancedBattleTestSubModule.OpenBattleTest());
        }
    }
}
