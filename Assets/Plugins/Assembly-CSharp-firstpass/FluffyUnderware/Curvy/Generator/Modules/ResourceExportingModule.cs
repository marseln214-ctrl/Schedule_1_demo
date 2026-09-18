using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	public abstract class ResourceExportingModule : CGModule
	{
		public GameObject SaveToScene(Transform parent = null)
		{
			GetManagedResources(out var components, out var _);
			if (components.Count == 0)
			{
				return null;
			}
			GameObject gameObject = new GameObject(base.ModuleName + " Exported Resources");
			gameObject.transform.parent = parent;
			for (int i = 0; i < components.Count; i++)
			{
				SaveResourceToScene(components[i], gameObject.transform);
			}
			gameObject.transform.position = base.transform.position;
			gameObject.transform.rotation = base.transform.rotation;
			gameObject.transform.localScale = base.transform.localScale;
			return gameObject;
		}

		protected abstract GameObject SaveResourceToScene(Component managedResource, Transform newParent);
	}
}
