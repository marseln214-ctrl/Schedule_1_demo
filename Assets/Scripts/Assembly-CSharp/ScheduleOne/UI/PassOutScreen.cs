using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class PassOutScreen : Singleton<PassOutScreen>
	{
		public const float CASH_LOSS_MIN = 50f;

		public const float CASH_LOSS_MAX = 500f;

		[Header("References")]
		public Canvas Canvas;

		public CanvasGroup Group;

		public Transform RecoveryPointsContainer;

		public TextMeshProUGUI MainLabel;

		public TextMeshProUGUI ContextLabel;

		public Animation Anim;

		private float cashLoss;

		public bool isOpen { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			Canvas.enabled = false;
			Group.alpha = 0f;
			Group.interactable = false;
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
				float fadeTime = 1f;
				for (float l = 0f; l < fadeTime; l += Time.deltaTime)
				{
					Group.alpha = Mathf.Lerp(1f, 0f, l / fadeTime);
					yield return new WaitForEndOfFrame();
				}
				MainLabel.gameObject.SetActive(value: false);
				Player.Local.SendPassOutRecovery();
				Player.Local.Health.RecoverHealth(100f);
				Transform child = RecoveryPointsContainer.GetChild(Random.Range(0, RecoveryPointsContainer.childCount));
				PlayerSingleton<PlayerMovement>.Instance.Teleport(child.position);
				Player.Local.transform.forward = child.forward;
				yield return new WaitForSeconds(0.5f);
				bool fadeBlur = false;
				if (Player.Local.IsArrested)
				{
					Singleton<ArrestNoticeScreen>.Instance.RecordCrimes();
					Player.Local.Free();
					Singleton<ArrestNoticeScreen>.Instance.Open();
					PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
					yield return new WaitForSeconds(1f);
				}
				else
				{
					ContextLabel.text = "You awaken in a new location, unsure of how you got there.";
					if (cashLoss > 0f)
					{
						NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - cashLoss);
						ContextLabel.text = ContextLabel.text + "\n\n<color=#54E717>" + MoneyManager.FormatAmount(cashLoss) + "</color> is missing from your wallet.";
					}
					ContextLabel.gameObject.SetActive(value: true);
					for (float l = 0f; l < fadeTime; l += Time.deltaTime)
					{
						Group.alpha = Mathf.Lerp(0f, 1f, l / fadeTime);
						yield return new WaitForEndOfFrame();
					}
					fadeBlur = true;
					yield return new WaitForSeconds(4f);
					for (float l = 0f; l < fadeTime; l += Time.deltaTime)
					{
						Group.alpha = Mathf.Lerp(1f, 0f, l / fadeTime);
						yield return new WaitForEndOfFrame();
					}
					Group.alpha = 0f;
				}
				yield return new WaitForSeconds(1f);
				float lerpTime = 2f;
				for (float l = 0f; l < lerpTime; l += Time.deltaTime)
				{
					Singleton<EyelidOverlay>.Instance.SetOpen(Mathf.Lerp(0f, 1f, l / lerpTime));
					if (fadeBlur)
					{
						Singleton<PostProcessingManager>.Instance.SetBlur(1f - l / lerpTime);
					}
					yield return new WaitForEndOfFrame();
				}
				Singleton<EyelidOverlay>.Instance.SetOpen(1f);
				if (fadeBlur)
				{
					Singleton<PostProcessingManager>.Instance.SetBlur(0f);
				}
				Close();
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
				Singleton<EyelidOverlay>.Instance.Canvas.sortingOrder = 5;
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
				cashLoss = Mathf.Min(Random.Range(50f, 500f), NetworkSingleton<MoneyManager>.Instance.cashBalance);
				StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				MainLabel.gameObject.SetActive(value: true);
				ContextLabel.gameObject.SetActive(value: false);
				yield return new WaitForSeconds(0.5f);
				Singleton<EyelidOverlay>.Instance.AutoUpdate = false;
				float lerpTime = 2f;
				float startOpenness = Singleton<EyelidOverlay>.Instance.CurrentOpen;
				float endOpenness = 0f;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					Singleton<EyelidOverlay>.Instance.SetOpen(Mathf.Lerp(startOpenness, endOpenness, i / lerpTime));
					Singleton<PostProcessingManager>.Instance.SetBlur(i / lerpTime);
					yield return new WaitForEndOfFrame();
				}
				Singleton<EyelidOverlay>.Instance.SetOpen(0f);
				Singleton<PostProcessingManager>.Instance.SetBlur(1f);
				yield return new WaitForSeconds(0.5f);
				Anim.Play();
				Canvas.enabled = true;
				yield return new WaitForSeconds(3f);
				Continue();
			}
		}

		public void Close()
		{
			isOpen = false;
			Canvas.enabled = false;
			Singleton<EyelidOverlay>.Instance.Canvas.sortingOrder = -1;
			Singleton<EyelidOverlay>.Instance.AutoUpdate = true;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			if (!Singleton<ArrestNoticeScreen>.Instance.isOpen)
			{
				Player.Activate();
			}
		}
	}
}
