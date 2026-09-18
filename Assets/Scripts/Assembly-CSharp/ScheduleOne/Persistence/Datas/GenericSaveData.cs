using System;
using System.Collections.Generic;

namespace ScheduleOne.Persistence.Datas
{
	[Serializable]
	public class GenericSaveData : SaveData
	{
		[Serializable]
		public class BoolValue
		{
			public string key;

			public bool value;
		}

		[Serializable]
		public class FloatValue
		{
			public string key;

			public float value;
		}

		[Serializable]
		public class IntValue
		{
			public string key;

			public int value;
		}

		[Serializable]
		public class StringValue
		{
			public string key;

			public string value;
		}

		public string GUID = string.Empty;

		public List<BoolValue> boolValues = new List<BoolValue>();

		public List<FloatValue> floatValues = new List<FloatValue>();

		public List<IntValue> intValues = new List<IntValue>();

		public List<StringValue> stringValues = new List<StringValue>();

		public GenericSaveData(string guid)
		{
			GUID = guid;
		}

		public void Add(string key, bool value)
		{
			boolValues.Add(new BoolValue
			{
				key = key,
				value = value
			});
		}

		public void Add(string key, float value)
		{
			floatValues.Add(new FloatValue
			{
				key = key,
				value = value
			});
		}

		public void Add(string key, int value)
		{
			intValues.Add(new IntValue
			{
				key = key,
				value = value
			});
		}

		public void Add(string key, string value)
		{
			stringValues.Add(new StringValue
			{
				key = key,
				value = value
			});
		}

		public bool GetBool(string key, bool defaultValue = false)
		{
			return boolValues.Find((BoolValue x) => x.key == key)?.value ?? defaultValue;
		}

		public float GetFloat(string key, float defaultValue = 0f)
		{
			return floatValues.Find((FloatValue x) => x.key == key)?.value ?? defaultValue;
		}

		public int GetInt(string key, int defaultValue = 0)
		{
			return intValues.Find((IntValue x) => x.key == key)?.value ?? defaultValue;
		}

		public string GetString(string key, string defaultValue = "")
		{
			StringValue stringValue = stringValues.Find((StringValue x) => x.key == key);
			if (stringValue != null)
			{
				return stringValue.value;
			}
			return defaultValue;
		}
	}
}
