using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest.Data.MissionData.Logic
{
    public sealed class EnhancedBattleTestEnvironmentLogic : MissionLogic
    {
        private readonly float _timeOfDay;
        private readonly float _rainDensity;
        private readonly float _snowDensity;
        private readonly SunInformation _sunInfo;
        private readonly float _fogDensity;
        private readonly Vec3 _fogColor;
        private readonly float _fogFalloff;
        private readonly AtmosphereModel.ExposureInfo? _exposureInfo;
        private bool _reappliedAfterStart;

        public EnhancedBattleTestEnvironmentLogic(
            float timeOfDay,
            SunInformation sunInfo,
            float rainDensity,
            float snowDensity,
            float fogDensity,
            Vec3 fogColor,
            float fogFalloff,
            AtmosphereModel.ExposureInfo? exposureInfo)
        {
            _timeOfDay = timeOfDay;
            _sunInfo = sunInfo;
            _rainDensity = rainDensity;
            _snowDensity = snowDensity;
            _fogDensity = fogDensity;
            _fogColor = fogColor;
            _fogFalloff = fogFalloff;
            _exposureInfo = exposureInfo;
        }

        public override void AfterStart()
        {
            base.AfterStart();
            ApplyEnvironment();
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (_reappliedAfterStart)
                return;

            _reappliedAfterStart = true;
            ApplyEnvironment();
        }

        private void ApplyEnvironment()
        {
            Scene scene = Mission?.Scene;
            if (scene == null)
                return;

            scene.TimeOfDay = _timeOfDay;
            //scene.SetWinterTimeFactor(_isWinter ? 0.75f : 0f);
            if (scene.IsAtmosphereIndoor)
                return;

            // The issue of setting sun position is that,
            // the sun light on cloud is bound to the current atmosphere.
            // setting sun position won't change the light on cloud
            // which causes wierd visual result.
            //scene.SetSunAngleAltitude(
            //    _sunInfo.Angle,
            //    _sunInfo.Altitude);
            //scene.SetSunSize(_sunInfo.Size);
            //scene.SetSunShaftStrength(_sunInfo.RayStrength);
            scene.SetRainDensity(_rainDensity);
            scene.SetSnowDensity(_snowDensity);

            if (_exposureInfo.HasValue)
            {
                AtmosphereModel.ExposureInfo exposureInfo =
                    _exposureInfo.Value;
                if (exposureInfo.Min.HasValue)
                    scene.SetMinExposure(exposureInfo.Min.Value);
                if (exposureInfo.Max.HasValue)
                    scene.SetMaxExposure(exposureInfo.Max.Value);
                if (exposureInfo.Target.HasValue)
                    scene.SetTargetExposure(exposureInfo.Target.Value);
            }

            if (_fogDensity < 0f)
                return;

            var fogColor = _fogColor;
            scene.SetFog(_fogDensity, ref fogColor, _fogFalloff);
            scene.SetFogAdvanced(0f, _fogDensity / 640, 0f);
        }
    }
}
