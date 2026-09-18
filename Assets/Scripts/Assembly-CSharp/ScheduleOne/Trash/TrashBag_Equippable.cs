using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.Trash
{
	public class TrashBag_Equippable : Equippable_Viewmodel
	{
		public const float TRASH_CONTAINER_INTERACT_DISTANCE = 2.75f;

		public const float BAG_TRASH_TIME = 1f;

		public const float PICKUP_RANGE = 3f;

		public const float PICKUP_AREA_RADIUS = 0.5f;

		public LayerMask PickupLookMask;

		[Header("References")]
		public DecalProjector PickupAreaProjector;

		public AudioSourceController RustleSound;

		public AudioSourceController BagSound;

		private float _bagTrashTime;

		private TrashContainer _baggedContainer;

		private float _pickupTrashTime;

		public static bool IsHoveringTrash => Singleton<TrashBagCanvas>.Instance.InputPrompt.gameObject.activeSelf;

		public bool IsBaggingTrash { get; private set; }

		public bool IsPickingUpTrash { get; private set; }

		public override void Equip(ItemInstance item)
		{
			base.Equip(item);
			Singleton<TrashBagCanvas>.Instance.InputPrompt.gameObject.SetActive(value: false);
			Singleton<TrashBagCanvas>.Instance.Open();
			PickupAreaProjector.transform.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
			PickupAreaProjector.transform.localScale = Vector3.one;
			PickupAreaProjector.transform.forward = -Vector3.up;
			PickupAreaProjector.gameObject.SetActive(value: false);
		}

		public override void Unequip()
		{
			base.Unequip();
			Singleton<TrashBagCanvas>.Instance.Close();
			Object.Destroy(PickupAreaProjector.gameObject);
		}

		protected override void Update()
		{
			base.Update();
			Singleton<TrashBagCanvas>.Instance.InputPrompt.gameObject.SetActive(value: false);
			TrashContainer hoveredTrashContainer = GetHoveredTrashContainer();
			PickupAreaProjector.gameObject.SetActive(value: false);
			if (IsBaggingTrash)
			{
				if (!GameInput.GetButton(GameInput.ButtonCode.Interact) || hoveredTrashContainer != _baggedContainer)
				{
					StopBagTrash(complete: false);
					return;
				}
				_bagTrashTime += Time.deltaTime;
				Singleton<TrashBagCanvas>.Instance.InputPrompt.SetLabel("Bag trash");
				Singleton<TrashBagCanvas>.Instance.InputPrompt.gameObject.SetActive(value: true);
				Singleton<HUD>.Instance.ShowRadialIndicator(_bagTrashTime / 1f);
				if (_bagTrashTime >= 1f)
				{
					StopBagTrash(complete: true);
				}
			}
			else if (IsPickingUpTrash)
			{
				List<TrashItem> list = new List<TrashItem>();
				if (RaycastLook(out var hit) && IsPickupLocationValid(hit))
				{
					list = GetTrashItemsAtPoint(hit.point);
				}
				if (!GameInput.GetButton(GameInput.ButtonCode.Interact) || list.Count == 0)
				{
					StopPickup(complete: false);
					return;
				}
				_pickupTrashTime += Time.deltaTime;
				Singleton<TrashBagCanvas>.Instance.InputPrompt.SetLabel("Bag trash");
				Singleton<TrashBagCanvas>.Instance.InputPrompt.gameObject.SetActive(value: true);
				Singleton<HUD>.Instance.ShowRadialIndicator(_pickupTrashTime / 1f);
				PickupAreaProjector.transform.position = hit.point + Vector3.up * 0.1f;
				PickupAreaProjector.gameObject.SetActive(value: true);
				if (_pickupTrashTime >= 1f)
				{
					StopPickup(complete: true);
				}
			}
			else if (hoveredTrashContainer != null && hoveredTrashContainer.CanBeBagged())
			{
				_baggedContainer = hoveredTrashContainer;
				Singleton<TrashBagCanvas>.Instance.InputPrompt.SetLabel("Bag trash");
				Singleton<TrashBagCanvas>.Instance.InputPrompt.gameObject.SetActive(value: true);
				if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
				{
					StartBagTrash(hoveredTrashContainer);
				}
			}
			else
			{
				if (!(hoveredTrashContainer == null) || !RaycastLook(out var hit2) || !IsPickupLocationValid(hit2))
				{
					return;
				}
				PickupAreaProjector.transform.position = hit2.point + Vector3.up * 0.1f;
				PickupAreaProjector.gameObject.SetActive(value: true);
				if (GetTrashItemsAtPoint(hit2.point).Count > 0)
				{
					PickupAreaProjector.fadeFactor = 0.5f;
					Singleton<TrashBagCanvas>.Instance.InputPrompt.SetLabel("Bag trash");
					Singleton<TrashBagCanvas>.Instance.InputPrompt.gameObject.SetActive(value: true);
					if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
					{
						StartPickup();
					}
				}
				else
				{
					PickupAreaProjector.fadeFactor = 0.05f;
				}
			}
		}

		private TrashContainer GetHoveredTrashContainer()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(2.75f, out var hit, Singleton<InteractionManager>.Instance.Interaction_SearchMask))
			{
				TrashContainer componentInParent = hit.collider.GetComponentInParent<TrashContainer>();
				if (componentInParent != null)
				{
					return componentInParent;
				}
			}
			return null;
		}

		private bool RaycastLook(out RaycastHit hit)
		{
			return PlayerSingleton<PlayerCamera>.Instance.LookRaycast(3f, out hit, PickupLookMask);
		}

		private bool IsPickupLocationValid(RaycastHit hit)
		{
			if (Vector3.Angle(hit.normal, Vector3.up) > 5f)
			{
				return false;
			}
			return true;
		}

		private List<TrashItem> GetTrashItemsAtPoint(Vector3 pos)
		{
			Collider[] array = Physics.OverlapSphere(pos, 0.45f, Singleton<InteractionManager>.Instance.Interaction_SearchMask, QueryTriggerInteraction.Collide);
			List<TrashItem> list = new List<TrashItem>();
			Collider[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				TrashItem componentInParent = array2[i].GetComponentInParent<TrashItem>();
				if (componentInParent != null && componentInParent.CanGoInContainer)
				{
					list.Add(componentInParent);
				}
			}
			return list;
		}

		private void StartBagTrash(TrashContainer container)
		{
			IsBaggingTrash = true;
			_bagTrashTime = 0f;
			_baggedContainer = container;
			RustleSound.Play();
		}

		private void StopBagTrash(bool complete)
		{
			IsBaggingTrash = false;
			_bagTrashTime = 0f;
			RustleSound.Stop();
			if (complete)
			{
				_baggedContainer.BagTrash();
				BagSound.PlayOneShot(duplicateAudioSource: true);
				itemInstance.ChangeQuantity(-1);
			}
			_baggedContainer = null;
		}

		private void StartPickup()
		{
			IsPickingUpTrash = true;
			_pickupTrashTime = 0f;
			RustleSound.Play();
		}

		private void StopPickup(bool complete)
		{
			IsPickingUpTrash = false;
			_pickupTrashTime = 0f;
			PickupAreaProjector.gameObject.SetActive(value: false);
			RustleSound.Stop();
			if (!complete)
			{
				return;
			}
			List<TrashItem> trashItemsAtPoint = GetTrashItemsAtPoint(PickupAreaProjector.transform.position);
			foreach (TrashItem item in trashItemsAtPoint)
			{
				item.DestroyTrash();
			}
			itemInstance.ChangeQuantity(-1);
			TrashContentData content = new TrashContentData(trashItemsAtPoint);
			NetworkSingleton<TrashManager>.Instance.CreateTrashBag(NetworkSingleton<TrashManager>.Instance.TrashBagPrefab.ID, PickupAreaProjector.transform.position + Vector3.up * 0.4f, Quaternion.identity, content);
			BagSound.PlayOneShot(duplicateAudioSource: true);
		}
	}
}
