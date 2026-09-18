using FishNet.Connection;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Quests;
using UnityEngine.Events;

namespace ScheduleOne.Variables
{
	public class Variable<T> : BaseVariable
	{
		public T Value;

		public UnityEvent<T> OnValueChanged = new UnityEvent<T>();

		public Variable(string name, EVariableReplicationMode replicationMode, bool persistent, EVariableMode mode, Player owner, T value)
			: base(name, replicationMode, persistent, mode, owner)
		{
			Value = value;
			ReplicateValue(null);
		}

		public override object GetValue()
		{
			return Value;
		}

		public override void SetValue(object value, bool replicate)
		{
			if (value is string)
			{
				if (TryDeserialize((string)value, out var value2))
				{
					value = value2;
				}
				else
				{
					string[] obj = new string[6] { "Failed to deserialize value '", null, null, null, null, null };
					T val = value2;
					obj[1] = val?.ToString();
					obj[2] = "' for variable ";
					obj[3] = Name;
					obj[4] = " of type ";
					obj[5] = typeof(T).Name;
					Console.LogWarning(string.Concat(obj));
				}
			}
			Value = (T)value;
			if (replicate)
			{
				ReplicateValue(null);
			}
			if (OnValueChanged != null)
			{
				OnValueChanged.Invoke(Value);
			}
			StateMachine.ChangeState();
		}

		public virtual bool TryDeserialize(string valueString, out T value)
		{
			value = default(T);
			return false;
		}

		public override void ReplicateValue(NetworkConnection conn)
		{
			if (VariableMode == EVariableMode.Global)
			{
				NetworkSingleton<VariableDatabase>.Instance.SendValue(conn, Name, Value.ToString());
			}
			else if (base.Owner.IsOwner)
			{
				Player.Local.SendValue(Name, Value.ToString(), sendToOwner: false);
			}
			else
			{
				base.Owner.SendValue(Name, Value.ToString(), sendToOwner: true);
			}
		}
	}
}
