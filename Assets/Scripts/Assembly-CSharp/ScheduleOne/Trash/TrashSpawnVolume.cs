using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Trash
{
	public class TrashSpawnVolume : MonoBehaviour
	{
		public BoxCollider CreatonVolume;

		public BoxCollider DetectionVolume;

		public int TrashLimit = 10;

		public void Awake()
		{
			NetworkSingleton<TimeManager>.Instance._onSleepStart.AddListener(SleepStart);
		}

		private void OnDestroy()
		{
			if (NetworkSingleton<TimeManager>.InstanceExists)
			{
				NetworkSingleton<TimeManager>.Instance._onSleepStart.RemoveListener(SleepStart);
			}
		}

		public void SleepStart()
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			Collider[] array = Physics.OverlapBox(DetectionVolume.transform.TransformPoint(DetectionVolume.center), Vector3.Scale(DetectionVolume.size, DetectionVolume.transform.lossyScale) * 0.5f, DetectionVolume.transform.rotation, 1 << LayerMask.NameToLayer("Trash"), QueryTriggerInteraction.Collide);
			int num = 0;
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (num >= TrashLimit)
				{
					break;
				}
				if (collider.GetComponentInParent<TrashItem>() != null)
				{
					num++;
				}
			}
			num = Mathf.Max(Random.Range(0, TrashLimit - num), 0);
			for (int j = num; j < TrashLimit; j++)
			{
				TrashItem randomGeneratableTrashPrefab = NetworkSingleton<TrashManager>.Instance.GetRandomGeneratableTrashPrefab();
				Vector3 posiiton = new Vector3(Random.Range(CreatonVolume.bounds.min.x, CreatonVolume.bounds.max.x), Random.Range(CreatonVolume.bounds.min.y, CreatonVolume.bounds.max.y), Random.Range(CreatonVolume.bounds.min.z, CreatonVolume.bounds.max.z));
				NetworkSingleton<TrashManager>.Instance.CreateTrashItem(randomGeneratableTrashPrefab.ID, posiiton, Random.rotation).SetContinuousCollisionDetection();
			}
		}
	}
}
