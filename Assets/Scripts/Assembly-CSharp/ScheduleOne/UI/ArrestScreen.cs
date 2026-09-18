using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class ArrestScreen : Singleton<ArrestScreen>
	{
		[Header("References")]
		public Canvas canvas;

		public CanvasGroup group;

		public AudioSourceController Sound;

		public Animation Anim;

		public bool isOpen { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			canvas.enabled = false;
			group.alpha = 0f;
			group.interactable = false;
		}

		private void Continue()
		{
			if (isOpen)
			{
				isOpen = false;
				StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				Singleton<BlackOverlay>.Instance.Open();
				yield return new WaitForSeconds(0.5f);
				Close();
				Singleton<ArrestNoticeScreen>.Instance.Open();
				Player.Local.Free();
				Player.Local.Health.SetHealth(100f);
				yield return new WaitForSeconds(2f);
				Singleton<BlackOverlay>.Instance.Close();
			}
		}

		private void LoadSaveClicked()
		{
			Close();
		}

		public void Open()
		{
			if (!isOpen)
			{
				isOpen = true;
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
				Sound.Play();
				StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(0.5f);
				Anim.Play();
				canvas.enabled = true;
				float lerpTime = 0.75f;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					Singleton<PostProcessingManager>.Instance.SetBlur(i / lerpTime);
					yield return new WaitForEndOfFrame();
				}
				Singleton<PostProcessingManager>.Instance.SetBlur(1f);
				yield return new WaitForSeconds(3f);
				Continue();
			}
		}

		public void Close()
		{
			isOpen = false;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			Singleton<PostProcessingManager>.Instance.SetBlur(0f);
			canvas.enabled = false;
		}
	}
}
