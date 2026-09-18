using System;
using Beautify.Universal;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScheduleOne.FX
{
	public class PlayerHealthVisuals : MonoBehaviour
	{
		[Header("References")]
		public Volume GlobalVolume;

		[Header("Vignette")]
		public float VignetteAlpha_MaxHealth;

		public float VignetteAlpha_MinHealth;

		public AnimationCurve OuterRingCurve;

		[Header("Saturation")]
		public float Saturation_MaxHealth = 0.5f;

		public float Saturation_MinHealth = -2f;

		[Header("Chromatic Abberation")]
		public float ChromAb_MaxHealth;

		public float ChromAb_MinHealth = 0.02f;

		[Header("Lens Dirt")]
		public float LensDirt_MaxHealth;

		public float LensDirt_MinHealth = 1f;

		//private Beautify.Universal.Beautify _beautifySettings;

		private void Awake()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(Spawned));
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Spawned));
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
			instance2.onMinutePass = (Action)Delegate.Combine(instance2.onMinutePass, new Action(MinPass));
			//GlobalVolume.sharedProfile.TryGet<Beautify.Universal.Beautify>(out _beautifySettings);
		}

		private void Spawned()
		{
			if (Player.Local.Owner.IsLocalClient)
			{
				UpdateEffects(Player.Local.Health.CurrentHealth);
				Player.Local.Health.onHealthChanged.AddListener(UpdateEffects);
			}
		}

		private void MinPass()
		{
			//_beautifySettings.vignettingOuterRing.value = OuterRingCurve.Evaluate(NetworkSingleton<TimeManager>.Instance.NormalizedTime);
		}

		private void UpdateEffects(float newHealth)
		{
			//_beautifySettings.vignettingColor.value = new Color(_beautifySettings.vignettingColor.value.r, _beautifySettings.vignettingColor.value.g, _beautifySettings.vignettingColor.value.b, Mathf.Lerp(VignetteAlpha_MinHealth, VignetteAlpha_MaxHealth, newHealth / 100f));
			//_beautifySettings.saturate.value = Mathf.Lerp(Saturation_MinHealth, Saturation_MaxHealth, newHealth / 100f);
			//_beautifySettings.chromaticAberrationIntensity.value = Mathf.Lerp(ChromAb_MinHealth, ChromAb_MaxHealth, newHealth / 100f);
			//_beautifySettings.lensDirtIntensity.value = Mathf.Lerp(LensDirt_MinHealth, LensDirt_MaxHealth, newHealth / 100f);
		}
	}
}
