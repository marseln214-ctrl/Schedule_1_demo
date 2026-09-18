using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI
{
	public class UISpawner : MonoBehaviour
	{
		public RectTransform SpawnArea;

		public GameObject[] Prefabs;

		public float MinInterval = 1f;

		public float MaxInterval = 5f;

		public float SpawnRateMultiplier = 1f;

		public Vector2 MinScale = Vector2.one;

		public Vector2 MaxScale = Vector2.one;

		public bool UniformScale = true;

		private float nextSpawnTime;

		public UnityEvent<GameObject> OnSpawn;

		private void Start()
		{
			nextSpawnTime = Time.time + Random.Range(MinInterval, MaxInterval);
		}

		private void Update()
		{
			if (SpawnRateMultiplier == 0f || !(Time.time > nextSpawnTime))
			{
				return;
			}
			nextSpawnTime = Time.time + Random.Range(MinInterval, MaxInterval) / SpawnRateMultiplier;
			if (Prefabs.Length != 0)
			{
				GameObject gameObject = Object.Instantiate(Prefabs[Random.Range(0, Prefabs.Length)], base.transform);
				if (UniformScale)
				{
					float num = Random.Range(MinScale.x, MaxScale.x);
					gameObject.transform.localScale = new Vector3(num, num, 1f);
				}
				else
				{
					gameObject.transform.localScale = new Vector3(Random.Range(MinScale.x, MaxScale.x), Random.Range(MinScale.y, MaxScale.y), 1f);
				}
				gameObject.transform.localPosition = new Vector3(Random.Range((0f - SpawnArea.rect.width) / 2f, SpawnArea.rect.width / 2f), Random.Range((0f - SpawnArea.rect.height) / 2f, SpawnArea.rect.height / 2f), 0f);
				if (OnSpawn != null)
				{
					OnSpawn.Invoke(gameObject);
				}
			}
		}
	}
}
