using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.ItemFramework;
using ScheduleOne.Law;
using ScheduleOne.Map;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Vehicles;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class ArrestNoticeScreen : Singleton<ArrestNoticeScreen>
	{
		public const float VEHICLE_POSSESSION_TIMEOUT = 30f;

		[Header("References")]
		public Canvas Canvas;

		public CanvasGroup CanvasGroup;

		public RectTransform CrimeEntryContainer;

		public RectTransform PenaltyEntryContainer;

		[Header("Prefabs")]
		public RectTransform CrimeEntryPrefab;

		public RectTransform PenaltyEntryPrefab;

		private Dictionary<Crime, int> recordedCrimes = new Dictionary<Crime, int>();

		private LandVehicle vehicle;

		public bool isOpen { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			isOpen = false;
			Canvas.enabled = false;
			CanvasGroup.alpha = 0f;
			GameInput.RegisterExitListener(Exit, 20);
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		}

		private void PlayerSpawned()
		{
			Player.Local.onArrested.AddListener(RecordCrimes);
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && isOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				Close();
			}
		}

		public void Open()
		{
			ClearEntries();
			isOpen = true;
			Canvas.enabled = true;
			CanvasGroup.alpha = 1f;
			CanvasGroup.interactable = true;
			Singleton<PostProcessingManager>.Instance.SetBlur(1f);
			Crime[] array = recordedCrimes.Keys.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.Instantiate(CrimeEntryPrefab, CrimeEntryContainer).GetComponentInChildren<TextMeshProUGUI>().text = recordedCrimes[array[i]] + "x " + array[i].CrimeName.ToLower();
			}
			List<string> list = PenaltyHandler.ProcessCrimeList(recordedCrimes);
			ConfiscateItems(EStealthLevel.None);
			for (int j = 0; j < list.Count; j++)
			{
				UnityEngine.Object.Instantiate(PenaltyEntryPrefab, PenaltyEntryContainer).GetComponentInChildren<TextMeshProUGUI>().text = list[j];
			}
			if (vehicle != null && !vehicle.isOccupied)
			{
				Transform[] possessedVehicleSpawnPoints = Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.PossessedVehicleSpawnPoints;
				Transform target = possessedVehicleSpawnPoints[UnityEngine.Random.Range(0, possessedVehicleSpawnPoints.Length - 1)];
				Tuple<Vector3, Quaternion> alignmentTransform = vehicle.GetAlignmentTransform(target, EParkingAlignment.RearToKerb);
				vehicle.SetTransform_Server(alignmentTransform.Item1, alignmentTransform.Item2);
			}
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			Player.Deactivate(freeMouse: true);
		}

		public void Close()
		{
			if (CanvasGroup.interactable && isOpen)
			{
				CanvasGroup.interactable = false;
				StartCoroutine(CloseRoutine());
			}
			IEnumerator CloseRoutine()
			{
				float lerpTime = 0.3f;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					CanvasGroup.alpha = Mathf.Lerp(1f, 0f, i / lerpTime);
					Singleton<PostProcessingManager>.Instance.SetBlur(CanvasGroup.alpha);
					yield return new WaitForEndOfFrame();
				}
				CanvasGroup.alpha = 0f;
				Canvas.enabled = false;
				Singleton<PostProcessingManager>.Instance.SetBlur(0f);
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				Player.Activate();
				ClearEntries();
				isOpen = false;
			}
		}

		public void RecordCrimes()
		{
			Debug.Log("Crimes recorded");
			recordedCrimes.Clear();
			if (Player.Local.LastDrivenVehicle != null && (Player.Local.TimeSinceVehicleExit < 30f || Player.Local.CrimeData.IsCrimeOnRecord(typeof(TransportingIllicitItems))))
			{
				vehicle = Player.Local.LastDrivenVehicle;
			}
			for (int i = 0; i < Player.Local.CrimeData.Crimes.Keys.Count; i++)
			{
				recordedCrimes.Add(Player.Local.CrimeData.Crimes.Keys.ElementAt(i), Player.Local.CrimeData.Crimes.Values.ElementAt(i));
			}
			if (Player.Local.CrimeData.EvadedArrest)
			{
				recordedCrimes.Add(new Evading(), 1);
			}
			RecordPossession(EStealthLevel.None);
		}

		private void RecordPossession(EStealthLevel maxStealthLevel)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			List<ItemSlot> allInventorySlots = PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots();
			if (Player.Local.LastDrivenVehicle != null && Player.Local.TimeSinceVehicleExit < 30f && Player.Local.LastDrivenVehicle.Storage != null)
			{
				allInventorySlots.AddRange(Player.Local.LastDrivenVehicle.Storage.ItemSlots);
			}
			foreach (ItemSlot item in allInventorySlots)
			{
				if (item.ItemInstance == null)
				{
					continue;
				}
				if (item.ItemInstance is ProductItemInstance)
				{
					ProductItemInstance productItemInstance = item.ItemInstance as ProductItemInstance;
					if (productItemInstance.AppliedPackaging == null || productItemInstance.AppliedPackaging.StealthLevel <= maxStealthLevel)
					{
						switch (item.ItemInstance.Definition.legalStatus)
						{
						case ELegalStatus.ControlledSubstance:
							num += productItemInstance.Quantity;
							break;
						case ELegalStatus.LowSeverityDrug:
							num2 += productItemInstance.Quantity;
							break;
						case ELegalStatus.ModerateSeverityDrug:
							num3 += productItemInstance.Quantity;
							break;
						case ELegalStatus.HighSeverityDrug:
							num4 += productItemInstance.Quantity;
							break;
						}
					}
				}
				else
				{
					switch (item.ItemInstance.Definition.legalStatus)
					{
					case ELegalStatus.ControlledSubstance:
						num += item.ItemInstance.Quantity;
						break;
					case ELegalStatus.LowSeverityDrug:
						num2 += item.ItemInstance.Quantity;
						break;
					case ELegalStatus.ModerateSeverityDrug:
						num3 += item.ItemInstance.Quantity;
						break;
					case ELegalStatus.HighSeverityDrug:
						num4 += item.ItemInstance.Quantity;
						break;
					}
				}
			}
			if (num > 0)
			{
				recordedCrimes.Add(new PossessingControlledSubstances(), num);
			}
			if (num2 > 0)
			{
				recordedCrimes.Add(new PossessingLowSeverityDrug(), num2);
			}
			if (num3 > 0)
			{
				recordedCrimes.Add(new PossessingModerateSeverityDrug(), num3);
			}
			if (num4 > 0)
			{
				recordedCrimes.Add(new PossessingHighSeverityDrug(), num4);
			}
		}

		private void ConfiscateItems(EStealthLevel maxStealthLevel)
		{
			List<ItemSlot> allInventorySlots = PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots();
			if (Player.Local.LastDrivenVehicle != null && Player.Local.TimeSinceVehicleExit < 30f && Player.Local.LastDrivenVehicle.Storage != null)
			{
				allInventorySlots.AddRange(Player.Local.LastDrivenVehicle.Storage.ItemSlots);
			}
			foreach (ItemSlot item in allInventorySlots)
			{
				if (item.ItemInstance == null)
				{
					continue;
				}
				if (item.ItemInstance is ProductItemInstance)
				{
					ProductItemInstance productItemInstance = item.ItemInstance as ProductItemInstance;
					if (productItemInstance.AppliedPackaging == null || productItemInstance.AppliedPackaging.StealthLevel <= maxStealthLevel)
					{
						item.ClearStoredInstance();
					}
				}
				else if (item.ItemInstance.Definition.legalStatus != 0)
				{
					item.ClearStoredInstance();
				}
			}
		}

		private void ClearEntries()
		{
			int childCount = CrimeEntryContainer.childCount;
			for (int i = 0; i < childCount; i++)
			{
				UnityEngine.Object.Destroy(CrimeEntryContainer.GetChild(i).gameObject);
			}
			childCount = PenaltyEntryContainer.childCount;
			for (int j = 0; j < childCount; j++)
			{
				UnityEngine.Object.Destroy(PenaltyEntryContainer.GetChild(j).gameObject);
			}
		}
	}
}
