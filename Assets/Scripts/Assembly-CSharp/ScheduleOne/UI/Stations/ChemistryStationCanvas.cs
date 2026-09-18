using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations
{
	public class ChemistryStationCanvas : Singleton<ChemistryStationCanvas>
	{
		public List<StationRecipe> Recipes = new List<StationRecipe>();

		[Header("Prefabs")]
		public StationRecipeEntry RecipeEntryPrefab;

		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public RectTransform InputSlotsContainer;

		public ItemSlotUI[] InputSlotUIs;

		public ItemSlotUI OutputSlotUI;

		public RectTransform RecipeSelectionContainer;

		public TextMeshProUGUI InstructionLabel;

		public Button BeginButton;

		public RectTransform SelectionIndicator;

		public RectTransform RecipeContainer;

		public RectTransform CookingInProgressContainer;

		public StationRecipeEntry InProgressRecipeEntry;

		public TextMeshProUGUI InProgressLabel;

		public TextMeshProUGUI ErrorLabel;

		private List<StationRecipeEntry> recipeEntries = new List<StationRecipeEntry>();

		private StationRecipeEntry selectedRecipe;

		public bool isOpen { get; protected set; }

		public ChemistryStation ChemistryStation { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			BeginButton.onClick.AddListener(BeginButtonPressed);
			for (int i = 0; i < Recipes.Count; i++)
			{
				StationRecipeEntry component = UnityEngine.Object.Instantiate(RecipeEntryPrefab, RecipeContainer).GetComponent<StationRecipeEntry>();
				component.AssignRecipe(Recipes[i]);
				recipeEntries.Add(component);
			}
		}

		protected override void Start()
		{
			base.Start();
			Close(removeUI: false);
		}

		protected virtual void Update()
		{
			if (isOpen)
			{
				if (ChemistryStation.CurrentCookOperation != null)
				{
					BeginButton.interactable = ChemistryStation.CurrentCookOperation.CurrentTime >= ChemistryStation.CurrentCookOperation.Recipe.CookTime_Mins;
					BeginButton.gameObject.SetActive(value: false);
				}
				else
				{
					BeginButton.interactable = selectedRecipe != null && selectedRecipe.IsValid && ChemistryStation.DoesOutputHaveSpace(selectedRecipe.Recipe);
					BeginButton.gameObject.SetActive(value: true);
				}
				if (BeginButton.interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
				{
					BeginButtonPressed();
				}
				UpdateInput();
				UpdateUI();
			}
		}

		private void LateUpdate()
		{
			if (isOpen && selectedRecipe != null)
			{
				SelectionIndicator.position = selectedRecipe.transform.position;
			}
		}

		private void UpdateUI()
		{
			ErrorLabel.enabled = false;
			if (ChemistryStation.CurrentCookOperation != null)
			{
				CookingInProgressContainer.gameObject.SetActive(value: true);
				RecipeSelectionContainer.gameObject.SetActive(value: false);
				if (ChemistryStation.CurrentCookOperation.CurrentTime >= ChemistryStation.CurrentCookOperation.Recipe.CookTime_Mins)
				{
					InProgressLabel.text = "Ready to finish";
				}
				else
				{
					InProgressLabel.text = "Cooking in progress...";
				}
				if (InProgressRecipeEntry.Recipe != ChemistryStation.CurrentCookOperation.Recipe)
				{
					InProgressRecipeEntry.AssignRecipe(ChemistryStation.CurrentCookOperation.Recipe);
				}
			}
			else
			{
				RecipeSelectionContainer.gameObject.SetActive(value: true);
				CookingInProgressContainer.gameObject.SetActive(value: false);
				if (selectedRecipe != null && !ChemistryStation.DoesOutputHaveSpace(selectedRecipe.Recipe))
				{
					ErrorLabel.text = "Output slot does not have enough space";
					ErrorLabel.enabled = true;
				}
			}
		}

		private void UpdateInput()
		{
			if (!(selectedRecipe != null))
			{
				return;
			}
			if (GameInput.MouseScrollDelta < 0f || GameInput.GetButtonDown(GameInput.ButtonCode.Backward) || UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
			{
				if (recipeEntries.IndexOf(selectedRecipe) < recipeEntries.Count - 1)
				{
					StationRecipeEntry stationRecipeEntry = recipeEntries[recipeEntries.IndexOf(selectedRecipe) + 1];
					if (stationRecipeEntry.IsValid)
					{
						SetSelectedRecipe(stationRecipeEntry);
					}
				}
			}
			else if ((GameInput.MouseScrollDelta > 0f || GameInput.GetButtonDown(GameInput.ButtonCode.Forward) || UnityEngine.Input.GetKeyDown(KeyCode.UpArrow)) && recipeEntries.IndexOf(selectedRecipe) > 0)
			{
				StationRecipeEntry stationRecipeEntry2 = recipeEntries[recipeEntries.IndexOf(selectedRecipe) - 1];
				if (stationRecipeEntry2.IsValid)
				{
					SetSelectedRecipe(stationRecipeEntry2);
				}
			}
		}

		public void Open(ChemistryStation station)
		{
			isOpen = true;
			ChemistryStation = station;
			UpdateUI();
			Canvas.enabled = true;
			Container.gameObject.SetActive(value: true);
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			}
			for (int i = 0; i < station.IngredientSlots.Length; i++)
			{
				InputSlotUIs[i].AssignSlot(station.IngredientSlots[i]);
				ItemSlot obj = station.IngredientSlots[i];
				obj.onItemDataChanged = (Action)Delegate.Combine(obj.onItemDataChanged, new Action(StationSlotsChanged));
			}
			OutputSlotUI.AssignSlot(station.OutputSlot);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
			List<ItemSlot> list = new List<ItemSlot>();
			list.AddRange(station.IngredientSlots);
			list.Add(station.OutputSlot);
			Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), list);
			Singleton<CompassManager>.Instance.SetVisible(visible: false);
			StationSlotsChanged();
		}

		public void Close(bool removeUI)
		{
			isOpen = false;
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			}
			for (int i = 0; i < InputSlotUIs.Length; i++)
			{
				InputSlotUIs[i].ClearSlot();
				if (ChemistryStation != null)
				{
					ItemSlot obj = ChemistryStation.IngredientSlots[i];
					obj.onItemDataChanged = (Action)Delegate.Remove(obj.onItemDataChanged, new Action(StationSlotsChanged));
				}
			}
			OutputSlotUI.ClearSlot();
			if (removeUI)
			{
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			}
			Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
			ChemistryStation = null;
		}

		public void BeginButtonPressed()
		{
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			new UseChemistryStationTask(ChemistryStation, selectedRecipe.Recipe);
			Close(removeUI: false);
		}

		private void StationSlotsChanged()
		{
			List<ItemInstance> list = new List<ItemInstance>();
			for (int i = 0; i < InputSlotUIs.Length; i++)
			{
				if (InputSlotUIs[i].assignedSlot.ItemInstance != null)
				{
					list.Add(InputSlotUIs[i].assignedSlot.ItemInstance);
				}
			}
			for (int j = 0; j < recipeEntries.Count; j++)
			{
				recipeEntries[j].RefreshValidity(list);
			}
			SortRecipes(list);
		}

		private void SortRecipes(List<ItemInstance> ingredients)
		{
			Dictionary<StationRecipeEntry, float> recipes = new Dictionary<StationRecipeEntry, float>();
			for (int i = 0; i < recipeEntries.Count; i++)
			{
				float ingredientsMatchDelta = recipeEntries[i].GetIngredientsMatchDelta(ingredients);
				recipes.Add(recipeEntries[i], ingredientsMatchDelta);
			}
			recipeEntries.Sort((StationRecipeEntry a, StationRecipeEntry b) => recipes[b].CompareTo(recipes[a]));
			for (int j = 0; j < recipeEntries.Count; j++)
			{
				recipeEntries[j].transform.SetAsLastSibling();
			}
			if (recipeEntries.Count > 0 && recipeEntries[0].IsValid)
			{
				SetSelectedRecipe(recipeEntries[0]);
			}
			else
			{
				SetSelectedRecipe(null);
			}
		}

		private void SetSelectedRecipe(StationRecipeEntry entry)
		{
			selectedRecipe = entry;
			if (entry != null)
			{
				SelectionIndicator.position = entry.transform.position;
				SelectionIndicator.gameObject.SetActive(value: true);
			}
			else
			{
				SelectionIndicator.gameObject.SetActive(value: false);
			}
		}
	}
}
