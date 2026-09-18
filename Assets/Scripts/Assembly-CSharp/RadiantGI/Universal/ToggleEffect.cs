using UnityEngine;
using UnityEngine.Rendering;

namespace RadiantGI.Universal
{
	public class ToggleEffect : MonoBehaviour
	{
		public VolumeProfile profile;

		private RadiantGlobalIllumination radiant;

		private void Start()
		{
			profile.TryGet<RadiantGlobalIllumination>(out radiant);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				radiant.active = !radiant.active;
			}
		}
	}
}
