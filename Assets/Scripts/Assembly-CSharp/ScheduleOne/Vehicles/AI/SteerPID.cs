using UnityEngine;

namespace ScheduleOne.Vehicles.AI
{
	public class SteerPID
	{
		private float error_old;

		private float error_sum;

		public float GetNewValue(float error, PID_Parameters pid_parameters)
		{
			float num = (0f - pid_parameters.P) * error;
			error_sum = AddValueToAverage(error_sum, Time.deltaTime * error, 1000f);
			float num2 = num - pid_parameters.I * error_sum;
			float num3 = (error - error_old) / Time.deltaTime;
			float result = num2 - pid_parameters.D * num3;
			error_old = error;
			return result;
		}

		public static float AddValueToAverage(float oldAverage, float valueToAdd, float count)
		{
			return (oldAverage * count + valueToAdd) / (count + 1f);
		}
	}
}
