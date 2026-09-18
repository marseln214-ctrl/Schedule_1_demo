using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.Persistence;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.MainMenu
{
	public class SaveDisplay : MonoBehaviour
	{
		[Header("References")]
		public RectTransform[] Slots;

		public void Awake()
		{
			Singleton<LoadManager>.Instance.onSaveInfoLoaded.AddListener(Refresh);
			Refresh();
		}

		public void Refresh()
		{
			for (int i = 0; i < LoadManager.SaveGames.Length; i++)
			{
				SetDisplayedSave(i, LoadManager.SaveGames[i]);
			}
		}

		public void SetDisplayedSave(int index, SaveInfo info)
		{
			Transform transform = Slots[index].Find("Container");
			if (info == null)
			{
				transform.gameObject.SetActive(value: false);
				return;
			}
			transform.Find("Organisation").GetComponent<TextMeshProUGUI>().text = info.OrganisationName;
			transform.Find("Version").GetComponent<TextMeshProUGUI>().text = "v" + info.SaveVersion;
			float networth = info.Networth;
			string empty = string.Empty;
			Color color = new Color32(75, byte.MaxValue, 10, byte.MaxValue);
			if (networth > 1000000f)
			{
				networth /= 1000000f;
				empty = "$" + RoundToDecimalPlaces(networth, 1) + "M";
				color = new Color32(byte.MaxValue, 225, 10, byte.MaxValue);
			}
			else if (networth > 1000f)
			{
				networth /= 1000f;
				empty = "$" + RoundToDecimalPlaces(networth, 1) + "K";
			}
			else
			{
				empty = MoneyManager.FormatAmount(networth);
			}
			transform.Find("NetWorth/Text").GetComponent<TextMeshProUGUI>().text = empty;
			transform.Find("NetWorth/Text").GetComponent<TextMeshProUGUI>().color = color;
			int hours = Mathf.RoundToInt((float)(DateTime.Now - info.DateCreated).TotalHours);
			transform.Find("Created/Text").GetComponent<TextMeshProUGUI>().text = GetTimeLabel(hours);
			int hours2 = Mathf.RoundToInt((float)(DateTime.Now - info.DateLastPlayed).TotalHours);
			transform.Find("LastPlayed/Text").GetComponent<TextMeshProUGUI>().text = GetTimeLabel(hours2);
			transform.gameObject.SetActive(value: true);
		}

		private float RoundToDecimalPlaces(float value, int decimalPlaces)
		{
			return ToSingle(System.Math.Floor((double)value * System.Math.Pow(10.0, decimalPlaces)) / System.Math.Pow(10.0, decimalPlaces));
		}

		public static float ToSingle(double value)
		{
			return (float)value;
		}

		private string GetTimeLabel(int hours)
		{
			int num = hours / 24;
			if (num == 0)
			{
				return "Today";
			}
			if (num == 1)
			{
				return "Yesterday";
			}
			if (num > 365)
			{
				return "More than a year ago";
			}
			return num + " days ago";
		}
	}
}
