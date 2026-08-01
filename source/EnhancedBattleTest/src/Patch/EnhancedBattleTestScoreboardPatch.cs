using EnhancedBattleTest.Data;
using HarmonyLib;
using SandBox.ViewModelCollection;
using System;
using System.Reflection;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;

namespace EnhancedBattleTest.Patch
{
    [HarmonyPatch(
        typeof(SPScoreboardVM),
        nameof(SPScoreboardVM.ExecuteQuitAction))]
    public static class EnhancedBattleTestScoreboardPatch
    {
        private static readonly MethodInfo OnToggleMethod =
            AccessTools.Method(
                typeof(ScoreboardBaseVM),
                "OnToggle",
                new[] { typeof(bool) });

        private static bool Prefix(SPScoreboardVM __instance)
        {
            if (EnhancedBattleTestPartyController.Current == null)
                return true;

            Mission mission = Mission.Current;
            BasicMissionHandler basicMissionHandler =
                mission?.GetMissionBehavior<BasicMissionHandler>();
            if (mission == null || basicMissionHandler == null)
                return true;

            BattleEndLogic battleEndLogic =
                mission.GetMissionBehavior<BattleEndLogic>();
            BattleEndLogic.ExitResult result =
                battleEndLogic?.TryExit()
                ?? (mission.MissionEnded
                    ? BattleEndLogic.ExitResult.True
                    : BattleEndLogic.ExitResult.NeedsPlayerConfirmation);

            if (result == BattleEndLogic.ExitResult.False)
                result = BattleEndLogic.ExitResult.NeedsPlayerConfirmation;

            if (result == BattleEndLogic.ExitResult.NeedsPlayerConfirmation
                || result == BattleEndLogic.ExitResult.SurrenderSiege)
            {
                OnToggleMethod.Invoke(__instance, new object[] { false });
                basicMissionHandler.CreateWarningWidgetForResult(result);
            }
            else if (battleEndLogic == null
                     && result == BattleEndLogic.ExitResult.True)
            {
                mission.EndMission();
            }

            return false;
        }
    }
}
