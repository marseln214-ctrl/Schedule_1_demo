using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Behaviour
{
	public class ConsumeProductBehaviour : Behaviour
	{
		public AvatarEquippable JointPrefab;

		private ProductItemInstance product;

		private Coroutine consumeRoutine;

		public AudioSourceController WeedConsumeSound;

		public ParticleSystem SmokeExhaleParticles;

		[Header("Debug")]
		public ProductDefinition TestProduct;

		public UnityEvent onConsumeDone;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public ProductItemInstance ConsumedProduct { get; private set; }

		protected virtual void Start()
		{
			ScheduleOne.GameTime.TimeManager.onSleepEnd = (Action<int>)Delegate.Remove(ScheduleOne.GameTime.TimeManager.onSleepEnd, new Action<int>(DayPass));
			ScheduleOne.GameTime.TimeManager.onSleepEnd = (Action<int>)Delegate.Combine(ScheduleOne.GameTime.TimeManager.onSleepEnd, new Action<int>(DayPass));
			if (TestProduct != null && Application.isEditor)
			{
				product = TestProduct.GetDefaultInstance() as ProductItemInstance;
				Enable();
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendProduct(ProductItemInstance _product)
		{
			RpcWriter___Server_SendProduct_2622925554(_product);
			RpcLogic___SendProduct_2622925554(_product);
		}

		[ObserversRpc(RunLocally = true)]
		public void SetProduct(ProductItemInstance _product)
		{
			RpcWriter___Observers_SetProduct_2622925554(_product);
			RpcLogic___SetProduct_2622925554(_product);
		}

		[ObserversRpc(RunLocally = true)]
		public void ClearEffects()
		{
			RpcWriter___Observers_ClearEffects_2166136261();
			RpcLogic___ClearEffects_2166136261();
		}

		protected override void Begin()
		{
			base.Begin();
			TryConsume();
		}

		protected override void Resume()
		{
			base.Resume();
			TryConsume();
		}

		private void TryConsume()
		{
			if (product == null)
			{
				Console.LogError("No product to consume");
				Disable();
				return;
			}
			switch ((product.Definition as ProductDefinition).DrugType)
			{
			case EDrugType.Marijuana:
				ConsumeWeed();
				break;
			case EDrugType.Methamphetamine:
			case EDrugType.Cocaine:
				break;
			}
		}

		public override void Disable()
		{
			base.Disable();
			Clear();
			End();
		}

		protected override void End()
		{
			base.End();
			if (consumeRoutine != null)
			{
				StopCoroutine(consumeRoutine);
				consumeRoutine = null;
			}
			base.Npc.SetEquippable_Return(string.Empty);
		}

		private void ConsumeWeed()
		{
			consumeRoutine = StartCoroutine(ConsumeWeedRoutine());
			IEnumerator ConsumeWeedRoutine()
			{
				base.Npc.SetEquippable_Return(JointPrefab.AssetPath);
				base.Npc.Avatar.Anim.SetBool("Smoking", value: true);
				WeedConsumeSound.Play();
				yield return new WaitForSeconds(3f);
				SmokeExhaleParticles.Play();
				yield return new WaitForSeconds(1.5f);
				base.Npc.Avatar.Anim.SetBool("Smoking", value: false);
				if (InstanceFinder.IsServer)
				{
					ApplyEffects();
					Disable_Networked(null);
				}
				if (onConsumeDone != null)
				{
					onConsumeDone.Invoke();
				}
			}
		}

		[ObserversRpc]
		private void ApplyEffects()
		{
			RpcWriter___Observers_ApplyEffects_2166136261();
		}

		private void Clear()
		{
			base.Npc.Avatar.Anim.SetBool("Smoking", value: false);
		}

		private void DayPass(int minsSlept)
		{
			if (ConsumedProduct != null)
			{
				ClearEffects();
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(15u, RpcReader___Server_SendProduct_2622925554);
				RegisterObserversRpc(16u, RpcReader___Observers_SetProduct_2622925554);
				RegisterObserversRpc(17u, RpcReader___Observers_ClearEffects_2166136261);
				RegisterObserversRpc(18u, RpcReader___Observers_ApplyEffects_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendProduct_2622925554(ProductItemInstance _product)
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
				writer.WriteProductItemInstance(_product);
				SendServerRpc(15u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendProduct_2622925554(ProductItemInstance _product)
		{
			SetProduct(_product);
		}

		private void RpcReader___Server_SendProduct_2622925554(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			ProductItemInstance productItemInstance = PooledReader0.ReadProductItemInstance();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendProduct_2622925554(productItemInstance);
			}
		}

		private void RpcWriter___Observers_SetProduct_2622925554(ProductItemInstance _product)
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
				writer.WriteProductItemInstance(_product);
				SendObserversRpc(16u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetProduct_2622925554(ProductItemInstance _product)
		{
			product = _product;
		}

		private void RpcReader___Observers_SetProduct_2622925554(PooledReader PooledReader0, Channel channel)
		{
			ProductItemInstance productItemInstance = PooledReader0.ReadProductItemInstance();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetProduct_2622925554(productItemInstance);
			}
		}

		private void RpcWriter___Observers_ClearEffects_2166136261()
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
				SendObserversRpc(17u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___ClearEffects_2166136261()
		{
			if (ConsumedProduct != null)
			{
				ConsumedProduct.ClearEffectsFromNPC(base.Npc);
				ConsumedProduct = null;
			}
		}

		private void RpcReader___Observers_ClearEffects_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ClearEffects_2166136261();
			}
		}

		private void RpcWriter___Observers_ApplyEffects_2166136261()
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
				SendObserversRpc(18u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ApplyEffects_2166136261()
		{
			if (ConsumedProduct != null)
			{
				ClearEffects();
			}
			ConsumedProduct = product;
			if (product != null)
			{
				product.ApplyEffectsToNPC(base.Npc);
			}
		}

		private void RpcReader___Observers_ApplyEffects_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___ApplyEffects_2166136261();
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
