using UnityEngine;

namespace ScheduleOne.Skating
{
	[RequireComponent(typeof(Skateboard))]
	public class SkateboardVisuals : MonoBehaviour
	{
		[Header("Settings")]
		public float MaxBoardLean = 8f;

		public float BoardLeanRate = 2f;

		[Header("References")]
		public Transform Board;

		private Skateboard skateboard;

		private void Awake()
		{
			skateboard = GetComponent<Skateboard>();
		}

		private void LateUpdate()
		{
			Vector3 euler = new Vector3(0f, 0f, skateboard.CurrentSteerInput * (0f - MaxBoardLean));
			Board.localRotation = Quaternion.Lerp(Board.localRotation, Quaternion.Euler(euler), Time.deltaTime * BoardLeanRate);
		}
	}
}
