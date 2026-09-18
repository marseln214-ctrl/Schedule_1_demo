using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public abstract class PersistentSingleton<T> : Singleton<T> where T : Singleton<T>
	{
		protected override void Awake()
		{
			base.Awake();
			if (!Destroyed)
			{
				base.transform.SetParent(null);
				Object.DontDestroyOnLoad(base.gameObject);
			}
		}
	}
}
