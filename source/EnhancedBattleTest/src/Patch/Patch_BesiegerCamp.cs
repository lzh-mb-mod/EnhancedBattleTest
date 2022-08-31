using System;
using System.Reflection;
using EnhancedBattleTest.SinglePlayer;
using HarmonyLib;
using SandBox;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest.Patch
{
    // skip setting position in siege because the temporarily created settlement doesn't have valid visual.
    public class Patch_BesiegerCamp
    {
        private static readonly Harmony _harmony = new Harmony(EnhancedBattleTestSubModule.ModuleId + nameof(Patch_BesiegerCamp));
        public static bool Patch()
        {
            try
            {
                _harmony.Patch(
                    typeof(BesiegerCamp).GetMethod("SetSiegeCampPartyPosition", BindingFlags.Instance | BindingFlags.NonPublic),
                    prefix: new HarmonyMethod(typeof(Patch_BesiegerCamp).GetMethod(nameof(Prefix_SetSiegeCampPartyPosition),
                        BindingFlags.Static | BindingFlags.Public)));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool Prefix_SetSiegeCampPartyPosition()
        {
            if (BattleStarter.IsEnhancedBattleTestBattle)
                return false;
            return true;
        }
    }
}
