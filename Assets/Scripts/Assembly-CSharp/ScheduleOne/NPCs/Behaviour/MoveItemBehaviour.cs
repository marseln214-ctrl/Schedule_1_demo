using System;
using System.Collections;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class MoveItemBehaviour : Behaviour
	{
		public enum EState
		{
			Idle = 0,
			WalkingToSource = 1,
			Grabbing = 2,
			WalkingToDestination = 3,
			Placing = 4
		}

		private TransitRoute assignedRoute;

		private string itemToRetrieveID = string.Empty;

		private int grabbedAmount;

		private int maxMoveAmount = -1;

		private EState currentState;

		private Coroutine walkToSourceRoutine;

		private Coroutine grabRoutine;

		private Coroutine walkToDestinationRoutine;

		private Coroutine placingRoutine;

		private bool skipPickup;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public bool Initialized { get; protected set; }

		public void Initialize(TransitRoute route, string _itemToRetrieveID, int _maxMoveAmount = -1, bool _skipPickup = false)
		{
			if (!IsTransitRouteValid(route, _itemToRetrieveID))
			{
				Console.LogError("Invalid transit route for move item behaviour!");
				return;
			}
			assignedRoute = route;
			itemToRetrieveID = _itemToRetrieveID;
			maxMoveAmount = _maxMoveAmount;
			skipPickup = _skipPickup;
		}

		public void Resume(TransitRoute route, string _itemToRetrieveID, int _maxMoveAmount = -1)
		{
			assignedRoute = route;
			itemToRetrieveID = _itemToRetrieveID;
			maxMoveAmount = _maxMoveAmount;
		}

		protected override void Begin()
		{
			base.Begin();
			StartTransit();
		}

		protected override void Pause()
		{
			base.Pause();
			StopCurrentActivity();
		}

		protected override void Resume()
		{
			base.Resume();
			StartTransit();
		}

		protected override void End()
		{
			base.End();
			skipPickup = false;
			EndTransit();
		}

		public override void Disable()
		{
			base.Disable();
			if (base.Active)
			{
				End();
			}
		}

		private void StartTransit()
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (base.Npc.Inventory._GetItemAmount(itemToRetrieveID) == 0)
			{
				if (!IsTransitRouteValid(assignedRoute, itemToRetrieveID))
				{
					Console.LogWarning("Invalid transit route for move item behaviour!");
					Disable_Networked(null);
					return;
				}
			}
			else if (!IsDestinationValid(assignedRoute, base.Npc.Inventory.GetFirstItem(itemToRetrieveID)))
			{
				Console.LogWarning("Invalid transit route for move item behaviour!");
				Disable_Networked(null);
				return;
			}
			currentState = EState.Idle;
		}

		private void EndTransit()
		{
			StopCurrentActivity();
			Initialized = false;
			assignedRoute = null;
			itemToRetrieveID = string.Empty;
			grabbedAmount = 0;
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (!InstanceFinder.IsServer || currentState != 0)
			{
				return;
			}
			if (base.Npc.Inventory._GetItemAmount(itemToRetrieveID) > 0)
			{
				if (IsAtDestination())
				{
					PlaceItem();
				}
				else
				{
					WalkToDestination();
				}
			}
			else if (skipPickup)
			{
				TakeItem();
				skipPickup = false;
			}
			else if (IsAtSource())
			{
				GrabItem();
			}
			else
			{
				WalkToSource();
			}
		}

		public void WalkToSource()
		{
			currentState = EState.WalkingToSource;
			walkToSourceRoutine = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				base.Npc.Movement.SetDestination(GetSourceAccessPoint(assignedRoute).position);
				yield return new WaitUntil(() => !base.Npc.Movement.IsMoving);
				currentState = EState.Idle;
				walkToSourceRoutine = null;
			}
		}

		public void GrabItem()
		{
			currentState = EState.Grabbing;
			grabRoutine = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				Transform sourceAccessPoint = GetSourceAccessPoint(assignedRoute);
				if (sourceAccessPoint == null)
				{
					Console.LogWarning("Could not find source access point!");
					grabRoutine = null;
					Disable_Networked(null);
				}
				else
				{
					Console.Log("Access Point: " + sourceAccessPoint.gameObject.name);
					base.Npc.Movement.FaceDirection(sourceAccessPoint.forward);
					base.Npc.SetAnimationTrigger_Networked(null, "GrabItem");
					float seconds = 0.5f;
					yield return new WaitForSeconds(seconds);
					if (!IsTransitRouteValid(assignedRoute, itemToRetrieveID))
					{
						Console.LogWarning("Transit route no longer valid!");
						grabRoutine = null;
						Disable_Networked(null);
					}
					else
					{
						TakeItem();
						yield return new WaitForSeconds(0.5f);
						grabRoutine = null;
						currentState = EState.Idle;
					}
				}
			}
		}

		private void TakeItem()
		{
			Debug.Log("Taking item");
			int amountToGrab = GetAmountToGrab();
			if (amountToGrab == 0)
			{
				Console.LogWarning("Amount to grab is 0!");
				return;
			}
			ItemSlot firstSlotContainingItem = assignedRoute.Source.GetFirstSlotContainingItem(itemToRetrieveID, ITransitEntity.ESlotType.Output);
			ItemInstance copy = (firstSlotContainingItem?.ItemInstance).GetCopy(amountToGrab);
			grabbedAmount = amountToGrab;
			firstSlotContainingItem.ChangeQuantity(-amountToGrab);
			base.Npc.Inventory.InsertItem(copy);
			assignedRoute.Destination.ReserveInputSlotsForItem(copy, base.Npc.NetworkObject);
		}

		public void WalkToDestination()
		{
			currentState = EState.WalkingToDestination;
			walkToDestinationRoutine = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				base.Npc.Movement.SetDestination(GetDestinationAccessPoint(assignedRoute).position);
				yield return new WaitUntil(() => !base.Npc.Movement.IsMoving);
				currentState = EState.Idle;
				walkToDestinationRoutine = null;
			}
		}

		public void PlaceItem()
		{
			currentState = EState.Placing;
			placingRoutine = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				if (GetDestinationAccessPoint(assignedRoute) != null)
				{
					base.Npc.Movement.FaceDirection(GetDestinationAccessPoint(assignedRoute).forward);
				}
				base.Npc.SetAnimationTrigger_Networked(null, "GrabItem");
				float seconds = 0.5f;
				yield return new WaitForSeconds(seconds);
				assignedRoute.Destination.RemoveSlotLocks(base.Npc.NetworkObject);
				ItemInstance firstItem = base.Npc.Inventory.GetFirstItem(itemToRetrieveID);
				if (firstItem != null)
				{
					ItemInstance copy = firstItem.GetCopy(grabbedAmount);
					firstItem.ChangeQuantity(-grabbedAmount);
					assignedRoute.Destination.InsertItemIntoInput(copy);
				}
				else
				{
					Console.LogWarning("Could not find carried item to place!");
				}
				yield return new WaitForSeconds(0.5f);
				placingRoutine = null;
				currentState = EState.Idle;
				Disable_Networked(null);
			}
		}

		private int GetAmountToGrab()
		{
			ItemInstance itemInstance = assignedRoute.Source.GetFirstSlotContainingItem(itemToRetrieveID, ITransitEntity.ESlotType.Output)?.ItemInstance;
			if (itemInstance == null)
			{
				return 0;
			}
			int num = itemInstance.Quantity;
			if (maxMoveAmount > 0)
			{
				num = Mathf.Min(maxMoveAmount, num);
				Debug.Log(num);
			}
			int inputCapacityForItem = assignedRoute.Destination.GetInputCapacityForItem(itemInstance);
			return Mathf.Min(num, inputCapacityForItem);
		}

		private void StopCurrentActivity()
		{
			switch (currentState)
			{
			case EState.WalkingToSource:
				if (walkToSourceRoutine != null)
				{
					StopCoroutine(walkToSourceRoutine);
				}
				break;
			case EState.Grabbing:
				if (grabRoutine != null)
				{
					StopCoroutine(grabRoutine);
				}
				break;
			case EState.WalkingToDestination:
				if (walkToDestinationRoutine != null)
				{
					StopCoroutine(walkToDestinationRoutine);
				}
				break;
			case EState.Placing:
				if (placingRoutine != null)
				{
					StopCoroutine(placingRoutine);
				}
				break;
			}
			currentState = EState.Idle;
		}

		public bool IsTransitRouteValid(TransitRoute route, string itemID)
		{
			if (route == null)
			{
				return false;
			}
			if (route.Source == null || route.Destination == null)
			{
				return false;
			}
			ItemInstance itemInstance = route.Source.GetFirstSlotContainingItem(itemID, ITransitEntity.ESlotType.Output)?.ItemInstance;
			if (itemInstance == null || itemInstance.Quantity <= 0)
			{
				return false;
			}
			if (!IsDestinationValid(route, itemInstance))
			{
				return false;
			}
			return true;
		}

		public bool IsDestinationValid(TransitRoute route, ItemInstance item)
		{
			if (route.Destination.GetInputCapacityForItem(item) == 0)
			{
				return false;
			}
			if (!CanGetToDestination(route))
			{
				Console.LogWarning("Cannot get to destination!");
				return false;
			}
			if (!CanGetToSource(route))
			{
				Console.LogWarning("Cannot get to source!");
				return false;
			}
			return true;
		}

		public bool CanGetToSource(TransitRoute route)
		{
			return GetSourceAccessPoint(route) != null;
		}

		private Transform GetSourceAccessPoint(TransitRoute route)
		{
			return NavMeshUtility.GetAccessPoint(route.Source, base.Npc);
		}

		private bool IsAtSource()
		{
			return NavMeshUtility.IsAtTransitEntity(assignedRoute.Source, base.Npc);
		}

		public bool CanGetToDestination(TransitRoute route)
		{
			return GetDestinationAccessPoint(route) != null;
		}

		private Transform GetDestinationAccessPoint(TransitRoute route)
		{
			if (route.Destination == null)
			{
				Console.LogWarning("Destination is null!");
				return null;
			}
			return NavMeshUtility.GetAccessPoint(route.Destination, base.Npc);
		}

		private bool IsAtDestination()
		{
			return NavMeshUtility.IsAtTransitEntity(assignedRoute.Destination, base.Npc);
		}

		public MoveItemData GetSaveData()
		{
			if (!base.Active || grabbedAmount == 0)
			{
				return null;
			}
			return new MoveItemData(itemToRetrieveID, grabbedAmount, (assignedRoute.Source as IGUIDRegisterable).GUID, (assignedRoute.Destination as IGUIDRegisterable).GUID);
		}

		public void Load(MoveItemData moveItemData)
		{
			if (moveItemData != null && moveItemData.GrabbedItemQuantity != 0 && !string.IsNullOrEmpty(moveItemData.GrabbedItemID))
			{
				ITransitEntity @object = GUIDManager.GetObject<ITransitEntity>(new Guid(moveItemData.SourceGUID));
				ITransitEntity object2 = GUIDManager.GetObject<ITransitEntity>(new Guid(moveItemData.DestinationGUID));
				if (@object == null)
				{
					Console.LogWarning("Failed to load source transit entity");
					return;
				}
				if (object2 == null)
				{
					Console.LogWarning("Failed to load destination transit entity");
					return;
				}
				TransitRoute route = new TransitRoute(@object, object2);
				grabbedAmount = moveItemData.GrabbedItemQuantity;
				Debug.Log("Resuming move item behaviour");
				Resume(route, moveItemData.GrabbedItemID);
				Enable_Networked(null);
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
