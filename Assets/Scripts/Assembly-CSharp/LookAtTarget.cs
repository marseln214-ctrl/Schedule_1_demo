using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
	[SerializeField]
	private Transform _target;

	[SerializeField]
	private float _speed = 0.5f;

	private Vector3 _lookAtTarget;

	private void Update()
	{
		_lookAtTarget = Vector3.Lerp(_lookAtTarget, _target.position, Time.deltaTime * _speed);
		base.transform.LookAt(_lookAtTarget);
	}
}
