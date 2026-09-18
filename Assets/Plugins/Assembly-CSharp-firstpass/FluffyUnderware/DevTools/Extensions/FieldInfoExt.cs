using System;
using System.Reflection;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class FieldInfoExt
	{
		public static T GetCustomAttribute<T>(this FieldInfo field) where T : Attribute
		{
			object[] customAttributes = field.GetCustomAttributes(typeof(T), inherit: true);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			return (T)customAttributes[0];
		}
	}
}
