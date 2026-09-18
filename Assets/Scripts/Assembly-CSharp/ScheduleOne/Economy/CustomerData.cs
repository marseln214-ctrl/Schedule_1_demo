using System;
using System.Collections.Generic;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Properties;
using UnityEngine;

namespace ScheduleOne.Economy
{
	[Serializable]
	[CreateAssetMenu(fileName = "CustomerData", menuName = "ScriptableObjects/CustomerData", order = 1)]
	public class CustomerData : ScriptableObject
	{
		public CustomerAffinityData DefaultAffinityData;

		[Header("Preferred Properties - Properties the customer prefers in a product.")]
		public List<ScheduleOne.Properties.Property> PreferredProperties = new List<ScheduleOne.Properties.Property>();

		[Header("Spending Behaviour")]
		public float MinWeeklySpend = 200f;

		public float MaxWeeklySpend = 500f;

		[Range(0f, 7f)]
		public int MinOrdersPerWeek = 1;

		[Range(0f, 7f)]
		public int MaxOrdersPerWeek = 5;

		[Header("Timing Settings")]
		public int OrderTime = 1200;

		public EDay PreferredOrderDay;

		[Header("Standards")]
		public ECustomerStandard Standards = ECustomerStandard.Moderate;

		[Header("Direct approaching")]
		public bool CanBeDirectlyApproached = true;

		public bool GuaranteeFirstSampleSuccess;

		[Tooltip("The average relationship of mutual customers to provide a 50% chance of success")]
		[Range(0f, 5f)]
		public float MinMutualRelationRequirement = 3f;

		[Tooltip("The average relationship of mutual customers to provide a 100% chance of success")]
		[Range(0f, 5f)]
		public float MaxMutualRelationRequirement = 5f;

		[Tooltip("If direct approach fails, whats the chance the police will be called?")]
		[Range(0f, 1f)]
		public float CallPoliceChance = 0.5f;

		[Header("Dependence")]
		[Tooltip("How quickly the customer builds dependence")]
		[Range(0f, 2f)]
		public float DependenceMultiplier = 1f;

		[Tooltip("The customer's starting (and lowest possible) dependence level")]
		[Range(0f, 1f)]
		public float BaseAddiction;

		public Action onChanged;

		public static float GetQualityScalar(EQuality quality)
		{
			return quality switch
			{
				EQuality.Trash => 0f, 
				EQuality.Poor => 0.25f, 
				EQuality.Standard => 0.5f, 
				EQuality.Premium => 0.75f, 
				EQuality.Heavenly => 1f, 
				_ => 0f, 
			};
		}

		public List<EDay> GetOrderDays(float dependence, float normalizedRelationship)
		{
			float t = Mathf.Max(dependence, normalizedRelationship);
			int num = Mathf.RoundToInt(Mathf.Lerp(MinOrdersPerWeek, MaxOrdersPerWeek, t));
			int preferredOrderDay = (int)PreferredOrderDay;
			int a = Mathf.RoundToInt(7f / (float)num);
			a = Mathf.Max(a, 1);
			List<EDay> list = new List<EDay>();
			for (int i = 0; i < 7; i += a)
			{
				list.Add((EDay)((i + preferredOrderDay) % 7));
			}
			return list;
		}

		public float GetAdjustedWeeklySpend(float normalizedRelationship)
		{
			return Mathf.Lerp(MinWeeklySpend, MaxWeeklySpend, normalizedRelationship);
		}
	}
}
