using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.Management;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools
{
	public class ManagementClipboard : Singleton<ManagementClipboard>
	{
		public bool IsEquipped;

		public const float OpenTime = 0.06f;

		[Header("References")]
		public Transform ClipboardTransform;

		public Camera OverlayCamera;

		public Light OverlayLight;

		public SelectionInfoUI SelectionInfo;

		[Header("Settings")]
		public float ClosedOffset = -0.2f;

		public UnityEvent onClipboardEquipped;

		public UnityEvent onClipboardUnequipped;

		public UnityEvent onOpened;

		public UnityEvent onClosed;

		private Coroutine lerpRoutine;

		private List<IConfigurable> CurrentConfigurables = new List<IConfigurable>();

		public bool IsOpen { get; protected set; }

		public bool StatePreserved { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			ClipboardTransform.gameObject.SetActive(value: false);
			ClipboardTransform.localPosition = new Vector3(ClipboardTransform.localPosition.x, ClosedOffset, ClipboardTransform.localPosition.z);
			GameInput.RegisterExitListener(Exit, 10);
		}

		private void Update()
		{
			for (int i = 0; i < CurrentConfigurables.Count; i++)
			{
				if (CurrentConfigurables[i].IsBeingConfiguredByOtherPlayer)
				{
					Close();
				}
			}
		}

		private void Exit(ExitAction exitAction)
		{
			if (IsOpen && !exitAction.used)
			{
				Close();
				exitAction.used = true;
			}
		}

		public void Open(List<IConfigurable> selection, ManagementClipboard_Equippable equippable)
		{
			IsOpen = true;
			OverlayCamera.enabled = true;
			OverlayLight.enabled = true;
			ClipboardTransform.gameObject.SetActive(value: true);
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0.06f);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0.06f);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			SelectionInfo.Set(selection);
			LerpToVerticalPosition(open: true, null);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			Singleton<ManagementInterface>.Instance.Open(selection, equippable);
			CurrentConfigurables.AddRange(selection);
			for (int i = 0; i < CurrentConfigurables.Count; i++)
			{
				CurrentConfigurables[i].SetConfigurer(Player.Local.NetworkObject);
			}
			if (onOpened != null)
			{
				onOpened.Invoke();
			}
		}

		public void Close(bool preserveState = false)
		{
			IsOpen = false;
			StatePreserved = preserveState;
			OverlayLight.enabled = false;
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0.06f);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			Singleton<ManagementInterface>.Instance.Close(preserveState);
			if (onClosed != null)
			{
				onClosed.Invoke();
			}
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			for (int i = 0; i < CurrentConfigurables.Count; i++)
			{
				if (CurrentConfigurables[i].CurrentPlayerConfigurer == Player.Local.NetworkObject)
				{
					CurrentConfigurables[i].SetConfigurer(null);
				}
			}
			LerpToVerticalPosition(open: false, delegate
			{
				Done();
			});
			void Done()
			{
				if (!Singleton<GameplayMenu>.Instance.IsOpen)
				{
					ClipboardTransform.gameObject.SetActive(value: false);
					OverlayCamera.enabled = false;
				}
			}
		}

		private void LerpToVerticalPosition(bool open, Action callback)
		{
			Vector3 endPos = new Vector3(ClipboardTransform.localPosition.x, open ? 0f : ClosedOffset, ClipboardTransform.localPosition.z);
			Vector3 startPos = ClipboardTransform.localPosition;
			if (lerpRoutine != null)
			{
				StopCoroutine(lerpRoutine);
			}
			lerpRoutine = StartCoroutine(Lerp());
			IEnumerator Lerp()
			{
				for (float i = 0f; i < 0.06f; i += Time.deltaTime)
				{
					ClipboardTransform.transform.localPosition = Vector3.Lerp(startPos, endPos, i / 0.06f);
					yield return new WaitForEndOfFrame();
				}
				ClipboardTransform.localPosition = endPos;
				if (callback != null)
				{
					callback();
				}
				lerpRoutine = null;
			}
		}
	}
}
