using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
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

        public static AtmosphereInfo CreateAtmosphereInfoForMission(
            int dayOfYear = 1,
            float timeOfDay = 6f,
            string weather = "clear",
            float fogDensity = -1f,
            bool isInSettlement = false)
        {
            float seasonOfYear =
                (MBMath.ClampInt(dayOfYear, 1, CampaignTime.DaysInYear) - 1f)
                / CampaignTime.DaysInSeason;
            (CampaignTime.Seasons season, bool isRaining, float rainValue, float snowValue)
                = GetSeasonRainAndSnowDataForOpeningMission(
                    dayOfYear,
                    weather);
            bool isWinter = season == CampaignTime.Seasons.Winter;
            string atmosphereName = GetSelectedAtmosphereId(
                isWinter,
                isRaining,
                snowValue,
                rainValue);
            float hourNormalized = timeOfDay / 24f;
            float seasonFactor = GetSeasonTimeFactor(seasonOfYear);
            SunInfo sunInfo = GetSunInfo(hourNormalized, seasonFactor);
            float environmentMultiplier = GetEnvironmentMultiplier(
                sunInfo.SunPosition,
                seasonFactor,
                sunInfo.IsMoon);
            float modifiedEnvironmentMultiplier = GetModifiedEnvironmentMultiplier(
                environmentMultiplier,
                sunInfo.IsMoon);
            modifiedEnvironmentMultiplier = Math.Max(
                (float)Math.Pow(modifiedEnvironmentMultiplier, 1.5),
                0.001f);
            Vec3 position = MobileParty.MainParty?.GetPositionAsVec3()
                            ?? Vec3.Zero;
            CampaignVec2 campaignPosition = MobileParty.MainParty.Position;
            Vec2 windVector = Campaign.Current.Models.MapWeatherModel
                .GetWindForPosition(campaignPosition);
            AtmosphereState atmosphereState =
                Campaign.Current.Models.MapWeatherModel
                    .GetInterpolatedAtmosphereState(
                        CampaignTime.Now,
                        position);
            float calculatedFogDensity = GetFogDensity(
                environmentMultiplier,
                position,
                sunInfo.IsMoon);
            float selectedFogDensity = fogDensity < 0f
                ? calculatedFogDensity
                : fogDensity;
            return new AtmosphereInfo()
            {
                Seed = (uint)CampaignTime.Now.ToSeconds,
                //AtmosphereName = "TOD_12_00_SemiCloudy",
                InterpolatedAtmosphereName = atmosphereName,
                SunInfo =
                {
                    Altitude = sunInfo.SunPosition.Altitude,
                    Angle = sunInfo.SunPosition.Angle,
                    Color = GetSunColor(
                        environmentMultiplier,
                        sunInfo.IsMoon),
                    Brightness = GetSunBrightness(
                        environmentMultiplier,
                        sunInfo.IsMoon),
                    Size = GetSunSize(environmentMultiplier),
                    RayStrength = GetSunRayStrength(environmentMultiplier),
                    MaxBrightness = GetSunBrightness(
                        1f,
                        sunInfo.IsMoon,
                        true)
                },
                AmbientInfo =
                {
                    EnvironmentMultiplier = Math.Max(
                        modifiedEnvironmentMultiplier * 0.5f,
                        0.001f),
                    AmbientColor = GetAmbientFogColor(
                        modifiedEnvironmentMultiplier),
                    MieScatterStrength = GetMieScatterStrength(
                        environmentMultiplier),
                    RayleighConstant = GetRayleighConstant(
                        environmentMultiplier)
                },
                SkyInfo =
                {
                    Brightness = GetSkyBrightness(
                        hourNormalized,
                        environmentMultiplier,
                        sunInfo.IsMoon)
                },
                FogInfo =
                {
                    Density = selectedFogDensity,
                    Color = GetFogColor(
                        modifiedEnvironmentMultiplier,
                        sunInfo.IsMoon),
                    Falloff = 1.48f
                },
                TimeInfo = new TimeInformation
                {
                    Season = (int)season,
                    TimeOfDay = timeOfDay,
                    WinterTimeFactor = GetWinterTimeFactor(seasonOfYear),
                    DrynessFactor = GetDrynessFactor(seasonOfYear),
                    NightTimeFactor = GetNightTimeFactor(timeOfDay)
                },
                RainInfo = new RainInformation
                {
                    Density = rainValue
                },
                SnowInfo = new SnowInformation
                {
                    Density = snowValue
                },
                NauticalInfo =
                {
                    WaveStrength = campaignPosition.IsOnLand
                        ? 0.26f
                        : windVector.Length,
                    WindVector = windVector,
                    CanUseLowAltitudeAtmosphere = 0,
                    UseSceneWindDirection = 1,
                    IsRiverBattle = 0
                },
                AreaInfo =
                {
                    Temperature = GetTemperature(
                        ref atmosphereState,
                        seasonFactor),
                    Humidity = GetHumidity(
                        ref atmosphereState,
                        seasonFactor)
                },
                PostProInfo =
                {
                    MinExposure = MBMath.Lerp(
                        -3f,
                        -2f,
                        GetExposureCoefBetweenDayNight(timeOfDay)),
                    MaxExposure = MBMath.Lerp(
                        2f,
                        0f,
                        modifiedEnvironmentMultiplier),
                    BrightpassThreshold = MBMath.Lerp(
                        0.7f,
                        0.9f,
                        modifiedEnvironmentMultiplier),
                    MiddleGray = 0.1f
                }
            };
        }

        private static (
            CampaignTime.Seasons season,
            bool isRaining,
            float rainValue,
            float snowValue)
            GetSeasonRainAndSnowDataForOpeningMission(
                int dayOfYear,
                string weather)
        {
            var season = GetSeason(dayOfYear);
            float rainValue = 0f;
            float snowValue = 0.85f;
            bool isRaining = false;
            switch (weather)
            {
                case "clear":
                    season = GetNonWinterSeason(dayOfYear, season);
                    break;
                case "light_rain":
                    season = GetNonWinterSeason(dayOfYear, season);
                    rainValue = 0.701f;
                    break;
                case "heavy_rain":
                    season = GetNonWinterSeason(dayOfYear, season);
                    isRaining = true;
                    rainValue = 0.85f
                                + MBRandom.RandomFloatRanged(0f, 0.14999998f);
                    break;
                case "snowy":
                    season = CampaignTime.Seasons.Winter;
                    rainValue = 0.55f;
                    snowValue = 0.55f
                                + MBRandom.RandomFloatRanged(0f, 0.3f);
                    break;
                case "blizzard":
                    season = CampaignTime.Seasons.Winter;
                    rainValue = 0.85f;
                    snowValue = 0.85f;
                    break;
            }

            return (season, isRaining, rainValue, snowValue);
        }

        private static CampaignTime.Seasons GetNonWinterSeason(
            int dayOfYear,
            CampaignTime.Seasons season)
        {
            if (season != CampaignTime.Seasons.Winter)
                return season;

            int dayOfSeason =
                (MBMath.ClampInt(dayOfYear, 1, CampaignTime.DaysInYear) - 1)
                % CampaignTime.DaysInSeason;
            return dayOfSeason <= 10
                ? CampaignTime.Seasons.Autumn
                : CampaignTime.Seasons.Spring;
        }

        private static string GetSelectedAtmosphereId(
            bool isWinter,
            bool isRaining,
            float snowDensity,
            float rainDensity)
        {
            string result = "semicloudy_field_battle";
            if (isWinter)
                return snowDensity >= 0.85f ? "dense_snowy" : "semi_snowy";

            if (rainDensity < 0.6f)
                return result;
            result = "wet";
            if (isRaining)
                result = rainDensity >= 0.85f
                    ? "dense_rainy"
                    : "semi_rainy";
            return result;
        }

        public static int GetSeasonIndex(int dayOfYear)
        {
            return (MBMath.ClampInt(
                        dayOfYear,
                        1,
                        CampaignTime.DaysInYear)
                    - 1)
                   / CampaignTime.DaysInSeason;
        }

        private static CampaignTime.Seasons GetSeason(int dayOfYear)
        {
            return (CampaignTime.Seasons)GetSeasonIndex(dayOfYear);
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
            //return isMoon
            //    ? Vec3.Vec3Max(
            //        new Vec3((float)(1.0 - (double)environmentMultiplier * 10.0),
            //            (float)(0.75 + (double)environmentMultiplier * 1.5),
            //            (float)(0.649999976158142 + (double)environmentMultiplier * 2.0)),
            //        new Vec3(0.55f, 0.59f, 0.6f))
            //    : new Vec3((float)(1.0 - (1.0 - (double)environmentMultiplier) / 7.0),
            //        (float)(0.75 - (double)environmentMultiplier / 4.0),
            //        (float)(0.550000011920929 - (double)environmentMultiplier / 5.0));
            return new Vec3(0.72f, 0.76f, 0.8f);
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
                num = MBMath.SplitLerp(0.0f, 0.75f, 0.0f, 0.5f, Math.Abs((float)Math.IEEERemainder(seasonOfYear, 1.0)), 1E-05f);
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
            float result = 0.0f;
            if (seasonNum > 1.5f && seasonNum <= 3.5f)
                result = MBMath.Map(seasonNum, 1.5f, 3.5f, 0f, 1f);
            else if (seasonNum <= 1.5f)
                result = MBMath.Map(seasonNum, 0f, 1.5f, 0.75f, 0f);
            else if (seasonNum > 3.5f)
                result = MBMath.Map(seasonNum, 3.5f, 4f, 1f, 0.75f);
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
