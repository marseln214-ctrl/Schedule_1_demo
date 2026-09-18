using FishNet;
using ScheduleOne.Doors;
using ScheduleOne.Misc;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Map
{
	public class AccessZone : MonoBehaviour
	{
		[Header("Settings")]
		public bool AllowExitWhenClosed;

		public bool AutoCloseDoor = true;

		[Header("References")]
		public DoorController[] Doors;

		public ToggleableLight[] Lights;

		public UnityEvent onOpen;

		public UnityEvent onClose;

		public bool IsOpen { get; protected set; }

		protected virtual void Awake()
		{
			IsOpen = true;
			SetIsOpen(open: false);
		}

		public void SetIsOpen(bool open)
		{
			bool isOpen = IsOpen;
			IsOpen = open;
			DoorController[] doors = Doors;
			foreach (DoorController doorController in doors)
			{
				if (IsOpen)
				{
					doorController.PlayerAccess = EDoorAccess.Open;
				}
				else if (AllowExitWhenClosed)
				{
					doorController.PlayerAccess = EDoorAccess.ExitOnly;
				}
				else
				{
					doorController.PlayerAccess = EDoorAccess.Locked;
				}
			}
			for (int j = 0; j < Lights.Length; j++)
			{
				Lights[j].isOn = IsOpen;
			}
			if (IsOpen && !isOpen && onOpen != null)
			{
				onOpen.Invoke();
			}
			if (!IsOpen && isOpen && onClose != null)
			{
				onClose.Invoke();
			}
			if (!InstanceFinder.IsServer || IsOpen || !AutoCloseDoor)
			{
				return;
			}
			doors = Doors;
			foreach (DoorController doorController2 in doors)
			{
				if ((!doorController2.openedByNPC || !(doorController2.timeSinceNPCSensed < 1f)) && doorController2.IsOpen && ((doorController2.timeSincePlayerSensed > 0.5f && doorController2.playerDetectedSinceOpened) || doorController2.timeSincePlayerSensed > 15f))
				{
					doorController2.SetIsOpen(null, open: false, EDoorSide.Interior);
				}
			}
		}
	}
}
