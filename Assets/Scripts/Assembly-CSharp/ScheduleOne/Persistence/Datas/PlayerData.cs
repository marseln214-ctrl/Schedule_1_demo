using System;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas
{
	[Serializable]
	public class PlayerData : SaveData
	{
		public string PlayerCode;

		public Vector3 Position = Vector3.zero;

		public float Rotation;

		public bool IntroCompleted;

		public PlayerData(string playerCode, Vector3 playerPos, float playerRot, bool introCompleted)
		{
			PlayerCode = playerCode;
			Position = playerPos;
			Rotation = playerRot;
			IntroCompleted = introCompleted;
		}

		public PlayerData()
		{
		}
	}
}
