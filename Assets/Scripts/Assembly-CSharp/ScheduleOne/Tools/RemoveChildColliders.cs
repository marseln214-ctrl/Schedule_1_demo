using UnityEngine;

namespace ScheduleOne.Tools
{
	public class RemoveChildColliders : MonoBehaviour
	{
		private void Start()
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i]);
			}
		}
	}
}
