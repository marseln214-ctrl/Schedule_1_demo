using UnityEngine;

public class MouseMove : MonoBehaviour
{
	[SerializeField]
	private float _sensitivity = 0.5f;

	private Vector3 _originalPos;

	private void Start()
	{
		_originalPos = base.transform.position;
	}

	private void Update()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.x /= Screen.width;
		mousePosition.y /= Screen.height;
		mousePosition.x -= 0.5f;
		mousePosition.y -= 0.5f;
		mousePosition *= 2f * _sensitivity;
		base.transform.position = _originalPos + mousePosition;
	}
}
