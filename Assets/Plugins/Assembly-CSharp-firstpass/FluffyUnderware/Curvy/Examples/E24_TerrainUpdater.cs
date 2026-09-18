using FluffyUnderware.Curvy.Generator;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E24_TerrainUpdater : MonoBehaviour
	{
		public CurvyGenerator CurvyGenerator;

		[UsedImplicitly]
		private void Update()
		{
			Vector3 position = base.transform.position;
			position.x = 1f * Mathf.Sin(Time.time);
			position.z = 1f * Mathf.Cos(Time.time);
			base.transform.position = position;
			CurvyGenerator.Refresh(forceUpdate: true);
		}
	}
}
