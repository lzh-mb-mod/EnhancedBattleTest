using System;
using EnhancedBattleTest.Config;
using EnhancedBattleTest.SinglePlayer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
	public class EBTRealisticWeatherMissionBehavior : MissionBehavior
	{
		public override MissionBehaviorType BehaviorType
		{
			get
			{
				return MissionBehaviorType.Other;
			}
		}

		public override void AfterStart()
		{
			BattleConfig _config = BattleConfig.Deserialize(EnhancedBattleTestSubModule.IsMultiplayer);
			Scene scene = base.Mission.Scene;
			bool flag = !scene.IsAtmosphereIndoor;
			if (flag)
			{
				float rainDensity = 0f;
				float fogDensity = 1f;
				bool hasDust = false;

				if (BattleStarter.IsEnhancedBattleTestBattle)
                {
					rainDensity = _config.MapConfig.RainDensity;
                    fogDensity = _config.MapConfig.FogDensity; ;
                    hasDust = (fogDensity == 0f);
                }

                bool flag7 = rainDensity > 0f;
				if (flag7)
				{
					Vec3 sunColor = new Vec3(255f, 255f, 255f, 255f);
					float sunAltitude = 50f * MathF.Cos(3.14159274f * scene.TimeOfDay / 6f) + 50f;
					float sunIntensity = (1f - rainDensity) / 1000f;
					scene.SetRainDensity(rainDensity);
					scene.SetSun(ref sunColor, sunAltitude, 0f, sunIntensity);
				}
				bool flag8 = fogDensity > 1f;
				if (flag8)
				{
					Vec3 fogColor = new Vec3(1f, 1f, 1f, 1f);
					float fogFalloff = 0.5f * MathF.Sin(3.14159274f * scene.TimeOfDay / 24f);
					scene.SetFog(fogDensity, ref fogColor, fogFalloff);
					scene.SetFogAdvanced(0f, 0.1f, 0f);
				}
				bool flag9 = hasDust && rainDensity == 0f;
				if (flag9)
				{
					try
					{
						GameEntity.Instantiate(scene, "dust_prefab_entity", base.Mission.GetSceneMiddleFrame().ToGroundMatrixFrame());
						scene.SetSkyBrightness((scene.TimeOfDay < 12f) ? ((MathF.Pow(2f, scene.TimeOfDay) - 1f) / 10f) : ((MathF.Pow(2f, 24f - scene.TimeOfDay) - 1f) / 10f));
						this._dustSound = SoundEvent.CreateEvent(SoundEvent.GetEventIdFromString("dust_storm"), scene);
						this._dustSound.Play();
					}
					catch (Exception)
					{
					}
				}
			}
			this._hasSetSkyboxAndParticles = false;
		}

		public override void OnMissionTick(float dt)
		{
			bool flag = !this._hasSetSkyboxAndParticles;
			if (flag)
			{
				Scene scene = base.Mission.Scene;
				Mesh skyboxMesh = scene.GetFirstEntityWithName("__skybox__").GetFirstMesh();
				Material skyboxMaterial = skyboxMesh.GetMaterial().CreateCopy();
				GameEntity rainPrefab = scene.GetFirstEntityWithName("rain_prefab_entity") ?? scene.GetFirstEntityWithName("snow_prefab_entity");
				float rainDensity = scene.GetRainDensity();
				bool flag2 = rainDensity > 0f && rainPrefab != null;
				if (flag2)
				{
					skyboxMaterial.SetTexture(Material.MBTextureType.DiffuseMap, Texture.GetFromResource("sky_photo_overcast_01"));
					skyboxMesh.SetMaterial(skyboxMaterial);
					foreach (GameEntity entity in rainPrefab.GetChildren())
					{
						bool flag3 = entity.Name != "rain_far";
						if (flag3)
						{
							entity.SetRuntimeEmissionRateMultiplier(2f * (rainDensity / 0.25f) - 1f);
						}
						else
						{
							int i = 1;
							while ((float)i < 4f * (rainDensity / 0.25f) - 3f)
							{
								GameEntity rain = GameEntity.CopyFromPrefab(entity);
								MatrixFrame rainFrame = new MatrixFrame(rain.GetFrame().rotation, rain.GetFrame().origin + new Vec3(MBRandom.RandomFloat * 10f, MBRandom.RandomFloat * 10f, MBRandom.RandomFloat * 10f, -1f));
								rain.SetFrame(ref rainFrame);
								rainPrefab.AddChild(rain, false);
								i++;
							}
						}
					}
					bool flag4 = rainPrefab.Name != "snow_prefab_entity";
					if (flag4)
					{
						bool flag5 = rainDensity >= 0.25f && rainDensity < 0.5f;
						if (flag5)
						{
							this._rainSound = SoundEvent.CreateEvent(SoundEvent.GetEventIdFromString("rain_light"), scene);
						}
						else
						{
							bool flag6 = rainDensity >= 0.5f && rainDensity < 0.75f;
							if (flag6)
							{
								this._rainSound = SoundEvent.CreateEvent(SoundEvent.GetEventIdFromString("rain_moderate"), scene);
							}
							else
							{
								bool flag7 = rainDensity >= 0.75f;
								if (flag7)
								{
									this._rainSound = SoundEvent.CreateEvent(SoundEvent.GetEventIdFromString("rain_heavy"), scene);
								}
							}
						}
					}
					else
					{
						bool flag8 = rainDensity >= 0.25f;
						if (flag8)
						{
							this._rainSound = SoundEvent.CreateEvent(SoundEvent.GetEventIdFromString("event:/mission/ambient/area/winter"), scene);
						}
						bool flag9 = rainDensity >= 0.75f;
						if (flag9)
						{
							this._windSound = SoundEvent.CreateEvent(SoundEvent.GetEventIdFromString("wind"), scene);
						}
					}
					SoundEvent rainSound = this._rainSound;
					if (rainSound != null)
					{
						rainSound.Play();
					}
					SoundEvent windSound = this._windSound;
					if (windSound != null)
					{
						windSound.Play();
					}
					this._hasSetSkyboxAndParticles = true;
				}
			}
		}

		public override void HandleOnCloseMission()
		{
			SoundEvent rainSound = this._rainSound;
			if (rainSound != null)
			{
				rainSound.Stop();
			}
			SoundEvent windSound = this._windSound;
			if (windSound != null)
			{
				windSound.Stop();
			}
			SoundEvent dustSound = this._dustSound;
			if (dustSound != null)
			{
				dustSound.Stop();
			}
		}

		private bool _hasSetSkyboxAndParticles;

		private SoundEvent _rainSound;

		private SoundEvent _windSound;

		private SoundEvent _dustSound;
	}
}
