using EasyButtons;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class PlayAnimation : MonoBehaviour
	{
		[Button]
		public void Play()
		{
			GetComponent<Animation>().Play();
		}

		public void Play(string animationName)
		{
			GetComponent<Animation>().Play(animationName);
		}
	}
}
