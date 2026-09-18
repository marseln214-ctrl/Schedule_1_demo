using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;

namespace ScheduleOne.Money
{
	public class MoneyManager : NetworkSingleton<MoneyManager>, IBaseSaveable, ISaveable
	{
		public class FloatContainer
		{
			public float value { get; private set; }

			public void ChangeValue(float value)
			{
				this.value += value;
			}
		}

		public const string MONEY_TEXT_COLOR = "#54E717";

		public const string MONEY_TEXT_COLOR_DARKER = "#46CB4F";

		public List<Transaction> ledger = new List<Transaction>();

		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public float onlineBalance;

		public AudioSourceController CashSound;

		[Header("Prefabs")]
		[SerializeField]
		protected GameObject moneyChangePrefab;

		[SerializeField]
		protected GameObject cashChangePrefab;

		public Sprite LaunderingNotificationIcon;

		public Action<FloatContainer> onNetworthCalculation;

		private MoneyLoader loader = new MoneyLoader();

		public SyncVar<float> syncVar___onlineBalance;

		private bool NetworkInitialize___EarlyScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted;

		public float LastCalculatedNetworth { get; protected set; }

		public float cashBalance => cashInstance.Balance;

		protected CashInstance cashInstance => PlayerSingleton<PlayerInventory>.Instance.cashInstance;

		public string SaveFolderName => "Money";

		public string SaveFileName => "Money";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => false;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		public float SyncAccessor_onlineBalance
		{
			get
			{
				return onlineBalance;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					onlineBalance = value;
				}
				if (Application.isPlaying)
				{
					syncVar___onlineBalance.SetValue(value, value != 0);
				}
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EMoney_002EMoneyManager_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		protected override void Start()
		{
			base.Start();
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(Loaded);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (Singleton<LoadManager>.InstanceExists)
			{
				Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(Loaded);
			}
		}

		private void Loaded()
		{
			GetNetWorth();
		}

		private void Update()
		{
			HasChanged = true;
			Singleton<HUD>.Instance.OnlineBalanceDisplay.SetBalance(SyncAccessor_onlineBalance);
			if (NetworkSingleton<VariableDatabase>.InstanceExists)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Online_Balance", onlineBalance.ToString(), network: false);
				if (PlayerSingleton<PlayerInventory>.InstanceExists)
				{
					NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Cash_Balance", cashBalance.ToString(), network: false);
					NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Total_Money", (SyncAccessor_onlineBalance + cashBalance).ToString(), network: false);
				}
			}
		}

		public CashInstance GetCashInstance(float amount)
		{
			CashInstance obj = Registry.GetItem<CashDefinition>("cash").GetDefaultInstance() as CashInstance;
			obj.SetBalance(amount);
			return obj;
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void CreateOnlineTransaction(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
		{
			RpcWriter___Server_CreateOnlineTransaction_1419830531(_transaction_Name, _unit_Amount, _quantity, _transaction_Note);
			RpcLogic___CreateOnlineTransaction_1419830531(_transaction_Name, _unit_Amount, _quantity, _transaction_Note);
		}

		[ObserversRpc]
		private void ReceiveOnlineTransaction(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
		{
			RpcWriter___Observers_ReceiveOnlineTransaction_1419830531(_transaction_Name, _unit_Amount, _quantity, _transaction_Note);
		}

		protected IEnumerator ShowOnlineBalanceChange(RectTransform changeDisplay)
		{
			TextMeshProUGUI text = changeDisplay.GetComponent<TextMeshProUGUI>();
			float startVert = changeDisplay.anchoredPosition.y;
			float lerpTime = 2.5f;
			float vertOffset = startVert + 60f;
			for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
			{
				text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(1f, 0f, i / lerpTime));
				changeDisplay.anchoredPosition = new Vector2(changeDisplay.anchoredPosition.x, Mathf.Lerp(startVert, vertOffset, i / lerpTime));
				yield return new WaitForEndOfFrame();
			}
			UnityEngine.Object.Destroy(changeDisplay.gameObject);
		}

		public void ChangeCashBalance(float change, bool visualizeChange = true, bool playCashSound = false)
		{
			float num = Mathf.Clamp(cashInstance.Balance + change, 0f, float.MaxValue) - cashInstance.Balance;
			cashInstance.ChangeBalance(change);
			if (playCashSound && num != 0f)
			{
				CashSound.Play();
			}
			if (visualizeChange && num != 0f)
			{
				RectTransform component = UnityEngine.Object.Instantiate(cashChangePrefab, Singleton<HUD>.Instance.cashSlotContainer).GetComponent<RectTransform>();
				component.position = new Vector3(Singleton<HUD>.Instance.cashSlotUI.position.x, component.position.y);
				component.anchoredPosition = new Vector2(component.anchoredPosition.x, 10f);
				TextMeshProUGUI component2 = component.GetComponent<TextMeshProUGUI>();
				if (num > 0f)
				{
					component2.text = "+ " + FormatAmount(num);
					component2.color = new Color32(25, 240, 30, byte.MaxValue);
				}
				else
				{
					component2.text = FormatAmount(num);
					component2.color = new Color32(176, 63, 59, byte.MaxValue);
				}
				Singleton<CoroutineService>.Instance.StartCoroutine(ShowCashChange(component));
			}
		}

		protected IEnumerator ShowCashChange(RectTransform changeDisplay)
		{
			TextMeshProUGUI text = changeDisplay.GetComponent<TextMeshProUGUI>();
			float startVert = changeDisplay.anchoredPosition.y;
			float lerpTime = 2.5f;
			float vertOffset = startVert + 60f;
			for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
			{
				text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(1f, 0f, i / lerpTime));
				changeDisplay.anchoredPosition = new Vector2(changeDisplay.anchoredPosition.x, Mathf.Lerp(startVert, vertOffset, i / lerpTime));
				yield return new WaitForEndOfFrame();
			}
			UnityEngine.Object.Destroy(changeDisplay.gameObject);
		}

		public static string FormatAmount(float amount, bool showDecimals = false)
		{
			string text = string.Empty;
			if (amount < 0f)
			{
				text = "-";
			}
			if (showDecimals)
			{
				return text + string.Format(new CultureInfo("en-US"), "{0:C}", Mathf.Abs(amount));
			}
			return text + string.Format(new CultureInfo("en-US"), "{0:C0}", Mathf.RoundToInt(Mathf.Abs(amount)));
		}

		public virtual string GetSaveString()
		{
			return new MoneyData(SyncAccessor_onlineBalance, GetNetWorth()).GetJson();
		}

		public void Load(MoneyData data)
		{
			syncVar___onlineBalance.SetValue(data.OnlineBalance, true);
		}

		public float GetNetWorth()
		{
			float num = 0f;
			num += SyncAccessor_onlineBalance;
			if (onNetworthCalculation != null)
			{
				FloatContainer floatContainer = new FloatContainer();
				onNetworthCalculation(floatContainer);
				num += floatContainer.value;
			}
			LastCalculatedNetworth = num;
			return num;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar___onlineBalance = new SyncVar<float>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, onlineBalance);
				RegisterServerRpc(0u, RpcReader___Server_CreateOnlineTransaction_1419830531);
				RegisterObserversRpc(1u, RpcReader___Observers_ReceiveOnlineTransaction_1419830531);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EMoney_002EMoneyManager);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar___onlineBalance.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_CreateOnlineTransaction_1419830531(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
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
				writer.WriteString(_transaction_Name);
				writer.WriteSingle(_unit_Amount);
				writer.WriteSingle(_quantity);
				writer.WriteString(_transaction_Note);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___CreateOnlineTransaction_1419830531(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
		{
			ReceiveOnlineTransaction(_transaction_Name, _unit_Amount, _quantity, _transaction_Note);
		}

		private void RpcReader___Server_CreateOnlineTransaction_1419830531(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string transaction_Name = PooledReader0.ReadString();
			float unit_Amount = PooledReader0.ReadSingle();
			float quantity = PooledReader0.ReadSingle();
			string transaction_Note = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___CreateOnlineTransaction_1419830531(transaction_Name, unit_Amount, quantity, transaction_Note);
			}
		}

		private void RpcWriter___Observers_ReceiveOnlineTransaction_1419830531(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
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
				writer.WriteString(_transaction_Name);
				writer.WriteSingle(_unit_Amount);
				writer.WriteSingle(_quantity);
				writer.WriteString(_transaction_Note);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveOnlineTransaction_1419830531(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
		{
			Transaction transaction = new Transaction(_transaction_Name, _unit_Amount, _quantity, _transaction_Note);
			ledger.Add(transaction);
            syncVar___onlineBalance.SetValue(SyncAccessor_onlineBalance + transaction.total_Amount, true);
			Singleton<HUD>.Instance.OnlineBalanceDisplay.Show();
			RectTransform component = UnityEngine.Object.Instantiate(moneyChangePrefab, Singleton<HUD>.Instance.cashSlotContainer).GetComponent<RectTransform>();
			component.position = new Vector3(Singleton<HUD>.Instance.onlineBalanceSlotUI.position.x, component.position.y);
			component.anchoredPosition = new Vector2(component.anchoredPosition.x, 10f);
			TextMeshProUGUI component2 = component.GetComponent<TextMeshProUGUI>();
			if (transaction.total_Amount > 0f)
			{
				component2.text = "+ " + FormatAmount(transaction.total_Amount);
				component2.color = new Color32(25, 190, 240, byte.MaxValue);
			}
			else
			{
				component2.text = FormatAmount(transaction.total_Amount);
				component2.color = new Color32(176, 63, 59, byte.MaxValue);
			}
			Singleton<CoroutineService>.Instance.StartCoroutine(ShowOnlineBalanceChange(component));
			HasChanged = true;
		}

		private void RpcReader___Observers_ReceiveOnlineTransaction_1419830531(PooledReader PooledReader0, Channel channel)
		{
			string transaction_Name = PooledReader0.ReadString();
			float unit_Amount = PooledReader0.ReadSingle();
			float quantity = PooledReader0.ReadSingle();
			string transaction_Note = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveOnlineTransaction_1419830531(transaction_Name, unit_Amount, quantity, transaction_Note);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EMoney_002EMoneyManager(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 0)
			{
				if (PooledReader0 == null)
				{
                    syncVar___onlineBalance.SetValue(syncVar___onlineBalance.GetValue(calledByUser: true), true);
					return true;
				}
				float value = PooledReader0.ReadSingle();
                syncVar___onlineBalance.SetValue(value, Boolean2);
				return true;
			}
			return false;
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EMoney_002EMoneyManager_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			InitializeSaveable();
		}
	}
}
