using System;
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
	public class PackageProductTask : Task
	{
		protected PackagingStation station;

		protected FunctionalPackaging Packaging;

		protected List<FunctionalProduct> Products = new List<FunctionalProduct>();

		public override string TaskName { get; protected set; } = "Package product";


		public PackageProductTask(PackagingStation _station)
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
			ClickDetectionRadius = 0.02f;
			Packaging = UnityEngine.Object.Instantiate((station.PackagingSlot.ItemInstance.Definition as PackagingDefinition).FunctionalPackaging, station.Container);
			Packaging.Initialize(station, station.ActivePackagingAlignent);
			EnableMultiDragging(station.Container);
			int quantity = (station.PackagingSlot.ItemInstance.Definition as PackagingDefinition).Quantity;
			for (int i = 0; i < quantity; i++)
			{
				FunctionalProduct functionalProduct = UnityEngine.Object.Instantiate((station.ProductSlot.ItemInstance.Definition as ProductDefinition).FunctionalProduct, station.Container);
				functionalProduct.Initialize(station, station.ProductSlot.ItemInstance, station.ActiveProductAlignments[i]);
				functionalProduct.ClampZ = true;
				functionalProduct.DragProjectionMode = Draggable.EDragProjectionMode.FlatCameraForward;
				Products.Add(functionalProduct);
			}
			FunctionalPackaging packaging = Packaging;
			packaging.onFullyPacked = (Action)Delegate.Combine(packaging.onFullyPacked, new Action(FullyPacked));
			FunctionalPackaging packaging2 = Packaging;
			packaging2.onSealed = (Action)Delegate.Combine(packaging2.onSealed, new Action(Sealed));
			FunctionalPackaging packaging3 = Packaging;
			packaging3.onReachOutput = (Action)Delegate.Combine(packaging3.onReachOutput, new Action(ReachedOutput));
			station.UpdatePackagingVisuals(station.PackagingSlot.Quantity - 1);
			station.UpdateProductVisuals(station.ProductSlot.Quantity - Packaging.Definition.Quantity);
			station.SetVisualsLocked(locked: true);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(65f, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(station.CameraPosition_Task.position, station.CameraPosition_Task.rotation, 0.2f);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("packaging");
			base.CurrentInstruction = "Place product into packaging";
		}

		public override void StopTask()
		{
			Packaging.Destroy();
			for (int i = 0; i < Products.Count; i++)
			{
				UnityEngine.Object.Destroy(Products[i].gameObject);
			}
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

		public override void Success()
		{
			station.PackSingleInstance();
			base.Success();
		}

		private void FullyPacked()
		{
			base.CurrentInstruction = Packaging.SealInstruction;
		}

		private void Sealed()
		{
			base.CurrentInstruction = "Place packaging in hopper";
			station.SetHatchOpen(open: true);
		}

		private void ReachedOutput()
		{
			Success();
		}
	}
}
