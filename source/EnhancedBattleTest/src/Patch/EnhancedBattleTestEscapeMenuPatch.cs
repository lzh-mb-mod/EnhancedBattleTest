using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;

namespace EnhancedBattleTest.Patch
{
    public static class EnhancedBattleTestEscapeMenuPatch
    {
        private static readonly PropertyInfo IsCurrentlyAtSeaProperty =
            AccessTools.Property(
                typeof(MobileParty),
                "IsCurrentlyAtSea");

        [HarmonyPatch(typeof(MapScreen), "GetEscapeMenuItems")]
        private static class MapScreenGetEscapeMenuItemsPatch
        {
            private static void Postfix(List<EscapeMenuItemVM> __result)
            {
                __result.Insert(
                    1,
                    new EscapeMenuItemVM(
                        GameTexts.FindText(
                            "str_ebt_enhanced_battle_test"),
                        _ =>
                        {
                            MapScreen.Instance.CloseEscapeMenu();
                            EnhancedBattleTestSubModule.OpenBattleTest();
                        },
                        null,
                        GetDisabledReason));
            }

            private static Tuple<bool, TextObject> GetDisabledReason()
            {
                TextObject reason = GetUnavailableReason();
                return Tuple.Create(reason != null, reason ?? TextObject.Empty);
            }

            private static TextObject GetUnavailableReason()
            {
                if (Campaign.Current == null
                    || !(GameStateManager.Current.ActiveState is MapState)
                    || MobileParty.MainParty == null)
                {
                    return GameTexts.FindText(
                        "str_ebt_escape_menu_campaign_only");
                }

                if (Hero.MainHero?.IsPrisoner == true)
                {
                    return GameTexts.FindText(
                        "str_ebt_escape_menu_captive");
                }

                MobileParty mainParty = MobileParty.MainParty;
                if (IsCurrentlyAtSeaProperty?.GetValue(mainParty)
                    is bool isAtSea
                    && isAtSea)
                {
                    return GameTexts.FindText(
                        "str_ebt_escape_menu_at_sea");
                }

                Settlement settlement = mainParty.CurrentSettlement;
                if (mainParty.MapEvent != null
                    || mainParty.SiegeEvent != null
                    || PlayerSiege.PlayerSiegeEvent != null
                    || settlement?.IsUnderSiege == true
                    || settlement?.IsUnderRaid == true)
                {
                    return GameTexts.FindText(
                        "str_ebt_escape_menu_party_busy");
                }

                return null;
            }
        }
    }
}
