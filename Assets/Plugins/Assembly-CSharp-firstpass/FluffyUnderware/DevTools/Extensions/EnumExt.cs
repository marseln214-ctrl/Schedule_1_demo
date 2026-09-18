using System;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class EnumExt
	{
		public static bool HasFlag(this Enum variable, params Enum[] flags)
		{
			if (flags.Length == 0)
			{
				throw new ArgumentNullException("flags");
			}
			int num = Convert.ToInt32(variable);
			Type type = variable.GetType();
			for (int i = 0; i < flags.Length; i++)
			{
				if (!Enum.IsDefined(type, flags[i]))
				{
					throw new ArgumentException($"Enumeration type mismatch.  The flag is of type '{flags[i].GetType()}', was expecting '{type}'.");
				}
				int num2 = Convert.ToInt32(flags[i]);
				if ((num & num2) == num2)
				{
					return true;
				}
			}
			return false;
		}

		public static bool HasFlag<T>(this T value, T flag) where T : struct
		{
			long num = Convert.ToInt64(value);
			long num2 = Convert.ToInt64(flag);
			return (num & num2) != 0;
		}

		public static T Set<T>(this Enum value, T append)
		{
			return value.Set(append, OnOff: true);
		}

		public static T Set<T>(this Enum value, T append, bool OnOff)
		{
			if (append == null)
			{
				throw new ArgumentNullException("append");
			}
			Type type = value.GetType();
			if (OnOff)
			{
				return (T)Enum.Parse(type, (Convert.ToUInt64(value) | Convert.ToUInt64(append)).ToString());
			}
			return (T)Enum.Parse(type, (Convert.ToUInt64(value) & ~Convert.ToUInt64(append)).ToString());
		}
	}
}
