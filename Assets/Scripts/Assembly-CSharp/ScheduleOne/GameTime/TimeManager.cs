using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.GameTime
{
	public class TimeManager : NetworkSingleton<TimeManager>, IBaseSaveable, ISaveable
	{
		public const float CYCLE_DURATION_MINS = 20f;

		public const float MINUTE_TIME = 5f / 6f;

		public const int DEFAULT_WAKE_TIME = 700;

		public const int END_OF_DAY = 400;

		public int DefaultTime = 900;

		public EDay DefaultDay;

		public float TimeProgressionMultiplier = 1f;

		private int savedTime;

		public Action onMinutePass;

		public Action onHourPass;

		public Action onDayPass;

		public Action onUpdate;

		public Action<int> onTimeSkip;

		public static Action onSleepStart;

		public UnityEvent _onSleepStart;

		public static Action<int> onSleepEnd;

		public UnityEvent _onSleepEnd;

		public UnityEvent onFirstNight;

		public Action onTimeChanged;

		public const int SelectedWakeTime = 700;

		private GameDateTime sleepStartTime;

		private int sleepEndTime;

		[SyncVar]
		public bool hostDailySummaryDone;

		private float defaultFixedTimeScale;

		private TimeLoader loader = new TimeLoader();

		public SyncVar<bool> syncVar___hostDailySummaryDone;

		private bool NetworkInitialize___EarlyScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted;

		public bool IsEndOfDay => CurrentTime == 400;

		public bool SleepInProgress { get; protected set; }

		public int ElapsedDays { get; protected set; }

		public int CurrentTime { get; protected set; }

		public float TimeOnCurrentMinute { get; protected set; }

		public int DailyMinTotal { get; protected set; }

		public bool IsNight
		{
			get
			{
				if (CurrentTime >= 600)
				{
					return CurrentTime >= 1800;
				}
				return true;
			}
		}

		public int DayIndex => ElapsedDays % 7;

		public float NormalizedTime => (float)DailyMinTotal / 1440f;

		public float Playtime { get; protected set; }

		public EDay CurrentDay => (EDay)DayIndex;

		public bool TimeOverridden { get; protected set; }

		public string SaveFolderName => "Time";

		public string SaveFileName => "Time";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => false;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		public bool SyncAccessor_hostDailySummaryDone
		{
			get
			{
				return hostDailySummaryDone;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					hostDailySummaryDone = value;
				}
				if (Application.isPlaying)
				{
					syncVar___hostDailySummaryDone.SetValue(value, value);
				}
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EGameTime_002ETimeManager_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (!connection.IsHost)
			{
				SendTimeData(connection);
			}
		}

		public override void OnStartClient()
		{
			base.OnStartClient();
			StartCoroutine(TimeLoop());
		}

		private void Clean()
		{
			onSleepStart = null;
			onSleepEnd = null;
			onSleepStart = null;
			onSleepEnd = null;
			onMinutePass = null;
			onHourPass = null;
			onDayPass = null;
			onTimeChanged = null;
		}

		public void SendTimeData(NetworkConnection connection)
		{
			StartCoroutine(WaitForPlayerReady());
			IEnumerator WaitForPlayerReady()
			{
				yield return new WaitUntil(() => Player.GetPlayer(connection) != null && Player.GetPlayer(connection).PlayerInitializedOverNetwork);
				SetData(connection, ElapsedDays, CurrentTime, DateTime.UtcNow.Ticks / 10000000);
			}
		}

		[ObserversRpc(RunLocally = true, ExcludeServer = true)]
		[TargetRpc]
		private void SetData(NetworkConnection conn, int _elapsedDays, int _time, float sendTime)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetData_2661156041(conn, _elapsedDays, _time, sendTime);
				RpcLogic___SetData_2661156041(conn, _elapsedDays, _time, sendTime);
			}
			else
			{
				RpcWriter___Target_SetData_2661156041(conn, _elapsedDays, _time, sendTime);
			}
		}

		protected virtual void Update()
		{
			if (CurrentTime != 400)
			{
				TimeOnCurrentMinute += Time.deltaTime * TimeProgressionMultiplier;
			}
			Playtime += Time.unscaledDeltaTime;
			if (Time.timeScale >= 1f)
			{
				Time.fixedDeltaTime = defaultFixedTimeScale * Time.timeScale;
			}
			else
			{
				Time.fixedDeltaTime = defaultFixedTimeScale;
			}
			if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.RightArrow) && InstanceFinder.IsServer && (Application.isEditor || Debug.isDebugBuild))
			{
				for (int i = 0; i < 60; i++)
				{
					Tick();
				}
				SetData(null, ElapsedDays, CurrentTime, DateTime.UtcNow.Ticks / 10000000);
			}
			if (InstanceFinder.IsHost)
			{
				if (SleepInProgress)
				{
					if (IsCurrentTimeWithinRange(sleepEndTime, AddMinutesTo24HourTime(sleepEndTime, 60)))
					{
						EndSleep();
					}
				}
				else if (Player.AreAllPlayersReadyToSleep())
				{
					StartSleep();
				}
			}
			if (onUpdate != null)
			{
				onUpdate();
			}
		}

		private IEnumerator TimeLoop()
		{
			float lastWaitExcess = 0f;
			Console.Log("Starting time loop");
			while (base.gameObject != null)
			{
				if (TimeProgressionMultiplier <= 0f)
				{
					yield return new WaitUntil(() => TimeProgressionMultiplier > 0f);
				}
				if (Time.timeScale == 0f)
				{
					yield return new WaitUntil(() => Time.timeScale > 0f);
				}
				float timeToWait = 5f / 6f / (TimeProgressionMultiplier * Time.timeScale) - lastWaitExcess;
				if (timeToWait > 0f)
				{
					float timeOnWaitStart = Time.realtimeSinceStartup;
					yield return new WaitForSecondsRealtime(timeToWait);
					float num = Time.realtimeSinceStartup - timeOnWaitStart;
					lastWaitExcess = Mathf.Max(num - timeToWait, 0f);
				}
				else
				{
					lastWaitExcess -= 5f / 6f / TimeProgressionMultiplier;
				}
				Tick();
				yield return null;
			}
		}

		private void Tick()
		{
			if (Player.Local == null)
			{
				Console.LogWarning("Local player does not exist. Waiting for player to spawn.");
				return;
			}
			TimeOnCurrentMinute = 0f;
			try
			{
				if (onMinutePass != null)
				{
					onMinutePass();
				}
			}
			catch (Exception ex)
			{
				Console.LogError("Error invoking onMinutePass: " + ex.Message + "\nSite:" + ex.StackTrace);
			}
			if (CurrentTime == 400)
			{
				return;
			}
			if (CurrentTime == 2359)
			{
				ElapsedDays++;
				CurrentTime = 0;
				DailyMinTotal = 0;
				if (onDayPass != null)
				{
					onDayPass();
				}
				if (onHourPass != null)
				{
					onHourPass();
				}
			}
			else if (CurrentTime % 100 >= 59)
			{
				CurrentTime += 41;
				if (onHourPass != null)
				{
					onHourPass();
				}
			}
			else
			{
				CurrentTime++;
			}
			DailyMinTotal = GetMinSumFrom24HourTime(CurrentTime);
			HasChanged = true;
			if (ElapsedDays == 0 && CurrentTime == 2000 && onFirstNight != null)
			{
				onFirstNight.Invoke();
			}
		}

		public void SetTime(int _time, bool local = false)
		{
			if (!InstanceFinder.IsHost && InstanceFinder.NetworkManager != null && !local)
			{
				Console.LogWarning("SetTime can only be called by host");
				return;
			}
			Console.Log("Setting time to: " + _time);
			CurrentTime = _time;
			TimeOnCurrentMinute = 0f;
			DailyMinTotal = GetMinSumFrom24HourTime(CurrentTime);
			SetData(null, ElapsedDays, CurrentTime, DateTime.UtcNow.Ticks / 10000000);
		}

		public void SetElapsedDays(int days)
		{
			if (!InstanceFinder.IsHost && InstanceFinder.NetworkManager != null)
			{
				Console.LogWarning("SetElapsedDays can only be called by host");
				return;
			}
			ElapsedDays = days;
			SetData(null, ElapsedDays, CurrentTime, DateTime.UtcNow.Ticks / 10000000);
		}

		public static string Get12HourTime(float _time, bool appendDesignator = true)
		{
			string text = _time.ToString();
			while (text.Length < 4)
			{
				text = "0" + text;
			}
			int num = Convert.ToInt32(text.Substring(0, 2));
			int num2 = Convert.ToInt32(text.Substring(2, 2));
			string text2 = "AM";
			if (num == 0)
			{
				num = 12;
			}
			else if (num == 12)
			{
				text2 = "PM";
			}
			else if (num > 12)
			{
				num -= 12;
				text2 = "PM";
			}
			string text3 = $"{num}:{num2:00}";
			if (appendDesignator)
			{
				text3 = text3 + " " + text2;
			}
			return text3;
		}

		public static int Get24HourTimeFromMinSum(int minSum)
		{
			if (minSum < 0)
			{
				minSum = 1440 - minSum;
			}
			minSum %= 1440;
			int num = (int)((float)minSum / 60f);
			int num2 = minSum - 60 * num;
			return num * 100 + num2;
		}

		public static int GetMinSumFrom24HourTime(int _time)
		{
			int num = (int)((float)_time / 100f);
			int num2 = _time - num * 100;
			return num * 60 + num2;
		}

		public bool IsCurrentTimeWithinRange(int min, int max)
		{
			return IsGivenTimeWithinRange(CurrentTime, min, max);
		}

		public static bool IsGivenTimeWithinRange(int givenTime, int min, int max)
		{
			if (max > min)
			{
				if (givenTime >= min)
				{
					return givenTime <= max;
				}
				return false;
			}
			if (givenTime < min)
			{
				return givenTime <= max;
			}
			return true;
		}

		public static bool IsValid24HourTime(string input)
		{
			string pattern = "^([01]?[0-9]|2[0-3])[0-5][0-9]$";
			return Regex.IsMatch(input, pattern);
		}

		public bool IsCurrentDateWithinRange(GameDateTime start, GameDateTime end)
		{
			int totalMinSum = GetTotalMinSum();
			if (totalMinSum >= start.GetMinSum())
			{
				return totalMinSum <= end.GetMinSum();
			}
			return false;
		}

		public void FastForwardToWakeTime()
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (CurrentTime > 1200)
			{
				ElapsedDays++;
				DailyMinTotal = GetMinSumFrom24HourTime(CurrentTime);
				HasChanged = true;
				if (onDayPass != null)
				{
					onDayPass();
				}
			}
			int minSumFrom24HourTime = GetMinSumFrom24HourTime(CurrentTime);
			int obj = Mathf.Abs(GetMinSumFrom24HourTime(700) - minSumFrom24HourTime);
			int time = 700;
			if (GameManager.IS_TUTORIAL)
			{
				time = 300;
			}
			SetTime(time);
			if (onTimeSkip != null)
			{
				onTimeSkip(obj);
			}
			if (onSleepEnd != null)
			{
				onSleepEnd(obj);
			}
			if (_onSleepEnd != null)
			{
				_onSleepEnd.Invoke();
			}
		}

		public GameDateTime GetDateTime()
		{
			return new GameDateTime(ElapsedDays, CurrentTime);
		}

		public int GetTotalMinSum()
		{
			return ElapsedDays * 1440 + DailyMinTotal;
		}

		public static int AddMinutesTo24HourTime(int time, int minsToAdd)
		{
			int num = GetMinSumFrom24HourTime(time) + minsToAdd;
			if (num < 0)
			{
				num = 1440 + num;
			}
			return Get24HourTimeFromMinSum(num);
		}

		public static List<int> GetAllTimeInRange(int min, int max)
		{
			List<int> list = new List<int>();
			int num = min;
			while (num != max)
			{
				list.Add(num);
				num++;
				if (num >= 2360)
				{
					num = 0;
				}
				else if (num % 100 >= 60)
				{
					num += 40;
				}
			}
			list.Add(max);
			return list;
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public void SetWakeTime(int amount)
		{
			RpcWriter___Server_SetWakeTime_3316948804(amount);
			RpcLogic___SetWakeTime_3316948804(amount);
		}

		[ObserversRpc(RunLocally = true)]
		private void StartSleep()
		{
			RpcWriter___Observers_StartSleep_2166136261();
			RpcLogic___StartSleep_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		private void EndSleep()
		{
			RpcWriter___Observers_EndSleep_2166136261();
			RpcLogic___EndSleep_2166136261();
		}

		public virtual string GetSaveString()
		{
			return new TimeData(CurrentTime, ElapsedDays, Mathf.RoundToInt(Playtime)).GetJson();
		}

		public void SetPlaytime(float time)
		{
			Playtime = time;
		}

		public void SetTimeOverridden(bool overridden, int time = 1200)
		{
			if (overridden && TimeOverridden)
			{
				Console.LogWarning("Time already overridden.");
				return;
			}
			TimeOverridden = overridden;
			if (overridden)
			{
				savedTime = CurrentTime;
				SetTime(time);
			}
			else
			{
				SetTime(savedTime);
			}
			if (onMinutePass != null)
			{
				onMinutePass();
			}
		}

		private void SetRandomTime()
		{
			int minSum = UnityEngine.Random.Range(0, 1440);
			SetTime(Get24HourTimeFromMinSum(minSum));
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar___hostDailySummaryDone = new SyncVar<bool>(this, 0u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, hostDailySummaryDone);
				RegisterObserversRpc(0u, RpcReader___Observers_SetData_2661156041);
				RegisterTargetRpc(1u, RpcReader___Target_SetData_2661156041);
				RegisterServerRpc(2u, RpcReader___Server_SetWakeTime_3316948804);
				RegisterObserversRpc(3u, RpcReader___Observers_StartSleep_2166136261);
				RegisterObserversRpc(4u, RpcReader___Observers_EndSleep_2166136261);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EGameTime_002ETimeManager);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar___hostDailySummaryDone.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_SetData_2661156041(NetworkConnection conn, int _elapsedDays, int _time, float sendTime)
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
				writer.WriteInt32(_elapsedDays);
				writer.WriteInt32(_time);
				writer.WriteSingle(sendTime);
				SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetData_2661156041(NetworkConnection conn, int _elapsedDays, int _time, float sendTime)
		{
			ElapsedDays = _elapsedDays;
			CurrentTime = _time;
			DailyMinTotal = GetMinSumFrom24HourTime(CurrentTime);
			HasChanged = true;
			try
			{
				if (onTimeChanged != null)
				{
					onTimeChanged();
				}
			}
			catch (Exception ex)
			{
				Console.LogError("Error invoking onTimeChanged: " + ex.Message + "\nSite:" + ex.StackTrace);
			}
		}

		private void RpcReader___Observers_SetData_2661156041(PooledReader PooledReader0, Channel channel)
		{
			int elapsedDays = PooledReader0.ReadInt32();
			int time = PooledReader0.ReadInt32();
			float sendTime = PooledReader0.ReadSingle();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetData_2661156041(null, elapsedDays, time, sendTime);
			}
		}

		private void RpcWriter___Target_SetData_2661156041(NetworkConnection conn, int _elapsedDays, int _time, float sendTime)
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
				writer.WriteInt32(_elapsedDays);
				writer.WriteInt32(_time);
				writer.WriteSingle(sendTime);
				SendTargetRpc(1u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetData_2661156041(PooledReader PooledReader0, Channel channel)
		{
			int elapsedDays = PooledReader0.ReadInt32();
			int time = PooledReader0.ReadInt32();
			float sendTime = PooledReader0.ReadSingle();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetData_2661156041(base.LocalConnection, elapsedDays, time, sendTime);
			}
		}

		private void RpcWriter___Server_SetWakeTime_3316948804(int amount)
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
				writer.WriteInt32(amount);
				SendServerRpc(2u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetWakeTime_3316948804(int amount)
		{
		}

		private void RpcReader___Server_SetWakeTime_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int amount = PooledReader0.ReadInt32();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetWakeTime_3316948804(amount);
			}
		}

		private void RpcWriter___Observers_StartSleep_2166136261()
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
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___StartSleep_2166136261()
		{
			Debug.Log("Start sleep");
			sleepStartTime = GetDateTime();
			sleepEndTime = 700;
			if (NetworkSingleton<GameManager>.Instance.IsTutorial)
			{
				sleepEndTime = 100;
			}
			SleepInProgress = true;
			Time.timeScale = 1f;
			if (onSleepStart != null)
			{
				onSleepStart();
			}
			if (_onSleepStart != null)
			{
				_onSleepStart.Invoke();
			}
		}

		private void RpcReader___Observers_StartSleep_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___StartSleep_2166136261();
			}
		}

		private void RpcWriter___Observers_EndSleep_2166136261()
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
				SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___EndSleep_2166136261()
		{
			Debug.Log("End sleep");
			SleepInProgress = false;
			Time.timeScale = 1f;
			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsHost)
			{
				SendTimeData(null);
			}
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Sleep_Count", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Sleep_Count") + 1f).ToString(), network: false);
			if (onSleepEnd != null)
			{
				onSleepEnd(GetDateTime().GetMinSum() - sleepStartTime.GetMinSum());
			}
			if (_onSleepEnd != null)
			{
				_onSleepEnd.Invoke();
			}
		}

		private void RpcReader___Observers_EndSleep_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___EndSleep_2166136261();
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EGameTime_002ETimeManager(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 0)
			{
				if (PooledReader0 == null)
				{
					syncVar___hostDailySummaryDone.SetValue(syncVar___hostDailySummaryDone.GetValue(calledByUser: true), true);
					return true;
				}
				bool value = PooledReader0.ReadBoolean();
                syncVar___hostDailySummaryDone.SetValue(value, Boolean2);
				return true;
			}
			return false;
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EGameTime_002ETimeManager_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			defaultFixedTimeScale = Time.fixedDeltaTime;
			if (!Singleton<Lobby>.InstanceExists || !Singleton<Lobby>.Instance.IsInLobby || Singleton<Lobby>.Instance.IsHost)
			{
				SetTime(DefaultTime, local: true);
				ElapsedDays = (int)DefaultDay;
				DailyMinTotal = GetMinSumFrom24HourTime(CurrentTime);
			}
			InitializeSaveable();
			Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(Clean);
			SetWakeTime(700);
		}
	}
}
