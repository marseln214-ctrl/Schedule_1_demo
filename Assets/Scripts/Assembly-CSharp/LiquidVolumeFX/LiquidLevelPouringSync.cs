using UnityEngine;

namespace LiquidVolumeFX
{
	public class LiquidLevelPouringSync : MonoBehaviour
	{
		public float fillSpeed = 0.01f;

		public float sinkFactor = 0.1f;

		private LiquidVolume lv;

		private Rigidbody rb;

		private void Start()
		{
			rb = GetComponent<Rigidbody>();
			lv = base.transform.parent.GetComponent<LiquidVolume>();
			UpdateColliderPos();
		}

		private void OnParticleCollision(GameObject other)
		{
			if (lv.level < 1f)
			{
				lv.level += fillSpeed;
			}
			UpdateColliderPos();
		}

		private void UpdateColliderPos()
		{
			Vector3 position = new Vector3(base.transform.position.x, lv.liquidSurfaceYPosition - base.transform.localScale.y * 0.5f - sinkFactor, base.transform.position.z);
			rb.position = position;
			if (lv.level >= 1f)
			{
				base.transform.localRotation = Quaternion.Euler(Random.value * 30f - 15f, Random.value * 30f - 15f, Random.value * 30f - 15f);
			}
			else
			{
				base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
		}
	}
}
