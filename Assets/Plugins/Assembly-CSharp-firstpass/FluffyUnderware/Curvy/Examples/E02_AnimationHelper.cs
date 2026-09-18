using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E02_AnimationHelper : MonoBehaviour
	{
		public void Play(Animation animation)
		{
			animation.Play();
		}

		public void RewindThenPlay(Animation animation)
		{
			animation.Rewind();
			animation.Play();
		}
	}
}
