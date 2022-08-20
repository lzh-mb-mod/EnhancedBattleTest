using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnhancedBattleTest.SinglePlayer;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;

namespace EnhancedBattleTest.Patch.Fix
{
    public class Patch_AssignPlayerRoleInTeamMissionController
    {
        private static readonly Harmony _harmony = new Harmony(EnhancedBattleTestSubModule.ModuleId + nameof(Patch_AssignPlayerRoleInTeamMissionController));
        public static bool Patch()
        {
            try
            {
                _harmony.Patch(
                    typeof(AssignPlayerRoleInTeamMissionController).GetMethod(
                        nameof(AssignPlayerRoleInTeamMissionController.OnPlayerChoiceMade),
                        BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(int), typeof(bool) },
                        null),
                    prefix: new HarmonyMethod(typeof(Patch_AssignPlayerRoleInTeamMissionController).GetMethod(
                        nameof(Prefix_OnPlayerChoiceMade),
                        BindingFlags.Static | BindingFlags.Public)));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        // Fix the issue that when no player character is spawned, the player choice will be automatically made in OrderOfBattleVM.
        // See the following code in OrderOfBattleVM.FinalizeDeployment():
        // OrderOfBattleFormationItemVM battleFormationItemVm = this._allFormations.FirstOrDefault<OrderOfBattleFormationItemVM>((Func<OrderOfBattleFormationItemVM, bool>) (f => f.Commander.Agent == Agent.Main));
        // if (battleFormationItemVm != null)
        //    this._mission.GetMissionBehavior<AssignPlayerRoleInTeamMissionController>().OnPlayerChoiceMade(battleFormationItemVm.Formation.Index, true);
        // The issue is that the Agent.Main is null and the commander of the first formation is also null.
        // Then the null agent will be assigned as sergeant of the first formation.
        public static bool Prefix_OnPlayerChoiceMade(AssignPlayerRoleInTeamMissionController __instance)
        {
            if (__instance.Mission.MainAgent == null)
            {
                return false;
            }
            return true;
        }
    }
}
