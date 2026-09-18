using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.UI.WorldspacePopup
{
	public class WorldspacePopup : MonoBehaviour
	{
		public static List<WorldspacePopup> ActivePopups = new List<WorldspacePopup>();

		[Range(0f, 1f)]
		public float CurrentFillLevel = 1f;

		[Header("Settings")]
		public WorldspacePopupUI UIPrefab;

		public bool DisplayOnHUD = true;

		public bool ScaleWithDistance = true;

		public Vector3 WorldspaceOffset;

		public float Range = 50f;

		public float SizeMultiplier = 1f;

		[HideInInspector]
		public WorldspacePopupUI WorldspaceUI;

		[HideInInspector]
		public RectTransform HUDUI;

		[HideInInspector]
		public WorldspacePopupUI HUDUIIcon;

		[HideInInspector]
		public CanvasGroup HUDUICanvasGroup;

		private List<WorldspacePopupUI> UIs = new List<WorldspacePopupUI>();

		private Coroutine popupCoroutine;

		private void OnEnable()
		{
			if (!ActivePopups.Contains(this))
			{
				ActivePopups.Add(this);
			}
		}

		private void OnDisable()
		{
			ActivePopups.Remove(this);
		}

		public WorldspacePopupUI CreateUI(RectTransform parent)
		{
			WorldspacePopupUI newUI = Object.Instantiate(UIPrefab, parent);
			newUI.Popup = this;
			newUI.SetFill(CurrentFillLevel);
			UIs.Add(newUI);
			newUI.onDestroyed.AddListener(delegate
			{
				UIs.Remove(newUI);
			});
			return newUI;
		}

		private void LateUpdate()
		{
			foreach (WorldspacePopupUI uI in UIs)
			{
				uI.SetFill(CurrentFillLevel);
			}
		}

		public void Popup()
		{
			if (popupCoroutine != null)
			{
				StopCoroutine(popupCoroutine);
			}
			popupCoroutine = StartCoroutine(PopupCoroutine());
			IEnumerator PopupCoroutine()
			{
				base.enabled = true;
				SizeMultiplier = 0f;
				float lerpTime = 0.25f;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					SizeMultiplier = i / lerpTime;
					yield return new WaitForEndOfFrame();
				}
				SizeMultiplier = 1f;
				yield return new WaitForSeconds(0.6f);
				base.enabled = false;
				popupCoroutine = null;
			}
		}
	}
}
