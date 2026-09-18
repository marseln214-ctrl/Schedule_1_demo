using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class CrimeStatusUI : MonoBehaviour
	{
		public const float SmallTextSize = 0.75f;

		public const float LargeTextSize = 1f;

		[Header("References")]
		public RectTransform CrimeStatusContainer;

		public CanvasGroup CrimeStatusGroup;

		public GameObject BodysearchLabel;

		public Image InvestigatingMask;

		public Image UnderArrestMask;

		public Image WantedMask;

		public Image WantedDeadMask;

		public GameObject ArrestProgressContainer;

		private bool animateText;

		private Coroutine routine;

		public void UpdateStatus()
		{
			float b = 0f;
			animateText = false;
			PlayerCrimeData.EPursuitLevel currentPursuitLevel = Player.Local.CrimeData.CurrentPursuitLevel;
			InvestigatingMask.gameObject.SetActive(currentPursuitLevel == PlayerCrimeData.EPursuitLevel.Investigating);
			UnderArrestMask.gameObject.SetActive(currentPursuitLevel == PlayerCrimeData.EPursuitLevel.Arresting);
			WantedMask.gameObject.SetActive(currentPursuitLevel == PlayerCrimeData.EPursuitLevel.NonLethal);
			WantedDeadMask.gameObject.SetActive(currentPursuitLevel == PlayerCrimeData.EPursuitLevel.Lethal);
			BodysearchLabel.SetActive(currentPursuitLevel == PlayerCrimeData.EPursuitLevel.None && Player.Local.CrimeData.BodySearchPending);
			if (currentPursuitLevel != 0)
			{
				b = 0.6f;
				if (Player.Local.CrimeData.TimeSinceSighted < 3f)
				{
					b = 1f;
					animateText = true;
					if (routine == null)
					{
						routine = StartCoroutine(Routine());
					}
				}
			}
			float fillAmount = 1f - Mathf.Clamp01((Player.Local.CrimeData.TimeSinceSighted - 3f) / Player.Local.CrimeData.GetSearchTime());
			InvestigatingMask.fillAmount = fillAmount;
			UnderArrestMask.fillAmount = fillAmount;
			WantedMask.fillAmount = fillAmount;
			WantedDeadMask.fillAmount = fillAmount;
			CrimeStatusGroup.alpha = Mathf.Lerp(CrimeStatusGroup.alpha, b, Time.deltaTime);
		}

		private void OnDestroy()
		{
			if (routine != null && Singleton<CoroutineService>.InstanceExists)
			{
				Singleton<CoroutineService>.Instance.StopCoroutine(Routine());
			}
		}

		private IEnumerator Routine()
		{
			CrimeStatusContainer.localScale = Vector3.one * 0.75f;
			while (true)
			{
				if (!animateText)
				{
					yield return new WaitForEndOfFrame();
					continue;
				}
				float lerpTime = 1.5f;
				float t = 0f;
				while (t < lerpTime)
				{
					t += Time.deltaTime;
					CrimeStatusContainer.localScale = Vector3.one * Mathf.Lerp(0.75f, 1f, (Mathf.Sin(t / lerpTime * 2f * MathF.PI) + 1f) / 2f);
					yield return new WaitForEndOfFrame();
				}
			}
		}
	}
}
