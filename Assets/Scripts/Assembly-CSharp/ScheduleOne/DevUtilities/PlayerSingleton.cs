using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public abstract class PlayerSingleton<T> : MonoBehaviour where T : PlayerSingleton<T>
	{
		private static T instance;

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

		protected virtual void Awake()
		{
			OnStartClient(IsOwner: true);
		}

		protected virtual void Start()
		{
		}

		public virtual void OnStartClient(bool IsOwner)
		{
			if (!IsOwner)
			{
				Console.Log("Destroying non-local player singleton: " + base.name);
				Object.Destroy(this);
			}
			else if (instance != null)
			{
				Console.LogWarning("Multiple instances of " + base.name + " exist. Keeping prior instance reference.");
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
