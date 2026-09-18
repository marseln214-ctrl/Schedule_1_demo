using UnityEngine;

namespace ScheduleOne.Product
{
	public static class PropertyMethods
	{
		public static string GetName(this EProperty property)
		{
			return PropertyUtility.GetPropertyData(property).Name;
		}

		public static string GetDescription(this EProperty property)
		{
			return PropertyUtility.GetPropertyData(property).Description;
		}

		public static Color GetColor(this EProperty property)
		{
			return PropertyUtility.GetPropertyData(property).Color;
		}
	}
}
