using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class TransformExt
	{
		public static void UndoableSetParent([NotNull] this Transform child, [NotNull] Transform newParent, bool worldPositionStays, [NotNull] string undoOperationName)
		{
			child.SetParent(newParent, worldPositionStays);
		}

		public static void DeleteChildren([NotNull] this Transform transform, bool isUndoable, bool doPrefabCheck)
		{
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < transform.childCount; i++)
			{
				list.Add(transform.GetChild(i));
			}
			foreach (Transform item in list)
			{
				item.gameObject.Destroy(isUndoable, doPrefabCheck);
			}
		}
	}
}
