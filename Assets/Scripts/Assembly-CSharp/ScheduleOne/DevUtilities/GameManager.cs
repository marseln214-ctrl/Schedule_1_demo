using System.Collections.Generic;
using System.IO;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.SceneManagement;

namespace ScheduleOne.DevUtilities
{
	public class GameManager : NetworkSingleton<GameManager>, IBaseSaveable, ISaveable
	{
		public const bool IS_DEMO = true;

		public static bool IS_BETA;

		[SerializeField]
		private int seed;

		public string OrganisationName = "Organisation";

		public Transform SpawnPoint;

		public Transform NoHomeRespawnPoint;

		public Transform Temp;

		private GameDataLoader loader = new GameDataLoader();

		private bool NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted;

		public static bool IS_TUTORIAL => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Tutorial";

		public static int Seed
		{
			get
			{
				if (NetworkSingleton<GameManager>.Instance != null)
				{
					return NetworkSingleton<GameManager>.Instance.seed;
				}
				return 0;
			}
		}

		public Sprite OrganisationLogo { get; protected set; }

		public bool IsTutorial => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Tutorial";

		public string SaveFolderName => "Game";

		public string SaveFileName => "Game";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => false;

		public List<string> LocalExtraFiles { get; set; } = new List<string> { "Logo.png" };


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EDevUtilities_002EGameManager_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Start()
		{
			base.Start();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (!connection.IsHost)
			{
				SetGameData(connection, new GameData(OrganisationName, seed));
			}
		}

		[TargetRpc]
		public void SetGameData(NetworkConnection conn, GameData data)
		{
			RpcWriter___Target_SetGameData_3076874643(conn, data);
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public virtual string GetSaveString()
		{
			return new GameData(OrganisationName, seed).GetJson();
		}

		public void Load(GameData data, string path)
		{
			OrganisationName = data.OrganisationName;
			seed = data.Seed;
			string text = Path.Combine(path, "Logo.png");
			if (File.Exists(path))
			{
				byte[] data2 = File.ReadAllBytes(text);
				Texture2D texture2D = new Texture2D(2, 2);
				texture2D.LoadImage(data2);
				OrganisationLogo = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
			}
			else
			{
				Console.LogWarning("Logo file not found at " + text);
			}
			HasChanged = true;
		}

		public void EndTutorial()
		{
			if (IsTutorial)
			{
				if (Singleton<LoadManager>.Instance.StoredSaveInfo == null)
				{
					Console.LogError("No stored save info found");
					return;
				}
				Singleton<SaveManager>.Instance.DisablePlayTutorial(Singleton<LoadManager>.Instance.StoredSaveInfo);
				Singleton<LoadManager>.Instance.StoredSaveInfo.MetaData.PlayTutorial = false;
				Singleton<LoadManager>.Instance.ExitToMenu(Singleton<LoadManager>.Instance.StoredSaveInfo);
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterTargetRpc(0u, RpcReader___Target_SetGameData_3076874643);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Target_SetGameData_3076874643(NetworkConnection conn, GameData data)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerated(writer, data);
				SendTargetRpc(0u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetGameData_3076874643(NetworkConnection conn, GameData data)
		{
			OrganisationName = data.OrganisationName;
			seed = data.Seed;
		}

		private void RpcReader___Target_SetGameData_3076874643(PooledReader PooledReader0, Channel channel)
		{
			GameData data = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___SetGameData_3076874643(base.LocalConnection, data);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EDevUtilities_002EGameManager_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			CrashReportHandler.logBufferSize = 50u;
			InitializeSaveable();
		}
	}
}
