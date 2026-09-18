using System;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class ObjectExt
	{
		[UsedImplicitly]
		[Obsolete("Use the other overload of this method")]
		public static bool Destroy(this UnityEngine.Object @object)
		{
			return @object.Destroy(isUndoable: true, doPrefabCheck: true);
		}

		public static bool Destroy(this UnityEngine.Object @object, bool isUndoable, bool doPrefabCheck)
		{
			if (DTUtility.IsInEditMode)
			{
				return false;
			}
			if (@object is Component)
			{
				UnityEngine.Object.DestroyImmediate(@object);
			}
			else
			{
				UnityEngine.Object.Destroy(@object);
			}
			return true;
		}

		public static string ToDumpString(this object o)
		{
			return new DTObjectDump(o).ToString();
		}
	}
}
