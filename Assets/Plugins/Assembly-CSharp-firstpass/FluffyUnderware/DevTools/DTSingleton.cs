using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FluffyUnderware.DevTools
{
	public class DTSingleton<T> : MonoBehaviour, IDTSingleton where T : MonoBehaviour, IDTSingleton
	{
		private static volatile T _instance;

		private static readonly object _lock = new object();

		public static bool HasInstance => _instance != null;

		[CanBeNull]
		public static T Instance
		{
			get
			{
				if (_instance != null)
				{
					return _instance;
				}
				lock (_lock)
				{
					if (_instance == null)
					{
						Object[] array = Object.FindObjectsOfType(typeof(T));
						for (int i = 0; i < array.Length; i++)
						{
							T val = (T)array[i];
							if (val != null)
							{
								_instance = val;
								break;
							}
						}
						if (_instance == null && SceneManager.GetActiveScene().isLoaded)
						{
							GameObject obj = new GameObject();
							obj.SetActive(value: false);
							_instance = obj.AddComponent<T>();
							obj.SetActive(value: true);
						}
					}
				}
				return _instance;
			}
		}

		protected static void InitializeStaticFields()
		{
			_instance = null;
		}

		public virtual void Awake()
		{
			bool flag = false;
			lock (_lock)
			{
				T instance = Instance;
				if (instance != null && instance.GetInstanceID() != GetInstanceID())
				{
					instance.MergeDoubleLoaded(this);
					flag = true;
				}
			}
			if (flag && !base.gameObject.Destroy(isUndoable: false, doPrefabCheck: true))
			{
				DTLog.LogError("[Curvy] Couldn't destroy duplicate singleton " + base.gameObject.name + " gameobject. Will destroy only its singleton component instead.");
				this.Destroy(isUndoable: false, doPrefabCheck: false);
			}
		}

		public virtual void MergeDoubleLoaded(IDTSingleton newInstance)
		{
		}
	}
}
