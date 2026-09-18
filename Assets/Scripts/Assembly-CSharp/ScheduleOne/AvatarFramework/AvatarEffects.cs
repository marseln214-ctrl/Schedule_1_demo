using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.AvatarFramework
{
	public class AvatarEffects : MonoBehaviour
	{
		[Header("References")]
		public Avatar Avatar;

		public ParticleSystem[] StinkParticles;

		public ParticleSystem VomitParticles;

		public ParticleSystem HairPoofParticles;

		public ParticleSystem FartParticles;

		public ParticleSystem AntiGravParticles;

		public ParticleSystem FireParticles;

		public OptimizedLight FireLight;

		public Transform HeadBone;

		public Transform NeckBone;

		public AvatarEffects[] MirrorEffectsTo;

		[Header("Settings")]
		public bool DisableHead;

		[Header("Sounds")]
		public AudioSourceController GurgleSound;

		public AudioSourceController VomitSound;

		public AudioSourceController PoofSound;

		public AudioSourceController FartSound;

		public AudioSourceController FireSound;

		[Header("Smoothers")]
		[SerializeField]
		private FloatSmoother AdditionalWeightController;

		[SerializeField]
		private FloatSmoother AdditionalGenderController;

		[SerializeField]
		private FloatSmoother HeadSizeBoost;

		[SerializeField]
		private FloatSmoother NeckSizeBoost;

		[SerializeField]
		private ColorSmoother SkinColorSmoother;

		private bool laxativeEnabled;

		private void Start()
		{
			AdditionalWeightController.Initialize();
			AdditionalWeightController.SetDefault(0f);
			AdditionalGenderController.Initialize();
			AdditionalGenderController.SetDefault(0f);
			HeadSizeBoost.Initialize();
			HeadSizeBoost.SetDefault(0f);
			NeckSizeBoost.Initialize();
			NeckSizeBoost.SetDefault(0f);
			SkinColorSmoother.Initialize();
			if (Avatar.CurrentSettings != null)
			{
				SetDefaultSkinColor();
			}
			Avatar.onSettingsLoaded.AddListener(delegate
			{
				SetDefaultSkinColor();
			});
		}

		public void Update()
		{
			Avatar.SetAdditionalWeight(AdditionalWeightController.CurrentValue);
			Avatar.SetAdditionalGender(AdditionalGenderController.CurrentValue);
			Avatar.SetSkinColor(SkinColorSmoother.CurrentValue);
			if (DisableHead)
			{
				HeadBone.transform.localScale = Vector3.zero;
			}
			else
			{
				HeadBone.transform.localScale = Vector3.one * (1f + HeadSizeBoost.CurrentValue);
			}
			if (FireParticles.isPlaying)
			{
				FireSound.VolumeMultiplier = Mathf.MoveTowards(FireSound.VolumeMultiplier, 1f, Time.deltaTime);
				if (!FireSound.isPlaying)
				{
					FireSound.Play();
				}
			}
			else
			{
				FireSound.VolumeMultiplier = Mathf.MoveTowards(FireSound.VolumeMultiplier, 0f, Time.deltaTime);
				if (FireSound.VolumeMultiplier <= 0f)
				{
					FireSound.Stop();
				}
			}
			NeckBone.transform.localScale = Vector3.one * (1f + NeckSizeBoost.CurrentValue);
		}

		public void SetStinkParticlesActive(bool active, bool mirror = true)
		{
			ParticleSystem[] stinkParticles = StinkParticles;
			foreach (ParticleSystem particleSystem in stinkParticles)
			{
				if (active)
				{
					particleSystem.Play();
				}
				else
				{
					particleSystem.Stop();
				}
			}
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].SetStinkParticlesActive(active, mirror: false);
				}
			}
		}

		public void TriggerSick(bool mirror = true)
		{
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].TriggerSick(mirror: false);
				}
			}
			StartCoroutine(Routine());
			IEnumerator Routine()
			{
				GurgleSound.Play();
				yield return new WaitForSeconds(4.5f);
				VomitSound.Play();
				VomitParticles.gameObject.layer = LayerMask.NameToLayer("Default");
				VomitParticles.Play();
			}
		}

		public void SetAntiGrav(bool active, bool mirror = true)
		{
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].SetAntiGrav(active, mirror: false);
				}
			}
			if (active)
			{
				AntiGravParticles.Play();
			}
			else
			{
				AntiGravParticles.Stop();
			}
		}

		public void VanishHair(bool mirror = true)
		{
			HairPoofParticles.Play();
			PoofSound.Play();
			Avatar.SetHairVisible(visible: false);
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].VanishHair(mirror: false);
				}
			}
		}

		public void ReturnHair(bool mirror = true)
		{
			HairPoofParticles.Play();
			PoofSound.Play();
			Avatar.SetHairVisible(visible: true);
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].ReturnHair(mirror: false);
				}
			}
		}

		public void OverrideHairColor(Color color, bool mirror = true)
		{
			HairPoofParticles.Play();
			PoofSound.Play();
			Avatar.OverrideHairColor(color);
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].OverrideHairColor(color, mirror: false);
				}
			}
		}

		public void ResetHairColor(bool mirror = true)
		{
			HairPoofParticles.Play();
			PoofSound.Play();
			Avatar.ResetHairColor();
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].ResetHairColor(mirror: false);
				}
			}
		}

		public void EnableLaxative(bool mirror = true)
		{
			laxativeEnabled = true;
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].EnableLaxative(mirror: false);
				}
			}
			Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				do
				{
					FartParticles.Play();
					FartSound.Play();
					yield return new WaitForSeconds(Random.Range(3f, 20f));
				}
				while (laxativeEnabled);
			}
		}

		public void DisableLaxative(bool mirror = true)
		{
			laxativeEnabled = false;
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].DisableLaxative(mirror: false);
				}
			}
		}

		public void SetFireActive(bool active, bool mirror = true)
		{
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].SetFireActive(active, mirror: false);
				}
			}
			FireLight.Enabled = active;
			if (active)
			{
				FireParticles.Play();
			}
			else
			{
				FireParticles.Stop();
			}
		}

		public void SetBigHeadActive(bool active, bool mirror = true)
		{
			if (active)
			{
				HeadSizeBoost.AddOverride(0.4f, 7, "big head");
			}
			else
			{
				HeadSizeBoost.RemoveOverride("big head");
			}
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].SetBigHeadActive(active, mirror: false);
				}
			}
		}

		public void SetGiraffeActive(bool active, bool mirror = true)
		{
			if (active)
			{
				HeadSizeBoost.AddOverride(-0.5f, 8, "giraffe");
				NeckSizeBoost.AddOverride(1f, 8, "giraffe");
			}
			else
			{
				HeadSizeBoost.RemoveOverride("giraffe");
				NeckSizeBoost.RemoveOverride("giraffe");
			}
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].SetGiraffeActive(active, mirror: false);
				}
			}
		}

		public void SetSkinColorInverted(bool inverted, bool mirror = true)
		{
			if (inverted)
			{
				if (Avatar.IsWhite())
				{
					SkinColorSmoother.AddOverride(new Color32(58, 49, 42, byte.MaxValue), 7, "inverted");
				}
				else
				{
					SkinColorSmoother.AddOverride(new Color32(223, 189, 161, byte.MaxValue), 7, "inverted");
				}
			}
			else
			{
				SkinColorSmoother.RemoveOverride("inverted");
			}
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].SetSkinColorInverted(inverted, mirror: false);
				}
			}
		}

		private void SetDefaultSkinColor(bool mirror = true)
		{
			if (Avatar.CurrentSettings == null)
			{
				return;
			}
			SkinColorSmoother.SetDefault(Avatar.CurrentSettings.SkinColor);
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].SetDefaultSkinColor(mirror: false);
				}
			}
		}

		public void SetGenderInverted(bool inverted, bool mirror = true)
		{
			if (inverted)
			{
				if (Avatar.IsMale())
				{
					AdditionalGenderController.AddOverride(1f, 7, "jennerising");
				}
				else
				{
					AdditionalGenderController.AddOverride(-1f, 7, "jennerising");
				}
			}
			else
			{
				AdditionalGenderController.RemoveOverride("jennerising");
			}
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].SetGenderInverted(inverted, mirror: false);
				}
			}
		}

		public void AddAdditionalWeightOverride(float value, int priority, string label, bool mirror = true)
		{
			AdditionalWeightController.AddOverride(value, priority, label);
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].AddAdditionalWeightOverride(value, priority, label, mirror: false);
				}
			}
		}

		public void RemoveAdditionalWeightOverride(string label, bool mirror = true)
		{
			AdditionalWeightController.RemoveOverride(label);
			if (mirror)
			{
				AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
				for (int i = 0; i < mirrorEffectsTo.Length; i++)
				{
					mirrorEffectsTo[i].RemoveAdditionalWeightOverride(label, mirror: false);
				}
			}
		}
	}
}
