using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGSplineResourceLoader : ICGResourceLoader
	{
		[EnvironmentAgnosticInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		protected static void InitializeOnLoad()
		{
			CGResourceHandler.RegisterResourceLoader("Spline", new CGSplineResourceLoader());
		}

		public Component Create(CGModule cgModule, string context)
		{
			CurvySpline curvySpline = CurvySpline.Create();
			curvySpline.transform.position = Vector3.zero;
			curvySpline.Closed = true;
			curvySpline.Add(new Vector3(0f, 0f, 0f), new Vector3(5f, 0f, 10f), new Vector3(-5f, 0f, 10f));
			return curvySpline;
		}

		public void Destroy(CGModule cgModule, Component obj, string context, bool kill)
		{
			if (obj != null)
			{
				obj.gameObject.Destroy(isUndoable: false, doPrefabCheck: false);
			}
		}
	}
}
