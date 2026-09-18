using ScheduleOne.Persistence.Datas;
using UnityEngine.Events;

namespace ScheduleOne.Management
{
	public class NumberField : ConfigField
	{
		public UnityEvent<float> onItemChanged = new UnityEvent<float>();

		public float Value { get; protected set; }

		public float MinValue { get; protected set; }

		public float MaxValue { get; protected set; } = 100f;


		public bool WholeNumbers { get; protected set; }

		public NumberField(EntityConfiguration parentConfig)
			: base(parentConfig)
		{
		}

		public void SetValue(float value, bool network)
		{
			Value = value;
			if (network)
			{
				base.ParentConfig.ReplicateField(this);
			}
			if (onItemChanged != null)
			{
				onItemChanged.Invoke(Value);
			}
		}

		public void Configure(float minValue, float maxValue, bool wholeNumbers)
		{
			MinValue = minValue;
			MaxValue = maxValue;
			WholeNumbers = wholeNumbers;
		}

		public override bool IsValueDefault()
		{
			return Value == 0f;
		}

		public NumberFieldData GetData()
		{
			return new NumberFieldData(Value);
		}

		public void Load(NumberFieldData data)
		{
			if (data != null)
			{
				SetValue(data.Value, network: true);
			}
		}
	}
}
