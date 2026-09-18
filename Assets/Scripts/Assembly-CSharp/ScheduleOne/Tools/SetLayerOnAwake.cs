using UnityEngine;

namespace ScheduleOne.Tools
{
	public class SetLayerOnAwake : MonoBehaviour
	{
		public LayerMask Layer;

		private void Awake()
		{
			base.gameObject.layer = Layer.value;
		}
	}
}
