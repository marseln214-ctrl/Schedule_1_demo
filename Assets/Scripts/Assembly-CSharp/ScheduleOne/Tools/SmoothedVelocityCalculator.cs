using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class SmoothedVelocityCalculator : MonoBehaviour
	{
		public Vector3 Velocity = Vector3.zero;

		[Header("Settings")]
		public float SampleLength = 0.2f;

		public float MaxReasonableVelocity = 25f;

		private List<Tuple<Vector3, float>> VelocityHistory = new List<Tuple<Vector3, float>>();

		private int maxSamples = 20;

		private Vector3 lastFramePosition = Vector3.zero;

		private bool zeroOut;

		private void Start()
		{
			lastFramePosition = base.transform.position;
		}

		protected virtual void Update()
		{
			if (zeroOut)
			{
				Velocity = Vector3.zero;
				return;
			}
			Vector3 item = (base.transform.position - lastFramePosition) / Time.deltaTime;
			if (item.magnitude <= MaxReasonableVelocity)
			{
				VelocityHistory.Add(new Tuple<Vector3, float>(item, Time.timeSinceLevelLoad));
			}
			if (VelocityHistory.Count > maxSamples)
			{
				VelocityHistory.RemoveAt(0);
			}
			Velocity = GetAverageVelocity();
			lastFramePosition = base.transform.position;
		}

		private Vector3 GetAverageVelocity()
		{
			Vector3 zero = Vector3.zero;
			int num = 0;
			int num2 = VelocityHistory.Count - 1;
			while (num2 >= 0 && !(Time.timeSinceLevelLoad - VelocityHistory[num2].Item2 > SampleLength))
			{
				zero += VelocityHistory[num2].Item1;
				num++;
				num2--;
			}
			if (num == 0)
			{
				return Vector3.zero;
			}
			return zero / num;
		}

		public void FlushBuffer()
		{
			VelocityHistory.Clear();
			Velocity = Vector3.zero;
			lastFramePosition = base.transform.position;
		}

		public void ZeroOut(float duration)
		{
			zeroOut = true;
			StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(duration);
				zeroOut = false;
			}
		}
	}
}
