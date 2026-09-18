using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public interface ICGResourceLoader
	{
		[NotNull]
		Component Create(CGModule cgModule, [NotNull] string context);

		void Destroy(CGModule cgModule, Component obj, [NotNull] string context, bool kill);
	}
}
