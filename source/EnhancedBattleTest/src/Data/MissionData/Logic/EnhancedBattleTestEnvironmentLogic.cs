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
        private readonly float _fogDensity;
        private readonly Vec3 _fogColor;
        private readonly float _fogFalloff;
        private bool _reappliedAfterStart;

        public EnhancedBattleTestEnvironmentLogic(
            float timeOfDay,
            float rainDensity,
            float snowDensity,
            float fogDensity,
            Vec3 fogColor,
            float fogFalloff)
        {
            _timeOfDay = timeOfDay;
            _rainDensity = rainDensity;
            _snowDensity = snowDensity;
            _fogDensity = fogDensity;
            _fogColor = fogColor;
            _fogFalloff = fogFalloff;
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

            //scene.TimeOfDay = _timeOfDay;
            //scene.SetWinterTimeFactor(_isWinter ? 0.75f : 0f);
            if (scene.IsAtmosphereIndoor)
                return;

            //scene.SetRainDensity(_rainDensity);
            //scene.SetSnowDensity(_snowDensity);
            if (_fogDensity < 0f)
                return;

            var fogColor = _fogColor;
            scene.SetFog(_fogDensity, ref fogColor, _fogFalloff);
            scene.SetFogAdvanced(0f, _fogDensity / 640, 0f);
        }
    }
}
