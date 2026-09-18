using UnityEngine;

namespace ScheduleOne.Noise
{
	public static class NoiseUtility
	{
		public static void EmitNoise(Vector3 origin, ENoiseType type, float range, GameObject source = null)
		{
			NoiseEvent nEvent = new NoiseEvent(origin, range, type, source);
			for (int i = 0; i < Listener.listeners.Count; i++)
			{
				if (Listener.listeners[i].enabled && Vector3.Magnitude(origin - Listener.listeners[i].HearingOrigin.position) <= Listener.listeners[i].Sensitivity * range)
				{
					Listener.listeners[i].Notify(nEvent);
				}
			}
		}
	}
}
