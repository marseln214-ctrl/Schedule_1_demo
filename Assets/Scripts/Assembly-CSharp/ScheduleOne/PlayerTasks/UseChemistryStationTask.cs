using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Stations;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class UseChemistryStationTask : Task
	{
		public const float STIR_TIME = 2.4f;

		public const float TEMPERATURE_TIME = 2f;

		private Beaker beaker;

		private StirringRod stirringRod;

		private List<StationItem> items = new List<StationItem>();

		private List<IngredientPiece> ingredientPieces = new List<IngredientPiece>();

		private float stirProgress;

		private float timeInTemperatureRange;

		private ItemInstance[] RemovedIngredients;

		public ChemistryStation.EStep CurrentStep { get; private set; }

		public ChemistryStation Station { get; private set; }

		public StationRecipe Recipe { get; private set; }

		public static string GetStepDescription(ChemistryStation.EStep step)
		{
			return step switch
			{
				ChemistryStation.EStep.CombineIngredients => "Combine ingredients in beaker", 
				ChemistryStation.EStep.Stir => "Stir mixture", 
				ChemistryStation.EStep.LowerBoilingFlask => "Lower boiling flask", 
				ChemistryStation.EStep.PourIntoBoilingFlask => "Pour mixture into boiling flask", 
				ChemistryStation.EStep.RaiseBoilingFlask => "Raise boiling flask above burner", 
				ChemistryStation.EStep.StartHeat => "Start burner", 
				ChemistryStation.EStep.Cook => "Wait for the mixture to finish cooking", 
				ChemistryStation.EStep.LowerBoilingFlaskAgain => "Lower boiling flask", 
				ChemistryStation.EStep.PourThroughFilter => "Pour mixture through filter", 
				_ => "Unknown step", 
			};
		}

		public UseChemistryStationTask(ChemistryStation station, StationRecipe recipe)
		{
			Station = station;
			Recipe = recipe;
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.2f);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			beaker = station.CreateBeaker();
			station.StaticBeaker.gameObject.SetActive(value: false);
			EnableMultiDragging(station.ItemContainer, 0.1f);
			RemovedIngredients = new ItemInstance[station.IngredientSlots.Length];
			for (int i = 0; i < recipe.Ingredients.Count; i++)
			{
				StorableItemDefinition storableItemDefinition = null;
				foreach (ItemDefinition item in recipe.Ingredients[i].Items)
				{
					StorableItemDefinition storableItemDefinition2 = item as StorableItemDefinition;
					for (int j = 0; j < station.IngredientSlots.Length; j++)
					{
						if (station.IngredientSlots[j].ItemInstance != null && station.IngredientSlots[j].ItemInstance.Definition.ID == storableItemDefinition2.ID)
						{
							storableItemDefinition = storableItemDefinition2;
							RemovedIngredients[j] = station.IngredientSlots[j].ItemInstance.GetCopy(recipe.Ingredients[i].Quantity);
							station.IngredientSlots[j].ChangeQuantity(-recipe.Ingredients[i].Quantity);
							break;
						}
					}
				}
				if (storableItemDefinition.StationItem == null)
				{
					Console.LogError("Ingredient '" + storableItemDefinition.Name + "' does not have a station item");
					continue;
				}
				StationItem stationItem = Object.Instantiate(storableItemDefinition.StationItem, station.ItemContainer);
				stationItem.transform.position = station.IngredientTransforms[i].position;
				stationItem.Initialize(storableItemDefinition);
				stationItem.transform.rotation = station.IngredientTransforms[i].rotation;
				if (stationItem.HasModule<IngredientModule>())
				{
					stationItem.ActivateModule<IngredientModule>();
					IngredientPiece[] pieces = stationItem.GetModule<IngredientModule>().Pieces;
					foreach (IngredientPiece ingredientPiece in pieces)
					{
						ingredientPieces.Add(ingredientPiece);
						ingredientPiece.GetComponent<Draggable>().CanBeMultiDragged = true;
					}
				}
				else if (stationItem.HasModule<PourableModule>())
				{
					stationItem.ActivateModule<PourableModule>();
					Draggable componentInChildren = stationItem.GetComponentInChildren<Draggable>();
					if (componentInChildren != null)
					{
						componentInChildren.CanBeMultiDragged = false;
					}
				}
				else
				{
					Console.LogError("Ingredient '" + storableItemDefinition.Name + "' does not have an ingredient or pourable module");
				}
				Draggable[] componentsInChildren = stationItem.GetComponentsInChildren<Draggable>();
				foreach (Draggable obj in componentsInChildren)
				{
					obj.DragProjectionMode = Draggable.EDragProjectionMode.FlatCameraForward;
					DraggableConstraint component = obj.gameObject.GetComponent<DraggableConstraint>();
					if (component != null)
					{
						component.ProportionalZClamp = true;
					}
				}
				items.Add(stationItem);
			}
		}

		public override void Update()
		{
			base.Update();
			CheckProgress();
			UpdateInstruction();
		}

		private void UpdateInstruction()
		{
			base.CurrentInstruction = GetStepDescription(CurrentStep);
			if (CurrentStep == ChemistryStation.EStep.Stir)
			{
				base.CurrentInstruction = base.CurrentInstruction + " (" + Mathf.RoundToInt(stirProgress * 100f) + "%)";
			}
			if (CurrentStep == ChemistryStation.EStep.StartHeat)
			{
				base.CurrentInstruction = base.CurrentInstruction + " (" + Mathf.RoundToInt(timeInTemperatureRange / 2f * 100f) + "%)";
			}
		}

		private void CheckProgress()
		{
			switch (CurrentStep)
			{
			case ChemistryStation.EStep.CombineIngredients:
				CheckStep_CombineIngredients();
				break;
			case ChemistryStation.EStep.Stir:
				CheckStep_StirMixture();
				break;
			case ChemistryStation.EStep.LowerBoilingFlask:
				CheckStep_LowerBoilingFlask();
				break;
			case ChemistryStation.EStep.PourIntoBoilingFlask:
				CheckStep_PourIntoBoilingFlask();
				break;
			case ChemistryStation.EStep.RaiseBoilingFlask:
				CheckStep_RaiseBoilingFlask();
				break;
			case ChemistryStation.EStep.StartHeat:
				CheckStep_StartHeat();
				break;
			}
		}

		private void ProgressStep()
		{
			CurrentStep++;
			if (CurrentStep == ChemistryStation.EStep.Stir)
			{
				PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Station.CameraPosition_Stirring.position, Station.CameraPosition_Stirring.rotation, 0.2f);
				stirringRod = Station.CreateStirringRod();
				Station.StaticStirringRod.gameObject.SetActive(value: false);
			}
			if (CurrentStep == ChemistryStation.EStep.LowerBoilingFlask)
			{
				if (stirringRod != null)
				{
					stirringRod.Destroy();
				}
				PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Station.CameraPosition_Default.position, Station.CameraPosition_Default.rotation, 0.2f);
				Station.LabStand.SetInteractable(e: true);
			}
			if (CurrentStep == ChemistryStation.EStep.PourIntoBoilingFlask)
			{
				beaker.SetStatic(stat: false);
				beaker.ActivateModule<PourableModule>();
				beaker.Fillable.enabled = false;
				PourableModule module = beaker.GetModule<PourableModule>();
				module.SetLiquidLevel(module.LiquidContainer.CurrentLiquidLevel);
				module.LiquidColor = module.LiquidContainer.LiquidVolume.liquidColor1;
				module.PourParticlesColor = module.LiquidColor;
			}
			if (CurrentStep == ChemistryStation.EStep.RaiseBoilingFlask)
			{
				Station.LabStand.SetInteractable(e: true);
			}
			if (CurrentStep == ChemistryStation.EStep.StartHeat)
			{
				Station.Burner.SetInteractable(e: true);
				Station.BoilingFlask.SetCanvasVisible(visible: true);
				Station.BoilingFlask.SetRecipe(Recipe);
			}
		}

		private void CheckStep_CombineIngredients()
		{
			bool flag = true;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].HasModule<PourableModule>())
				{
					if (items[i].GetModule<PourableModule>().NormalizedLiquidLevel > 0.05f)
					{
						flag = false;
						break;
					}
				}
				else
				{
					if (!items[i].HasModule<IngredientModule>())
					{
						continue;
					}
					IngredientPiece[] pieces = items[i].GetModule<IngredientModule>().Pieces;
					for (int j = 0; j < pieces.Length; j++)
					{
						if (pieces[j].CurrentLiquidContainer == null)
						{
							flag = false;
							break;
						}
					}
				}
			}
			if (flag)
			{
				ProgressStep();
			}
		}

		private void CheckStep_StirMixture()
		{
			float num = stirringRod.CurrentStirringSpeed * Time.deltaTime / 2.4f;
			if (num > 0f)
			{
				stirProgress = Mathf.Clamp(stirProgress + num, 0f, 1f);
				foreach (IngredientPiece ingredientPiece in ingredientPieces)
				{
					ingredientPiece.DissolveAmount(num, num > 0.001f);
				}
			}
			if (stirProgress >= 1f)
			{
				ProgressStep();
			}
		}

		private void CheckStep_LowerBoilingFlask()
		{
			if (Station.LabStand.CurrentPosition <= Station.LabStand.FunnelThreshold)
			{
				Station.LabStand.SetPosition(0f);
				Station.LabStand.SetInteractable(e: false);
				ProgressStep();
			}
		}

		private void CheckStep_PourIntoBoilingFlask()
		{
			if (beaker.Pourable.NormalizedLiquidLevel <= 0.01f)
			{
				beaker.Pourable.LiquidContainer.SetLiquidLevel(0f);
				ProgressStep();
			}
		}

		private void CheckStep_RaiseBoilingFlask()
		{
			if (Station.LabStand.CurrentPosition >= 0.95f)
			{
				Station.LabStand.SetPosition(1f);
				Station.LabStand.SetInteractable(e: false);
				ProgressStep();
			}
		}

		private void CheckStep_StartHeat()
		{
			if (Station.BoilingFlask.IsTemperatureInRange)
			{
				timeInTemperatureRange += Time.deltaTime;
			}
			else
			{
				timeInTemperatureRange = Mathf.Clamp(timeInTemperatureRange - Time.deltaTime, 0f, 2f);
			}
			if (timeInTemperatureRange >= 2f)
			{
				ProgressStep();
				Station.BoilingFlask.SetCanvasVisible(visible: false);
				Station.Burner.SetInteractable(e: false);
				Success();
			}
		}

		public override void Success()
		{
			EQuality productQuality = Recipe.CalculateQuality(RemovedIngredients.ToList());
			ChemistryCookOperation op = new ChemistryCookOperation(Recipe, productQuality, Station.BoilingFlask.LiquidContainer.LiquidVolume.liquidColor1, Station.BoilingFlask.LiquidContainer.CurrentLiquidLevel);
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Chemical_Operations_Started", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Chemical_Operations_Started") + 1f).ToString());
			Station.SendCookOperation(op);
			base.Success();
		}

		public override void StopTask()
		{
			base.StopTask();
			if (Outcome != EOutcome.Success)
			{
				for (int i = 0; i < RemovedIngredients.Length; i++)
				{
					if (RemovedIngredients[i] != null)
					{
						Station.IngredientSlots[i].AddItem(RemovedIngredients[i]);
					}
				}
				Station.ResetStation();
			}
			Singleton<ChemistryStationCanvas>.Instance.Open(Station);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(65f, 0.2f);
			beaker.Destroy();
			Station.StaticBeaker.gameObject.SetActive(value: true);
			Station.StaticFunnel.gameObject.SetActive(value: true);
			Station.StaticStirringRod.gameObject.SetActive(value: true);
			Station.LabStand.SetPosition(1f);
			Station.LabStand.SetInteractable(e: false);
			Station.Burner.SetInteractable(e: false);
			Station.BoilingFlask.SetCanvasVisible(visible: false);
			if (stirringRod != null)
			{
				stirringRod.Destroy();
			}
			foreach (StationItem item in items)
			{
				item.Destroy();
			}
			items.Clear();
		}
	}
}
