using FishNet.Object;

namespace ScheduleOne.DevUtilities
{
	public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
	{
		private static T instance;

		protected bool Destroyed;

		private bool NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted;

		public static bool InstanceExists => instance != null;

		public static T Instance
		{
			get
			{
				return instance;
			}
			protected set
			{
				instance = value;
			}
		}

		protected virtual void Start()
		{
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EDevUtilities_002ENetworkSingleton_00601_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected virtual void OnDestroy()
		{
			if (instance == this)
			{
				instance = null;
			}
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EDevUtilities_002ENetworkSingleton_00601_Assembly_002DCSharp_002Edll()
		{
			if (instance != null)
			{
				Console.LogWarning("Multiple instances of " + base.name + " exist. Keeping prior instance reference.");
			}
			else
			{
				instance = (T)this;
			}
		}
	}
}
