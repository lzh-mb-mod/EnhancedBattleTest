using EnhancedBattleTest.GameMode;
using HarmonyLib;
using SandBox;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using System.Reflection;
using EnhancedBattleTest.SinglePlayer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;

namespace EnhancedBattleTest.Patch
{
    /* 
     * Patch SandboxMissions.CreateSandBoxMissionInitializerRecord 
     * to returns a MissionInitializerRecord
     * with the weather/time wanted
     */
    public class Patch_Initializer
    {
        private static readonly Harmony _harmony = new Harmony(EnhancedBattleTestSubModule.ModuleId + nameof(Patch_Initializer));
        static AtmosphereInfo Atmosphere;
        static float? TimeOfDay;

        public static bool Patch()
        {
            try
            {
                _harmony.Patch(
                    typeof(SandBoxMissions).GetMethod(nameof(SandBoxMissions.CreateSandBoxMissionInitializerRecord), BindingFlags.Static | BindingFlags.Public),
                    postfix: new HarmonyMethod(typeof(Patch_Initializer).GetMethod(nameof(Postfix_CreateSandBoxMissionInitializerRecord),
                        BindingFlags.Static | BindingFlags.Public)));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static void Init(AtmosphereInfo atmosphere, float timeOfDay)
        {
            Atmosphere = atmosphere;
            TimeOfDay = timeOfDay;
        }

        public static void Reset()
        {
            Atmosphere = null;
            TimeOfDay = null;
        }

        public static void Postfix_CreateSandBoxMissionInitializerRecord(ref MissionInitializerRecord __result)
        {
            if (BattleStarter.IsEnhancedBattleTestBattle && Atmosphere != null && TimeOfDay != null)
            {
                __result.AtmosphereOnCampaign = Atmosphere;
                __result.TimeOfDay = (float)TimeOfDay;
            }
        }
    }
}
