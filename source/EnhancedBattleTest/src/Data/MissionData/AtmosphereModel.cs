using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace EnhancedBattleTest.Data.MissionData
{
    public class AtmosphereModel
    {
        public struct SunPosition
        {
            public float Angle { get; private set; }

            public float Altitude { get; private set; }

            public SunPosition(float angle, float altitude)
                : this()
            {
                Angle = angle;
                Altitude = altitude;
            }
        }

        public struct SunInfo
        {
            public SunPosition SunPosition;
            public bool IsMoon;
        }

        public static AtmosphereInfo CreateAtmosphereInfoForMission(string seasonString = "", int timeOfDay = 6, bool isInSettlement = false)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            dictionary.Add("spring", 0);
            dictionary.Add("summer", 1);
            dictionary.Add("fall", 2);
            dictionary.Add("winter", 3);
            int season = 0;
            dictionary.TryGetValue(seasonString, out season);
            Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
            dictionary2.Add(6, "TOD_06_00_SemiCloudy");
            dictionary2.Add(12, "TOD_12_00_SemiCloudy");
            dictionary2.Add(15, "TOD_04_00_SemiCloudy");
            dictionary2.Add(18, "TOD_03_00_SemiCloudy");
            dictionary2.Add(22, "TOD_01_00_SemiCloudy");
            string atmosphereName = "field_battle";
            dictionary2.TryGetValue(timeOfDay, out atmosphereName);
            string atmosphereTypeName = isInSettlement ? "empire" : "field_battle";
            return new AtmosphereInfo()
            {
                AtmosphereTypeName = atmosphereTypeName,
                AtmosphereName = atmosphereName,
                TimeInfo = new TimeInformation
                {
                    Season = season,
                    TimeOfDay = timeOfDay
                }
            };
        }

        private static SunInfo GetSunInfo(
            float hourNorm,
            float seasonFactor)
        {
            float altitude;
            float angle;
            var isMoon = false;
            if (hourNorm >= 0.0833333358168602 && hourNorm < 0.916666686534882)
            {
                altitude = MBMath.Lerp(0.0f, 180f, (float) ((hourNorm - 0.0833333358168602) / 0.833333373069763));
                angle = 50f * seasonFactor;
            }
            else
            {
                isMoon = true;
                if (hourNorm >= 0.916666686534882)
                    --hourNorm;
                float num = (float) ((hourNorm - -0.0833333134651184) / 0.166666656732559);
                altitude = MBMath.Lerp(180f, 0.0f, (double) num < 0.0 ? 0.0f : ((double) num > 1.0 ? 1f : num));
                angle = 50f * seasonFactor;
            }

            return new SunInfo
            {
                SunPosition = new SunPosition(angle, altitude),
                IsMoon = isMoon
            };
        }

        private static float GetFogDensity(float environmentMultiplier, Vec3 pos, bool isMoon)
        {
            float val2 = 10f;
            return Math.Min((float)(0.5 + (double)(isMoon ? 0.5f : 0.4f) * (double)(1f - environmentMultiplier)) * (1f - MBMath.ClampFloat((float)(((double)pos.z - 30.0) / 200.0), 0.0f, 0.9f)), val2);
        }

        private static Vec3 GetFogColor(float environmentMultiplier, bool isMoon)
        {
            return isMoon
                ? Vec3.Vec3Max(
                    new Vec3((float) (1.0 - (double) environmentMultiplier * 10.0),
                        (float) (0.75 + (double) environmentMultiplier * 1.5),
                        (float) (0.649999976158142 + (double) environmentMultiplier * 2.0)),
                    new Vec3(0.55f, 0.59f, 0.6f))
                : new Vec3((float) (1.0 - (1.0 - (double) environmentMultiplier) / 7.0),
                    (float) (0.75 - (double) environmentMultiplier / 4.0),
                    (float) (0.550000011920929 - (double) environmentMultiplier / 5.0));
        }


        private static float GetMieScatterStrength(float envMultiplier)
        {
            return (float) ((1.0 + (1.0 - envMultiplier)) * 10.0);
        }

        private static float GetRayleighConstant(float envMultiplier)
        {
            float num = (float)(((double)envMultiplier - 1.0 / 1000.0) / 0.999000012874603);
            return Math.Min(Math.Max((float)(1.0 - Math.Sin(Math.Pow((double)num, 0.449999988079071) * Math.PI / 2.0) + (0.140000000596046 + (double)num * 2.0)), 0.65f), 0.99f);
        }


        private static Vec3 GetAmbientFogColor(float moddedEnvMul)
        {
            return Vec3.Vec3Min(
                new Vec3(0.15f, 0.3f, 0.5f) + new Vec3(moddedEnvMul / 3f, moddedEnvMul / 2f, moddedEnvMul / 1.5f),
                new Vec3(1f, 1f, 1f));
        }

        private static float GetNightTimeFactor(float timeOfDay)
        {
            float num = (timeOfDay - 2f) % 24f;
            while (num < 0.0)
                num += 24f;
            return Math.Min(Math.Max(num - 20f, 0.0f) / 0.1f, 1f);
        }
        private static float GetExposureCoefBetweenDayNight(float timeOfDay)
        {
            float hourOfDay = timeOfDay;
            float num = 0.0f;
            if (hourOfDay > 2.0 && hourOfDay < 4.0)
                num = (float)(1.0 - (hourOfDay - 2.0) / 2.0);
            if (hourOfDay < 22.0 && hourOfDay > 20.0)
                num = (float)((hourOfDay - 20.0) / 2.0);
            if (hourOfDay > 22.0 || hourOfDay < 2.0)
                num = 1f;
            return num;
        }

        private static float GetWinterTimeFactor(float seasonOfYear)
        {
            float num = 0.0f;
            if ((int)seasonOfYear == 3)
                num = MBMath.SplitLerp(0.0f, 0.35f, 0.0f, 0.5f, Math.Abs((float)Math.IEEERemainder(seasonOfYear, 1.0)), 1E-05f);
            return num;
        }

        private static float GetDrynessFactor(float seasonOfYear)
        {
            float num = 0.0f;
            float amount = Math.Abs((float)Math.IEEERemainder(seasonOfYear, 1.0));
            switch ((int)seasonOfYear)
            {
                case 1:
                    num = MBMath.Lerp(0.0f, 1f, MBMath.ClampFloat(amount * 2f, 0.0f, 1f));
                    break;
                case 2:
                    num = 1f;
                    break;
                case 3:
                    num = MBMath.Lerp(1f, 0.0f, amount);
                    break;
            }
            return num;
        }

        private static float GetSeasonTimeFactor(float seasonNum)
        {
            float num1 = seasonNum;
            float result = 0.0f;
            if (num1 > 1.5 && num1 < 3.5)
                result = MBMath.Lerp(0.0f, 1f, (float) ((num1 - 1.5) / 2.0));
            else if (num1 < 1.5)
                result = MBMath.Lerp(0.75f, 0.0f, num1 / 1.5f);
            else if (num1 > 3.5)
                result = MBMath.Lerp(1f, 0.75f, (float) ((num1 - 3.5) * 2.0));
            return result;
        }
        private static float GetTemperature(ref AtmosphereState gridInfo, float seasonFactor)
        {
            if (gridInfo == null)
                return 0.0f;
            double temperatureAverage = gridInfo.TemperatureAverage;
            float num1 = (float)((seasonFactor - 0.5) * -2.0);
            double num2 = gridInfo.TemperatureVariance * num1;
            return (float)(temperatureAverage + num2);
        }

        private static float GetHumidity(ref AtmosphereState gridInfo, float seasonFactor)
        {
            if (gridInfo == null)
                return 0.0f;
            double humidityAverage = gridInfo.HumidityAverage;
            float num1 = (float)((seasonFactor - 0.5) * 2.0);
            double num2 = gridInfo.HumidityVariance * num1;
            return MBMath.ClampFloat((float)(humidityAverage + num2), 0.0f, 100f);
        }

        private static float GetEnvironmentMultiplier(SunPosition sunPos, float seasonFactor, bool isMoon)
        {
            float num = (float)(sunPos.Altitude / 180.0 * 2.0);
            return (float) (MBMath.ClampFloat(
                    Math.Min(
                        (float) Math.Sin(Math.Pow(
                            MBMath.ClampFloat(
                                (float) Math.Pow((double) num > 1.0 ? 2.0 - num : num, 0.5) *
                                (float) (1.0 - 0.0111111113801599 * sunPos.Angle), 0.0f, 1f), 2.0)) * 2f, 1f), 0.0f,
                    1f) *
                0.999000012874603 + 1.0 / 1000.0);
        }
        private static float GetSkyBrightness(float hourNorm, float envMultiplier, bool isMoon)
        {
            float num = (float)(((double)envMultiplier - 1.0 / 1000.0) / 0.999000012874603);
            return isMoon ? 0.055f : Math.Min(Math.Max((float)Math.Sin(Math.Pow((double)num, 1.29999995231628) * (Math.PI / 2.0)) * 80f - 1f, 0.055f), 25f);
        }

        private static float GetModifiedEnvironmentMultiplier(float envMultiplier, bool isMoon)
        {
            return isMoon
                ? (float) ((envMultiplier - 1.0 / 1000.0) / 0.999000012874603 * 0.0 + 1.0 / 1000.0)
                : (float) ((envMultiplier - 1.0 / 1000.0) / 0.999000012874603 * 0.999000012874603 +
                           1.0 / 1000.0);
        }

        private static Vec3 GetSunColor(float environmentMultiplier, bool isMoon)
        {
            return isMoon
                ? Vec3.Vec3Max(
                    new Vec3(0.85f - (float) Math.Pow(environmentMultiplier, 0.400000005960464),
                        0.8f - (float) Math.Pow(environmentMultiplier, 0.5),
                        0.8f - (float) Math.Pow(environmentMultiplier, 0.800000011920929)),
                    new Vec3(0.05f, 0.05f, 0.1f))
                : new Vec3(1f,
                    (float) (1.0 - (1.0 - Math.Pow(environmentMultiplier, 0.300000011920929)) / 2.0),
                    (float) (0.899999976158142 -
                             (1.0 - Math.Pow(environmentMultiplier, 0.300000011920929)) / 2.5));
        }
        private static float GetSunBrightness(float environmentMultiplier, bool isMoon, bool forceDay = false)
        {
            return !(!isMoon  | forceDay)
                ? 0.2f
                : Math.Min(
                    Math.Max(
                        (float) Math.Sin(Math.Pow((environmentMultiplier - 1.0 / 1000.0) / 0.999000012874603,
                            1.20000004768372) * (Math.PI / 2.0)) * 85f, 0.2f), 35f);
        }
        private static float GetSunSize(float envMultiplier)
        {
            return (float) (0.100000001490116 + (1.0 - envMultiplier) / 8.0);
        }

        private static float GetSunRayStrength(float envMultiplier)
        {
            return Math.Min(
                Math.Max(
                    (float) Math.Sin(Math.Pow((envMultiplier - 1.0 / 1000.0) / 0.999000012874603,
                        0.400000005960464) * Math.PI / 2.0) - 0.15f, 0.01f), 0.5f);
        }
    }
}
