using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.StationFramework;
using ScheduleOne.Trash;
using ScheduleOne.UI;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.PlayerTasks.Tasks
{
	public class UseMixingStationTask : Task
	{
		public enum EStep
		{
			CombineIngredients = 0,
			StartMixing = 1
		}

		private List<StationItem> items = new List<StationItem>();

		private List<StationItem> mixerItems = new List<StationItem>();

		private List<IngredientPiece> ingredientPieces = new List<IngredientPiece>();

		private ItemInstance[] removedIngredients;

		private Beaker Jug;

		public MixingStation Station { get; private set; }

		public EStep CurrentStep { get; private set; }

		public static string GetStepDescription(EStep step)
		{
			return step switch
			{
				EStep.CombineIngredients => "Combine ingredients in bowl", 
				EStep.StartMixing => "Start mixing machine", 
				_ => "Unknown step", 
			};
		}

		public UseMixingStationTask(MixingStation station)
		{
			MixingStation station2 = station;
			//base._002Ector();
			UseMixingStationTask useMixingStationTask = this;
			Station = station2;
			Station.onStartButtonClicked.AddListener(StartButtonPressed);
			ClickDetectionRadius = 0.012f;
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Station.CameraPosition_CombineIngredients.position, Station.CameraPosition_CombineIngredients.rotation, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(TaskName);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			removedIngredients = new ItemInstance[2];
			int mixQuantity = station2.GetMixQuantity();
			removedIngredients[0] = station2.ProductSlot.ItemInstance.GetCopy(mixQuantity);
			removedIngredients[1] = station2.MixerSlot.ItemInstance.GetCopy(mixQuantity);
			station2.ProductSlot.ChangeQuantity(-mixQuantity);
			station2.MixerSlot.ChangeQuantity(-mixQuantity);
			EnableMultiDragging(station2.ItemContainer, 0.12f);
			int num = 0;
			Singleton<InputPromptsCanvas>.Instance.LoadModule("packaging");
			for (int i = 0; i < mixQuantity; i++)
			{
				SetupIngredient(removedIngredients[1].Definition as StorableItemDefinition, num, mixer: true);
				num++;
			}
			for (int j = 0; j < mixQuantity; j++)
			{
				SetupIngredient(removedIngredients[0].Definition as StorableItemDefinition, num, mixer: false);
				num++;
			}
			if (Jug != null)
			{
				Jug.Pourable.LiquidCapacity_L = Jug.Fillable.LiquidCapacity_L;
				Jug.Pourable.DefaultLiquid_L = Jug.Fillable.GetTotalLiquidVolume();
				Jug.Pourable.SetLiquidLevel(Jug.Pourable.DefaultLiquid_L);
				Jug.Pourable.PourParticlesColor = Jug.Fillable.LiquidContainer.LiquidColor;
				Jug.Pourable.LiquidColor = Jug.Fillable.LiquidContainer.LiquidColor;
				Jug.Fillable.FillableEnabled = false;
			}
			void SetupIngredient(StorableItemDefinition def, int index, bool mixer)
			{
				if (def.StationItem == null)
				{
					Console.LogError("Ingredient '" + def.Name + "' does not have a station item");
				}
				else
				{
					if (mixer)
					{
						mixerItems.Add(def.StationItem);
					}
					if (def.StationItem.HasModule<PourableModule>())
					{
						if (Jug == null)
						{
							Jug = CreateJug();
						}
						PourableModule module = def.StationItem.GetModule<PourableModule>();
						Jug.Fillable.AddLiquid(module.LiquidType, module.LiquidCapacity_L, module.LiquidColor);
					}
					else
					{
						StationItem stationItem = Object.Instantiate(def.StationItem, station2.ItemContainer);
						stationItem.transform.rotation = station2.IngredientTransforms[items.Count].rotation;
						Vector3 eulerAngles = stationItem.transform.eulerAngles;
						eulerAngles.y = Random.Range(0f, 360f);
						stationItem.transform.eulerAngles = eulerAngles;
						stationItem.transform.position = station2.IngredientTransforms[items.Count].position;
						stationItem.Initialize(def);
						if (stationItem.HasModule<IngredientModule>())
						{
							stationItem.ActivateModule<IngredientModule>();
							IngredientPiece[] pieces = stationItem.GetModule<IngredientModule>().Pieces;
							foreach (IngredientPiece ingredientPiece in pieces)
							{
								ingredientPieces.Add(ingredientPiece);
								ingredientPiece.DisableInteractionInLiquid = false;
							}
						}
						else
						{
							Console.LogError("Ingredient '" + def.Name + "' does not have an ingredient or pourable module");
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
			}
		}

		private Beaker CreateJug()
		{
			Beaker component = Object.Instantiate(Station.JugPrefab, Station.ItemContainer).GetComponent<Beaker>();
			component.transform.position = Station.JugAlignment.position;
			component.transform.rotation = Station.JugAlignment.rotation;
			component.GetComponent<DraggableConstraint>().Container = Station.ItemContainer;
			component.ActivateModule<PourableModule>();
			return component;
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
			if (CurrentStep == EStep.CombineIngredients)
			{
				int num = items.Count;
				if (Jug != null)
				{
					num++;
				}
				int combinedIngredients = GetCombinedIngredients();
				base.CurrentInstruction = base.CurrentInstruction + " (" + combinedIngredients + "/" + num + ")";
			}
		}

		private void CheckProgress()
		{
			if (CurrentStep == EStep.CombineIngredients)
			{
				CheckStep_CombineIngredients();
			}
		}

		private void CheckStep_CombineIngredients()
		{
			if (GetCombinedIngredients() >= items.Count + ((Jug != null) ? 1 : 0))
			{
				ProgressStep();
			}
		}

		private int GetCombinedIngredients()
		{
			int num = 0;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].HasModule<IngredientModule>())
				{
					IngredientModule module = items[i].GetModule<IngredientModule>();
					bool flag = true;
					IngredientPiece[] pieces = module.Pieces;
					for (int j = 0; j < pieces.Length; j++)
					{
						if (pieces[j].CurrentLiquidContainer != Station.BowlFillable.LiquidContainer)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						num++;
					}
				}
				else if (items[i].HasModule<PourableModule>() && items[i].GetModule<PourableModule>().NormalizedLiquidLevel <= 0.02f)
				{
					num++;
				}
			}
			if (Jug != null && Jug.Pourable.NormalizedLiquidLevel <= 0.02f)
			{
				num++;
			}
			return num;
		}

		private void ProgressStep()
		{
			CurrentStep++;
			if (CurrentStep == EStep.StartMixing)
			{
				Station.SetStartButtonClickable(clickable: true);
			}
		}

		private void StartButtonPressed()
		{
			if (CurrentStep == EStep.StartMixing)
			{
				Success();
			}
		}

		public override void Success()
		{
			ProductItemInstance productItemInstance = removedIngredients[0] as ProductItemInstance;
			string iD = removedIngredients[1].Definition.ID;
			CreateTrash();
			Singleton<MixingStationCanvas>.Instance.StartMixOperation(new MixOperation(productItemInstance.ID, productItemInstance.Quality, iD, productItemInstance.Quantity));
			base.Success();
		}

		private void CreateTrash()
		{
			BoxCollider trashSpawnVolume = Station.TrashSpawnVolume;
			for (int i = 0; i < Mathf.CeilToInt((float)mixerItems.Count / 2f); i++)
			{
				if (!(mixerItems[0].TrashPrefab == null))
				{
					Vector3 posiiton = trashSpawnVolume.transform.TransformPoint(new Vector3(Random.Range((0f - trashSpawnVolume.size.x) / 2f, trashSpawnVolume.size.x / 2f), 0f, Random.Range((0f - trashSpawnVolume.size.z) / 2f, trashSpawnVolume.size.z / 2f)));
					Vector3 forward = trashSpawnVolume.transform.forward;
					forward = Quaternion.Euler(0f, Random.Range(-45f, 45f), 0f) * forward;
					float num = Random.Range(0.25f, 0.4f);
					NetworkSingleton<TrashManager>.Instance.CreateTrashItem(mixerItems[0].TrashPrefab.ID, posiiton, Random.rotation, forward * num);
				}
			}
		}

		public override void StopTask()
		{
			Station.onStartButtonClicked.RemoveListener(StartButtonPressed);
			Station.BowlFillable.ResetContents();
			if (Outcome != EOutcome.Success)
			{
				Station.ProductSlot.AddItem(removedIngredients[0]);
				Station.MixerSlot.AddItem(removedIngredients[1]);
			}
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			foreach (StationItem item in items)
			{
				item.Destroy();
			}
			items.Clear();
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(TaskName);
			Station.Open();
			if (Jug != null)
			{
				Object.Destroy(Jug.gameObject);
			}
			base.StopTask();
		}
	}
}
