using UnityEngine;

namespace LiquidVolumeFX
{
	public class RandomMove : MonoBehaviour
	{
		[Range(-10f, 10f)]
		public float right = 2f;

		[Range(-10f, 10f)]
		public float left = -2f;

		[Range(-10f, 10f)]
		public float back = 2f;

		[Range(-10f, 10f)]
		public float front = -1f;

		[Range(0f, 0.2f)]
		public float speed = 0.5f;

		[Range(0f, 2f)]
		public float rotationSpeed = 1f;

		[Range(0.1f, 2f)]
		public float randomSpeed;

		public bool automatic;

		private Vector3 velocity = Vector3.zero;

		private int flaskType;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F))
			{
				flaskType++;
				if (flaskType >= 3)
				{
					flaskType = 0;
				}
				base.transform.Find("SphereFlask").gameObject.SetActive(flaskType == 0);
				base.transform.Find("CylinderFlask").gameObject.SetActive(flaskType == 1);
				base.transform.Find("CubeFlask").gameObject.SetActive(flaskType == 2);
			}
			Vector3 vector = Vector3.zero;
			if (automatic)
			{
				if (Random.value > 0.99f)
				{
					vector = Vector3.right * (speed + (Random.value - 0.5f) * randomSpeed);
				}
			}
			else
			{
				if (Input.GetKey(KeyCode.RightArrow))
				{
					vector += Vector3.right * speed;
				}
				if (Input.GetKey(KeyCode.LeftArrow))
				{
					vector += Vector3.left * speed;
				}
				if (Input.GetKey(KeyCode.UpArrow))
				{
					vector += Vector3.forward * speed;
				}
				if (Input.GetKey(KeyCode.DownArrow))
				{
					vector += Vector3.back * speed;
				}
			}
			float num = 60f * Time.deltaTime;
			velocity += vector * num;
			float num2 = 0.005f * num;
			if (velocity.magnitude > num2)
			{
				velocity -= velocity.normalized * num2;
			}
			else
			{
				velocity = Vector3.zero;
			}
			base.transform.localPosition += velocity * num;
			if (Input.GetKey(KeyCode.W))
			{
				base.transform.Rotate(0f, 0f, rotationSpeed * num);
			}
			else if (Input.GetKey(KeyCode.S))
			{
				base.transform.Rotate(0f, 0f, (0f - rotationSpeed) * num);
			}
			if (base.transform.localPosition.x > right)
			{
				base.transform.localPosition = new Vector3(right, base.transform.localPosition.y, base.transform.localPosition.z);
				velocity.Set(0f, 0f, 0f);
			}
			if (base.transform.localPosition.x < left)
			{
				base.transform.localPosition = new Vector3(left, base.transform.localPosition.y, base.transform.localPosition.z);
				velocity.Set(0f, 0f, 0f);
			}
			if (base.transform.localPosition.z > back)
			{
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, back);
				velocity.Set(0f, 0f, 0f);
			}
			if (base.transform.localPosition.z < front)
			{
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, front);
				velocity.Set(0f, 0f, 0f);
			}
		}
	}
}
