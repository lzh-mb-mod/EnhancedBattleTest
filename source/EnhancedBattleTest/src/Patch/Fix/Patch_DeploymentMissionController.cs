using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;

namespace EnhancedBattleTest.Patch.Fix
{
    public class Patch_DeploymentMissionController
    {
        private static readonly Harmony _harmony = new Harmony(EnhancedBattleTestSubModule.ModuleId + nameof(Patch_DeploymentMissionController));
        public static bool Patch()
        {
            try
            {
                _harmony.Patch(
                    typeof(DeploymentMissionController).GetMethod(
                        nameof(DeploymentMissionController.FinishPlayerDeployment),
                        BindingFlags.Instance | BindingFlags.Public),
                    prefix: new HarmonyMethod(typeof(Patch_DeploymentMissionController).GetMethod(
                        nameof(Prefix_FinishPlayerDeployment),
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
        // OrderOfBattleFormationItemVM battleFormationItemVm = __instance._allFormations.FirstOrDefault<OrderOfBattleFormationItemVM>((Func<OrderOfBattleFormationItemVM, bool>) (f => f.Commander.Agent == Agent.Main));
        // if (battleFormationItemVm != null)
        //    __instance._mission.GetMissionBehavior<AssignPlayerRoleInTeamMissionController>().OnPlayerChoiceMade(battleFormationItemVm.Formation.Index, true);
        // The issue is that the Agent.Main is null and the commander of the first formation is also null.
        // Then the null agent will be assigned as sergeant of the first formation.
        public static bool Prefix_FinishPlayerDeployment(DeploymentMissionController __instance, BattleDeploymentHandler ____battleDeploymentHandler, bool ____isPlayerAttacker, MissionAgentSpawnLogic ___MissionAgentSpawnLogic)
        {
            if (__instance.Mission.MainAgent == null)
            {
                __instance.OnBeforePlayerDeploymentFinished();
                if (____isPlayerAttacker)
                {
                    foreach (Agent agent in __instance.Mission.Agents)
                    {
                        if (agent.IsHuman && agent.Team is { Side: BattleSideEnum.Defender })
                        {
                            agent.SetRenderCheckEnabled(true);
                            agent.AgentVisuals.SetVisible(true);
                            agent.MountAgent?.SetRenderCheckEnabled(true);
                            agent.MountAgent?.AgentVisuals.SetVisible(true);
                        }
                    }
                }
                typeof(DeploymentMissionController)
                    .GetMethod("InvokePlayerDeploymentFinish", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(__instance, new object[] { });
                //__instance.InvokePlayerDeploymentFinish();
                Mission.Current.IsTeleportingAgents = false;
                foreach (Agent agent in __instance.Mission.Agents)
                {
                    if (agent.IsAIControlled)
                    {
                        agent.AIStateFlags |= Agent.AIStateFlag.Alarmed;
                        agent.SetIsAIPaused(false);
                        if (agent.GetAgentFlags().HasAnyFlag(AgentFlag.CanWieldWeapon))
                            agent.ResetEnemyCaches();
                        agent.HumanAIComponent?.SyncBehaviorParamsIfNecessary();
                    }
                }

                // main agent is null
                //Agent mainAgent = __instance.Mission.MainAgent;
                //mainAgent.PromoteToGeneral();
                //mainAgent.SetDetachableFromFormation(true);
                //mainAgent.Controller = Agent.ControllerType.Player;
                Mission.Current.AllowAiTicking = true;
                __instance.Mission.DisableDying = false;
                ___MissionAgentSpawnLogic.SetReinforcementsSpawnTimerEnabled(true);
                __instance.Mission.RemoveMissionBehavior(__instance);
                return false;
            }
            return true;
        }
    }
}
