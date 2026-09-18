using ScheduleOne.NPCs;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Animation
{
	public class AvatarSeat : MonoBehaviour
	{
		public Transform SittingPoint;

		public Transform AccessPoint;

		public bool IsOccupied => Occupant != null;

		public NPC Occupant { get; protected set; }

		private void Awake()
		{
		}

		public void SetOccupant(NPC npc)
		{
			if (npc != null && IsOccupied)
			{
				Debug.LogWarning("Seat is already occupied");
			}
			else
			{
				Occupant = npc;
			}
		}
	}
}
