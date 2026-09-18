using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Law
{
	public class CurfewManager : NetworkSingleton<CurfewManager>
	{
		public const int WARNING_TIME = 1930;

		public const int CURFEW_START_TIME = 2000;

		public const int CURFEW_END_TIME = 500;

		[Header("References")]
		public VMSBoard[] VMSBoards;

		public AudioSourceController CurfewWarningSound;

		public AudioSourceController CurfewAlarmSound;

		public UnityEvent onCurfewEnabled;

		public UnityEvent onCurfewDisabled;

		public UnityEvent onCurfewHint;

		public UnityEvent onCurfewWarning;

		private bool warningPlayed;

		private bool NetworkInitialize___EarlyScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted;

		public bool IsEnabled { get; protected set; }

		public bool IsCurrentlyActive { get; protected set; }

		public bool IsCurrentlyActiveWithTolerance
		{
			get
			{
				if (IsCurrentlyActive)
				{
					return NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(2015, 500);
				}
				return false;
			}
		}

		protected override void Start()
		{
			base.Start();
			TimeManager timeManager = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			timeManager.onMinutePass = (Action)Delegate.Combine(timeManager.onMinutePass, new Action(MinPass));
			Disable();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (IsEnabled)
			{
				Enable(connection);
			}
		}

		[ObserversRpc]
		[TargetRpc]
		public void Enable(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_Enable_328543758(conn);
			}
			else
			{
				RpcWriter___Target_Enable_328543758(conn);
			}
		}

		[ObserversRpc]
		public void Disable()
		{
			RpcWriter___Observers_Disable_2166136261();
		}

		private void MinPass()
		{
			if (IsEnabled)
			{
				string text = "CURFEW TONIGHT\n8PM - 5AM";
				if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentTime == 1930 && !warningPlayed)
				{
					warningPlayed = true;
					if (onCurfewWarning != null)
					{
						onCurfewWarning.Invoke();
					}
					if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.ElapsedDays == 0 && onCurfewHint != null)
					{
						onCurfewHint.Invoke();
					}
					CurfewWarningSound.Play();
				}
				if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(2000, 500))
				{
					if (!IsCurrentlyActive)
					{
						if (!NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.SleepInProgress && Singleton<LoadManager>.Instance.TimeSinceGameLoaded > 3f)
						{
							CurfewAlarmSound.Play();
						}
						IsCurrentlyActive = true;
					}
					text = "CURFEW ACTIVE\n UNTIL 5AM";
					VMSBoard[] vMSBoards = VMSBoards;
					for (int i = 0; i < vMSBoards.Length; i++)
					{
						vMSBoards[i].SetText(text, new Color32(byte.MaxValue, 85, 60, byte.MaxValue));
					}
				}
				else if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(2000, -60), 2000))
				{
					warningPlayed = false;
					IsCurrentlyActive = false;
					text = "CURFEW SOON\n" + (ScheduleOne.GameTime.TimeManager.GetMinSumFrom24HourTime(2000) - ScheduleOne.GameTime.TimeManager.GetMinSumFrom24HourTime(NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentTime)) + " MINS";
					VMSBoard[] vMSBoards = VMSBoards;
					for (int i = 0; i < vMSBoards.Length; i++)
					{
						vMSBoards[i].SetText(text);
					}
				}
				else
				{
					warningPlayed = false;
					IsCurrentlyActive = false;
					VMSBoard[] vMSBoards = VMSBoards;
					for (int i = 0; i < vMSBoards.Length; i++)
					{
						vMSBoards[i].SetText(text);
					}
				}
			}
			else
			{
				IsCurrentlyActive = false;
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(0u, RpcReader___Observers_Enable_328543758);
				RegisterTargetRpc(1u, RpcReader___Target_Enable_328543758);
				RegisterObserversRpc(2u, RpcReader___Observers_Disable_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_Enable_328543758(NetworkConnection conn)
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
				SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___Enable_328543758(NetworkConnection conn)
		{
			IsEnabled = true;
			if (onCurfewEnabled != null)
			{
				onCurfewEnabled.Invoke();
			}
			VMSBoard[] vMSBoards = VMSBoards;
			for (int i = 0; i < vMSBoards.Length; i++)
			{
				vMSBoards[i].gameObject.SetActive(value: true);
			}
		}

		private void RpcReader___Observers_Enable_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___Enable_328543758(null);
			}
		}

		private void RpcWriter___Target_Enable_328543758(NetworkConnection conn)
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
				SendTargetRpc(1u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_Enable_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___Enable_328543758(base.LocalConnection);
			}
		}

		private void RpcWriter___Observers_Disable_2166136261()
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

		public void RpcLogic___Disable_2166136261()
		{
			IsEnabled = false;
			if (onCurfewDisabled != null)
			{
				onCurfewDisabled.Invoke();
			}
			VMSBoard[] vMSBoards = VMSBoards;
			for (int i = 0; i < vMSBoards.Length; i++)
			{
				vMSBoards[i].gameObject.SetActive(value: false);
			}
		}

		private void RpcReader___Observers_Disable_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___Disable_2166136261();
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
