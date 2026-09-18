using System.Collections;
using UnityEngine;

public class GarageDoorController : MonoBehaviour
{
	public GarageDoorStatus doorStatus;

	public Transform garageDoor;

	public Quaternion targetRotation = new Quaternion(80f, 0f, 0f, 0f);

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("MainCamera"))
		{
			if (Input.GetKeyUp(KeyCode.E) && !doorStatus.doorIsOpen && doorStatus.canRotate)
			{
				doorStatus.canRotate = false;
				StartCoroutine(Rotate(Vector3.right, -80f));
			}
			if (Input.GetKeyUp(KeyCode.E) && doorStatus.doorIsOpen && doorStatus.canRotate)
			{
				doorStatus.canRotate = false;
				StartCoroutine(Rotate(Vector3.right, 80f));
			}
		}
	}

	private IEnumerator Rotate(Vector3 axis, float angle, float duration = 1f)
	{
		Quaternion from = garageDoor.rotation;
		Quaternion to = garageDoor.rotation;
		to *= Quaternion.Euler(axis * angle);
		float elapsed = 0f;
		while (elapsed < duration)
		{
			garageDoor.rotation = Quaternion.Slerp(from, to, elapsed / duration);
			elapsed += Time.deltaTime;
			yield return null;
		}
		garageDoor.rotation = to;
		doorStatus.doorIsOpen = !doorStatus.doorIsOpen;
		doorStatus.canRotate = true;
	}
}
