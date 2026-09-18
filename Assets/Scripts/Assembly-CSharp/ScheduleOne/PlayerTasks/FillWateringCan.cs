using ScheduleOne.DevUtilities;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property.Utilities.Water;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class FillWateringCan : Task
	{
		protected Tap tap;

		protected WateringCanInstance instance;

		protected WateringCanVisuals visuals;

		private bool audioPlayed;

		public new string TaskName { get; protected set; } = "Fill watering can";


		public FillWateringCan(Tap _tap, WateringCanInstance _instance)
		{
			tap = _tap;
			instance = _instance;
			ClickDetectionEnabled = true;
			tap.SetPlayerUser(Player.Local.NetworkObject);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(tap.CameraPos.position, tap.CameraPos.rotation, 0.25f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.25f);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			base.CurrentInstruction = "Click and hold tap to refill watering can";
			visuals = tap.CreateWateringCanModel_Local(instance.ID, force: true).GetComponent<WateringCanVisuals>();
			visuals.SetFillLevel(instance.CurrentFillAmount / 15f);
			visuals.FillSound.VolumeMultiplier = 0f;
			tap.SendWateringCanModel(instance.ID);
			tap.HandleClickable.onClickStart.AddListener(HandleClickStart);
			tap.HandleClickable.onClickEnd.AddListener(HandleClickEnd);
		}

		public override void Update()
		{
			base.Update();
			if (tap.ActualFlowRate > 0f)
			{
				instance.ChangeFillAmount(tap.ActualFlowRate * Time.deltaTime);
				if (!visuals.FillSound.isPlaying && !audioPlayed)
				{
					visuals.FillSound.Play();
					audioPlayed = true;
				}
				visuals.FillSound.VolumeMultiplier = Mathf.MoveTowards(visuals.FillSound.VolumeMultiplier, 1f, Time.deltaTime * 4f);
			}
			else
			{
				audioPlayed = false;
				if (visuals.FillSound.isPlaying)
				{
					visuals.FillSound.VolumeMultiplier = Mathf.MoveTowards(visuals.FillSound.VolumeMultiplier, 0f, Time.deltaTime * 4f);
					if (visuals.FillSound.VolumeMultiplier <= 0f)
					{
						visuals.FillSound.Stop();
					}
				}
			}
			visuals.SetFillLevel(instance.CurrentFillAmount / 15f);
			if (instance.CurrentFillAmount >= 15f)
			{
				Success();
			}
			else if (tap.ActualFlowRate > 0f && instance.CurrentFillAmount >= 15f)
			{
				visuals.SetOverflowParticles(enabled: true);
			}
			else
			{
				visuals.SetOverflowParticles(enabled: false);
			}
		}

		public override void StopTask()
		{
			tap.SetHeldOpen(open: false);
			tap.SetPlayerUser(null);
			tap.SendClearWateringCanModelModel();
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.25f);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.25f);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			Object.Destroy(visuals.gameObject);
			base.StopTask();
		}

		private void HandleClickStart(RaycastHit hit)
		{
			tap.SetHeldOpen(open: true);
		}

		private void HandleClickEnd()
		{
			tap.SetHeldOpen(open: false);
		}
	}
}
