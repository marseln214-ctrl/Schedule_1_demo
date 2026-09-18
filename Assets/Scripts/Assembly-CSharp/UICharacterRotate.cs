using UnityEngine;
using UnityEngine.EventSystems;

public class UICharacterRotate : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public UIControllerDEMO uIController;

	public float mouseRotateCharacterPower = 8f;

	private bool toogle;

	public void OnPointerDown(PointerEventData eventData)
	{
		toogle = true;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		toogle = false;
		Cursor.lockState = CursorLockMode.None;
	}

	private void Update()
	{
		if (toogle)
		{
			uIController.CharacterCustomization.transform.localRotation = Quaternion.Euler(uIController.CharacterCustomization.transform.localEulerAngles + Vector3.up * (0f - Input.GetAxis("Mouse X")) * mouseRotateCharacterPower);
		}
	}
}
