using UnityEngine;

namespace ScheduleOne.Doors
{
	public class RollerDoor : MonoBehaviour
	{
		[Header("Settings")]
		public Transform Door;

		public Vector3 LocalPos_Open;

		public Vector3 LocalPos_Closed;

		public float LerpTime = 1f;

		public GameObject Blocker;

		private Vector3 startPos = Vector3.zero;

		private float timeSinceValueChange;

		public bool IsOpen { get; protected set; } = true;


		private void Awake()
		{
			Door.localPosition = (IsOpen ? LocalPos_Open : LocalPos_Closed);
		}

		private void LateUpdate()
		{
			timeSinceValueChange += Time.deltaTime;
			if (timeSinceValueChange < LerpTime)
			{
				Vector3 b = (IsOpen ? LocalPos_Open : LocalPos_Closed);
				Door.localPosition = Vector3.Lerp(startPos, b, timeSinceValueChange / LerpTime);
			}
			else
			{
				Door.localPosition = (IsOpen ? LocalPos_Open : LocalPos_Closed);
			}
			if (Blocker != null)
			{
				Blocker.gameObject.SetActive(!IsOpen);
			}
		}

		public void Open()
		{
			if (!IsOpen && CanOpen())
			{
				IsOpen = true;
				timeSinceValueChange = 0f;
				startPos = Door.localPosition;
			}
		}

		public void Close()
		{
			if (IsOpen)
			{
				IsOpen = false;
				timeSinceValueChange = 0f;
				startPos = Door.localPosition;
			}
		}

		protected virtual bool CanOpen()
		{
			return true;
		}
	}
}
