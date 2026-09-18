using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGGameObjectResourceLoader : ICGResourceLoader
	{
		[EnvironmentAgnosticInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		protected static void InitializeOnLoad()
		{
			CGResourceHandler.RegisterResourceLoader("GameObject", new CGGameObjectResourceLoader());
		}

		public Component Create(CGModule cgModule, string context)
		{
			return cgModule.Generator.PoolManager.GetPrefabPool(context).Pop().transform;
		}

		public void Destroy(CGModule cgModule, Component obj, string context, bool kill)
		{
			if (obj != null)
			{
				if (kill)
				{
					obj.gameObject.Destroy(isUndoable: false, doPrefabCheck: false);
				}
				else
				{
					cgModule.Generator.PoolManager.GetPrefabPool(context).Push(obj.gameObject);
				}
			}
		}
	}
}
