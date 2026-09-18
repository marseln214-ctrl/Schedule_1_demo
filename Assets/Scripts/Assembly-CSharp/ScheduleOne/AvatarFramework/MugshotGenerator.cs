using System;
using System.Collections;
using System.IO;
using EasyButtons;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.AvatarFramework
{
	public class MugshotGenerator : Singleton<MugshotGenerator>
	{
		public string OutputPath;

		public AvatarSettings Settings;

		[Header("References")]
		public Avatar MugshotRig;

		public IconGenerator Generator;

		public AvatarSettings DefaultSettings;

		public Transform LookAtPosition;

		private Texture2D finalTexture;

		private bool generate;

		protected override void Awake()
		{
			base.Awake();
			MugshotRig.gameObject.SetActive(value: false);
		}

		private void LateUpdate()
		{
			if (generate)
			{
				generate = false;
				FinalizeMugshot();
			}
		}

		private void FinalizeMugshot()
		{
			finalTexture = Generator.GetTexture(MugshotRig.transform);
			Debug.Log("Mugshot capture");
		}

		[Button]
		public void GenerateMugshot()
		{
			GenerateMugshot(Settings, fileToFile: true, null);
		}

		public void GenerateMugshot(AvatarSettings settings, bool fileToFile, Action<Texture2D> callback)
		{
			finalTexture = null;
			Debug.Log("Mugshot start");
			AvatarSettings avatarSettings = UnityEngine.Object.Instantiate(settings);
			avatarSettings.Height = 1f;
			MugshotRig.gameObject.SetActive(value: true);
			MugshotRig.LoadAvatarSettings(avatarSettings);
			LayerUtility.SetLayerRecursively(MugshotRig.gameObject, LayerMask.NameToLayer("IconGeneration"));
			SkinnedMeshRenderer[] componentsInChildren = MugshotRig.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].updateWhenOffscreen = true;
			}
			generate = true;
			StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitUntil(() => finalTexture != null);
				if (fileToFile)
				{
					string text = OutputPath + "/" + settings.name + "_Mugshot.png";
					byte[] bytes = finalTexture.EncodeToPNG();
					Debug.Log("Writing to: " + text);
					File.WriteAllBytes(text, bytes);
				}
				if (callback != null)
				{
					callback(finalTexture);
				}
				MugshotRig.LoadAvatarSettings(DefaultSettings);
				MugshotRig.gameObject.SetActive(value: false);
			}
		}
	}
}
