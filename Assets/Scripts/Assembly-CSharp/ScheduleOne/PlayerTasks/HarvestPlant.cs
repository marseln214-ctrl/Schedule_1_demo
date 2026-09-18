using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.Interaction;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class HarvestPlant : Task
	{
		protected Pot pot;

		private int HarvestCount;

		private int HarvestTotal;

		private float rotation;

		private static bool hintShown;

		private static bool CanDrag;

		private AudioSourceController SoundLoop;

		public override string TaskName { get; protected set; } = "Harvest plant";


		public HarvestPlant(Pot _pot, bool canDrag, AudioSourceController soundLoopPrefab)
		{
			if (_pot == null)
			{
				Console.LogWarning("HarvestPlant: pot null");
				StopTask();
				return;
			}
			if (_pot.Plant == null)
			{
				Console.LogWarning("HarvestPlant: pot has no plant in it");
			}
			ClickDetectionEnabled = true;
			CanDrag = canDrag;
			ClickDetectionRadius = 0.02f;
			pot = _pot;
			pot.SetPlayerUser(Player.Local.NetworkObject);
			pot.PositionCameraContainer();
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(pot.FullshotPosition.position, pot.FullshotPosition.rotation, 0.25f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.25f);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			pot.Plant.Collider.enabled = false;
			pot.IntObj.GetComponent<Collider>().enabled = false;
			if (pot.AlignLeafDropToPlayer)
			{
				pot.LeafDropPoint.transform.rotation = Quaternion.LookRotation(Player.Local.Avatar.CenterPoint - pot.LeafDropPoint.position, Vector3.up);
			}
			HarvestTotal = pot.Plant.ActiveHarvestables.Count;
			UpdateInstructionText();
			if (soundLoopPrefab != null)
			{
				SoundLoop = Object.Instantiate(soundLoopPrefab, NetworkSingleton<GameManager>.Instance.Temp);
				SoundLoop.VolumeMultiplier = 0f;
				SoundLoop.transform.position = pot.transform.position + Vector3.up * 1f;
				SoundLoop.Play();
			}
			Singleton<InputPromptsCanvas>.Instance.LoadModule("harvestplant");
		}

		private void UpdateInstructionText()
		{
			if (!(pot == null) && !(pot.Plant == null))
			{
				if (CanDrag)
				{
					base.CurrentInstruction = "Click and hold over " + pot.Plant.HarvestTarget + " to harvest (" + HarvestCount + "/" + HarvestTotal + ")";
				}
				else
				{
					base.CurrentInstruction = "Click " + pot.Plant.HarvestTarget + " to harvest (" + HarvestCount + "/" + HarvestTotal + ")";
				}
			}
		}

		public override void StopTask()
		{
			base.StopTask();
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.25f);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.25f);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			if (pot.Plant != null)
			{
				pot.Plant.Collider.enabled = true;
			}
			if (SoundLoop != null)
			{
				Object.Destroy(SoundLoop.gameObject);
			}
			pot.IntObj.GetComponent<Collider>().enabled = true;
			pot.SetPlayerUser(null);
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
		}

		protected override void UpdateCursor()
		{
			if (GetHoveredHarvestable() != null)
			{
				Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Scissors);
			}
			else
			{
				Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
			}
		}

		public override void Update()
		{
			base.Update();
			if (pot == null || pot.Plant == null)
			{
				StopTask();
				return;
			}
			PlantHarvestable hoveredHarvestable = GetHoveredHarvestable();
			if (SoundLoop != null)
			{
				if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
				{
					SoundLoop.VolumeMultiplier = Mathf.MoveTowards(SoundLoop.VolumeMultiplier, 1f, Time.deltaTime * 4f);
				}
				else
				{
					SoundLoop.VolumeMultiplier = Mathf.MoveTowards(SoundLoop.VolumeMultiplier, 0f, Time.deltaTime * 4f);
				}
			}
			if (hoveredHarvestable != null)
			{
				if (!PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(hoveredHarvestable.Product.GetDefaultInstance(), hoveredHarvestable.ProductQuantity))
				{
					Singleton<MouseTooltip>.Instance.ShowIcon(Singleton<MouseTooltip>.Instance.Sprite_Cross, Singleton<MouseTooltip>.Instance.Color_Invalid);
					Singleton<MouseTooltip>.Instance.ShowTooltip("Inventory full", Singleton<MouseTooltip>.Instance.Color_Invalid);
				}
				else if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) || (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && CanDrag))
				{
					GameObject gameObject = Object.Instantiate(pot.Plant.SnipSound.gameObject);
					gameObject.transform.position = hoveredHarvestable.transform.position;
					gameObject.GetComponent<AudioSourceController>().PlayOneShot();
					Object.Destroy(gameObject, 1f);
					hoveredHarvestable.Harvest();
					HarvestCount++;
					UpdateInstructionText();
					if (pot.Plant == null)
					{
						Success();
					}
				}
			}
			if (GameInput.GetButton(GameInput.ButtonCode.Left))
			{
				rotation -= Time.deltaTime * 100f;
			}
			if (GameInput.GetButton(GameInput.ButtonCode.Right))
			{
				rotation += Time.deltaTime * 100f;
			}
			pot.OverrideRotation(rotation);
		}

		private PlantHarvestable GetHoveredHarvestable()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(3f, out var hit, Singleton<InteractionManager>.Instance.Interaction_SearchMask))
			{
				return hit.collider.gameObject.GetComponentInParent<PlantHarvestable>();
			}
			return null;
		}
	}
}
