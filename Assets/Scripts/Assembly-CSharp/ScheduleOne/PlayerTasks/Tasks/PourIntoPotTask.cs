using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Trash;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.PlayerTasks.Tasks
{
	public class PourIntoPotTask : Task
	{
		protected Pot pot;

		protected ItemInstance item;

		protected Pourable pourable;

		protected bool removeItemAfterInitialPour;

		public override string TaskName { get; protected set; } = "Pour";


		protected virtual bool UseCoverage { get; }

		protected virtual bool FailOnEmpty { get; } = true;


		protected virtual Pot.ECameraPosition CameraPosition { get; } = Pot.ECameraPosition.Midshot;


		public PourIntoPotTask(Pot _pot, ItemInstance _itemInstance, Pourable _pourablePrefab)
		{
			if (_pot == null)
			{
				Console.LogWarning("PourIntoPotTask: pot null");
				StopTask();
				return;
			}
			if (_pourablePrefab == null)
			{
				Console.LogWarning("PourIntoPotTask: pourablePrefab null");
				StopTask();
				return;
			}
			ClickDetectionEnabled = true;
			item = _itemInstance;
			pot = _pot;
			if (pot.Plant != null)
			{
				pot.Plant.SetVisible(vis: false);
			}
			pot.SetPlayerUser(Player.Local.NetworkObject);
			pot.PositionCameraContainer();
			Transform cameraPosition = pot.GetCameraPosition(CameraPosition);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(cameraPosition.position, cameraPosition.rotation, 0.25f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.25f);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			pourable = UnityEngine.Object.Instantiate(_pourablePrefab.gameObject, NetworkSingleton<GameManager>.Instance.Temp).GetComponent<Pourable>();
			pourable.transform.position = pot.PourableStartPoint.position;
			pourable.Origin = pot.PourableStartPoint.position;
			pourable.MaxDistanceFromOrigin = 0.5f;
			pourable.LocationRestrictionEnabled = true;
			pourable.TargetPot = _pot;
			Pourable obj = pourable;
			obj.onInitialPour = (Action)Delegate.Combine(obj.onInitialPour, new Action(OnInitialPour));
			Vector3 vector = PlayerSingleton<PlayerCamera>.Instance.transform.position - pourable.transform.position;
			pourable.transform.forward = new Vector3(vector.x, 0f, vector.z);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("pourable");
			if (UseCoverage)
			{
				pot.SoilCover.Reset();
				pot.SoilCover.gameObject.SetActive(value: true);
				pot.SoilCover.onSufficientCoverage.AddListener(FullyCovered);
			}
		}

		public override void Update()
		{
			base.Update();
			if (FailOnEmpty && pourable.currentQuantity <= 0f)
			{
				Fail();
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
			UnityEngine.Object.Destroy(pourable.gameObject);
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			if (UseCoverage)
			{
				pot.SoilCover.onSufficientCoverage.RemoveListener(FullyCovered);
				pot.SoilCover.gameObject.SetActive(value: false);
			}
			if (pot.Plant != null)
			{
				pot.Plant.SetVisible(vis: true);
			}
			pot.SetPlayerUser(null);
		}

		private void OnInitialPour()
		{
			if (removeItemAfterInitialPour)
			{
				RemoveItem();
			}
		}

		protected void RemoveItem()
		{
			PlayerSingleton<PlayerInventory>.Instance.RemoveAmountOfItem(item.ID);
			if (pourable.TrashItem != null)
			{
				NetworkSingleton<TrashManager>.Instance.CreateTrashItem(pourable.TrashItem.ID, Player.Local.Avatar.transform.position + Vector3.up * 0.3f, UnityEngine.Random.rotation);
			}
		}

		protected virtual void FullyCovered()
		{
		}
	}
}
