using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		private static T instance;

		protected bool Destroyed;

		public static bool InstanceExists => instance != null;

		public static T Instance
		{
			get
			{
				return instance;
			}
			protected set
			{
				instance = value;
			}
		}

		protected virtual void Start()
		{
		}

		protected virtual void Awake()
		{
			if (instance != null)
			{
				Console.LogWarning("Multiple instances of " + base.name + " exist. Destroying this instance.");
				Object.Destroy(base.gameObject);
			}
			else
			{
				instance = (T)this;
			}
		}

		protected virtual void OnDestroy()
		{
			if (instance == this)
			{
				instance = null;
			}
		}
	}
}
