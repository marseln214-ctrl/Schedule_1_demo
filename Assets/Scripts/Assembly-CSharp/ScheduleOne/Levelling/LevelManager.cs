using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Levelling
{
	public class LevelManager : NetworkSingleton<LevelManager>, IBaseSaveable, ISaveable
	{
		public const int TIERS_PER_RANK = 5;

		public const int XP_PER_TIER_MIN = 125;

		public const int XP_PER_TIER_MAX = 1000;

		private int rankCount;

		public Action<FullRank, FullRank> onRankUp;

		public Dictionary<FullRank, List<Unlockable>> Unlockables = new Dictionary<FullRank, List<Unlockable>>();

		private RankLoader loader = new RankLoader();

		private bool NetworkInitialize___EarlyScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted;

		public ERank Rank { get; private set; }

		public int Tier { get; private set; } = 1;


		public int XP { get; private set; }

		public int TotalXP { get; private set; }

		public float XPToNextTier => Mathf.Lerp(125f, 1000f, (float)Rank / (float)rankCount);

		public string SaveFolderName => "Rank";

		public string SaveFileName => "Rank";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => false;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ELevelling_002ELevelManager_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Start()
		{
			base.Start();
			InitializeSaveable();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			SetData(connection, Rank, Tier, XP, TotalXP);
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		[ServerRpc(RequireOwnership = false)]
		public void AddXP(int xp)
		{
			RpcWriter___Server_AddXP_3316948804(xp);
		}

		[ObserversRpc]
		private void AddXPLocal(int xp)
		{
			RpcWriter___Observers_AddXPLocal_3316948804(xp);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void SetData(NetworkConnection conn, ERank rank, int tier, int xp, int totalXp)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetData_20965027(conn, rank, tier, xp, totalXp);
				RpcLogic___SetData_20965027(conn, rank, tier, xp, totalXp);
			}
			else
			{
				RpcWriter___Target_SetData_20965027(conn, rank, tier, xp, totalXp);
			}
		}

		[ObserversRpc]
		private void IncreaseTierNetworked(FullRank before, FullRank after)
		{
			RpcWriter___Observers_IncreaseTierNetworked_3953286437(before, after);
		}

		private void IncreaseTier()
		{
			XP -= (int)XPToNextTier;
			Tier++;
			if (Tier > 5 && Rank != ERank.Kingpin)
			{
				Tier = 1;
				Rank++;
			}
		}

		public virtual string GetSaveString()
		{
			return new RankData((int)Rank, Tier, XP, TotalXP).GetJson();
		}

		public FullRank GetFullRank()
		{
			return new FullRank(Rank, Tier);
		}

		public void AddUnlockable(Unlockable unlockable)
		{
			if (!Unlockables.ContainsKey(unlockable.Rank))
			{
				Unlockables.Add(unlockable.Rank, new List<Unlockable>());
			}
			if (Unlockables[unlockable.Rank].Find((Unlockable x) => x.Title.ToLower() == unlockable.Title.ToLower() && x.Icon == unlockable.Icon) == null)
			{
				Unlockables[unlockable.Rank].Add(unlockable);
			}
		}

		public int GetTotalXPForRank(FullRank fullrank)
		{
			int num = 0;
			ERank[] array = (ERank[])Enum.GetValues(typeof(ERank));
			foreach (ERank eRank in array)
			{
				int xPForTier = GetXPForTier(eRank);
				for (int j = 1; j <= 5; j++)
				{
					if (eRank == fullrank.Rank && j == fullrank.Tier)
					{
						return num;
					}
					num += xPForTier;
				}
			}
			Console.LogError("Rank not found: " + fullrank.ToString());
			return 0;
		}

		public FullRank GetFullRank(int totalXp)
		{
			int num = totalXp;
			ERank[] array = (ERank[])Enum.GetValues(typeof(ERank));
			foreach (ERank rank in array)
			{
				int xPForTier = GetXPForTier(rank);
				for (int j = 1; j <= 5; j++)
				{
					if (num < xPForTier)
					{
						return new FullRank(rank, j);
					}
					num -= xPForTier;
				}
			}
			Console.LogError("Rank not found for XP: " + totalXp);
			return new FullRank(ERank.Street_Rat, 1);
		}

		public int GetXPForTier(ERank rank)
		{
			return Mathf.RoundToInt(Mathf.Lerp(125f, 1000f, (float)rank / (float)rankCount));
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(0u, RpcReader___Server_AddXP_3316948804);
				RegisterObserversRpc(1u, RpcReader___Observers_AddXPLocal_3316948804);
				RegisterObserversRpc(2u, RpcReader___Observers_SetData_20965027);
				RegisterTargetRpc(3u, RpcReader___Target_SetData_20965027);
				RegisterObserversRpc(4u, RpcReader___Observers_IncreaseTierNetworked_3953286437);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_AddXP_3316948804(int xp)
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
				writer.WriteInt32(xp);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___AddXP_3316948804(int xp)
		{
			AddXPLocal(xp);
		}

		private void RpcReader___Server_AddXP_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int xp = PooledReader0.ReadInt32();
			if (base.IsServerInitialized)
			{
				RpcLogic___AddXP_3316948804(xp);
			}
		}

		private void RpcWriter___Observers_AddXPLocal_3316948804(int xp)
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
				writer.WriteInt32(xp);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___AddXPLocal_3316948804(int xp)
		{
			NetworkSingleton<DailySummary>.Instance.AddXP(xp);
			XP += xp;
			TotalXP += xp;
			HasChanged = true;
			Console.Log("Rank progress: " + XP + "/" + XPToNextTier + " (Total " + TotalXP + ")");
			if (InstanceFinder.IsServer)
			{
				FullRank fullRank = GetFullRank();
				bool flag = false;
				while ((float)XP >= XPToNextTier)
				{
					IncreaseTier();
					flag = true;
				}
				SetData(null, Rank, Tier, XP, TotalXP);
				if (flag)
				{
					IncreaseTierNetworked(fullRank, GetFullRank());
				}
			}
		}

		private void RpcReader___Observers_AddXPLocal_3316948804(PooledReader PooledReader0, Channel channel)
		{
			int xp = PooledReader0.ReadInt32();
			if (base.IsClientInitialized)
			{
				RpcLogic___AddXPLocal_3316948804(xp);
			}
		}

		private void RpcWriter___Observers_SetData_20965027(NetworkConnection conn, ERank rank, int tier, int xp, int totalXp)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerated(writer, rank);
				writer.WriteInt32(tier);
				writer.WriteInt32(xp);
				writer.WriteInt32(totalXp);
				SendObserversRpc(2u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetData_20965027(NetworkConnection conn, ERank rank, int tier, int xp, int totalXp)
		{
			Rank = rank;
			Tier = tier;
			XP = xp;
			TotalXP = totalXp;
		}

		private void RpcReader___Observers_SetData_20965027(PooledReader PooledReader0, Channel channel)
		{
			ERank rank = GeneratedReaders___Internal.Read___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerateds(PooledReader0);
			int tier = PooledReader0.ReadInt32();
			int xp = PooledReader0.ReadInt32();
			int totalXp = PooledReader0.ReadInt32();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetData_20965027(null, rank, tier, xp, totalXp);
			}
		}

		private void RpcWriter___Target_SetData_20965027(NetworkConnection conn, ERank rank, int tier, int xp, int totalXp)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerated(writer, rank);
				writer.WriteInt32(tier);
				writer.WriteInt32(xp);
				writer.WriteInt32(totalXp);
				SendTargetRpc(3u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetData_20965027(PooledReader PooledReader0, Channel channel)
		{
			ERank rank = GeneratedReaders___Internal.Read___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerateds(PooledReader0);
			int tier = PooledReader0.ReadInt32();
			int xp = PooledReader0.ReadInt32();
			int totalXp = PooledReader0.ReadInt32();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetData_20965027(base.LocalConnection, rank, tier, xp, totalXp);
			}
		}

		private void RpcWriter___Observers_IncreaseTierNetworked_3953286437(FullRank before, FullRank after)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerated(writer, before);
				GeneratedWriters___Internal.Write___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerated(writer, after);
				SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___IncreaseTierNetworked_3953286437(FullRank before, FullRank after)
		{
			onRankUp?.Invoke(before, after);
			HasChanged = true;
			Console.Log("Ranked up to " + Rank.ToString() + ": " + Tier);
		}

		private void RpcReader___Observers_IncreaseTierNetworked_3953286437(PooledReader PooledReader0, Channel channel)
		{
			FullRank before = GeneratedReaders___Internal.Read___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerateds(PooledReader0);
			FullRank after = GeneratedReaders___Internal.Read___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___IncreaseTierNetworked_3953286437(before, after);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ELevelling_002ELevelManager_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			rankCount = Enum.GetValues(typeof(ERank)).Length;
		}
	}
}
