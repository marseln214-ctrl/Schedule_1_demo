using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ItemFramework
{
	[RequireComponent(typeof(InteractableObject))]
	public class ItemPickup : NetworkBehaviour
	{
		public ItemDefinition ItemToGive;

		public bool DestroyOnPickup = true;

		public bool ConditionallyActive;

		public Condition ActiveCondition;

		public bool Networked = true;

		[Header("References")]
		public InteractableObject IntObj;

		public UnityEvent onPickup;

		private bool NetworkInitialize___EarlyScheduleOne_002EItemFramework_002EItemPickupAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EItemFramework_002EItemPickupAssembly_002DCSharp_002Edll_Excuted;

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EItemFramework_002EItemPickup_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		private void Start()
		{
			if (Player.Local != null)
			{
				Init();
			}
			else
			{
				Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Init));
			}
		}

		private void Init()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(Init));
			Singleton<CoroutineService>.Instance.StartCoroutine(Wait());
			IEnumerator Wait()
			{
				yield return new WaitUntil(() => Player.Local.playerDataRetrieveReturned);
				if (ConditionallyActive && ActiveCondition != null)
				{
					base.gameObject.SetActive(ActiveCondition.Evaluate());
				}
			}
		}

		protected virtual void Hovered()
		{
			if (CanPickup())
			{
				IntObj.SetMessage("Pick up " + ItemToGive.Name);
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				IntObj.SetMessage("Inventory Full");
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			}
		}

		private void Interacted()
		{
			if (CanPickup())
			{
				Pickup();
			}
		}

		protected virtual bool CanPickup()
		{
			if (ItemToGive != null)
			{
				return PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(ItemToGive.GetDefaultInstance());
			}
			return false;
		}

		protected virtual void Pickup()
		{
			if (ItemToGive != null)
			{
				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(ItemToGive.GetDefaultInstance());
			}
			if (onPickup != null)
			{
				onPickup.Invoke();
			}
			if (DestroyOnPickup)
			{
				if (Networked)
				{
					Destroy();
				}
				else
				{
					base.gameObject.SetActive(value: false);
				}
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void Destroy()
		{
			RpcWriter___Server_Destroy_2166136261();
			RpcLogic___Destroy_2166136261();
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EItemFramework_002EItemPickupAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EItemFramework_002EItemPickupAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_Destroy_2166136261);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EItemFramework_002EItemPickupAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EItemFramework_002EItemPickupAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_Destroy_2166136261()
		{
			if (!base.IsClientInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___Destroy_2166136261()
		{
			if (base.IsServer)
			{
				base.NetworkObject.Despawn();
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void RpcReader___Server_Destroy_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___Destroy_2166136261();
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EItemFramework_002EItemPickup_Assembly_002DCSharp_002Edll()
		{
			if (ItemToGive != null)
			{
				IntObj.SetMessage("Pick up " + ItemToGive.Name);
			}
			IntObj.onHovered.AddListener(Hovered);
			IntObj.onInteractStart.AddListener(Interacted);
		}
	}
}
