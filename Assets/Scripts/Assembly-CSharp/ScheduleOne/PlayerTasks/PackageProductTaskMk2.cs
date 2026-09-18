using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Packaging;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.UI;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class PackageProductTaskMk2 : Task
	{
		protected PackagingStationMk2 station;

		protected FunctionalPackaging Packaging;

		protected List<FunctionalProduct> Products = new List<FunctionalProduct>();

		public override string TaskName { get; protected set; } = "Package product";


		public PackageProductTaskMk2(PackagingStationMk2 _station)
		{
			if (_station == null)
			{
				Console.LogError("Station is null!");
				return;
			}
			if (_station.GetState(PackagingStation.EMode.Package) != 0)
			{
				Console.LogError("Station not ready to begin packaging!");
				return;
			}
			station = _station;
			ClickDetectionRadius = 0.01f;
			EnableMultiDragging(station.PackagingTool.ProductContainer);
			int quantity = _station.ProductSlot.Quantity;
			int quantity2 = _station.PackagingSlot.Quantity;
			int quantity3 = (_station.PackagingSlot.ItemInstance.Definition as PackagingDefinition).Quantity;
			int num = Mathf.Min(quantity, quantity2 * quantity3);
			num -= num % quantity3;
			int num2 = Mathf.CeilToInt((float)num / (float)quantity3);
			station.UpdatePackagingVisuals(station.PackagingSlot.Quantity - num2);
			station.UpdateProductVisuals(station.ProductSlot.Quantity - num2);
			station.SetVisualsLocked(locked: true);
			FunctionalPackaging functionalPackaging = (_station.PackagingSlot.ItemInstance.Definition as PackagingDefinition).FunctionalPackaging;
			station.PackagingTool.Initialize(this, functionalPackaging, num2, _station.ProductSlot.ItemInstance as ProductItemInstance, num);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(65f, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(station.CameraPosition_Task.position, station.CameraPosition_Task.rotation, 0.2f);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("packagingmk2");
			base.CurrentInstruction = "Insert product into packaging";
		}

		public override void StopTask()
		{
			station.PackagingTool.Deinitialize();
			station.SetVisualsLocked(locked: false);
			station.SetHatchOpen(open: false);
			station.UpdateProductVisuals();
			station.UpdatePackagingVisuals();
			base.StopTask();
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(station.CameraPosition.position, station.CameraPosition.rotation, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.2f);
			if (Outcome == EOutcome.Success && station.GetState(PackagingStation.EMode.Package) == PackagingStation.EState.CanBegin)
			{
				new PackageProductTask(station);
			}
			else
			{
				Singleton<PackagingStationCanvas>.Instance.SetIsOpen(station, open: true);
			}
		}
	}
}
