using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.Misc;
using ScheduleOne.Money;
using ScheduleOne.Trash;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts
{
	public class Recycler : NetworkBehaviour
	{
		public enum EState
		{
			HatchClosed = 0,
			HatchOpen = 1,
			Processing = 2
		}

		public LayerMask DetectionMask;

		[Header("References")]
		public InteractableObject HandleIntObj;

		public InteractableObject ButtonIntObj;

		public InteractableObject CashIntObj;

		public ToggleableLight ButtonLight;

		public Animation ButtonAnim;

		public Animation HatchAnim;

		public Animation CashAnim;

		public RectTransform OpenHatchInstruction;

		public RectTransform InsertTrashInstruction;

		public RectTransform PressBeginInstruction;

		public RectTransform ProcessingScreen;

		public TextMeshProUGUI ProcessingLabel;

		public TextMeshProUGUI ValueLabel;

		public BoxCollider CheckCollider;

		public Transform Cash;

		public GameObject BankNote;

		[Header("Sound")]
		public AudioSourceController OpenSound;

		public AudioSourceController CloseSound;

		public AudioSourceController PressSound;

		public AudioSourceController DoneSound;

		public AudioSourceController CashEjectSound;

		private float cashValue;

		public UnityEvent onStart;

		public UnityEvent onStop;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted;

		public EState State { get; protected set; }

		public bool IsHatchOpen { get; private set; }

		public void Start()
		{
			HandleIntObj.onInteractStart.AddListener(HandleInteracted);
			ButtonIntObj.onInteractStart.AddListener(ButtonInteracted);
			CashIntObj.onInteractStart.AddListener(CashInteracted);
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			SetState(connection, State, force: true);
		}

		private void OnDestroy()
		{
			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.InstanceExists)
			{
				TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			}
		}

		private void MinPass()
		{
			if (State == EState.HatchOpen)
			{
				OpenHatchInstruction.gameObject.SetActive(value: false);
				InsertTrashInstruction.gameObject.SetActive(value: false);
				PressBeginInstruction.gameObject.SetActive(value: false);
				ProcessingScreen.gameObject.SetActive(value: false);
				if (GetTrash().Length != 0)
				{
					ButtonIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
					ButtonLight.isOn = true;
					PressBeginInstruction.gameObject.SetActive(value: true);
				}
				else
				{
					ButtonIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
					ButtonLight.isOn = false;
					InsertTrashInstruction.gameObject.SetActive(value: true);
				}
			}
		}

		public void HandleInteracted()
		{
			SendState(EState.HatchOpen);
		}

		public void ButtonInteracted()
		{
			ProcessingLabel.text = "Processing...";
			ValueLabel.text = MoneyManager.FormatAmount(0f);
			PressSound.Play();
			SendState(EState.Processing);
			StartCoroutine(Process());
		}

		public void CashInteracted()
		{
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(cashValue);
			SendState(EState.HatchClosed);
			BankNote.gameObject.SetActive(value: false);
			cashValue = 0f;
			SendCashCollected();
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendCashCollected()
		{
			RpcWriter___Server_SendCashCollected_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		private void CashCollected()
		{
			RpcWriter___Observers_CashCollected_2166136261();
			RpcLogic___CashCollected_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		private void EnableCash()
		{
			RpcWriter___Observers_EnableCash_2166136261();
			RpcLogic___EnableCash_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		private void SetCashValue(float amount)
		{
			RpcWriter___Observers_SetCashValue_431000436(amount);
			RpcLogic___SetCashValue_431000436(amount);
		}

		private IEnumerator Process()
		{
			yield return new WaitForSeconds(0.5f);
			if (onStart != null)
			{
				onStart.Invoke();
			}
			TrashItem[] trash = GetTrash();
			float value = 0f;
			TrashItem[] array = trash;
			foreach (TrashItem trashItem in array)
			{
				if (trashItem is TrashBag)
				{
					foreach (TrashContent.Entry entry in ((TrashBag)trashItem).Content.Entries)
					{
						value += (float)(entry.UnitValue * entry.Quantity);
					}
				}
				else
				{
					value += (float)trashItem.SellValue;
				}
				if (InstanceFinder.IsServer)
				{
					trashItem.DestroyTrash();
				}
			}
			if (cashValue <= 0f)
			{
				SetCashValue(value);
			}
			float lerpTime = 1.5f;
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				float t = i / lerpTime;
				float amount = Mathf.Lerp(0f, cashValue, t);
				ValueLabel.text = MoneyManager.FormatAmount(amount, showDecimals: true);
				yield return new WaitForEndOfFrame();
			}
			if (onStop != null)
			{
				onStop.Invoke();
			}
			ProcessingLabel.text = "Thank you";
			ValueLabel.text = MoneyManager.FormatAmount(value);
			DoneSound.Play();
			yield return new WaitForSeconds(0.3f);
			CashEjectSound.Play();
			CashAnim.Play();
			yield return new WaitForSeconds(0.25f);
			if (InstanceFinder.IsServer)
			{
				EnableCash();
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendState(EState state)
		{
			RpcWriter___Server_SendState_3569965459(state);
			RpcLogic___SendState_3569965459(state);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void SetState(NetworkConnection conn, EState state, bool force = false)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetState_3790170803(conn, state, force);
				RpcLogic___SetState_3790170803(conn, state, force);
			}
			else
			{
				RpcWriter___Target_SetState_3790170803(conn, state, force);
			}
		}

		private void SetHatchOpen(bool open)
		{
			if (open != IsHatchOpen)
			{
				IsHatchOpen = open;
				if (IsHatchOpen)
				{
					OpenSound.Play();
					HatchAnim.Play("Recycler open");
				}
				else
				{
					CloseSound.Play();
					HatchAnim.Play("Recycler close");
				}
			}
		}

		private TrashItem[] GetTrash()
		{
			List<TrashItem> list = new List<TrashItem>();
			Vector3 center = CheckCollider.transform.TransformPoint(CheckCollider.center);
			Vector3 halfExtents = Vector3.Scale(CheckCollider.size, CheckCollider.transform.lossyScale) * 0.5f;
			Collider[] array = Physics.OverlapBox(center, halfExtents, CheckCollider.transform.rotation, DetectionMask, QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				TrashItem componentInParent = array[i].GetComponentInParent<TrashItem>();
				if (componentInParent != null && !list.Contains(componentInParent))
				{
					list.Add(componentInParent);
				}
			}
			return list.ToArray();
		}

		private void OnDrawGizmos()
		{
			Vector3 center = CheckCollider.transform.TransformPoint(CheckCollider.center);
			Vector3 vector = Vector3.Scale(CheckCollider.size, CheckCollider.transform.lossyScale) * 0.5f;
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(center, vector * 2f);
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SendCashCollected_2166136261);
				RegisterObserversRpc(1u, RpcReader___Observers_CashCollected_2166136261);
				RegisterObserversRpc(2u, RpcReader___Observers_EnableCash_2166136261);
				RegisterObserversRpc(3u, RpcReader___Observers_SetCashValue_431000436);
				RegisterServerRpc(4u, RpcReader___Server_SendState_3569965459);
				RegisterObserversRpc(5u, RpcReader___Observers_SetState_3790170803);
				RegisterTargetRpc(6u, RpcReader___Target_SetState_3790170803);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendCashCollected_2166136261()
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

		private void RpcLogic___SendCashCollected_2166136261()
		{
			CashCollected();
		}

		private void RpcReader___Server_SendCashCollected_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized)
			{
				RpcLogic___SendCashCollected_2166136261();
			}
		}

		private void RpcWriter___Observers_CashCollected_2166136261()
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___CashCollected_2166136261()
		{
			SendState(EState.HatchClosed);
			BankNote.gameObject.SetActive(value: false);
			cashValue = 0f;
		}

		private void RpcReader___Observers_CashCollected_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___CashCollected_2166136261();
			}
		}

		private void RpcWriter___Observers_EnableCash_2166136261()
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				SendObserversRpc(2u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___EnableCash_2166136261()
		{
			CashIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}

		private void RpcReader___Observers_EnableCash_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___EnableCash_2166136261();
			}
		}

		private void RpcWriter___Observers_SetCashValue_431000436(float amount)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteSingle(amount);
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetCashValue_431000436(float amount)
		{
			cashValue = amount;
		}

		private void RpcReader___Observers_SetCashValue_431000436(PooledReader PooledReader0, Channel channel)
		{
			float amount = PooledReader0.ReadSingle();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetCashValue_431000436(amount);
			}
		}

		private void RpcWriter___Server_SendState_3569965459(EState state)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerated(writer, state);
				SendServerRpc(4u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendState_3569965459(EState state)
		{
			SetState(null, state);
		}

		private void RpcReader___Server_SendState_3569965459(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			EState state = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendState_3569965459(state);
			}
		}

		private void RpcWriter___Observers_SetState_3790170803(NetworkConnection conn, EState state, bool force = false)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerated(writer, state);
				writer.WriteBoolean(force);
				SendObserversRpc(5u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetState_3790170803(NetworkConnection conn, EState state, bool force = false)
		{
			if (State == state && !force)
			{
				return;
			}
			State = state;
			HandleIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			ButtonIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			CashIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			OpenHatchInstruction.gameObject.SetActive(value: false);
			InsertTrashInstruction.gameObject.SetActive(value: false);
			PressBeginInstruction.gameObject.SetActive(value: false);
			ProcessingScreen.gameObject.SetActive(value: false);
			ButtonLight.isOn = false;
			Cash.gameObject.SetActive(value: false);
			switch (State)
			{
			case EState.HatchClosed:
				HandleIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				OpenHatchInstruction.gameObject.SetActive(value: true);
				break;
			case EState.HatchOpen:
				if (GetTrash().Length != 0)
				{
					ButtonIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
					ButtonLight.isOn = true;
					PressBeginInstruction.gameObject.SetActive(value: true);
				}
				else
				{
					InsertTrashInstruction.gameObject.SetActive(value: true);
				}
				SetHatchOpen(open: true);
				break;
			case EState.Processing:
				StartCoroutine(Process());
				ProcessingScreen.gameObject.SetActive(value: true);
				ButtonAnim.Play();
				SetHatchOpen(open: false);
				break;
			}
		}

		private void RpcReader___Observers_SetState_3790170803(PooledReader PooledReader0, Channel channel)
		{
			EState state = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerateds(PooledReader0);
			bool force = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetState_3790170803(null, state, force);
			}
		}

		private void RpcWriter___Target_SetState_3790170803(NetworkConnection conn, EState state, bool force = false)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerated(writer, state);
				writer.WriteBoolean(force);
				SendTargetRpc(6u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetState_3790170803(PooledReader PooledReader0, Channel channel)
		{
			EState state = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerateds(PooledReader0);
			bool force = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetState_3790170803(base.LocalConnection, state, force);
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}
	}
}
