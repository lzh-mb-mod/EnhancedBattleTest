using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EnhancedBattleTest.Patch;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screens;

namespace EnhancedBattleTest.src.Patch.Fix
{
    public class Patch_MissionScreen
    {
        private static readonly Harmony _harmony = new Harmony(EnhancedBattleTestSubModule.ModuleId + nameof(Patch_MissionScreen));
        public static bool Patch()
        {
            try
            {
                var missionListenerOnMissionModeChange = typeof(IMissionListener).GetMethod("OnMissionModeChange", BindingFlags.Instance | BindingFlags.Public);

                var mapping = typeof(MissionScreen).GetInterfaceMap(missionListenerOnMissionModeChange.DeclaringType);
                var index = Array.IndexOf(mapping.InterfaceMethods, missionListenerOnMissionModeChange);
                _harmony.Patch(
                    mapping.TargetMethods[index],
                    prefix: new HarmonyMethod(typeof(Patch_MissionScreen).GetMethod(nameof(Prefix_OnMissionModeChange),
                        BindingFlags.Static | BindingFlags.Public), Priority.LowerThanNormal));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool Prefix_OnMissionModeChange(MissionScreen __instance, MissionMode oldMissionMode, bool atStart)
        {
            if (__instance.Mission.Mode == MissionMode.Battle && oldMissionMode == MissionMode.Deployment)
            {
                if (__instance.Mission.MainAgent == null)
                    return false;
            }

            return true;
        }
    }
}
