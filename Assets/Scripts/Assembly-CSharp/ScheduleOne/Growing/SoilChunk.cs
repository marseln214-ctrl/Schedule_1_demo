using System.Collections;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.Growing
{
	public class SoilChunk : Clickable
	{
		public Transform EndTransform;

		public float LerpTime = 0.4f;

		private Vector3 localPos_Start;

		private Vector3 localEulerAngles_Start;

		private Vector3 localScale_Start;

		private Coroutine lerpRoutine;

		public float CurrentLerp { get; protected set; }

		protected virtual void Awake()
		{
			localPos_Start = base.transform.localPosition;
			localEulerAngles_Start = base.transform.localEulerAngles;
			localScale_Start = base.transform.localScale;
		}

		public void SetLerpedTransform(float _lerp)
		{
			CurrentLerp = Mathf.Clamp(_lerp, 0f, 1f);
			base.transform.localPosition = Vector3.Lerp(localPos_Start, EndTransform.localPosition, CurrentLerp);
			base.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(localEulerAngles_Start), Quaternion.Euler(EndTransform.localEulerAngles), CurrentLerp);
			base.transform.localScale = Vector3.Lerp(localScale_Start, EndTransform.localScale, CurrentLerp);
		}

		public override void StartClick(RaycastHit hit)
		{
			base.StartClick(hit);
			ClickableEnabled = false;
			StopLerp();
			lerpRoutine = StartCoroutine(Lerp());
			IEnumerator Lerp()
			{
				for (float i = 0f; i < LerpTime; i += Time.deltaTime)
				{
					SetLerpedTransform(Mathf.Lerp(0f, 1f, i / LerpTime));
					yield return new WaitForEndOfFrame();
				}
				SetLerpedTransform(1f);
				lerpRoutine = null;
			}
		}

		public void StopLerp()
		{
			if (lerpRoutine != null)
			{
				StopCoroutine(lerpRoutine);
			}
		}
	}
}
