using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.ScriptableObjects;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone
{
	public class Phone : PlayerSingleton<Phone>
	{
		public static GameObject ActiveApp;

		public PhoneCallData testData;

		public CallerID testCalller;

		[Header("References")]
		[SerializeField]
		protected GameObject phoneModel;

		[SerializeField]
		protected Transform orientation_Vertical;

		[SerializeField]
		protected Transform orientation_Horizontal;

		[SerializeField]
		protected GraphicRaycaster raycaster;

		[SerializeField]
		protected GameObject PhoneFlashlight;

		[SerializeField]
		protected AudioSourceController FlashlightToggleSound;

		[Header("Settings")]
		public float rotationTime = 0.1f;

		public float LookOffsetMax = 0.45f;

		public float LookOffsetMin = 0.29f;

		public float OpenVerticalOffset = 0.1f;

		public Action onPhoneOpened;

		public Action onPhoneClosed;

		public Action closeApps;

		private EventSystem eventSystem;

		private VisibilityAttribute flashlightVisibility;

		private Coroutine rotationCoroutine;

		private Coroutine lookOffsetCoroutine;

		public bool IsOpen { get; protected set; }

		public bool isHorizontal { get; protected set; }

		public bool isOpenable { get; protected set; } = true;


		public bool FlashlightOn { get; protected set; }

		public float ScaledLookOffset => Mathf.Lerp(LookOffsetMax, LookOffsetMin, CanvasScaler.NormalizedCanvasScaleFactor);

		protected override void Awake()
		{
			base.Awake();
			eventSystem = EventSystem.current;
		}

		public override void OnStartClient(bool IsOwner)
		{
			base.OnStartClient(IsOwner);
			if (!IsOwner)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		protected override void Start()
		{
			base.Start();
			if (flashlightVisibility == null)
			{
				flashlightVisibility = new VisibilityAttribute("Flashlight", 0f);
			}
			base.transform.localRotation = orientation_Vertical.localRotation;
		}

		protected virtual void Update()
		{
			if (IsOpen)
			{
				Singleton<HUD>.Instance.OnlineBalanceDisplay.Show();
			}
			if (!GameInput.IsTyping && !Singleton<PauseMenu>.Instance.IsPaused && (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount == 0 || IsOpen) && GameInput.GetButtonDown(GameInput.ButtonCode.ToggleFlashlight))
			{
				ToggleFlashlight();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			ActiveApp = null;
		}

		private void ToggleFlashlight()
		{
			FlashlightOn = !FlashlightOn;
			PhoneFlashlight.SetActive(FlashlightOn);
			FlashlightToggleSound.PitchMultiplier = (FlashlightOn ? 1f : 0.9f);
			FlashlightToggleSound.Play();
			flashlightVisibility.pointsChange = (FlashlightOn ? 10f : 0f);
			flashlightVisibility.multiplier = (FlashlightOn ? 1.5f : 1f);
			Player.Local.SendFlashlightOn(FlashlightOn);
		}

		public void SetOpenable(bool o)
		{
			isOpenable = o;
		}

		public void SetIsOpen(bool o)
		{
			IsOpen = o;
			if (IsOpen)
			{
				if (onPhoneOpened != null)
				{
					onPhoneOpened();
				}
				if (ActiveApp == null)
				{
					SetLookOffsetMultiplier(1f);
				}
			}
			else if (onPhoneClosed != null)
			{
				onPhoneClosed();
			}
		}

		public void SetIsHorizontal(bool h)
		{
			isHorizontal = h;
			if (rotationCoroutine != null)
			{
				StopCoroutine(rotationCoroutine);
			}
			rotationCoroutine = StartCoroutine(SetIsHorizontal_Process(h));
		}

		protected IEnumerator SetIsHorizontal_Process(bool h)
		{
			float adjustedRotationTime2 = rotationTime;
			Quaternion startRotation = base.transform.localRotation;
			_ = Quaternion.identity;
			Quaternion endRotation;
			if (h)
			{
				endRotation = orientation_Horizontal.localRotation;
				adjustedRotationTime2 *= Quaternion.Angle(base.transform.localRotation, orientation_Horizontal.localRotation) / 90f;
			}
			else
			{
				endRotation = orientation_Vertical.localRotation;
				adjustedRotationTime2 *= Quaternion.Angle(base.transform.localRotation, orientation_Vertical.localRotation) / 90f;
			}
			for (float i = 0f; i < adjustedRotationTime2; i += Time.deltaTime)
			{
				base.transform.localRotation = Quaternion.Lerp(startRotation, endRotation, i / adjustedRotationTime2);
				yield return new WaitForEndOfFrame();
			}
			base.transform.localRotation = endRotation;
			rotationCoroutine = null;
		}

		public void SetLookOffsetMultiplier(float multiplier)
		{
			float lookOffset_Process = ScaledLookOffset * multiplier;
			if (lookOffsetCoroutine != null)
			{
				StopCoroutine(lookOffsetCoroutine);
			}
			lookOffsetCoroutine = StartCoroutine(SetLookOffset_Process(lookOffset_Process));
		}

		public void RequestCloseApp()
		{
			if (ActiveApp != null && closeApps != null)
			{
				closeApps();
			}
		}

		protected IEnumerator SetLookOffset_Process(float lookOffset)
		{
			float startOffset = base.transform.localPosition.z;
			float moveTime = 0.1f;
			for (float i = 0f; i < moveTime; i += Time.deltaTime)
			{
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, Mathf.Lerp(startOffset, lookOffset, i / moveTime));
				yield return new WaitForEndOfFrame();
			}
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, lookOffset);
			rotationCoroutine = null;
		}

		public bool MouseRaycast(out RaycastResult result)
		{
			PointerEventData pointerEventData = new PointerEventData(eventSystem);
			pointerEventData.position = UnityEngine.Input.mousePosition;
			List<RaycastResult> list = new List<RaycastResult>();
			raycaster.Raycast(pointerEventData, list);
			if (list.Count > 0)
			{
				result = list[0];
			}
			else
			{
				result = default(RaycastResult);
			}
			return list.Count > 0;
		}
	}
}
