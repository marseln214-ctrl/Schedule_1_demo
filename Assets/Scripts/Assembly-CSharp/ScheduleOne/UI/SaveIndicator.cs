using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class SaveIndicator : MonoBehaviour
	{
		public Canvas Canvas;

		public RectTransform Icon;

		public Animation Anim;

		public void Awake()
		{
			Canvas.enabled = false;
		}

		public void Start()
		{
			Singleton<SaveManager>.Instance.onSaveStart.AddListener(Display);
		}

		public void OnDestroy()
		{
			if (Singleton<SaveManager>.InstanceExists)
			{
				Singleton<SaveManager>.Instance.onSaveStart.RemoveListener(Display);
			}
		}

		public void Display()
		{
			StartCoroutine(Routine());
			IEnumerator Routine()
			{
				Canvas.enabled = true;
				Icon.gameObject.SetActive(value: true);
				while (Singleton<SaveManager>.Instance.IsSaving)
				{
					Icon.Rotate(Vector3.forward, 360f * Time.unscaledDeltaTime);
					yield return new WaitForEndOfFrame();
				}
				Icon.gameObject.SetActive(value: false);
				Anim.Play();
				yield return new WaitForSecondsRealtime(5f);
				Canvas.enabled = false;
			}
		}
	}
}
