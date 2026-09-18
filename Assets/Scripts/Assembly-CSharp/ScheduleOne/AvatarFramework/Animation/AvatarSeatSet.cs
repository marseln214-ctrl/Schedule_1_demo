using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Animation
{
	public class AvatarSeatSet : MonoBehaviour
	{
		public AvatarSeat[] Seats;

		public AvatarSeat GetFirstFreeSeat()
		{
			for (int i = 0; i < Seats.Length; i++)
			{
				if (!Seats[i].IsOccupied)
				{
					return Seats[i];
				}
			}
			Console.LogWarning("Failed to find a free seat! Returning the first seat.");
			return Seats[0];
		}

		public AvatarSeat GetRandomFreeSeat()
		{
			List<AvatarSeat> list = Seats.Where((AvatarSeat x) => !x.IsOccupied).ToList();
			if (list.Count == 0)
			{
				Console.LogWarning("Failed to find a free seat! Returning the first seat.");
				return Seats[0];
			}
			return list[Random.Range(0, list.Count)];
		}
	}
}
