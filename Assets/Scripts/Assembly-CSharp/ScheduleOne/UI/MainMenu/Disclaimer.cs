using System.Collections;
using UnityEngine;

namespace ScheduleOne.UI.MainMenu
{
	public class Disclaimer : MonoBehaviour
	{
		public static bool Shown;

		public CanvasGroup Group;

		public CanvasGroup TextGroup;

		public float Duration = 3.8f;

		private void Awake()
		{
			if (Application.isEditor || Shown)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			Shown = true;
			Group.alpha = 1f;
			TextGroup.alpha = 0f;
			Fade();
		}

		private void Fade()
		{
			StartCoroutine(Fade());
			IEnumerator Fade()
			{
				while (TextGroup.alpha < 1f)
				{
					TextGroup.alpha += Time.deltaTime * 2f;
					yield return null;
				}
				for (float i = 0f; i < Duration; i += Time.deltaTime)
				{
					if (UnityEngine.Input.GetKey(KeyCode.Space))
					{
						break;
					}
					yield return new WaitForEndOfFrame();
				}
				while (Group.alpha > 0f)
				{
					Group.alpha -= Time.deltaTime * 2f;
					yield return null;
				}
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
