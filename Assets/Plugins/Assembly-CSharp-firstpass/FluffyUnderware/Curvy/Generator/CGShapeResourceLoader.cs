using FluffyUnderware.Curvy.Shapes;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGShapeResourceLoader : ICGResourceLoader
	{
		[EnvironmentAgnosticInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		protected static void InitializeOnLoad()
		{
			CGResourceHandler.RegisterResourceLoader("Shape", new CGShapeResourceLoader());
		}

		public Component Create(CGModule cgModule, string context)
		{
			CurvySpline curvySpline = CurvySpline.Create();
			curvySpline.transform.position = Vector3.zero;
			curvySpline.RestrictTo2D = true;
			curvySpline.Closed = true;
			curvySpline.Orientation = CurvyOrientation.None;
			curvySpline.gameObject.AddComponent<CSCircle>().Refresh();
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
