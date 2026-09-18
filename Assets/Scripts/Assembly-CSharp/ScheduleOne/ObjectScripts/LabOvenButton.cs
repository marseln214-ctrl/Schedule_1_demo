using System.Collections;
using ScheduleOne.Misc;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class LabOvenButton : MonoBehaviour
	{
		public Transform Button;

		public Transform PressedTransform;

		public Transform DepressedTransform;

		public ToggleableLight Light;

		public Clickable Clickable;

		private Coroutine pressCoroutine;

		public bool Pressed { get; private set; }

		private void Start()
		{
			SetInteractable(interactable: false);
			Clickable.onClickStart.AddListener(Press);
		}

		public void SetInteractable(bool interactable)
		{
			Clickable.ClickableEnabled = interactable;
		}

		public void Press(RaycastHit hit)
		{
			SetPressed(pressed: true);
		}

		public void SetPressed(bool pressed)
		{
			if (Pressed == pressed)
			{
				return;
			}
			Pressed = pressed;
			Light.isOn = pressed;
			if (Pressed)
			{
				if (pressCoroutine != null)
				{
					StopCoroutine(pressCoroutine);
				}
				pressCoroutine = StartCoroutine(MoveButton(PressedTransform));
			}
			else
			{
				if (pressCoroutine != null)
				{
					StopCoroutine(pressCoroutine);
				}
				pressCoroutine = StartCoroutine(MoveButton(DepressedTransform));
			}
		}

		private IEnumerator MoveButton(Transform destination)
		{
			Vector3 startPos = Button.localPosition;
			Vector3 endPos = destination.localPosition;
			float lerpTime = 0.2f;
			for (float t = 0f; t < lerpTime; t += Time.deltaTime)
			{
				Button.localPosition = Vector3.Lerp(startPos, endPos, t / lerpTime);
				yield return null;
			}
			Button.localPosition = endPos;
			pressCoroutine = null;
		}
	}
}
