using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.AvatarFramework
{
	public class Eye : MonoBehaviour
	{
		[Serializable]
		public struct EyeLidConfiguration
		{
			[Range(0f, 1f)]
			public float topLidOpen;

			[Range(0f, 1f)]
			public float bottomLidOpen;

			public override string ToString()
			{
				return "Top: " + topLidOpen + ", Bottom: " + bottomLidOpen;
			}

			public static EyeLidConfiguration Lerp(EyeLidConfiguration start, EyeLidConfiguration end, float lerp)
			{
				EyeLidConfiguration result = default(EyeLidConfiguration);
				result.topLidOpen = Mathf.Lerp(start.topLidOpen, end.topLidOpen, lerp);
				result.bottomLidOpen = Mathf.Lerp(start.bottomLidOpen, end.bottomLidOpen, lerp);
				return result;
			}
		}

		public const float PupilLookSpeed = 10f;

		private static Vector3 defaultScale = new Vector3(0.03f, 0.03f, 0.015f);

		private static Vector3 maxRotation = new Vector3(40f, 35f, 0f);

		private static Vector3 minRotation = new Vector3(-40f, -90f, 0f);

		[Header("References")]
		public Transform Container;

		public Transform TopLidContainer;

		public Transform BottomLidContainer;

		public Transform PupilContainer;

		public MeshRenderer TopLidRend;

		public MeshRenderer BottomLidRend;

		public MeshRenderer EyeBallRend;

		public Transform EyeLookOrigin;

		public SkinnedMeshRenderer PupilRend;

		private Coroutine blinkRoutine;

		private Coroutine stateRoutine;

		private Avatar avatar;

		public Vector2 AngleOffset = Vector2.zero;

		public EyeLidConfiguration CurrentConfiguration { get; protected set; }

		public bool IsBlinking => blinkRoutine != null;

		private void Awake()
		{
			avatar = GetComponentInParent<Avatar>();
		}

		public void SetSize(float size)
		{
			Container.localScale = defaultScale * size;
		}

		public void SetLidColor(Color color)
		{
			TopLidRend.material.color = color;
			BottomLidRend.material.color = color;
		}

		public void SetEyeballMaterial(Material mat, Color col)
		{
			EyeBallRend.material = mat;
		}

		public void SetEyeballColor(Color col)
		{
			EyeBallRend.material.color = col;
			EyeBallRend.material.SetColor("_EmissionColor", col * 0.115f);
		}

		public void SetDilation(float dil)
		{
			PupilRend.SetBlendShapeWeight(0, dil * 100f);
		}

		public void SetEyeLidState(EyeLidConfiguration config, float time)
		{
			EyeLidConfiguration startConfig = CurrentConfiguration;
			StopExistingRoutines();
			if (Singleton<CoroutineService>.InstanceExists)
			{
				stateRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				for (float i = 0f; i < time; i += Time.deltaTime)
				{
					EyeLidConfiguration config2 = default(EyeLidConfiguration);
					config2.topLidOpen = Mathf.Lerp(startConfig.topLidOpen, config.topLidOpen, i / time);
					config2.bottomLidOpen = Mathf.Lerp(startConfig.bottomLidOpen, config.bottomLidOpen, i / time);
					SetEyeLidState(config2);
					yield return new WaitForEndOfFrame();
				}
				SetEyeLidState(config);
				stateRoutine = null;
			}
		}

		private void StopExistingRoutines()
		{
			if (blinkRoutine != null)
			{
				StopCoroutine(blinkRoutine);
			}
			if (stateRoutine != null)
			{
				StopCoroutine(stateRoutine);
			}
		}

		public void SetEyeLidState(EyeLidConfiguration config, bool debug = false)
		{
			if (!(TopLidContainer == null) && !(BottomLidContainer == null))
			{
				if (debug)
				{
					EyeLidConfiguration eyeLidConfiguration = config;
					Console.Log("Setting eye lid state: " + eyeLidConfiguration.ToString());
				}
				TopLidContainer.localRotation = Quaternion.Lerp(Quaternion.Euler(0f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f), config.topLidOpen);
				BottomLidContainer.localRotation = Quaternion.Lerp(Quaternion.Euler(0f, 0f, 0f), Quaternion.Euler(90f, 0f, 0f), config.bottomLidOpen);
				CurrentConfiguration = config;
			}
		}

		public void LookAt(Vector3 position, bool instant = false)
		{
			Vector3 normalized = (position - EyeLookOrigin.position).normalized;
			normalized = EyeLookOrigin.InverseTransformDirection(normalized);
			normalized.z = Mathf.Clamp(normalized.z, 0.1f, float.MaxValue);
			normalized = EyeLookOrigin.TransformDirection(normalized);
			Vector3 direction = EyeLookOrigin.InverseTransformDirection(normalized);
			direction.x = 0f;
			direction = EyeLookOrigin.TransformDirection(direction);
			float num = Vector3.SignedAngle(EyeLookOrigin.forward, direction, EyeLookOrigin.right);
			Vector3 direction2 = EyeLookOrigin.InverseTransformDirection(normalized);
			direction2.y = 0f;
			direction2 = EyeLookOrigin.TransformDirection(direction2);
			float num2 = Vector3.SignedAngle(EyeLookOrigin.forward, direction2, EyeLookOrigin.up);
			Vector3 vector = new Vector3(Mathf.Clamp(num + AngleOffset.x, minRotation.y, maxRotation.y), Mathf.Clamp(num2 + AngleOffset.y, minRotation.x, maxRotation.x), 0f);
			if (instant)
			{
				Vector3 vector2 = vector;
				Debug.Log("instant: " + vector2.ToString());
				PupilContainer.localRotation = Quaternion.Euler(vector);
			}
			else
			{
				PupilContainer.localRotation = Quaternion.Lerp(PupilContainer.localRotation, Quaternion.Euler(vector), Time.deltaTime * 10f);
			}
		}

		public void Blink(float blinkDuration, EyeLidConfiguration endState, bool debug = false)
		{
			StopExistingRoutines();
			if (!(avatar == null) && !(avatar.EmotionManager == null) && !avatar.EmotionManager.IsSwitchingEmotion)
			{
				blinkRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				EyeLidConfiguration start2 = CurrentConfiguration;
				EyeLidConfiguration end2 = new EyeLidConfiguration
				{
					bottomLidOpen = 0f,
					topLidOpen = 0f
				};
				float holdTime = blinkDuration * 0.1f;
				float duration = (blinkDuration - holdTime) / 2f;
				for (float j = 0f; j < duration; j += Time.deltaTime)
				{
					EyeLidConfiguration eyeLidConfiguration = default(EyeLidConfiguration);
					eyeLidConfiguration.bottomLidOpen = Mathf.Lerp(start2.bottomLidOpen, end2.bottomLidOpen, j / duration);
					eyeLidConfiguration.topLidOpen = Mathf.Lerp(start2.topLidOpen, end2.topLidOpen, j / duration);
					EyeLidConfiguration config = eyeLidConfiguration;
					SetEyeLidState(config, debug);
					yield return new WaitForEndOfFrame();
				}
				SetEyeLidState(end2, debug);
				yield return new WaitForSeconds(holdTime);
				start2 = CurrentConfiguration;
				end2 = new EyeLidConfiguration
				{
					bottomLidOpen = endState.bottomLidOpen,
					topLidOpen = endState.topLidOpen
				};
				for (float j = 0f; j < duration; j += Time.deltaTime)
				{
					EyeLidConfiguration eyeLidConfiguration = default(EyeLidConfiguration);
					eyeLidConfiguration.bottomLidOpen = Mathf.Lerp(start2.bottomLidOpen, end2.bottomLidOpen, j / duration);
					eyeLidConfiguration.topLidOpen = Mathf.Lerp(start2.topLidOpen, end2.topLidOpen, j / duration);
					EyeLidConfiguration config2 = eyeLidConfiguration;
					SetEyeLidState(config2, debug);
					yield return new WaitForEndOfFrame();
				}
				blinkRoutine = null;
			}
		}
	}
}
