using UnityEngine;

namespace LiquidVolumeFX
{
	public class FishAnimator : MonoBehaviour
	{
		private void Update()
		{
			Vector3 position = Camera.main.transform.position;
			base.transform.LookAt(new Vector3(0f - position.x, base.transform.position.y, 0f - position.z));
		}
	}
}
