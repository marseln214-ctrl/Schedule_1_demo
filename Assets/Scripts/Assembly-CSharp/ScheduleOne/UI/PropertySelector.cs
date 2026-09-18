using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class PropertySelector : MonoBehaviour
	{
		public delegate void PropertySelected(ScheduleOne.Property.Property p);

		[Header("References")]
		[SerializeField]
		protected GameObject container;

		[SerializeField]
		protected RectTransform buttonContainer;

		[Header("Prefabs")]
		[SerializeField]
		protected GameObject buttonPrefab;

		private PropertySelected pCallback;

		public bool isOpen => container.activeSelf;

		protected virtual void Awake()
		{
			ScheduleOne.Property.Property.onPropertyAcquired = (ScheduleOne.Property.Property.PropertyChange)Delegate.Combine(ScheduleOne.Property.Property.onPropertyAcquired, new ScheduleOne.Property.Property.PropertyChange(PropertyAcquired));
			container.SetActive(value: false);
		}

		protected virtual void Start()
		{
			GameInput.RegisterExitListener(Exit, 5);
		}

		public virtual void Exit(ExitAction exit)
		{
			if (!exit.used && exit.exitType != 0 && container.activeSelf)
			{
				exit.used = true;
				Close(reenableShit: true);
			}
		}

		public void OpenSelector(PropertySelected p)
		{
			pCallback = p;
			container.SetActive(value: true);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		}

		private void PropertyAcquired(ScheduleOne.Property.Property p)
		{
		}

		private void SelectProperty(ScheduleOne.Property.Property p)
		{
			pCallback(p);
			Close(reenableShit: false);
		}

		private void Close(bool reenableShit)
		{
			container.SetActive(value: false);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			if (reenableShit)
			{
				PlayerSingleton<PlayerMovement>.Instance.canMove = true;
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
			}
		}
	}
}
