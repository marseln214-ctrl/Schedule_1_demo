using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.Law;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.WorldspacePopup;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class CallPoliceBehaviour : Behaviour
	{
		public const float CALL_POLICE_TIME = 4f;

		[Header("References")]
		public WorldspacePopup PhoneCallPopup;

		public AvatarEquippable PhonePrefab;

		public AudioSourceController CallSound;

		private float currentCallTime;

		public Player Target;

		public Crime ReportedCrime;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted;

		protected override void Begin()
		{
			base.Begin();
			if (!IsTargetValid())
			{
				End();
				Disable();
				return;
			}
			if (ReportedCrime == null)
			{
				Console.LogError("CallPoliceBehaviour doesn't have a crime set, disabling.");
				Disable();
				End();
				return;
			}
			Console.Log("CallPoliceBehaviour started on player " + Target.PlayerName);
			currentCallTime = 0f;
			RefreshIcon();
			if (Target.Owner.IsLocalClient)
			{
				PhoneCallPopup.enabled = true;
			}
			CallSound.Play();
			if (InstanceFinder.IsServer)
			{
				base.Npc.SetEquippable_Networked(null, PhonePrefab.AssetPath);
			}
		}

		public void SetData(NetworkObject player, Crime crime)
		{
		}

		protected override void Resume()
		{
			base.Resume();
			if (!IsTargetValid())
			{
				End();
				Disable();
				return;
			}
			currentCallTime = 0f;
			RefreshIcon();
			if (Target.Owner.IsLocalClient)
			{
				PhoneCallPopup.enabled = true;
			}
			CallSound.Play();
			if (InstanceFinder.IsServer)
			{
				base.Npc.SetEquippable_Networked(null, PhonePrefab.AssetPath);
			}
		}

		protected override void End()
		{
			base.End();
			currentCallTime = 0f;
			PhoneCallPopup.enabled = false;
			CallSound.Stop();
			if (InstanceFinder.IsServer)
			{
				base.Npc.SetEquippable_Networked(null, string.Empty);
			}
		}

		protected override void Pause()
		{
			base.Pause();
			currentCallTime = 0f;
			PhoneCallPopup.enabled = false;
			CallSound.Stop();
			if (InstanceFinder.IsServer)
			{
				base.Npc.SetEquippable_Networked(null, string.Empty);
			}
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			currentCallTime += Time.deltaTime;
			RefreshIcon();
			base.Npc.Avatar.LookController.OverrideLookTarget(Target.EyePosition.position, 1, rotateBody: true);
			if (currentCallTime >= 4f && InstanceFinder.IsServer)
			{
				FinalizeCall();
			}
		}

		private void RefreshIcon()
		{
			PhoneCallPopup.CurrentFillLevel = currentCallTime / 4f;
		}

		[ObserversRpc(RunLocally = true)]
		private void FinalizeCall()
		{
			RpcWriter___Observers_FinalizeCall_2166136261();
			RpcLogic___FinalizeCall_2166136261();
		}

		private bool IsTargetValid()
		{
			if (Target == null)
			{
				return false;
			}
			if (!Target.Health.IsAlive)
			{
				return false;
			}
			if (Target.IsArrested)
			{
				return false;
			}
			return true;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_FinalizeCall_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_FinalizeCall_2166136261()
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
				SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___FinalizeCall_2166136261()
		{
			if (!base.Active)
			{
				return;
			}
			if (!IsTargetValid())
			{
				End();
				Disable();
				return;
			}
			Debug.Log("Call finalized on player " + Target.PlayerName);
			Target.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: true);
			Target.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Investigating);
			Target.CrimeData.AddCrime(ReportedCrime);
			if (InstanceFinder.IsServer)
			{
				Singleton<LawManager>.Instance.PoliceCalled(Target, ReportedCrime);
			}
			End();
			Disable();
		}

		private void RpcReader___Observers_FinalizeCall_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___FinalizeCall_2166136261();
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
