using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public class EnvironmentAgnosticInitializeOnLoadMethodAttribute : RuntimeInitializeOnLoadMethodAttribute
	{
		public EnvironmentAgnosticInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType loadType)
			: base(loadType)
		{
		}
	}
}
