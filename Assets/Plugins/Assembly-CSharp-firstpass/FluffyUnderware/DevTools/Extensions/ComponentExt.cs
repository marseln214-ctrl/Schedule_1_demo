using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class ComponentExt
	{
		public static void StripComponents(this Component c, params Type[] toKeep)
		{
			if (toKeep.Length == 0)
			{
				c.gameObject.StripComponents(c.GetType());
			}
			else
			{
				c.gameObject.StripComponents(toKeep);
			}
		}

		[UsedImplicitly]
		[Obsolete]
		public static GameObject AddChildGameObject(this Component c, string name)
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.SetParent(c.transform);
			return gameObject;
		}

		[UsedImplicitly]
		[Obsolete]
		public static T AddChildGameObject<T>(this Component c, string name) where T : Component
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.SetParent(c.transform);
			return gameObject.AddComponent<T>();
		}

		[NotNull]
		public static T DuplicateGameObject<T>([NotNull] this Component source, [CanBeNull] Transform newParent) where T : Component
		{
			if (source.gameObject == null)
			{
				throw new ArgumentException("source.gameObject is null");
			}
			GameObject gameObject = source.gameObject;
			int num = new List<Component>(gameObject.GetComponents<Component>()).IndexOf(source);
			return (T)UnityEngine.Object.Instantiate(gameObject, newParent, worldPositionStays: false).GetComponents<Component>()[num];
		}

		[UsedImplicitly]
		[Obsolete("Use the other DuplicateGameObject method instead")]
		[CanBeNull]
		public static T DuplicateGameObject<T>(this Component source, Transform newParent, bool keepPrefabConnection) where T : Component
		{
			if (!source || !source.gameObject)
			{
				return null;
			}
			int num = new List<Component>(source.gameObject.GetComponents<Component>()).IndexOf(source);
			GameObject gameObject = UnityEngine.Object.Instantiate(source.gameObject, newParent, worldPositionStays: false);
			if ((bool)gameObject)
			{
				return gameObject.GetComponents<Component>()[num] as T;
			}
			return null;
		}

		[UsedImplicitly]
		[Obsolete("Use the other DuplicateGameObject method instead")]
		public static Component DuplicateGameObject(this Component source, Transform newParent, bool keepPrefabConnection = false)
		{
			if (!source || !source.gameObject || !newParent)
			{
				return null;
			}
			int num = new List<Component>(source.gameObject.GetComponents<Component>()).IndexOf(source);
			GameObject gameObject = UnityEngine.Object.Instantiate(source.gameObject);
			if ((bool)gameObject)
			{
				gameObject.transform.SetParent(newParent, worldPositionStays: false);
				return gameObject.GetComponents<Component>()[num];
			}
			return null;
		}
	}
}
