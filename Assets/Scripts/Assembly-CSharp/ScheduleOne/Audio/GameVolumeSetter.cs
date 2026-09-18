using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Audio
{
	public class GameVolumeSetter : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float VolumeMultiplier = 1f;

		private void Update()
		{
			Singleton<AudioManager>.Instance.SetGameVolumeMultipler(VolumeMultiplier);
		}
	}
}
