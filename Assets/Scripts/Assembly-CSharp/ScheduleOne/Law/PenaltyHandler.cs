using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using UnityEngine;

namespace ScheduleOne.Law
{
	public static class PenaltyHandler
	{
		public const float CONTROLLED_SUBSTANCE_FINE = 5f;

		public const float LOW_SEVERITY_DRUG_FINE = 10f;

		public const float MED_SEVERITY_DRUG_FINE = 20f;

		public const float HIGH_SEVERITY_DRUG_FINE = 30f;

		public const float FAILURE_TO_COMPLY_FINE = 50f;

		public const float EVADING_ARREST_FINE = 50f;

		public const float VIOLATING_CURFEW_TIME = 100f;

		public const float ATTEMPT_TO_SELL_FINE = 150f;

		public const float ASSAULT_FINE = 75f;

		public const float DEADLY_ASSAULT_FINE = 150f;

		public const float VANDALISM_FINE = 50f;

		public const float THEFT_FINE = 50f;

		public const float BRANDISHING_FINE = 50f;

		public const float DISCHARGE_FIREARM_FINE = 50f;

		public static List<string> ProcessCrimeList(Dictionary<Crime, int> crimes)
		{
			List<string> list = new List<string>();
			float num = 0f;
			Crime[] array = crimes.Keys.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is PossessingControlledSubstances)
				{
					float num2 = 5f * (float)crimes[array[i]];
					num += num2;
					list.Add(crimes[array[i]] + " controlled substances confiscated");
				}
				else if (array[i] is PossessingLowSeverityDrug)
				{
					float num3 = 10f * (float)crimes[array[i]];
					num += num3;
					list.Add(crimes[array[i]] + " low-severity drugs confiscated");
				}
				else if (array[i] is PossessingModerateSeverityDrug)
				{
					float num4 = 20f * (float)crimes[array[i]];
					num += num4;
					list.Add(crimes[array[i]] + " moderate-severity drugs confiscated");
				}
				else if (array[i] is PossessingHighSeverityDrug)
				{
					float num5 = 30f * (float)crimes[array[i]];
					num += num5;
					list.Add(crimes[array[i]] + " high-severity drugs confiscated");
				}
				else if (array[i] is Evading)
				{
					num += 50f;
				}
				else if (array[i] is FailureToComply)
				{
					num += 50f;
				}
				else if (array[i] is ViolatingCurfew)
				{
					num += 100f;
				}
				else if (array[i] is AttemptingToSell)
				{
					num += 150f;
				}
				else if (array[i] is Assault)
				{
					num += 75f;
				}
				else if (array[i] is DeadlyAssault)
				{
					num += 150f;
				}
				else if (array[i] is Vandalism)
				{
					num += 50f;
				}
				else if (array[i] is Theft)
				{
					num += 50f;
				}
				else if (array[i] is BrandishingWeapon)
				{
					num += 50f;
				}
				else if (array[i] is DischargeFirearm)
				{
					num += 50f;
				}
			}
			if (num > 0f)
			{
				float num6 = Mathf.Min(num, NetworkSingleton<MoneyManager>.Instance.cashBalance);
				string text = MoneyManager.FormatAmount(num, showDecimals: true) + " fine";
				if (num6 == num)
				{
					text += " (paid in cash)";
				}
				else
				{
					text = text + " (" + MoneyManager.FormatAmount(num6, showDecimals: true) + " paid";
					text += " - insufficient cash)";
				}
				list.Add(text);
				if (num6 > 0f)
				{
					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - num6);
				}
			}
			return list;
		}
	}
}
