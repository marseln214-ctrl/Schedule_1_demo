using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class NewCustomerPopup : Singleton<NewCustomerPopup>
	{
		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public CanvasGroup Group;

		public Animation Anim;

		public TextMeshProUGUI Title;

		public RectTransform[] Entries;

		public AudioSourceController SoundEffect;

		private Coroutine routine;

		public bool IsPlaying { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			DisableEntries();
		}

		public void PlayPopup(Customer customer)
		{
			IsPlaying = true;
			RectTransform rectTransform = null;
			int num = 0;
			for (int i = 0; i < Entries.Length; i++)
			{
				num++;
				if (!Entries[i].gameObject.activeSelf)
				{
					rectTransform = Entries[i];
					break;
				}
			}
			if (!(rectTransform == null))
			{
				rectTransform.Find("Mask/Icon").GetComponent<Image>().sprite = customer.NPC.MugshotSprite;
				rectTransform.Find("Name").GetComponent<TextMeshProUGUI>().text = customer.NPC.FirstName + "\n" + customer.NPC.LastName;
				rectTransform.gameObject.SetActive(value: true);
				if (num == 1)
				{
					Title.text = "New Customer Unlocked!";
				}
				else
				{
					Title.text = "New Customers Unlocked!";
				}
				if (routine != null)
				{
					StopCoroutine(routine);
					Anim.Stop();
					routine = null;
				}
				routine = StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				yield return new WaitUntil(() => !Singleton<DealCompletionPopup>.Instance.IsPlaying);
				Group.alpha = 0.01f;
				Canvas.enabled = true;
				Container.gameObject.SetActive(value: true);
				SoundEffect.Play();
				Anim.Play();
				yield return new WaitForSeconds(0.1f);
				yield return new WaitUntil(() => Group.alpha == 0f);
				Canvas.enabled = false;
				Container.gameObject.SetActive(value: false);
				routine = null;
				IsPlaying = false;
				DisableEntries();
			}
		}

		private void DisableEntries()
		{
			for (int i = 0; i < Entries.Length; i++)
			{
				Entries[i].gameObject.SetActive(value: false);
			}
		}
	}
}
