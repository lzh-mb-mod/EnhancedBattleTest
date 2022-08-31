using EnhancedBattleTest.GameMode;
using HarmonyLib;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;

namespace EnhancedBattleTest.Patch
{
    public class Patch_MapScreen
    {
        private static readonly Harmony _harmony = new Harmony(EnhancedBattleTestSubModule.ModuleId + nameof(Patch_MapScreen));
        public static bool Patch()
        {
            try
            {
                _harmony.Patch(
                    typeof(MapScreen).GetMethod("GetEscapeMenuItems", BindingFlags.Instance | BindingFlags.NonPublic),
                    postfix: new HarmonyMethod(typeof(Patch_MapScreen).GetMethod(nameof(Postfix_GetEscapeMenuItems),
                        BindingFlags.Static | BindingFlags.Public)));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        public static void Postfix_GetEscapeMenuItems(MapScreen __instance, List<EscapeMenuItemVM> __result)
        {
            __result.Add(new EscapeMenuItemVM(GameTexts.FindText("str_ebt_singleplayer_battle_option"), 
                o =>
                {
                    __instance.CloseEscapeMenu();
                    if (PlayerEncounter.Current != null)
                    {
                        PlayerEncounter.Finish();
                        typeof(MapScreen).GetMethod("ExitMenuContext", BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(__instance, new object[] { });
                    }

                    Game.Current.GameStateManager.PushState(Game.Current.GameStateManager.CreateState<EnhancedBattleTestState>());
                }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty)));
        }
    }
}
