using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.EntityFramework;
using ScheduleOne.Interaction;
using ScheduleOne.Tiles;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts
{
	public class Sprinkler : GridItem
	{
		[Header("References")]
		public InteractableObject IntObj;

		public ParticleSystem[] WaterParticles;

		public AudioSourceController ClickSound;

		public AudioSourceController WaterSound;

		[Header("Settings")]
		public float ApplyWaterDelay = 6f;

		public float ParticleStopDelay = 2.5f;

		public UnityEvent onSprinklerStart;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted;

		public bool IsSprinkling { get; private set; }

		public void Hovered()
		{
			if (!isGhost)
			{
				if (CanWater())
				{
					IntObj.SetMessage("Activate sprinkler");
					IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				}
				else
				{
					IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
				}
			}
		}

		public void Interacted()
		{
			if (!isGhost && CanWater())
			{
				SendWater();
			}
		}

		private bool CanWater()
		{
			return !IsSprinkling;
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		private void SendWater()
		{
			RpcWriter___Server_SendWater_2166136261();
			RpcLogic___SendWater_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		private void Water()
		{
			RpcWriter___Observers_Water_2166136261();
			RpcLogic___Water_2166136261();
		}

		public void ApplyWater()
		{
			if (InstanceFinder.IsServer)
			{
				Console.Log("Applying water...");
				List<Pot> pots = GetPots();
				for (int i = 0; i < pots.Count; i++)
				{
					pots[i].ChangeWaterAmount(pots[i].WaterCapacity);
				}
			}
		}

		protected virtual List<Pot> GetPots()
		{
			List<Pot> list = new List<Pot>();
			Coordinate coord = new Coordinate(OriginCoordinate) + Coordinate.RotateCoordinates(new Coordinate(0, 1), Rotation);
			Coordinate coord2 = new Coordinate(OriginCoordinate) + Coordinate.RotateCoordinates(new Coordinate(1, 1), Rotation);
			Tile tile = base.OwnerGrid.GetTile(coord);
			Tile tile2 = base.OwnerGrid.GetTile(coord2);
			if (tile != null && tile2 != null)
			{
				Pot pot = null;
				if (tile.BuildableOccupants.Count > 0)
				{
					pot = tile.BuildableOccupants[0] as Pot;
				}
				if (pot != null && tile2.BuildableOccupants.Contains(pot))
				{
					list.Add(pot);
				}
			}
			return list;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(8u, RpcReader___Server_SendWater_2166136261);
				RegisterObserversRpc(9u, RpcReader___Observers_Water_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendWater_2166136261()
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
				SendServerRpc(8u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendWater_2166136261()
		{
			Water();
		}

		private void RpcReader___Server_SendWater_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendWater_2166136261();
			}
		}

		private void RpcWriter___Observers_Water_2166136261()
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
				SendObserversRpc(9u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___Water_2166136261()
		{
			if (!IsSprinkling)
			{
				IsSprinkling = true;
				ClickSound.Play();
				StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				if (onSprinklerStart != null)
				{
					onSprinklerStart.Invoke();
				}
				WaterSound.Play();
				for (int i = 0; i < WaterParticles.Length; i++)
				{
					WaterParticles[i].Play();
				}
				yield return new WaitForSeconds(ApplyWaterDelay);
				if (InstanceFinder.IsServer)
				{
					ApplyWater();
				}
				yield return new WaitForSeconds(ParticleStopDelay);
				for (int j = 0; j < WaterParticles.Length; j++)
				{
					WaterParticles[j].Stop();
				}
				WaterSound.Stop();
				IsSprinkling = false;
			}
		}

		private void RpcReader___Observers_Water_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Water_2166136261();
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
