using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.Lighting;
using ScheduleOne.PlayerScripts;
using ScheduleOne.ScriptableObjects;
using ScheduleOne.UI.Phone;
using UnityEngine;

namespace ScheduleOne.Calling
{
	public class PayPhone : MonoBehaviour
	{
		public const float RING_INTERVAL = 4f;

		public const float RING_RANGE = 9f;

		public BlinkingLight Light;

		public AudioSourceController RingSound;

		public AudioSourceController AnswerSound;

		public InteractableObject IntObj;

		public Transform CameraPosition;

		private float timeSinceLastRing = 100f;

		public PhoneCallData QueuedCall => Singleton<CallManager>.Instance.QueuedCallData;

		public PhoneCallData ActiveCall => Singleton<CallInterface>.Instance.ActiveCallData;

		public void FixedUpdate()
		{
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				timeSinceLastRing += Time.fixedDeltaTime;
				float num = Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, base.transform.position);
				Light.IsOn = QueuedCall != null && ActiveCall == null;
				if (num < 9f && QueuedCall != null && timeSinceLastRing >= 4f && ActiveCall == null)
				{
					timeSinceLastRing = 0f;
					RingSound.Play();
				}
			}
		}

		public void Hovered()
		{
			if (CanInteract())
			{
				IntObj.SetMessage("Answer phone");
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}

		public void Interacted()
		{
			if (CanInteract())
			{
				Singleton<CallInterface>.Instance.StartCall(QueuedCall, QueuedCall.CallerID);
				RingSound.Stop();
				AnswerSound.Play();
				PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0.2f);
				PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.2f);
			}
		}

		private bool CanInteract()
		{
			if (QueuedCall == null)
			{
				return false;
			}
			if (ActiveCall != null)
			{
				return false;
			}
			if (Singleton<CallInterface>.Instance.IsOpen)
			{
				return false;
			}
			return true;
		}
	}
}
