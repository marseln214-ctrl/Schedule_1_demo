using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGMeshResourceLoader : ICGResourceLoader
	{
		[EnvironmentAgnosticInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		protected static void InitializeOnLoad()
		{
			CGResourceHandler.RegisterResourceLoader("Mesh", new CGMeshResourceLoader());
		}

		public Component Create(CGModule cgModule, string context)
		{
			return cgModule.Generator.PoolManager.GetComponentPool<CGMeshResource>().Pop();
		}

		public void Destroy(CGModule cgModule, Component obj, string context, bool kill)
		{
			if (obj != null)
			{
				if (kill)
				{
					obj.gameObject.Destroy(isUndoable: false, doPrefabCheck: false);
					return;
				}
				obj.StripComponents(typeof(CGMeshResource), typeof(MeshFilter), typeof(MeshRenderer));
				cgModule.Generator.PoolManager.GetComponentPool<CGMeshResource>().Push(obj);
			}
		}
	}
}
