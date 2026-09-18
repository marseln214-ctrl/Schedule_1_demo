using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Cutscenes
{
	[RequireComponent(typeof(Animation))]
	public class Cutscene : MonoBehaviour
	{
		[Header("Settings")]
		public string Name = "Cutscene";

		public bool DisablePlayerControl = true;

		public bool OverrideFOV;

		public float CameraFOV = 70f;

		[Header("References")]
		public Transform CameraControl;

		[Header("Events")]
		public UnityEvent onPlay;

		public UnityEvent onEnd;

		private Animation animation;

		public bool IsPlaying { get; private set; }

		protected virtual void Awake()
		{
			animation = GetComponent<Animation>();
		}

		private void LateUpdate()
		{
			if (IsPlaying)
			{
				PlayerSingleton<PlayerCamera>.Instance.transform.position = CameraControl.position;
				PlayerSingleton<PlayerCamera>.Instance.transform.rotation = CameraControl.rotation;
			}
		}

		public void Play()
		{
			Console.Log("Playing cutscene: " + Name);
			animation.Play();
			IsPlaying = true;
			if (onPlay != null)
			{
				onPlay.Invoke();
			}
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement("Cutscene (" + Name + ")");
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraControl.position, CameraControl.rotation, 0f);
			Singleton<HUD>.Instance.canvas.enabled = false;
			if (DisablePlayerControl)
			{
				PlayerSingleton<PlayerMovement>.Instance.canMove = false;
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			}
			if (OverrideFOV)
			{
				PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(CameraFOV, 0f);
			}
		}

		public void InvokeEnd()
		{
			Console.Log("Cutscene ended: " + Name);
			animation.Stop();
			IsPlaying = false;
			if (onEnd != null)
			{
				onEnd.Invoke();
			}
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement("Cutscene (" + Name + ")");
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.25f);
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: true, returnToOriginalRotation: false);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			Singleton<HUD>.Instance.canvas.enabled = true;
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		}
	}
}
