using EnhancedBattleTest.Data.MissionData.Logic;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

namespace EnhancedBattleTest.Patch
{
    public static class EnhancedBattleTestNoPlayerDeploymentPatch
    {
        [HarmonyPatch(
            typeof(GeneralsAndCaptainsAssignmentLogic),
            nameof(GeneralsAndCaptainsAssignmentLogic.OnDeploymentFinished))]
        private static class GeneralsAndCaptainsAssignmentLogicPatch
        {
            private static bool Prefix()
            {
                return !IsNoPlayerDeployment();
            }
        }

        [HarmonyPatch(
            typeof(MissionGauntletOrderOfBattleUIHandler),
            nameof(MissionGauntletOrderOfBattleUIHandler.OnDeploymentFinished))]
        private static class MissionGauntletOrderOfBattleUIHandlerPatch
        {
            private static bool Prefix()
            {
                return !IsNoPlayerDeployment();
            }
        }

        private static bool IsNoPlayerDeployment()
        {
            return Mission.Current?
                       .GetMissionBehavior<
                           EnhancedBattleTestNoPlayerDeploymentLogic>()
                   != null;
        }
    }
}
