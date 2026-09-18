using System;

namespace ScheduleOne.DevUtilities
{
	[Serializable]
	public class PID
	{
		public float pFactor;

		public float iFactor;

		public float dFactor;

		private float integral;

		private float lastError;

		public PID(float pFactor, float iFactor, float dFactor)
		{
			this.pFactor = pFactor;
			this.iFactor = iFactor;
			this.dFactor = dFactor;
		}

		public float Update(float setpoint, float actual, float timeFrame)
		{
			float num = setpoint - actual;
			integral += num * timeFrame;
			float num2 = (num - lastError) / timeFrame;
			lastError = num;
			return num * pFactor + integral * iFactor + num2 * dFactor;
		}
	}
}
