using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Materials;
using UnityEngine;

namespace ScheduleOne.Audio
{
	public class FootstepSounds : MonoBehaviour
	{
		[Serializable]
		public class FootstepSoundGroup
		{
			[Serializable]
			public class MaterialType
			{
				public EMaterialType type;
			}

			public string name;

			public List<AudioClip> clips = new List<AudioClip>();

			public List<MaterialType> appliesTo = new List<MaterialType>();

			public float PitchMin = 0.9f;

			public float PitchMax = 1.1f;

			public float Volume = 0.5f;
		}

		public const float COOLDOWN_TIME = 0.15f;

		public List<AudioSourceController> sources = new List<AudioSourceController>();

		public List<FootstepSoundGroup> soundGroups = new List<FootstepSoundGroup>();

		private Dictionary<EMaterialType, FootstepSoundGroup> materialFootstepSounds = new Dictionary<EMaterialType, FootstepSoundGroup>();

		private float lastStepTime;

		private void Start()
		{
			foreach (FootstepSoundGroup soundGroup in soundGroups)
			{
				foreach (FootstepSoundGroup.MaterialType item in soundGroup.appliesTo)
				{
					if (!materialFootstepSounds.ContainsKey(item.type))
					{
						materialFootstepSounds.Add(item.type, soundGroup);
					}
				}
			}
			foreach (EMaterialType value in Enum.GetValues(typeof(EMaterialType)))
			{
				if (!materialFootstepSounds.ContainsKey(value))
				{
					Console.Log("No footstep sounds for material type: " + value.ToString() + "\n Assigning to default group.");
					materialFootstepSounds.Add(value, soundGroups[0]);
				}
			}
			for (int i = 0; i < sources.Count; i++)
			{
				sources[i].AudioSource.enabled = false;
				sources[i].enabled = false;
			}
		}

		private void Update()
		{
			lastStepTime += Time.deltaTime;
		}

		public void Step(EMaterialType materialType, float hardness)
		{
			AudioSourceController source;
			if (!(lastStepTime < 0.15f))
			{
				lastStepTime = 0f;
				source = GetFreeSource();
				if (source == null)
				{
					Console.LogWarning("No free audio sources available for footstep sound.");
					return;
				}
				FootstepSoundGroup footstepSoundGroup = materialFootstepSounds[materialType];
				source.AudioSource.clip = footstepSoundGroup.clips[UnityEngine.Random.Range(0, footstepSoundGroup.clips.Count)];
				source.AudioSource.pitch = UnityEngine.Random.Range(footstepSoundGroup.PitchMin, footstepSoundGroup.PitchMax);
				source.SetVolume(footstepSoundGroup.Volume * hardness);
				source.AudioSource.enabled = true;
				source.enabled = true;
				source.Play();
				StartCoroutine(DisableSource());
			}
			IEnumerator DisableSource()
			{
				yield return new WaitForSeconds(source.AudioSource.clip.length);
				source.AudioSource.enabled = false;
				source.enabled = false;
			}
		}

		public AudioSourceController GetFreeSource()
		{
			return sources.FirstOrDefault((AudioSourceController source) => !source.enabled);
		}
	}
}
