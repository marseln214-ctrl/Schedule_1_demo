using UnityEngine;

public class Rotate : MonoBehaviour
{
	public float Speed = 5f;

	public Vector3 Axis = Vector3.up;

	private void Update()
	{
		base.transform.Rotate(Axis, Speed * Time.deltaTime, Space.Self);
	}
}
