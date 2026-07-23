using HarmonyLib;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.GameMenus;

namespace EnhancedBattleTest.Patch
{
    public static class EnhancedBattleTestMapMenuPatch
    {
        [HarmonyPatch(typeof(MapScreen), "HandleLeftMouseButtonClick")]
        private static class MapScreenHandleLeftMouseButtonClickPatch
        {
            private static void Postfix(MapScreen __instance)
            {
                if (!GameMode.EnhancedBattleTestCampaignBehavior.CanOpen())
                    return;

                if (__instance.CurrentVisualOfTooltip?.PartyBase != PartyBase.MainParty)
                    return;

                GameMenu.ActivateGameMenu(
                    GameMode.EnhancedBattleTestCampaignBehavior.GetMainPartyMenuId());
            }
        }
    }
}
