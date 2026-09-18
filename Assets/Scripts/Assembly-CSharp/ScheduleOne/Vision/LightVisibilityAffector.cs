using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vision
{
	[RequireComponent(typeof(Light))]
	public class LightVisibilityAffector : MonoBehaviour
	{
		public const float PointLightEffect = 15f;

		public const float SpotLightEffect = 10f;

		[Header("Settings")]
		public float EffectMultiplier = 1f;

		public string uniquenessCode = "Light";

		[Tooltip("How far does the player have to move for visibility to be recalculated?")]
		public int updateDistanceThreshold = 1;

		protected Light light;

		protected VisibilityAttribute attribute;

		protected virtual void Awake()
		{
			light = GetComponent<Light>();
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		}

		private void PlayerSpawned()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
			PlayerSingleton<PlayerMovement>.Instance.RegisterMovementEvent(updateDistanceThreshold, UpdateVisibility);
		}

		private void OnDestroy()
		{
			if (PlayerSingleton<PlayerMovement>.Instance != null)
			{
				PlayerSingleton<PlayerMovement>.Instance.DeregisterMovementEvent(UpdateVisibility);
			}
			ClearAttribute();
		}

		protected virtual void UpdateVisibility()
		{
			if (light == null || base.gameObject == null)
			{
				return;
			}
			if (!light.enabled || !base.gameObject.activeInHierarchy)
			{
				ClearAttribute();
			}
			else
			{
				if (Player.Local == null)
				{
					return;
				}
				float num = Player.Local.Visibility.CalculateExposureToPoint(base.transform.position, light.range);
				if (num == 0f)
				{
					ClearAttribute();
					return;
				}
				float num2 = Mathf.Pow(1f - Mathf.Clamp(Vector3.Distance(base.transform.position, Player.Local.Avatar.CenterPoint) / light.range, 0f, 1f), 2f);
				float num3 = 1f - Singleton<EnvironmentFX>.Instance.normalizedEnvironmentalBrightness;
				float num4 = 1f;
				if (light.type == LightType.Spot)
				{
					float num5 = Vector3.Angle(base.transform.forward, (Player.Local.Avatar.CenterPoint - base.transform.position).normalized);
					if (num5 > light.spotAngle * 0.5f)
					{
						num4 = 0f;
					}
					else
					{
						float num6 = light.spotAngle * 0.5f - num5;
						float num7 = light.spotAngle * 0.5f - light.innerSpotAngle * 0.5f;
						num4 = Mathf.Clamp(num6 / num7, 0f, 1f);
					}
				}
				float visibity = num * num2 * light.intensity * num3 * num4 * ((light.type == LightType.Spot) ? 10f : 15f) * EffectMultiplier;
				UpdateAttribute(visibity);
			}
		}

		private void UpdateAttribute(float visibity)
		{
			if (visibity <= 0f)
			{
				ClearAttribute();
			}
			else if (attribute == null)
			{
				if (uniquenessCode != string.Empty)
				{
					attribute = new UniqueVisibilityAttribute("Light Exposure (" + base.gameObject.name + ")", visibity, uniquenessCode);
				}
				else
				{
					attribute = new VisibilityAttribute("Light Exposure (" + base.gameObject.name + ")", visibity);
				}
			}
			else
			{
				attribute.pointsChange = visibity;
			}
		}

		private void ClearAttribute()
		{
			if (attribute != null)
			{
				attribute.Delete();
				attribute = null;
			}
		}
	}
}
