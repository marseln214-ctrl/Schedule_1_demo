using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.StationFramework;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class CauldronTask : Task
	{
		public enum EStep
		{
			CombineIngredients = 0,
			StartMixing = 1
		}

		private StationItem[] CocaLeaves;

		private StationItem Gasoline;

		private Draggable Tub;

		public Cauldron Cauldron { get; private set; }

		public EStep CurrentStep { get; private set; }

		public static string GetStepDescription(EStep step)
		{
			return step switch
			{
				EStep.CombineIngredients => "Combine leaves and gasoline in cauldron", 
				EStep.StartMixing => "Start cauldron", 
				_ => "Unknown step", 
			};
		}

		public CauldronTask(Cauldron caudron)
		{
			Cauldron = caudron;
			Cauldron.onStartButtonClicked.AddListener(StartButtonPressed);
			Cauldron.OverheadLight.enabled = true;
			ClickDetectionRadius = 0.012f;
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Cauldron.CameraPosition_CombineIngredients.position, Cauldron.CameraPosition_CombineIngredients.rotation, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(TaskName);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			EnableMultiDragging(Cauldron.ItemContainer, 0.15f);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("packaging");
			Gasoline = Object.Instantiate(Cauldron.GasolinePrefab, caudron.ItemContainer);
			Gasoline.transform.rotation = caudron.GasolineSpawnPoint.rotation;
			Gasoline.transform.position = caudron.GasolineSpawnPoint.position;
			Gasoline.transform.localScale = Vector3.one * 1.5f;
			Gasoline.ActivateModule<PourableModule>();
			Gasoline.GetComponentInChildren<Rigidbody>().rotation = caudron.GasolineSpawnPoint.rotation;
			CocaLeaves = new StationItem[20];
			for (int i = 0; i < CocaLeaves.Length; i++)
			{
				CocaLeaves[i] = Object.Instantiate(Cauldron.CocaLeafPrefab, caudron.ItemContainer);
				CocaLeaves[i].transform.rotation = caudron.LeafSpawns[i].rotation;
				CocaLeaves[i].transform.position = caudron.LeafSpawns[i].position;
				CocaLeaves[i].ActivateModule<IngredientModule>();
				CocaLeaves[i].transform.localScale = Vector3.one * 0.85f;
				CocaLeaves[i].GetModule<IngredientModule>().Pieces[0].transform.SetParent(caudron.ItemContainer);
			}
		}

		public override void Success()
		{
			EQuality quality = Cauldron.RemoveIngredients();
			Cauldron.SendCookOperation(Cauldron.CookTime, quality);
			base.Success();
		}

		public override void StopTask()
		{
			Cauldron.OverheadLight.enabled = false;
			Cauldron.onStartButtonClicked.RemoveListener(StartButtonPressed);
			Cauldron.StartButtonClickable.ClickableEnabled = false;
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(TaskName);
			Cauldron.Open();
			StationItem[] cocaLeaves = CocaLeaves;
			foreach (StationItem obj in cocaLeaves)
			{
				Object.Destroy(obj.GetModule<IngredientModule>().Pieces[0].gameObject);
				obj.Destroy();
			}
			Gasoline.Destroy();
			if (Outcome != EOutcome.Success)
			{
				Cauldron.CauldronFillable.ResetContents();
			}
			base.StopTask();
		}

		public override void Update()
		{
			base.Update();
			CheckProgress();
			UpdateInstruction();
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
			if (Gasoline.GetModule<PourableModule>().LiquidLevel > 0.01f)
			{
				return;
			}
			StationItem[] cocaLeaves = CocaLeaves;
			for (int i = 0; i < cocaLeaves.Length; i++)
			{
				if (cocaLeaves[i].GetModule<IngredientModule>().Pieces[0].CurrentLiquidContainer == null)
				{
					return;
				}
			}
			StartMixing();
		}

		private void StartMixing()
		{
			CurrentStep = EStep.StartMixing;
			bool isHeld = Gasoline.GetModule<PourableModule>().Draggable.IsHeld;
			Gasoline.GetModule<PourableModule>().Draggable.ClickableEnabled = false;
			if (isHeld)
			{
				Gasoline.GetModule<PourableModule>().Draggable.Rb.AddForce(Cauldron.transform.right * 10f, ForceMode.VelocityChange);
			}
			StationItem[] cocaLeaves = CocaLeaves;
			for (int i = 0; i < cocaLeaves.Length; i++)
			{
				cocaLeaves[i].GetModule<IngredientModule>().Pieces[0].GetComponent<Draggable>().ClickableEnabled = false;
			}
			Cauldron.StartButtonClickable.ClickableEnabled = clickable;
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Cauldron.CameraPosition_StartMachine.position, Cauldron.CameraPosition_StartMachine.rotation, 0.2f);
		}

		private void UpdateInstruction()
		{
			base.CurrentInstruction = GetStepDescription(CurrentStep);
		}

		private void StartButtonPressed()
		{
			if (CurrentStep == EStep.StartMixing)
			{
				Success();
			}
		}
	}
}
