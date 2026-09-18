using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class DailySummary : NetworkSingleton<DailySummary>
	{
		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public Animation Anim;

		public TextMeshProUGUI TitleLabel;

		public RectTransform[] ProductEntries;

		public TextMeshProUGUI PlayerEarningsLabel;

		public TextMeshProUGUI DealerEarningsLabel;

		public TextMeshProUGUI XPGainedLabel;

		public UnityEvent onClosed;

		private Dictionary<string, int> itemsSoldByPlayer = new Dictionary<string, int>();

		private float moneyEarnedByPlayer;

		private float moneyEarnedByDealers;

		private bool NetworkInitialize___EarlyScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted;

		public bool IsOpen { get; private set; }

		public int xpGained { get; private set; }

		protected override void Start()
		{
			base.Start();
			IsOpen = false;
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance._onSleepEnd.AddListener(SleepEnd);
		}

		public void Open()
		{
			IsOpen = true;
			TitleLabel.text = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentDay.ToString() + ", Day " + (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.ElapsedDays + 1);
			string[] items = itemsSoldByPlayer.Keys.ToArray();
			for (int i = 0; i < ProductEntries.Length; i++)
			{
				if (i < items.Length)
				{
					ItemDefinition item = Registry.GetItem(items[i]);
					ProductEntries[i].Find("Quantity").GetComponent<TextMeshProUGUI>().text = itemsSoldByPlayer[items[i]] + "x";
					ProductEntries[i].Find("Image").GetComponent<Image>().sprite = item.Icon;
					ProductEntries[i].Find("Name").GetComponent<TextMeshProUGUI>().text = item.Name;
					ProductEntries[i].gameObject.SetActive(value: true);
				}
				else
				{
					ProductEntries[i].gameObject.SetActive(value: false);
				}
			}
			PlayerEarningsLabel.text = MoneyManager.FormatAmount(moneyEarnedByPlayer);
			DealerEarningsLabel.text = MoneyManager.FormatAmount(moneyEarnedByDealers);
			XPGainedLabel.text = xpGained + " XP";
			Anim.Play("Daily summary 1");
			Canvas.enabled = true;
			Container.gameObject.SetActive(value: true);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			StartCoroutine(Wait());
			IEnumerator Wait()
			{
				yield return new WaitForSeconds(0.1f * (float)items.Length + 0.5f);
				if (IsOpen)
				{
					Anim.Play("Daily summary 2");
				}
			}
		}

		public void Close()
		{
			if (IsOpen)
			{
				IsOpen = false;
				Anim.Stop();
				Anim.Play("Daily summary close");
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			}
		}

		private void SleepEnd()
		{
			ClearStats();
		}

		[ObserversRpc]
		public void AddSoldItem(string id, int amount)
		{
			RpcWriter___Observers_AddSoldItem_3643459082(id, amount);
		}

		[ObserversRpc]
		public void AddPlayerMoney(float amount)
		{
			RpcWriter___Observers_AddPlayerMoney_431000436(amount);
		}

		[ObserversRpc]
		public void AddDealerMoney(float amount)
		{
			RpcWriter___Observers_AddDealerMoney_431000436(amount);
		}

		[ObserversRpc]
		public void AddXP(int xp)
		{
			RpcWriter___Observers_AddXP_3316948804(xp);
		}

		private void ClearStats()
		{
			itemsSoldByPlayer.Clear();
			moneyEarnedByPlayer = 0f;
			moneyEarnedByDealers = 0f;
			xpGained = 0;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(0u, RpcReader___Observers_AddSoldItem_3643459082);
				RegisterObserversRpc(1u, RpcReader___Observers_AddPlayerMoney_431000436);
				RegisterObserversRpc(2u, RpcReader___Observers_AddDealerMoney_431000436);
				RegisterObserversRpc(3u, RpcReader___Observers_AddXP_3316948804);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_AddSoldItem_3643459082(string id, int amount)
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
				writer.WriteString(id);
				writer.WriteInt32(amount);
				SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___AddSoldItem_3643459082(string id, int amount)
		{
			if (itemsSoldByPlayer.ContainsKey(id))
			{
				itemsSoldByPlayer[id] += amount;
			}
			else
			{
				itemsSoldByPlayer.Add(id, amount);
			}
		}

		private void RpcReader___Observers_AddSoldItem_3643459082(PooledReader PooledReader0, Channel channel)
		{
			string id = PooledReader0.ReadString();
			int amount = PooledReader0.ReadInt32();
			if (base.IsClientInitialized)
			{
				RpcLogic___AddSoldItem_3643459082(id, amount);
			}
		}

		private void RpcWriter___Observers_AddPlayerMoney_431000436(float amount)
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
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___AddPlayerMoney_431000436(float amount)
		{
			moneyEarnedByPlayer += amount;
		}

		private void RpcReader___Observers_AddPlayerMoney_431000436(PooledReader PooledReader0, Channel channel)
		{
			float amount = PooledReader0.ReadSingle();
			if (base.IsClientInitialized)
			{
				RpcLogic___AddPlayerMoney_431000436(amount);
			}
		}

		private void RpcWriter___Observers_AddDealerMoney_431000436(float amount)
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
				SendObserversRpc(2u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___AddDealerMoney_431000436(float amount)
		{
			moneyEarnedByDealers += amount;
		}

		private void RpcReader___Observers_AddDealerMoney_431000436(PooledReader PooledReader0, Channel channel)
		{
			float amount = PooledReader0.ReadSingle();
			if (base.IsClientInitialized)
			{
				RpcLogic___AddDealerMoney_431000436(amount);
			}
		}

		private void RpcWriter___Observers_AddXP_3316948804(int xp)
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
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___AddXP_3316948804(int xp)
		{
			xpGained += xp;
		}

		private void RpcReader___Observers_AddXP_3316948804(PooledReader PooledReader0, Channel channel)
		{
			int xp = PooledReader0.ReadInt32();
			if (base.IsClientInitialized)
			{
				RpcLogic___AddXP_3316948804(xp);
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
