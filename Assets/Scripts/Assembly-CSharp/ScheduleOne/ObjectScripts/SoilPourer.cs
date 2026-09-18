using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class SoilPourer : GridItem
	{
		public float AnimationDuration = 8f;

		[Header("References")]
		public InteractableObject HandleIntObj;

		public InteractableObject FillIntObj;

		public MeshRenderer DirtPlane;

		public Transform Dirt_Min;

		public Transform Dirt_Max;

		public ParticleSystem PourParticles;

		public Animation PourAnimation;

		public AudioSourceController FillSound;

		public AudioSourceController ActivateSound;

		public AudioSourceController DirtPourSound;

		private bool isDispensing;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted;

		public string SoilID { get; protected set; } = string.Empty;


		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (SoilID != string.Empty)
			{
				SoilDefinition item = Registry.GetItem<SoilDefinition>(SoilID);
				DirtPlane.material = item.DrySoilMat;
				SetSoilLevel(1f);
			}
		}

		public void HandleHovered()
		{
			if (!string.IsNullOrEmpty(SoilID) && !isDispensing)
			{
				HandleIntObj.SetMessage("Dispense soil");
				HandleIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				HandleIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}

		public void HandleInteracted()
		{
			if (!string.IsNullOrEmpty(SoilID) && !isDispensing)
			{
				SendPourSoil();
				isDispensing = true;
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		private void SendPourSoil()
		{
			RpcWriter___Server_SendPourSoil_2166136261();
			RpcLogic___SendPourSoil_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		private void PourSoil()
		{
			RpcWriter___Observers_PourSoil_2166136261();
			RpcLogic___PourSoil_2166136261();
		}

		private void ApplySoil(string ID)
		{
			Pot[] array = GetPots().ToArray();
			if (array != null && array.Length != 0 && array[0].SoilID == string.Empty)
			{
				array[0].SetSoilID(ID);
				array[0].SetSoilState(Pot.ESoilState.Flat);
				array[0].AddSoil(array[0].SoilCapacity);
				array[0].SetSoilUses(Registry.GetItem<SoilDefinition>(ID).Uses);
				if (InstanceFinder.IsServer)
				{
					array[0].PushSoilDataToServer();
				}
			}
		}

		public void FillHovered()
		{
			bool flag = false;
			if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance.Definition is SoilDefinition)
			{
				flag = true;
			}
			if (SoilID == string.Empty && flag)
			{
				FillIntObj.SetMessage("Insert soil");
				FillIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				FillIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}

		public void FillInteracted()
		{
			bool flag = false;
			if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance.Definition is SoilDefinition)
			{
				flag = true;
			}
			if (SoilID == string.Empty && flag)
			{
				FillSound.Play();
				SendSoil(PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance.Definition.ID);
				PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ChangeQuantity(-1);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendSoil(string ID)
		{
			RpcWriter___Server_SendSoil_3615296227(ID);
			RpcLogic___SendSoil_3615296227(ID);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		protected void SetSoil(NetworkConnection conn, string ID)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetSoil_2971853958(conn, ID);
				RpcLogic___SetSoil_2971853958(conn, ID);
			}
			else
			{
				RpcWriter___Target_SetSoil_2971853958(conn, ID);
			}
		}

		public void SetSoilLevel(float level)
		{
			DirtPlane.transform.localPosition = Vector3.Lerp(Dirt_Min.localPosition, Dirt_Max.localPosition, level);
			DirtPlane.gameObject.SetActive(level > 0f);
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

		public override string GetSaveString()
		{
			return new SoilPourerData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, OriginCoordinate, Rotation, SoilID).GetJson();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(8u, RpcReader___Server_SendPourSoil_2166136261);
				RegisterObserversRpc(9u, RpcReader___Observers_PourSoil_2166136261);
				RegisterServerRpc(10u, RpcReader___Server_SendSoil_3615296227);
				RegisterObserversRpc(11u, RpcReader___Observers_SetSoil_2971853958);
				RegisterTargetRpc(12u, RpcReader___Target_SetSoil_2971853958);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendPourSoil_2166136261()
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

		private void RpcLogic___SendPourSoil_2166136261()
		{
			PourSoil();
		}

		private void RpcReader___Server_SendPourSoil_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendPourSoil_2166136261();
			}
		}

		private void RpcWriter___Observers_PourSoil_2166136261()
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

		private void RpcLogic___PourSoil_2166136261()
		{
			if (!isDispensing)
			{
				isDispensing = true;
				StartCoroutine(PourRoutine());
			}
			IEnumerator PourRoutine()
			{
				SoilDefinition item = Registry.GetItem<SoilDefinition>(SoilID);
				if (item == null)
				{
					Console.LogError("Soil definition not found for ID: " + SoilID);
					isDispensing = false;
				}
				else
				{
					ActivateSound.Play();
					PourParticles.startColor = item.ParticleColor;
					PourParticles.Play();
					PourAnimation.Play();
					DirtPourSound.Play();
					Pot targetPot = GetPots().FirstOrDefault();
					if (targetPot != null)
					{
						targetPot.SetSoilID(SoilID);
						targetPot.SetSoilState(Pot.ESoilState.Flat);
						targetPot.SetSoilUses(item.Uses);
					}
					for (float i = 0f; i < AnimationDuration; i += Time.deltaTime)
					{
						float num = i / AnimationDuration;
						SetSoilLevel(1f - num);
						if (targetPot != null)
						{
							targetPot.AddSoil(targetPot.SoilCapacity * (Time.deltaTime / AnimationDuration));
						}
						yield return new WaitForEndOfFrame();
					}
					if (targetPot != null)
					{
						targetPot.AddSoil(targetPot.SoilCapacity - targetPot.SoilLevel);
					}
					ApplySoil(SoilID);
					SetSoil(null, string.Empty);
					PourParticles.Stop();
					isDispensing = false;
					yield return new WaitForSeconds(1f);
					DirtPourSound.Stop();
				}
			}
		}

		private void RpcReader___Observers_PourSoil_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___PourSoil_2166136261();
			}
		}

		private void RpcWriter___Server_SendSoil_3615296227(string ID)
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
				writer.WriteString(ID);
				SendServerRpc(10u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendSoil_3615296227(string ID)
		{
			SetSoil(null, ID);
		}

		private void RpcReader___Server_SendSoil_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string iD = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendSoil_3615296227(iD);
			}
		}

		private void RpcWriter___Observers_SetSoil_2971853958(NetworkConnection conn, string ID)
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
				writer.WriteString(ID);
				SendObserversRpc(11u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		protected void RpcLogic___SetSoil_2971853958(NetworkConnection conn, string ID)
		{
			SoilID = ID;
			if (ID != string.Empty)
			{
				SoilDefinition item = Registry.GetItem<SoilDefinition>(SoilID);
				DirtPlane.material = item.DrySoilMat;
				SetSoilLevel(1f);
			}
		}

		private void RpcReader___Observers_SetSoil_2971853958(PooledReader PooledReader0, Channel channel)
		{
			string iD = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetSoil_2971853958(null, iD);
			}
		}

		private void RpcWriter___Target_SetSoil_2971853958(NetworkConnection conn, string ID)
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
				writer.WriteString(ID);
				SendTargetRpc(12u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetSoil_2971853958(PooledReader PooledReader0, Channel channel)
		{
			string iD = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetSoil_2971853958(base.LocalConnection, iD);
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
