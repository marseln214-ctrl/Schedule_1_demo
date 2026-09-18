using System.Collections;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop
{
	public class ShopInterfaceDetailPanel : MonoBehaviour
	{
		[Header("References")]
		public RectTransform Panel;

		public VerticalLayoutGroup LayoutGroup;

		public TextMeshProUGUI DescriptionLabel;

		public TextMeshProUGUI UnlockLabel;

		private ListingUI listing;

		private void Awake()
		{
			Panel.gameObject.SetActive(value: false);
		}

		public void Open(ListingUI _listing)
		{
			listing = _listing;
			DescriptionLabel.text = listing.Listing.Item.Description;
			if (listing.Listing.Item.RequiresLevelToPurchase && !listing.Listing.Item.IsPurchasable)
			{
				UnlockLabel.text = "Unlocks at <color=#2DB92D>" + listing.Listing.Item.RequiredRank.ToString() + "</color>";
				UnlockLabel.gameObject.SetActive(value: true);
			}
			else
			{
				UnlockLabel.gameObject.SetActive(value: false);
			}
			Singleton<CoroutineService>.Instance.StartCoroutine(Wait());
			IEnumerator Wait()
			{
				LayoutGroup.enabled = false;
				yield return new WaitForEndOfFrame();
				Panel.gameObject.SetActive(value: true);
				LayoutRebuilder.ForceRebuildLayoutImmediate(Panel);
				LayoutGroup.CalculateLayoutInputVertical();
				LayoutGroup.enabled = true;
				Position();
			}
		}

		private void LateUpdate()
		{
			Position();
		}

		private void Position()
		{
			if (!(listing == null))
			{
				Panel.position = listing.DetailPanelAnchor.position;
				Panel.anchoredPosition = new Vector2(Panel.anchoredPosition.x + Panel.sizeDelta.x / 2f, Panel.anchoredPosition.y);
			}
		}

		public void Close()
		{
			listing = null;
			Panel.gameObject.SetActive(value: false);
		}
	}
}
