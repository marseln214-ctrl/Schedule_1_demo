using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.Map;
using ScheduleOne.NPCs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class RegionUnlockedCanvas : Singleton<RegionUnlockedCanvas>, IPostSleepEvent
	{
		public Animation OpenCloseAnim;

		public TextMeshProUGUI RegionLabel;

		public TextMeshProUGUI RegionDescription;

		public Image RegionImage;

		private EMapRegion region;

		public bool IsRunning { get; private set; }

		public int Order { get; private set; } = 5;


		public void QueueUnlocked(EMapRegion _region)
		{
			region = _region;
			Singleton<SleepCanvas>.Instance.AddPostSleepEvent(this);
		}

		public void StartEvent()
		{
			IsRunning = true;
			MapRegionData regionData = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(region);
			RegionLabel.text = regionData.Name;
			RegionImage.sprite = regionData.RegionSprite;
			List<NPC> nPCsInRegion = NPCManager.GetNPCsInRegion(region);
			int num = nPCsInRegion.Count((NPC x) => x.GetComponent<Customer>() != null);
			int num2 = nPCsInRegion.Count((NPC x) => x is Dealer);
			int num3 = nPCsInRegion.Count((NPC x) => x is Supplier);
			RegionDescription.text = string.Empty;
			if (num > 0)
			{
				TextMeshProUGUI regionDescription = RegionDescription;
				regionDescription.text = regionDescription.text + num + " potential customer" + ((num > 1) ? "s" : "");
			}
			if (num2 > 0)
			{
				if (RegionDescription.text.Length > 0)
				{
					RegionDescription.text += "\n";
				}
				TextMeshProUGUI regionDescription2 = RegionDescription;
				regionDescription2.text = regionDescription2.text + num2 + " dealer" + ((num2 > 1) ? "s" : "");
			}
			if (num3 > 0)
			{
				if (RegionDescription.text.Length > 0)
				{
					RegionDescription.text += "\n";
				}
				TextMeshProUGUI regionDescription3 = RegionDescription;
				regionDescription3.text = regionDescription3.text + num3 + " supplier" + ((num3 > 1) ? "s" : "");
			}
			OpenCloseAnim.Play("Rank up open");
		}

		public void EndEvent()
		{
			if (IsRunning)
			{
				OpenCloseAnim.Play("Rank up close");
				IsRunning = false;
			}
		}
	}
}
