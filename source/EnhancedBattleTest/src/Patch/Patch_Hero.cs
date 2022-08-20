using System;
using System.Collections.Generic;
using System.Reflection;
using EnhancedBattleTest.GameMode;
using EnhancedBattleTest.SinglePlayer;
using HarmonyLib;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace EnhancedBattleTest.Patch
{
    public class Patch_Hero
    {
        private static readonly Harmony _harmony = new Harmony(EnhancedBattleTestSubModule.ModuleId + nameof(Patch_Hero));
        public static bool Patch()
        {
            try
            {
                _harmony.Patch(
                    typeof(Hero).GetMethod(nameof(Hero.AddSkillXp), BindingFlags.Instance | BindingFlags.Public),
                    prefix: new HarmonyMethod(typeof(Patch_Hero).GetMethod(nameof(Prefix_AddSkillXp),
                        BindingFlags.Static | BindingFlags.Public)));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        public static bool Prefix_AddSkillXp(ref float xpAmount)
        {
            if (Mission.Current != null && BattleStarter.IsEnhancedBattleTestBattle)
            {
                xpAmount = 0;
            }

            return true;
        }
    }
}
