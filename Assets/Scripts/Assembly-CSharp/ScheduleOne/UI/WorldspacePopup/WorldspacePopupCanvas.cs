using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.UI.WorldspacePopup
{
	public class WorldspacePopupCanvas : MonoBehaviour
	{
		public const float WORLDSPACE_ICON_SCALE_MULTIPLIER = 0.4f;

		[Header("References")]
		public RectTransform WorldspaceContainer;

		public RectTransform HudContainer;

		[Header("Prefabs")]
		public GameObject HudIconContainerPrefab;

		private List<WorldspacePopupUI> activeWorldspaceUIs = new List<WorldspacePopupUI>();

		private List<RectTransform> activeHUDUIs = new List<RectTransform>();

		private List<WorldspacePopup> popupsWithUI = new List<WorldspacePopup>();

		private void Update()
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				return;
			}
			List<WorldspacePopup> list = new List<WorldspacePopup>();
			List<WorldspacePopup> list2 = new List<WorldspacePopup>();
			for (int i = 0; i < WorldspacePopup.ActivePopups.Count; i++)
			{
				if (!popupsWithUI.Contains(WorldspacePopup.ActivePopups[i]) && ShouldCreateUI(WorldspacePopup.ActivePopups[i]))
				{
					list.Add(WorldspacePopup.ActivePopups[i]);
				}
			}
			for (int j = 0; j < popupsWithUI.Count; j++)
			{
				if (!WorldspacePopup.ActivePopups.Contains(popupsWithUI[j]) || !ShouldCreateUI(popupsWithUI[j]))
				{
					list2.Add(popupsWithUI[j]);
				}
			}
			foreach (WorldspacePopup item in list)
			{
				CreateWorldspaceIcon(item);
				if (item.DisplayOnHUD)
				{
					CreateHUDIcon(item);
				}
			}
			foreach (WorldspacePopup item2 in list2)
			{
				DestroyWorldspaceIcon(item2);
				if (item2.DisplayOnHUD)
				{
					DestroyHUDIcon(item2);
				}
			}
		}

		private void LateUpdate()
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				return;
			}
			for (int i = 0; i < popupsWithUI.Count; i++)
			{
				if (PlayerSingleton<PlayerCamera>.Instance.transform.InverseTransformPoint(popupsWithUI[i].transform.position).z > 0f)
				{
					Vector3 vector = popupsWithUI[i].transform.position + popupsWithUI[i].WorldspaceOffset;
					Vector2 vector2 = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(vector);
					float num = 1f;
					if (popupsWithUI[i].ScaleWithDistance)
					{
						float f = Vector3.Distance(vector, PlayerSingleton<PlayerCamera>.Instance.transform.position);
						num = 1f / Mathf.Sqrt(f);
					}
					num *= popupsWithUI[i].SizeMultiplier;
					num *= 0.4f;
					popupsWithUI[i].WorldspaceUI.Rect.position = vector2;
					popupsWithUI[i].WorldspaceUI.Rect.localScale = new Vector3(num, num, 1f);
					popupsWithUI[i].WorldspaceUI.gameObject.SetActive(value: true);
				}
				else
				{
					popupsWithUI[i].WorldspaceUI.gameObject.SetActive(value: false);
				}
			}
			for (int j = 0; j < popupsWithUI.Count; j++)
			{
				if (popupsWithUI[j].HUDUI != null)
				{
					float num2 = Vector3.SignedAngle(Vector3.ProjectOnPlane(PlayerSingleton<PlayerCamera>.Instance.transform.forward, Vector3.up), (popupsWithUI[j].transform.position - PlayerSingleton<PlayerCamera>.Instance.transform.position).normalized, Vector3.up);
					popupsWithUI[j].HUDUI.localRotation = Quaternion.Euler(0f, 0f, 0f - num2);
					popupsWithUI[j].HUDUIIcon.transform.up = Vector3.up;
					float target = 1f;
					float num3 = Mathf.Abs(num2);
					float num4 = 15f;
					if (num3 < 45f)
					{
						target = (num3 - num4) / (45f - num4);
						target = Mathf.Clamp01(target);
					}
					popupsWithUI[j].HUDUICanvasGroup.alpha = Mathf.MoveTowards(popupsWithUI[j].HUDUICanvasGroup.alpha, target, Time.deltaTime * 3f);
				}
			}
		}

		private bool ShouldCreateUI(WorldspacePopup popup)
		{
			return Vector3.Distance(popup.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position) <= popup.Range;
		}

		private WorldspacePopupUI CreateWorldspaceIcon(WorldspacePopup popup)
		{
			WorldspacePopupUI worldspacePopupUI = popup.CreateUI(WorldspaceContainer);
			activeWorldspaceUIs.Add(worldspacePopupUI);
			popupsWithUI.Add(popup);
			popup.WorldspaceUI = worldspacePopupUI;
			return worldspacePopupUI;
		}

		private RectTransform CreateHUDIcon(WorldspacePopup popup)
		{
			RectTransform component = Object.Instantiate(HudIconContainerPrefab, HudContainer).GetComponent<RectTransform>();
			WorldspacePopupUI hUDUIIcon = popup.CreateUI(component.Find("Container").GetComponent<RectTransform>());
			popup.HUDUI = component;
			popup.HUDUIIcon = hUDUIIcon;
			popup.HUDUICanvasGroup = component.GetComponent<CanvasGroup>();
			popup.HUDUICanvasGroup.alpha = 0f;
			activeHUDUIs.Add(component);
			return component;
		}

		private void DestroyWorldspaceIcon(WorldspacePopup popup)
		{
			for (int i = 0; i < activeWorldspaceUIs.Count; i++)
			{
				if (activeWorldspaceUIs[i].Popup == popup)
				{
					activeWorldspaceUIs[i].Destroy();
					activeWorldspaceUIs.RemoveAt(i);
					popupsWithUI.Remove(popup);
					break;
				}
			}
		}

		private void DestroyHUDIcon(WorldspacePopup popup)
		{
			for (int i = 0; i < activeHUDUIs.Count; i++)
			{
				if (activeHUDUIs[i].GetComponentInChildren<WorldspacePopupUI>().Popup == popup)
				{
					activeHUDUIs[i].GetComponentInChildren<WorldspacePopupUI>().Destroy();
					Object.Destroy(activeHUDUIs[i].gameObject);
					activeHUDUIs.RemoveAt(i);
					break;
				}
			}
		}
	}
}
