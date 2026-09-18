using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.UI.Input;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class ManagementWorldspaceCanvas : Singleton<ManagementWorldspaceCanvas>
	{
		public const float VISIBILITY_RANGE = 5f;

		public const float PROPERTY_CANVAS_RANGE = 50f;

		[Header("References")]
		public Canvas Canvas;

		public AnimationCurve ScaleCurve;

		public TransitLineVisuals TransitRouteVisualsPrefab;

		public InputPrompt CrosshairPrompt;

		[Header("Settings")]
		public LayerMask ObjectSelectionLayerMask;

		public Color HoveredOutlineColor = Color.white;

		public Color SelectedOutlineColor = Color.white;

		private List<IConfigurable> ShownConfigurables = new List<IConfigurable>();

		public IConfigurable HoveredConfigurable;

		private IConfigurable OutlinedConfigurable;

		public List<IConfigurable> SelectedConfigurables = new List<IConfigurable>();

		public bool IsOpen { get; protected set; }

		public ScheduleOne.Property.Property CurrentProperty => Player.Local.CurrentProperty;

		public void Open()
		{
			IsOpen = true;
			Canvas.enabled = true;
			for (int i = 0; i < SelectedConfigurables.Count; i++)
			{
				SelectedConfigurables[i].Selected();
				SelectedConfigurables[i].ShowOutline(SelectedOutlineColor);
			}
			UpdateInputPrompt();
		}

		public void Close(bool preserveSelection = false)
		{
			IsOpen = false;
			Canvas.enabled = false;
			if (OutlinedConfigurable != null && !SelectedConfigurables.Contains(OutlinedConfigurable))
			{
				OutlinedConfigurable.Deselected();
				OutlinedConfigurable.HideOutline();
				OutlinedConfigurable = null;
			}
			if (HoveredConfigurable != null)
			{
				HoveredConfigurable.HideOutline();
				HoveredConfigurable = null;
			}
			if (!preserveSelection)
			{
				ClearSelection();
			}
		}

		private void Update()
		{
			_ = Player.Local == null;
		}

		private void UpdateInputPrompt()
		{
			List<IConfigurable> list = new List<IConfigurable>();
			if (HoveredConfigurable != null && !SelectedConfigurables.Contains(HoveredConfigurable))
			{
				list.Add(HoveredConfigurable);
			}
			list.AddRange(SelectedConfigurables);
			if (list.Count == 0)
			{
				HideCrosshairPrompt();
				return;
			}
			bool flag = true;
			if (list.Count > 1)
			{
				for (int i = 0; i < list.Count - 1; i++)
				{
					if (list[i].ConfigurableType != list[i + 1].ConfigurableType)
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				HideCrosshairPrompt();
				return;
			}
			string typeName = ConfigurableType.GetTypeName(list[0].ConfigurableType);
			if (list.Count > 1)
			{
				ShowCrosshairPrompt("Manage " + list.Count + "x " + typeName);
			}
			else
			{
				ShowCrosshairPrompt("Manage " + typeName);
			}
		}

		private void UpdateUIs()
		{
			foreach (ScheduleOne.Property.Property ownedProperty in ScheduleOne.Property.Property.OwnedProperties)
			{
				float num = Vector3.Distance(ownedProperty.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position);
				ownedProperty.WorldspaceUIContainer.gameObject.SetActive(IsOpen && num < 50f);
			}
			List<IConfigurable> configurablesToShow = GetConfigurablesToShow();
			RemoveNullConfigurables();
			for (int i = 0; i < ShownConfigurables.Count; i++)
			{
				if (!configurablesToShow.Contains(ShownConfigurables[i]) && ShownConfigurables[i].WorldspaceUI.IsEnabled)
				{
					IConfigurable config = ShownConfigurables[i];
					ShownConfigurables[i].WorldspaceUI.Hide(delegate
					{
						ShownConfigurables.Remove(config);
					});
				}
			}
			for (int j = 0; j < configurablesToShow.Count; j++)
			{
				if (!ShownConfigurables.Contains(configurablesToShow[j]))
				{
					configurablesToShow[j].WorldspaceUI.Show();
					if (!ShownConfigurables.Contains(configurablesToShow[j]))
					{
						ShownConfigurables.Add(configurablesToShow[j]);
					}
				}
			}
		}

		private void LateUpdate()
		{
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				RemoveNullConfigurables();
				ShownConfigurables.Sort((IConfigurable a, IConfigurable b) => Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, a.UIPoint.position).CompareTo(Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, b.UIPoint.position)));
				for (int i = 0; i < ShownConfigurables.Count; i++)
				{
					ShownConfigurables[i].WorldspaceUI.SetInternalScale(ScaleCurve.Evaluate(Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, ShownConfigurables[i].UIPoint.position) / 5f));
					ShownConfigurables[i].WorldspaceUI.UpdatePosition(ShownConfigurables[i].UIPoint.position);
					ShownConfigurables[i].WorldspaceUI.transform.SetAsFirstSibling();
				}
			}
		}

		private void UpdateSelection()
		{
			if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
			{
				ClearSelection();
			}
			if (HoveredConfigurable == null)
			{
				if (OutlinedConfigurable != null && !SelectedConfigurables.Contains(OutlinedConfigurable))
				{
					OutlinedConfigurable.Deselected();
					OutlinedConfigurable.HideOutline();
					OutlinedConfigurable = null;
				}
				return;
			}
			if (HoveredConfigurable != null && HoveredConfigurable.IsBeingConfiguredByOtherPlayer)
			{
				HoveredConfigurable.Deselected();
				HoveredConfigurable.HideOutline();
				HoveredConfigurable = null;
				return;
			}
			for (int i = 0; i < SelectedConfigurables.Count; i++)
			{
				if (SelectedConfigurables[i].IsBeingConfiguredByOtherPlayer)
				{
					RemoveFromSelection(SelectedConfigurables[i]);
					i--;
				}
			}
			if (!SelectedConfigurables.Contains(HoveredConfigurable) && OutlinedConfigurable != HoveredConfigurable)
			{
				if (OutlinedConfigurable != null && !SelectedConfigurables.Contains(OutlinedConfigurable))
				{
					OutlinedConfigurable.Deselected();
					OutlinedConfigurable.HideOutline();
					OutlinedConfigurable = null;
				}
				HoveredConfigurable.Selected();
				HoveredConfigurable.ShowOutline(HoveredOutlineColor);
				OutlinedConfigurable = HoveredConfigurable;
			}
			if (HoveredConfigurable != null && HoveredConfigurable.CanBeSelected && GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
			{
				if (SelectedConfigurables.Contains(HoveredConfigurable))
				{
					RemoveFromSelection(HoveredConfigurable);
				}
				else
				{
					AddToSelection(HoveredConfigurable);
				}
			}
		}

		private void AddToSelection(IConfigurable config)
		{
			if (!SelectedConfigurables.Contains(config))
			{
				config.ShowOutline(SelectedOutlineColor);
				config.Selected();
				SelectedConfigurables.Add(config);
			}
		}

		private void RemoveFromSelection(IConfigurable config)
		{
			if (HoveredConfigurable != config)
			{
				config.Deselected();
				config.HideOutline();
			}
			else
			{
				config.ShowOutline(HoveredOutlineColor);
			}
			if (SelectedConfigurables.Contains(config))
			{
				SelectedConfigurables.Remove(config);
			}
		}

		private void ClearSelection()
		{
			IConfigurable[] array = SelectedConfigurables.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				RemoveFromSelection(array[i]);
			}
		}

		private void RemoveNullConfigurables()
		{
			for (int i = 0; i < ShownConfigurables.Count; i++)
			{
				if (ShownConfigurables[i].IsDestroyed)
				{
					ShownConfigurables.RemoveAt(i);
					i--;
				}
			}
		}

		private IConfigurable GetHoveredConfigurable()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(5f, out var hit, ObjectSelectionLayerMask))
			{
				IConfigurable componentInParent = hit.collider.GetComponentInParent<IConfigurable>();
				if (componentInParent != null)
				{
					return componentInParent;
				}
			}
			return null;
		}

		private List<IConfigurable> GetConfigurablesToShow()
		{
			List<IConfigurable> list = new List<IConfigurable>();
			if (CurrentProperty != null && CurrentProperty.IsOwned)
			{
				for (int i = 0; i < CurrentProperty.Configurables.Count; i++)
				{
					if (Vector3.Distance(CurrentProperty.Configurables[i].Transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position) <= 5f)
					{
						list.Add(CurrentProperty.Configurables[i]);
					}
				}
			}
			for (int j = 0; j < SelectedConfigurables.Count; j++)
			{
				if (!list.Contains(SelectedConfigurables[j]))
				{
					list.Add(SelectedConfigurables[j]);
				}
			}
			if (!list.Contains(HoveredConfigurable) && HoveredConfigurable != null)
			{
				list.Add(HoveredConfigurable);
			}
			return list;
		}

		public void ShowCrosshairPrompt(string message)
		{
			CrosshairPrompt.SetLabel(message);
			CrosshairPrompt.gameObject.SetActive(value: true);
			CrosshairPrompt.transform.SetAsLastSibling();
		}

		public void HideCrosshairPrompt()
		{
			CrosshairPrompt.gameObject.SetActive(value: false);
		}
	}
}
