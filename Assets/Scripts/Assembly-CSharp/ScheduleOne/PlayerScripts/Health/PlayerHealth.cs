using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerScripts.Health
{
	public class PlayerHealth : NetworkBehaviour
	{
		public const float MAX_HEALTH = 100f;

		public const float HEALTH_RECOVERY_PER_MINUTE = 0.5f;

		[Header("References")]
		public Player Player;

		public ParticleSystem BloodParticles;

		public UnityEvent<float> onHealthChanged;

		public UnityEvent onDie;

		public UnityEvent onRevive;

		private bool NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted;

		public bool IsAlive { get; protected set; } = true;


		public float CurrentHealth { get; protected set; } = 100f;


		public float TimeSinceLastDamage { get; protected set; }

		public bool CanTakeDamage
		{
			get
			{
				if (IsAlive && !Player.Local.IsArrested)
				{
					return !Player.Local.IsUnconscious;
				}
				return false;
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealth_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		private void Start()
		{
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance2.onMinutePass = (Action)Delegate.Combine(instance2.onMinutePass, new Action(MinPass));
		}

		[ObserversRpc]
		public void TakeDamage(float damage, bool flinch = true, bool playBloodMist = true)
		{
			RpcWriter___Observers_TakeDamage_3505310624(damage, flinch, playBloodMist);
		}

		private void Update()
		{
			TimeSinceLastDamage += Time.deltaTime;
		}

		private void MinPass()
		{
			if (IsAlive && CurrentHealth < 100f && TimeSinceLastDamage > 30f)
			{
				RecoverHealth(0.5f);
			}
		}

		public void RecoverHealth(float recovery)
		{
			if (CurrentHealth == 0f)
			{
				Console.LogWarning("RecoverHealth called on dead player. Use Revive() instead.");
				return;
			}
			CurrentHealth = Mathf.Clamp(CurrentHealth + recovery, 0f, 100f);
			if (onHealthChanged != null)
			{
				onHealthChanged.Invoke(CurrentHealth);
			}
		}

		public void SetHealth(float health)
		{
			CurrentHealth = Mathf.Clamp(health, 0f, 100f);
			if (onHealthChanged != null)
			{
				onHealthChanged.Invoke(CurrentHealth);
			}
			if (CurrentHealth <= 0f)
			{
				SendDie();
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendDie()
		{
			RpcWriter___Server_SendDie_2166136261();
			RpcLogic___SendDie_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		public void Die()
		{
			RpcWriter___Observers_Die_2166136261();
			RpcLogic___Die_2166136261();
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendRevive(Vector3 position, Quaternion rotation)
		{
			RpcWriter___Server_SendRevive_3848837105(position, rotation);
			RpcLogic___SendRevive_3848837105(position, rotation);
		}

		[ObserversRpc(RunLocally = true, ExcludeOwner = true)]
		public void Revive(Vector3 position, Quaternion rotation)
		{
			RpcWriter___Observers_Revive_3848837105(position, rotation);
			RpcLogic___Revive_3848837105(position, rotation);
		}

		[ObserversRpc]
		public void PlayBloodMist()
		{
			RpcWriter___Observers_PlayBloodMist_2166136261();
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterObserversRpc(0u, RpcReader___Observers_TakeDamage_3505310624);
				RegisterServerRpc(1u, RpcReader___Server_SendDie_2166136261);
				RegisterObserversRpc(2u, RpcReader___Observers_Die_2166136261);
				RegisterServerRpc(3u, RpcReader___Server_SendRevive_3848837105);
				RegisterObserversRpc(4u, RpcReader___Observers_Revive_3848837105);
				RegisterObserversRpc(5u, RpcReader___Observers_PlayBloodMist_2166136261);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_TakeDamage_3505310624(float damage, bool flinch = true, bool playBloodMist = true)
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
				writer.WriteSingle(damage);
				writer.WriteBoolean(flinch);
				writer.WriteBoolean(playBloodMist);
				SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___TakeDamage_3505310624(float damage, bool flinch = true, bool playBloodMist = true)
		{
			if (!IsAlive)
			{
				return;
			}
			if (!CanTakeDamage)
			{
				Console.LogWarning("Player cannot take damage right now.");
				return;
			}
			CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0f, 100f);
			Console.Log(damage + " damange taken. New health: " + CurrentHealth);
			TimeSinceLastDamage = 0f;
			if (onHealthChanged != null)
			{
				onHealthChanged.Invoke(CurrentHealth);
			}
			if (Player.IsOwner)
			{
				if (flinch && PlayerSingleton<PlayerCamera>.InstanceExists)
				{
					PlayerSingleton<PlayerCamera>.Instance.JoltCamera();
				}
				if (CurrentHealth <= 0f)
				{
					SendDie();
				}
			}
			if (playBloodMist)
			{
				PlayBloodMist();
			}
		}

		private void RpcReader___Observers_TakeDamage_3505310624(PooledReader PooledReader0, Channel channel)
		{
			float damage = PooledReader0.ReadSingle();
			bool flinch = PooledReader0.ReadBoolean();
			bool playBloodMist = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___TakeDamage_3505310624(damage, flinch, playBloodMist);
			}
		}

		private void RpcWriter___Server_SendDie_2166136261()
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
				SendServerRpc(1u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendDie_2166136261()
		{
			Die();
		}

		private void RpcReader___Server_SendDie_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendDie_2166136261();
			}
		}

		private void RpcWriter___Observers_Die_2166136261()
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

		public void RpcLogic___Die_2166136261()
		{
			if (!IsAlive)
			{
				Console.LogWarning("Already dead!");
				return;
			}
			IsAlive = false;
			Debug.Log(Player?.ToString() + " died.");
			if (onDie != null)
			{
				onDie.Invoke();
			}
			Debug.Log("Dead!");
		}

		private void RpcReader___Observers_Die_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Die_2166136261();
			}
		}

		private void RpcWriter___Server_SendRevive_3848837105(Vector3 position, Quaternion rotation)
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
				writer.WriteVector3(position);
				writer.WriteQuaternion(rotation);
				SendServerRpc(3u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendRevive_3848837105(Vector3 position, Quaternion rotation)
		{
			Revive(position, rotation);
		}

		private void RpcReader___Server_SendRevive_3848837105(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendRevive_3848837105(position, rotation);
			}
		}

		private void RpcWriter___Observers_Revive_3848837105(Vector3 position, Quaternion rotation)
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
				writer.WriteVector3(position);
				writer.WriteQuaternion(rotation);
				SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: true);
				writer.Store();
			}
		}

		public void RpcLogic___Revive_3848837105(Vector3 position, Quaternion rotation)
		{
			if (IsAlive)
			{
				Console.LogWarning("Revive called on living player. Use RecoverHealth() instead.");
				return;
			}
			CurrentHealth = 100f;
			IsAlive = true;
			if (onHealthChanged != null)
			{
				onHealthChanged.Invoke(CurrentHealth);
			}
			if (onRevive != null)
			{
				onRevive.Invoke();
			}
			if (base.IsOwner)
			{
				Singleton<HUD>.Instance.canvas.enabled = true;
				Player.Local.Energy.RestoreEnergy();
				PlayerSingleton<PlayerMovement>.Instance.Teleport(position);
				Player.Local.transform.rotation = rotation;
				PlayerSingleton<PlayerCamera>.Instance.ResetRotation();
			}
		}

		private void RpcReader___Observers_Revive_3848837105(PooledReader PooledReader0, Channel channel)
		{
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Revive_3848837105(position, rotation);
			}
		}

		private void RpcWriter___Observers_PlayBloodMist_2166136261()
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
				SendObserversRpc(5u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___PlayBloodMist_2166136261()
		{
			LayerUtility.SetLayerRecursively(BloodParticles.gameObject, LayerMask.NameToLayer("Default"));
			BloodParticles.Play();
		}

		private void RpcReader___Observers_PlayBloodMist_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___PlayBloodMist_2166136261();
			}
		}

		private void Awake_UserLogic_ScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealth_Assembly_002DCSharp_002Edll()
		{
			Singleton<SleepCanvas>.Instance.onSleepFullyFaded.AddListener(delegate
			{
				SetHealth(100f);
			});
		}
	}
}
