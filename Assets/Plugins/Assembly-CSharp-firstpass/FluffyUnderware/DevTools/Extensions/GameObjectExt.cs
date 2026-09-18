using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class GameObjectExt
	{
		public static GameObject DuplicateGameObject(this GameObject source, Transform newParent, bool keepPrefabReference = false)
		{
			if (!source)
			{
				return null;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(source.gameObject);
			if ((bool)gameObject)
			{
				gameObject.transform.parent = newParent;
			}
			return gameObject;
		}

		public static void StripComponents(this GameObject go, params Type[] toKeep)
		{
			List<Type> list = new List<Type>(toKeep)
			{
				typeof(Transform),
				typeof(RectTransform)
			};
			Component[] components = go.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				if (!list.Contains(components[i].GetType()))
				{
					components[i].Destroy(isUndoable: false, doPrefabCheck: false);
				}
			}
		}

		public static T UndoableAddComponent<T>(this GameObject gameObject) where T : Component
		{
			return gameObject.AddComponent<T>();
		}
	}
}
