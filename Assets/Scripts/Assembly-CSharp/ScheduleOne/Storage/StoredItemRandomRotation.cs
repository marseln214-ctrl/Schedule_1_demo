using UnityEngine;

namespace ScheduleOne.Storage
{
	public class StoredItemRandomRotation : MonoBehaviour
	{
		public Transform ItemContainer;

		public void Awake()
		{
			ItemContainer.localEulerAngles = new Vector3(ItemContainer.localEulerAngles.x, Random.Range(0f, 360f), ItemContainer.localEulerAngles.z);
		}
	}
}
