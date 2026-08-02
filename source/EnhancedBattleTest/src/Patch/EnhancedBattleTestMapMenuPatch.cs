using HarmonyLib;
using SandBox.View.Map;
using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.GameMenus;

namespace EnhancedBattleTest.Patch
{
    public static class EnhancedBattleTestMapMenuPatch
    {
        [HarmonyPatch(typeof(MapScreen), "HandleLeftMouseButtonClick")]
        private static class MapScreenHandleLeftMouseButtonClickPatch
        {
            private static void Prefix(MapScreen __instance, out bool __state)
            {
                var partyVisual =
                    __instance.CurrentVisualOfTooltip as MapEntityVisual<PartyBase>;
                __state =
                    GameMode.EnhancedBattleTestCampaignBehavior.CanOpen()
                    && partyVisual?.MapEntity == PartyBase.MainParty;
            }

            [HarmonyPriority(Priority.Last)]
            private static void Postfix(bool __state)
            {
                if (!__state)
                    return;

                GameMenu.ActivateGameMenu(
                    GameMode.EnhancedBattleTestCampaignBehavior.MenuId);
            }
        }
    }
}
