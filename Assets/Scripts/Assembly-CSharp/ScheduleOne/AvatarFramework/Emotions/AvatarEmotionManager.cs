using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Emotions
{
	public class AvatarEmotionManager : MonoBehaviour
	{
		public const float MAX_UPDATE_DISTANCE = 30f;

		[Header("Settings")]
		public List<AvatarEmotionPreset> EmotionPresetList = new List<AvatarEmotionPreset>();

		[Header("References")]
		public Avatar Avatar;

		public EyeController EyeController;

		public EyebrowController EyebrowController;

		private EmotionOverride activeEmotionOverride;

		private List<EmotionOverride> overrideStack = new List<EmotionOverride>();

		private AvatarEmotionPreset neutralPreset;

		private Coroutine emotionLerpRoutine;

		private Dictionary<string, Coroutine> emotionRemovalRoutines = new Dictionary<string, Coroutine>();

		private int tempIndex;

		public string CurrentEmotion { get; protected set; } = "Neutral";


		public AvatarEmotionPreset CurrentEmotionPreset { get; protected set; }

		public bool IsSwitchingEmotion => emotionLerpRoutine != null;

		private void Start()
		{
			neutralPreset = EmotionPresetList.Find((AvatarEmotionPreset x) => x.PresetName == "Neutral");
			AddEmotionOverride("Neutral", "base_emotion", 0f, -1);
			InvokeRepeating("UpdateEmotion", 0f, 0.25f);
		}

		private void Update()
		{
		}

		public void UpdateEmotion()
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists || !(Vector3.Distance(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position) > 30f))
			{
				EmotionOverride highestPriorityOverride = GetHighestPriorityOverride();
				if (highestPriorityOverride != null && highestPriorityOverride != activeEmotionOverride)
				{
					activeEmotionOverride = highestPriorityOverride;
					LerpEmotion(GetEmotion(highestPriorityOverride.Emotion));
				}
			}
		}

		public void ConfigureNeutralFace(Texture2D faceTex, float restingBrowHeight, float restingBrowAngle, Eye.EyeLidConfiguration leftEyelidConfig, Eye.EyeLidConfiguration rightEyelidConfig)
		{
			neutralPreset = EmotionPresetList.Find((AvatarEmotionPreset x) => x.PresetName == "Neutral");
			if (neutralPreset == null)
			{
				Debug.LogError("Could not find neutral preset");
				return;
			}
			neutralPreset.FaceTexture = faceTex;
			neutralPreset.BrowAngleChange_R = restingBrowAngle;
			neutralPreset.BrowAngleChange_L = restingBrowAngle;
			neutralPreset.BrowHeightChange_L = restingBrowHeight;
			neutralPreset.BrowHeightChange_R = restingBrowHeight;
			neutralPreset.LeftEyeRestingState = leftEyelidConfig;
			neutralPreset.RightEyeRestingState = rightEyelidConfig;
			if (CurrentEmotionPreset == neutralPreset)
			{
				SetEmotion(neutralPreset);
			}
		}

		public virtual void AddEmotionOverride(string emotionName, string overrideLabel, float duration = 0f, int priority = 0)
		{
			EmotionOverride emotionOverride = overrideStack.Find((EmotionOverride x) => x.Label.ToLower() == overrideLabel.ToLower());
			if (emotionOverride != null)
			{
				emotionOverride.Emotion = emotionName;
				emotionOverride.Priority = priority;
				if (emotionOverride == activeEmotionOverride)
				{
					activeEmotionOverride = null;
				}
			}
			else
			{
				emotionOverride = new EmotionOverride(emotionName, overrideLabel, priority);
				overrideStack.Add(emotionOverride);
			}
			ClearRemovalRoutine(overrideLabel);
			if (duration > 0f)
			{
				Coroutine value = Singleton<CoroutineService>.Instance.StartCoroutine(RemoveEmotionAfterDuration());
				emotionRemovalRoutines.Add(overrideLabel.ToLower(), value);
			}
			IEnumerator RemoveEmotionAfterDuration()
			{
				yield return new WaitForSeconds(duration);
				RemoveEmotionOverride(overrideLabel.ToString());
			}
		}

		public void RemoveEmotionOverride(string label)
		{
			ClearRemovalRoutine(label);
			EmotionOverride emotionOverride = overrideStack.Find((EmotionOverride x) => x.Label.ToLower() == label.ToLower());
			if (emotionOverride == null)
			{
				Console.LogWarning("RemoveEmotionOverride: Could not find emotion override with label: " + label);
			}
			else
			{
				overrideStack.Remove(emotionOverride);
			}
		}

		public void ClearOverrides()
		{
			EmotionOverride[] array = overrideStack.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i].Label == "base_emotion"))
				{
					RemoveEmotionOverride(array[i].Label);
				}
			}
		}

		private void ClearRemovalRoutine(string label)
		{
			label = label.ToLower();
			if (emotionRemovalRoutines.ContainsKey(label))
			{
				if (emotionRemovalRoutines[label] != null)
				{
					StopCoroutine(emotionRemovalRoutines[label]);
				}
				emotionRemovalRoutines.Remove(label);
			}
		}

		public EmotionOverride GetHighestPriorityOverride()
		{
			return overrideStack.OrderByDescending((EmotionOverride x) => x.Priority).ToList().FirstOrDefault();
		}

		private void LerpEmotion(AvatarEmotionPreset preset, float animationTime = 0.2f)
		{
			if (CurrentEmotionPreset == null)
			{
				SetEmotion(preset);
				return;
			}
			if (emotionLerpRoutine != null)
			{
				StopCoroutine(emotionLerpRoutine);
			}
			emotionLerpRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				AvatarEmotionPreset startPreset = CurrentEmotionPreset;
				int num = 48;
				float timeStep = 1f / (float)num;
				for (float i = 0f; i < animationTime; i += timeStep)
				{
					AvatarEmotionPreset emotion = AvatarEmotionPreset.Lerp(startPreset, preset, neutralPreset, i / animationTime);
					SetEmotion(emotion);
					yield return new WaitForSeconds(timeStep);
				}
				SetEmotion(AvatarEmotionPreset.Lerp(startPreset, preset, neutralPreset, 1f));
				emotionLerpRoutine = null;
			}
		}

		private void SetEmotion(AvatarEmotionPreset preset)
		{
			CurrentEmotionPreset = preset;
			Avatar.SetFaceTexture(preset.FaceTexture, Color.black);
			EyeController.SetLeftEyeRestingLidState(preset.LeftEyeRestingState);
			EyeController.SetRightEyeRestingLidState(preset.RightEyeRestingState);
			EyeController.LeftRestingEyeState = preset.LeftEyeRestingState;
			EyeController.RightRestingEyeState = preset.RightEyeRestingState;
			EyebrowController.SetLeftBrowRestingHeight(preset.BrowHeightChange_L);
			EyebrowController.SetRightBrowRestingHeight(preset.BrowHeightChange_R);
			EyebrowController.leftBrow.SetRestingAngle(preset.BrowAngleChange_L);
			EyebrowController.rightBrow.SetRestingAngle(preset.BrowAngleChange_R);
		}

		public bool HasEmotion(string emotion)
		{
			return GetEmotion(emotion) != null;
		}

		public AvatarEmotionPreset GetEmotion(string emotion)
		{
			return EmotionPresetList.Find((AvatarEmotionPreset x) => x.PresetName.ToLower() == emotion.ToLower());
		}
	}
}
