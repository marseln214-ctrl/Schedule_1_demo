using UnityEngine;

namespace ScheduleOne.Tools
{
	public class ActiveOnMeshVisible : MonoBehaviour
	{
		public MeshRenderer Mesh;

		public GameObject[] ObjectsToActivate;

		public bool Reverse;

		private bool isVisible = true;

		private void LateUpdate()
		{
			if (Mesh.isVisible && !isVisible)
			{
				isVisible = true;
				GameObject[] objectsToActivate = ObjectsToActivate;
				for (int i = 0; i < objectsToActivate.Length; i++)
				{
					objectsToActivate[i].SetActive(!Reverse);
				}
			}
			else if (!Mesh.isVisible && isVisible)
			{
				isVisible = false;
				GameObject[] objectsToActivate = ObjectsToActivate;
				for (int i = 0; i < objectsToActivate.Length; i++)
				{
					objectsToActivate[i].SetActive(Reverse ? true : false);
				}
			}
		}
	}
}
