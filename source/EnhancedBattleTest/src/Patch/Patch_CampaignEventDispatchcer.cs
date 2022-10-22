using System;
using System.Collections.Generic;
using System.Reflection;
using EnhancedBattleTest.GameMode;
using EnhancedBattleTest.SinglePlayer;
using HarmonyLib;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;

namespace EnhancedBattleTest.Patch
{
    public class Patch_CampaignEventDispatcher
    {
        private static readonly Harmony _harmony = new Harmony(EnhancedBattleTestSubModule.ModuleId + nameof(Patch_CampaignEventDispatcher));
        public static bool Patch()
        {
            try
            {
                _harmony.Patch(
                    typeof(CampaignEventDispatcher).GetMethod("OnSiegeEventStarted", BindingFlags.Instance | BindingFlags.Public),
                    prefix: new HarmonyMethod(typeof(Patch_CampaignEventDispatcher).GetMethod(nameof(Prefix_OnSiegeEventStarted),
                        BindingFlags.Static | BindingFlags.Public)));
                _harmony.Patch(
                    typeof(CampaignEventDispatcher).GetMethod("SiegeCompleted", BindingFlags.Instance | BindingFlags.Public),
                    prefix: new HarmonyMethod(typeof(Patch_CampaignEventDispatcher).GetMethod(nameof(Prefix_SiegeCompleted),
                        BindingFlags.Static | BindingFlags.Public)));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        public static bool Prefix_OnSiegeEventStarted()
        {
            if (BattleStarter.IsEnhancedBattleTestBattle)
                return false;
            return true;
        }

        public static bool Prefix_SiegeCompleted()
        {
            if (BattleStarter.IsEnhancedBattleTestBattle)
                return false;
            return true;
        }
    }
}
