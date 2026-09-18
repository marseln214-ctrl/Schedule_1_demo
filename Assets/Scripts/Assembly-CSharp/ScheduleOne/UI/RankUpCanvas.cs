using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Levelling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class RankUpCanvas : MonoBehaviour, IPostSleepEvent
	{
		public Animation OpenCloseAnim;

		public Animation RankUpAnim;

		public TextMeshProUGUI OldRankLabel;

		public TextMeshProUGUI NewRankLabel;

		public Canvas Canvas;

		public GameObject UnlockedItemsContainer;

		public RectTransform[] UnlockedItems;

		public TextMeshProUGUI ExtraUnlocksLabel;

		public AudioSourceController SoundEffect;

		public Slider ProgressSlider;

		public TextMeshProUGUI ProgressLabel;

		public AudioSourceController BlipSound;

		public AudioSourceController ClickSound;

		private Coroutine coroutine;

		private List<Tuple<FullRank, FullRank>> queuedRankUps = new List<Tuple<FullRank, FullRank>>();

		public bool IsRunning { get; private set; }

		public int Order { get; private set; }

		public void Start()
		{
			Canvas.enabled = false;
			LevelManager instance = NetworkSingleton<LevelManager>.Instance;
			instance.onRankUp = (Action<FullRank, FullRank>)Delegate.Combine(instance.onRankUp, new Action<FullRank, FullRank>(RankUp));
			NetworkSingleton<TimeManager>.Instance._onSleepStart.AddListener(QueuePostSleepEvent);
		}

		private void QueuePostSleepEvent()
		{
			if (!GameManager.IS_TUTORIAL)
			{
				Singleton<SleepCanvas>.Instance.AddPostSleepEvent(this);
			}
		}

		public void StartEvent()
		{
			IsRunning = true;
			OpenCloseAnim.Play("Rank up open");
			int xpGained = NetworkSingleton<DailySummary>.Instance.xpGained;
			int num = NetworkSingleton<LevelManager>.Instance.TotalXP - xpGained;
			FullRank fullRank = NetworkSingleton<LevelManager>.Instance.GetFullRank(num);
			int num2 = num - NetworkSingleton<LevelManager>.Instance.GetTotalXPForRank(fullRank);
			int num3 = xpGained;
			List<Tuple<FullRank, int, int>> progressDisplays = new List<Tuple<FullRank, int, int>>();
			FullRank fullRank2 = fullRank;
			while (num3 > 0)
			{
				int num4 = Mathf.Min(num3, NetworkSingleton<LevelManager>.Instance.GetXPForTier(fullRank2.Rank));
				if (fullRank2 == fullRank)
				{
					num4 = Mathf.Min(num4, NetworkSingleton<LevelManager>.Instance.GetXPForTier(fullRank2.Rank) - num2);
					progressDisplays.Add(new Tuple<FullRank, int, int>(fullRank2, num2, num4 + num2));
				}
				else
				{
					progressDisplays.Add(new Tuple<FullRank, int, int>(fullRank2, 0, num4));
				}
				num3 -= num4;
				fullRank2 = fullRank2.NextRank();
			}
			ProgressSlider.value = (float)num2 / (float)NetworkSingleton<LevelManager>.Instance.GetXPForTier(fullRank.Rank);
			ProgressLabel.text = num2 + " / " + NetworkSingleton<LevelManager>.Instance.GetXPForTier(fullRank.Rank) + " XP";
			OldRankLabel.text = FullRank.GetString(fullRank);
			coroutine = Singleton<CoroutineService>.Instance.StartCoroutine(Routine());
			queuedRankUps.Clear();
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(0.3f);
				bool rankSoundPlayed = false;
				foreach (Tuple<FullRank, int, int> progress in progressDisplays)
				{
					FullRank oldRank = progress.Item1;
					FullRank newRank = progress.Item1.NextRank();
					int startXP = progress.Item2;
					int endXP = progress.Item3;
					float lerpTime = Mathf.Lerp(0.5f, 3f, (float)(endXP - startXP) / (float)NetworkSingleton<LevelManager>.Instance.GetXPForTier(oldRank.Rank));
					int xpForRank = NetworkSingleton<LevelManager>.Instance.GetXPForTier(oldRank.Rank);
					float blipSpacing = 0.04f;
					float blipTime = blipSpacing;
					for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
					{
						int num5 = Mathf.RoundToInt(Mathf.Lerp(startXP, endXP, i / lerpTime));
						ProgressSlider.value = (float)num5 / (float)xpForRank;
						ProgressLabel.text = num5 + " / " + xpForRank + " XP";
						blipTime -= Time.unscaledDeltaTime;
						if (blipTime <= 0f)
						{
							BlipSound.Play();
							blipTime = blipSpacing;
						}
						yield return new WaitForEndOfFrame();
					}
					ProgressSlider.value = (float)endXP / (float)xpForRank;
					ProgressLabel.text = endXP + " / " + xpForRank + " XP";
					if (endXP == xpForRank)
					{
						ClickSound.Play();
						PlayRankupAnimation(oldRank, newRank, !rankSoundPlayed);
						rankSoundPlayed = true;
						yield return new WaitForSeconds(3.5f);
					}
					Console.Log(progress.Item1.ToString() + " " + progress.Item2 + " -> " + progress.Item3);
				}
				coroutine = null;
			}
		}

		public void EndEvent()
		{
			if (IsRunning)
			{
				IsRunning = false;
				if (coroutine != null)
				{
					Singleton<CoroutineService>.Instance.StopCoroutine(coroutine);
					coroutine = null;
				}
				OpenCloseAnim.Play();
				OpenCloseAnim.Play("Rank up close");
			}
		}

		public void RankUp(FullRank oldRank, FullRank newRank)
		{
			queuedRankUps.Add(new Tuple<FullRank, FullRank>(oldRank, newRank));
		}

		private void PlayRankupAnimation(FullRank oldRank, FullRank newRank, bool playSound)
		{
			Canvas.enabled = true;
			OldRankLabel.text = FullRank.GetString(oldRank);
			NewRankLabel.text = FullRank.GetString(newRank);
			List<Unlockable> list = new List<Unlockable>();
			if (NetworkSingleton<LevelManager>.Instance.Unlockables.ContainsKey(newRank))
			{
				list = NetworkSingleton<LevelManager>.Instance.Unlockables[newRank];
			}
			UnlockedItemsContainer.gameObject.SetActive(list.Count > 0);
			for (int i = 0; i < UnlockedItems.Length; i++)
			{
				if (i < list.Count)
				{
					UnlockedItems[i].Find("Icon").GetComponent<Image>().sprite = list[i].Icon;
					UnlockedItems[i].GetComponentInChildren<TextMeshProUGUI>().text = list[i].Title;
					UnlockedItems[i].gameObject.SetActive(value: true);
				}
				else
				{
					UnlockedItems[i].gameObject.SetActive(value: false);
				}
			}
			ExtraUnlocksLabel.text = ((list.Count > UnlockedItems.Length) ? ("+" + (list.Count - UnlockedItems.Length) + " more") : "");
			RankUpAnim.Play();
			if (playSound)
			{
				SoundEffect.Play();
			}
		}
	}
}
