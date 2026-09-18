using System;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public static class DTTime
	{
		[UsedImplicitly]
		[Obsolete("Will get removed since it is not used by Curvy, and needs maintenance to be compatible with Unity's Enter Play Mode Settings")]
		private static float _EditorDeltaTime;

		[UsedImplicitly]
		[Obsolete("Will get removed since it is not used by Curvy, and needs maintenance to be compatible with Unity's Enter Play Mode Settings")]
		private static float _EditorLastTime;

		public static double TimeSinceStartup => Time.realtimeSinceStartupAsDouble;

		[UsedImplicitly]
		[Obsolete("Seems to me that this is not working properly. Probably because InitializeEditorTime and UpdateEditorTime are never called. Fix this before using it")]
		public static float deltaTime
		{
			get
			{
				if (!Application.isPlaying)
				{
					return _EditorDeltaTime;
				}
				return Time.deltaTime;
			}
		}

		[UsedImplicitly]
		[Obsolete("Will get removed since it is not used by Curvy, and needs maintenance to be compatible with Unity's Enter Play Mode Settings")]
		public static void InitializeEditorTime()
		{
			_EditorLastTime = Time.realtimeSinceStartup;
			_EditorDeltaTime = 0f;
		}

		[UsedImplicitly]
		[Obsolete("Will get removed since it is not used by Curvy, and needs maintenance to be compatible with Unity's Enter Play Mode Settings")]
		public static void UpdateEditorTime()
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			_EditorDeltaTime = realtimeSinceStartup - _EditorLastTime;
			_EditorLastTime = realtimeSinceStartup;
		}
	}
}
