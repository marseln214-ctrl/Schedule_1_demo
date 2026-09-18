using System.Collections;
using EasyButtons;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.AvatarFramework
{
	[ExecuteInEditMode]
	public class EyeController : MonoBehaviour
	{
		private static float eyeHeightMultiplier = 0.03f;

		public bool DEBUG;

		[Header("References")]
		[SerializeField]
		public Eye leftEye;

		[SerializeField]
		public Eye rightEye;

		[Header("Location Settings")]
		[Range(0f, 45f)]
		[SerializeField]
		protected float eyeSpacing = 20f;

		[Range(-1f, 1f)]
		[SerializeField]
		protected float eyeHeight;

		[Range(0.5f, 1.5f)]
		[SerializeField]
		protected float eyeSize = 1f;

		[Header("Eyelid Settings")]
		[SerializeField]
		protected Color leftEyeLidColor = Color.white;

		[SerializeField]
		protected Color rightEyeLidColor = Color.white;

		public Eye.EyeLidConfiguration LeftRestingEyeState;

		public Eye.EyeLidConfiguration RightRestingEyeState;

		[Header("Eyeball Settings")]
		[SerializeField]
		protected Material eyeBallMaterial;

		[SerializeField]
		protected Color eyeBallColor;

		[Header("Pupil State")]
		[Range(0f, 1f)]
		public float PupilDilation = 0.5f;

		[Header("Blinking Settings")]
		public bool BlinkingEnabled = true;

		[SerializeField]
		[Range(0f, 10f)]
		protected float blinkInterval = 3.5f;

		[SerializeField]
		[Range(0f, 2f)]
		protected float blinkIntervalSpread = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		protected float blinkDuration = 0.2f;

		private Avatar avatar;

		private Coroutine blinkRoutine;

		private float timeUntilNextBlink;

		private bool eyeBallTintOverridden;

		private bool eyeLidOverridden;

		private Eye.EyeLidConfiguration defaultLeftEyeRestingState;

		private Eye.EyeLidConfiguration defaultRightEyeRestingState;

		public bool EyesOpen { get; protected set; } = true;


		protected virtual void Awake()
		{
			avatar = GetComponentInParent<Avatar>();
			avatar.onRagdollChange.AddListener(RagdollChange);
			SetEyesOpen(open: true);
			ApplyDilation();
		}

		protected void Update()
		{
			if (Application.isPlaying)
			{
				if (BlinkingEnabled && blinkRoutine == null)
				{
					blinkRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(BlinkRoutine());
				}
				if (BlinkingEnabled)
				{
					timeUntilNextBlink -= Time.deltaTime;
				}
			}
		}

		private void OnEnable()
		{
			ApplyRestingEyeLidState();
		}

		[Button]
		public void ApplySettings()
		{
			leftEye.transform.localEulerAngles = new Vector3(0f, 0f - eyeSpacing, 0f);
			rightEye.transform.localEulerAngles = new Vector3(0f, eyeSpacing, 0f);
			rightEye.transform.localPosition = new Vector3(0f, eyeHeight * eyeHeightMultiplier, 0f);
			leftEye.transform.localPosition = new Vector3(0f, eyeHeight * eyeHeightMultiplier, 0f);
			leftEye.SetSize(eyeSize);
			rightEye.SetSize(eyeSize);
			leftEye.SetLidColor(leftEyeLidColor);
			rightEye.SetLidColor(rightEyeLidColor);
			leftEye.SetEyeballMaterial(eyeBallMaterial, eyeBallColor);
			rightEye.SetEyeballMaterial(eyeBallMaterial, eyeBallColor);
			ApplyDilation();
			ApplyRestingEyeLidState();
		}

		public void SetEyeballTint(Color col)
		{
			leftEye.SetEyeballColor(col);
			rightEye.SetEyeballColor(col);
		}

		public void OverrideEyeballTint(Color col)
		{
			leftEye.SetEyeballColor(col);
			rightEye.SetEyeballColor(col);
			eyeBallTintOverridden = true;
		}

		public void ResetEyeballTint()
		{
			leftEye.SetEyeballColor(eyeBallColor);
			rightEye.SetEyeballColor(eyeBallColor);
			eyeBallTintOverridden = false;
		}

		public void OverrideEyeLids(Eye.EyeLidConfiguration eyeLidConfiguration)
		{
			if (!eyeLidOverridden)
			{
				defaultLeftEyeRestingState = LeftRestingEyeState;
				defaultRightEyeRestingState = RightRestingEyeState;
			}
			LeftRestingEyeState = eyeLidConfiguration;
			RightRestingEyeState = eyeLidConfiguration;
			eyeLidOverridden = true;
		}

		public void ResetEyeLids()
		{
			LeftRestingEyeState = defaultLeftEyeRestingState;
			RightRestingEyeState = defaultRightEyeRestingState;
			eyeLidOverridden = false;
		}

		private void RagdollChange(bool oldValue, bool newValue, bool playStandUpAnim)
		{
			if (newValue)
			{
				ForceBlink();
			}
		}

		public void SetEyesOpen(bool open)
		{
			if (DEBUG)
			{
				Debug.Log("Setting eyes open: " + open);
			}
			EyesOpen = open;
			leftEye.SetEyeLidState(open ? LeftRestingEyeState : new Eye.EyeLidConfiguration
			{
				bottomLidOpen = 0f,
				topLidOpen = 0f
			}, 0.1f);
			rightEye.SetEyeLidState(open ? RightRestingEyeState : new Eye.EyeLidConfiguration
			{
				bottomLidOpen = 0f,
				topLidOpen = 0f
			}, 0.1f);
		}

		private void ApplyDilation()
		{
			leftEye.SetDilation(PupilDilation);
			rightEye.SetDilation(PupilDilation);
		}

		public void SetPupilDilation(float dilation)
		{
			PupilDilation = dilation;
			ApplyDilation();
		}

		private void ApplyRestingEyeLidState()
		{
			leftEye.SetEyeLidState(LeftRestingEyeState);
			rightEye.SetEyeLidState(RightRestingEyeState);
		}

		public void ForceBlink()
		{
			leftEye.Blink(blinkDuration, LeftRestingEyeState);
			rightEye.Blink(blinkDuration, RightRestingEyeState);
			ResetBlinkCounter();
		}

		public void SetLeftEyeRestingLidState(Eye.EyeLidConfiguration config)
		{
			LeftRestingEyeState = config;
			if (!leftEye.IsBlinking)
			{
				leftEye.SetEyeLidState(config);
			}
		}

		public void SetRightEyeRestingLidState(Eye.EyeLidConfiguration config)
		{
			RightRestingEyeState = config;
			if (!rightEye.IsBlinking)
			{
				rightEye.SetEyeLidState(config);
			}
		}

		private IEnumerator BlinkRoutine()
		{
			while (BlinkingEnabled)
			{
				if (EyesOpen)
				{
					if (DEBUG)
					{
						Debug.Log("Blinking");
					}
					leftEye.Blink(blinkDuration, LeftRestingEyeState, DEBUG);
					rightEye.Blink(blinkDuration, RightRestingEyeState, DEBUG);
				}
				ResetBlinkCounter();
				yield return new WaitUntil(() => timeUntilNextBlink <= 0f);
			}
			blinkRoutine = null;
		}

		private void ResetBlinkCounter()
		{
			timeUntilNextBlink = Random.Range(Mathf.Clamp(blinkInterval - blinkIntervalSpread, blinkDuration, float.MaxValue), blinkInterval + blinkIntervalSpread);
		}

		public void LookAt(Vector3 position, bool instant = false)
		{
			_ = DEBUG;
			leftEye.LookAt(position, instant);
			rightEye.LookAt(position, instant);
		}
	}
}
