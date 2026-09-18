using UnityEngine;

namespace ScheduleOne.Properties.MixMaps
{
	public class Effect : MonoBehaviour
	{
		public Property Property;

		[Range(0.05f, 3f)]
		public float Radius = 0.5f;

		public Vector2 Position => new Vector2(base.transform.position.x, base.transform.position.z);

		public void OnValidate()
		{
			if (!(Property == null))
			{
				base.gameObject.name = Property.Name;
			}
		}
	}
}
