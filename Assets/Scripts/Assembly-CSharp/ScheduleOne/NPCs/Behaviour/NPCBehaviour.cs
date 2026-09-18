using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class NPCBehaviour : NetworkBehaviour
	{
		public bool DEBUG_MODE;

		[Header("References")]
		public NPCScheduleManager ScheduleManager;

		[Header("Default Behaviours")]
		public CoweringBehaviour CoweringBehaviour;

		public RagdollBehaviour RagdollBehaviour;

		public CallPoliceBehaviour CallPoliceBehaviour;

		public GenericDialogueBehaviour GenericDialogueBehaviour;

		public HeavyFlinchBehaviour HeavyFlinchBehaviour;

		public FacePlayerBehaviour FacePlayerBehaviour;

		public DeadBehaviour DeadBehaviour;

		public UnconsciousBehaviour UnconsciousBehaviour;

		public Behaviour SummonBehaviour;

		public ConsumeProductBehaviour ConsumeProductBehaviour;

		public CombatBehaviour CombatBehaviour;

		public FleeBehaviour FleeBehaviour;

		public StationaryBehaviour StationaryBehaviour;

		public RequestProductBehaviour RequestProductBehaviour;

		[SerializeField]
		protected List<Behaviour> behaviourStack = new List<Behaviour>();

		private Coroutine summonRoutine;

		[SerializeField]
		private List<Behaviour> enabledBehaviours = new List<Behaviour>();

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public Behaviour activeBehaviour { get; set; }

		public NPC Npc { get; private set; }

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviour_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected virtual void Start()
		{
			Npc.Avatar.Anim.onHeavyFlinch.AddListener(HeavyFlinchBehaviour.Flinch);
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance2.onMinutePass = (Action)Delegate.Combine(instance2.onMinutePass, new Action(MinPass));
			for (int i = 0; i < behaviourStack.Count; i++)
			{
				Behaviour b = behaviourStack[i];
				if (b.Enabled)
				{
					enabledBehaviours.Add(b);
				}
				b.onEnable.AddListener(delegate
				{
					AddEnabledBehaviour(b);
				});
				b.onDisable.AddListener(delegate
				{
					RemoveEnabledBehaviour(b);
				});
			}
		}

		private void OnDestroy()
		{
			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.InstanceExists)
			{
				TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			behaviourStack = GetComponentsInChildren<Behaviour>().ToList();
			SortBehaviourStack();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (activeBehaviour != null)
			{
				activeBehaviour.Begin_Networked(connection);
			}
		}

		[ServerRpc(RequireOwnership = false)]
		public void Summon(string buildingGUID, int doorIndex, float duration)
		{
			RpcWriter___Server_Summon_900355577(buildingGUID, doorIndex, duration);
		}

		[ServerRpc(RequireOwnership = false)]
		public void ConsumeProduct(ProductItemInstance product)
		{
			RpcWriter___Server_ConsumeProduct_2622925554(product);
		}

		protected virtual void OnKnockOut()
		{
			CoweringBehaviour.Disable_Networked(null);
			RagdollBehaviour.Disable_Networked(null);
			CallPoliceBehaviour.Disable_Networked(null);
			GenericDialogueBehaviour.Disable_Networked(null);
			HeavyFlinchBehaviour.Disable_Networked(null);
			FacePlayerBehaviour.Disable_Networked(null);
			SummonBehaviour.Disable_Networked(null);
			ConsumeProductBehaviour.Disable_Networked(null);
			CombatBehaviour.Disable_Networked(null);
			FleeBehaviour.Disable_Networked(null);
			StationaryBehaviour.Disable_Networked(null);
			RequestProductBehaviour.Disable_Networked(null);
			foreach (Behaviour item in behaviourStack)
			{
				if (!(item == DeadBehaviour) && !(item == UnconsciousBehaviour) && item.Active)
				{
					item.End_Networked(null);
				}
			}
		}

		protected virtual void OnDie()
		{
			OnKnockOut();
			UnconsciousBehaviour.Disable_Networked(null);
		}

		public Behaviour GetBehaviour(string BehaviourName)
		{
			Behaviour behaviour = behaviourStack.Find((Behaviour x) => x.Name.ToLower() == BehaviourName.ToLower());
			if (behaviour == null)
			{
				Console.LogWarning("No behaviour found with name '" + BehaviourName + "'");
			}
			return behaviour;
		}

		public virtual void Update()
		{
			if (DEBUG_MODE && activeBehaviour != null)
			{
				Debug.Log("Active behaviour: " + activeBehaviour.Name);
			}
			if (InstanceFinder.IsHost)
			{
				Behaviour enabledBehaviour = GetEnabledBehaviour();
				if (enabledBehaviour != activeBehaviour)
				{
					if (activeBehaviour != null)
					{
						activeBehaviour.Pause_Networked(null);
					}
					if (enabledBehaviour != null)
					{
						if (enabledBehaviour.Started)
						{
							enabledBehaviour.Resume_Networked(null);
						}
						else
						{
							enabledBehaviour.Begin_Networked(null);
						}
					}
				}
			}
			if (activeBehaviour != null && activeBehaviour.Active)
			{
				activeBehaviour.BehaviourUpdate();
			}
		}

		protected virtual void MinPass()
		{
			if (activeBehaviour != null && activeBehaviour.Active)
			{
				activeBehaviour.ActiveMinPass();
			}
		}

		public void SortBehaviourStack()
		{
			behaviourStack = behaviourStack.OrderByDescending((Behaviour x) => x.Priority).ToList();
		}

		private Behaviour GetEnabledBehaviour()
		{
			return enabledBehaviours.FirstOrDefault();
		}

		private void AddEnabledBehaviour(Behaviour b)
		{
			if (!enabledBehaviours.Contains(b))
			{
				enabledBehaviours.Add(b);
				enabledBehaviours = enabledBehaviours.OrderByDescending((Behaviour x) => x.Priority).ToList();
			}
		}

		private void RemoveEnabledBehaviour(Behaviour b)
		{
			if (enabledBehaviours.Contains(b))
			{
				enabledBehaviours.Remove(b);
				enabledBehaviours = enabledBehaviours.OrderByDescending((Behaviour x) => x.Priority).ToList();
			}
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_Summon_900355577);
				RegisterServerRpc(1u, RpcReader___Server_ConsumeProduct_2622925554);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_Summon_900355577(string buildingGUID, int doorIndex, float duration)
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
				writer.WriteString(buildingGUID);
				writer.WriteInt32(doorIndex);
				writer.WriteSingle(duration);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___Summon_900355577(string buildingGUID, int doorIndex, float duration)
		{
			NPCEnterableBuilding @object = GUIDManager.GetObject<NPCEnterableBuilding>(new Guid(buildingGUID));
			if (@object == null)
			{
				Console.LogError("Failed to find building with GUID: " + buildingGUID);
				return;
			}
			StaticDoor lastEnteredDoor = @object.Doors[doorIndex];
			Npc.LastEnteredDoor = lastEnteredDoor;
			SummonBehaviour.Enable_Networked(null);
			if (summonRoutine != null)
			{
				StopCoroutine(summonRoutine);
			}
			summonRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			IEnumerator Routine()
			{
				float t = 0f;
				while (Npc.IsConscious)
				{
					if (SummonBehaviour.Active)
					{
						t += Time.deltaTime;
						if (t >= duration)
						{
							break;
						}
					}
					yield return new WaitForEndOfFrame();
				}
				SummonBehaviour.Disable_Networked(null);
			}
		}

		private void RpcReader___Server_Summon_900355577(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string buildingGUID = PooledReader0.ReadString();
			int doorIndex = PooledReader0.ReadInt32();
			float duration = PooledReader0.ReadSingle();
			if (base.IsServerInitialized)
			{
				RpcLogic___Summon_900355577(buildingGUID, doorIndex, duration);
			}
		}

		private void RpcWriter___Server_ConsumeProduct_2622925554(ProductItemInstance product)
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
				writer.WriteProductItemInstance(product);
				SendServerRpc(1u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___ConsumeProduct_2622925554(ProductItemInstance product)
		{
			ConsumeProductBehaviour.SendProduct(product);
			ConsumeProductBehaviour.Enable_Networked(null);
		}

		private void RpcReader___Server_ConsumeProduct_2622925554(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			ProductItemInstance product = PooledReader0.ReadProductItemInstance();
			if (base.IsServerInitialized)
			{
				RpcLogic___ConsumeProduct_2622925554(product);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviour_Assembly_002DCSharp_002Edll()
		{
			Npc = GetComponentInParent<NPC>();
			Npc.Health.onKnockedOut.AddListener(OnKnockOut);
			Npc.Health.onDie.AddListener(OnDie);
		}
	}
}
