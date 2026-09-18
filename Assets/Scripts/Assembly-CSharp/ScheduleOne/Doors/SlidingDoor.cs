using System.Collections;
using UnityEngine;

namespace ScheduleOne.Doors
{
	public class SlidingDoor : MonoBehaviour
	{
		[Header("Settings")]
		public Transform DoorTransform;

		public Transform ClosedPosition;

		public Transform OpenPosition;

		public float SlideDuration = 3f;

		public AnimationCurve SlideCurve;

		private Coroutine MoveRoutine;

		public bool IsOpen { get; protected set; }

		public virtual void Opened(EDoorSide openSide)
		{
			IsOpen = true;
			Move();
		}

		public virtual void Closed()
		{
			IsOpen = false;
			Move();
		}

		private void Move()
		{
			if (MoveRoutine != null)
			{
				StopCoroutine(MoveRoutine);
			}
			MoveRoutine = StartCoroutine(Move());
			IEnumerator Move()
			{
				Vector3 start = DoorTransform.position;
				Vector3 end = (IsOpen ? OpenPosition.position : ClosedPosition.position);
				for (float i = 0f; i < SlideDuration; i += Time.deltaTime)
				{
					DoorTransform.position = Vector3.Lerp(start, end, SlideCurve.Evaluate(i / SlideDuration));
					yield return new WaitForEndOfFrame();
				}
				DoorTransform.position = end;
				MoveRoutine = null;
			}
		}
	}
}
