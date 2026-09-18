using System;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[ExecuteAlways]
	[RequireComponent(typeof(MeshFilter))]
	[UsedImplicitly]
	[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
	public abstract class DuplicateEditorMesh : DTVersionedMonoBehaviour
	{
		private MeshFilter mFilter;

		public MeshFilter Filter
		{
			get
			{
				if ((object)mFilter == null)
				{
					mFilter = GetComponent<MeshFilter>();
				}
				return mFilter;
			}
		}
	}
}
