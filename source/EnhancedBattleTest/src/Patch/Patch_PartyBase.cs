using System;
using System.Reflection;
using EnhancedBattleTest.SinglePlayer;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;

namespace EnhancedBattleTest.Patch
{
    public class Patch_PartyBase
    {
        private static readonly Harmony _harmony = new Harmony(EnhancedBattleTestSubModule.ModuleId + nameof(Patch_PartyBase));
        public static bool Patch()
        {
            try
            {
                var getTacticsSkillAmount = typeof(IBattleCombatant).GetMethod(
                    nameof(IBattleCombatant.GetTacticsSkillAmount), BindingFlags.Instance | BindingFlags.Public);
                var mapping = typeof(PartyBase).GetInterfaceMap(getTacticsSkillAmount.DeclaringType);
                var index = Array.IndexOf(mapping.InterfaceMethods, getTacticsSkillAmount);
                _harmony.Patch(
                    mapping.TargetMethods[index],
                    prefix: new HarmonyMethod(typeof(Patch_PartyBase).GetMethod(nameof(Prefix_GetTacticsSkillAmount),
                        BindingFlags.Static | BindingFlags.Public)));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool Prefix_GetTacticsSkillAmount(PartyBase __instance, ref int __result)
        {
            if (BattleStarter.IsEnhancedBattleTestBattle)
            {
                if (__instance.IsMobile && __instance.MobileParty.PartyComponent is EnhancedBattleTestPartyComponent component)
                {
                    var tacticLevel = component.GetTacticLevel();
                    if (tacticLevel.HasValue)
                    {
                        __result = tacticLevel.Value;
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
