using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E12_ToggleBehaviourByTrigger : MonoBehaviour
	{
		public Behaviour UIElement;

		[UsedImplicitly]
		private void OnTriggerEnter()
		{
			if ((bool)UIElement)
			{
				UIElement.enabled = !UIElement.enabled;
			}
		}
	}
}
