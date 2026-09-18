using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class WorldspaceDialogueRenderer : MonoBehaviour
	{
		private const float FadeDist = 2f;

		[Header("Settings")]
		public float MaxRange = 10f;

		public float BaseScale = 0.01f;

		public AnimationCurve Scale;

		public Vector2 Padding;

		public Vector3 WorldSpaceOffset = Vector3.zero;

		[Header("References")]
		public Canvas Canvas;

		public CanvasGroup CanvasGroup;

		public RectTransform Background;

		public TextMeshProUGUI Text;

		public Animation Anim;

		private Vector3 localOffset = Vector3.zero;

		private float CurrentOpacity;

		private Coroutine hideCoroutine;

		public string ShownText { get; protected set; } = string.Empty;


		private void Awake()
		{
			localOffset = base.transform.localPosition;
			SetOpacity(0f);
		}

		private void FixedUpdate()
		{
			if (ShownText == string.Empty)
			{
				if (CurrentOpacity != 0f)
				{
					SetOpacity(0f);
				}
				return;
			}
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				if (CurrentOpacity != 0f)
				{
					SetOpacity(0f);
				}
				return;
			}
			if (Vector3.Distance(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position) > MaxRange)
			{
				if (CurrentOpacity != 0f)
				{
					SetOpacity(0f);
				}
				return;
			}
			float num = Vector3.Distance(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position);
			if (num < MaxRange - 2f)
			{
				SetOpacity(1f);
			}
			else
			{
				SetOpacity(1f - (num - (MaxRange - 2f)) / 2f);
			}
			Text.text = ShownText;
		}

		private void LateUpdate()
		{
			if (CurrentOpacity > 0f)
			{
				UpdatePosition();
			}
		}

		private void UpdatePosition()
		{
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				float num = BaseScale * Scale.Evaluate(Vector3.Distance(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position) / MaxRange);
				Canvas.transform.localScale = new Vector3(num, num, num);
				Background.sizeDelta = new Vector2(Text.renderedWidth + Padding.x, Text.renderedHeight + Padding.y);
				Canvas.transform.LookAt(PlayerSingleton<PlayerCamera>.Instance.transform.position);
				base.transform.localPosition = localOffset;
				base.transform.position = base.transform.position + WorldSpaceOffset;
			}
		}

		public void ShowText(string text, float duration = 0f)
		{
			if (hideCoroutine != null)
			{
				StopCoroutine(hideCoroutine);
				hideCoroutine = null;
			}
			ShownText = text;
			if (ShownText != string.Empty)
			{
				Text.text = ShownText;
				Text.ForceMeshUpdate();
				UpdatePosition();
			}
			if (!Canvas.enabled && Anim != null)
			{
				Anim.Play();
			}
			if (duration > 0f)
			{
				hideCoroutine = Singleton<CoroutineService>.Instance.StartCoroutine(Wait(duration));
			}
			IEnumerator Wait(float dur)
			{
				yield return new WaitForSeconds(dur);
				ShownText = string.Empty;
				hideCoroutine = null;
			}
		}

		public void HideText()
		{
			if (hideCoroutine != null)
			{
				StopCoroutine(hideCoroutine);
				hideCoroutine = null;
			}
			ShownText = string.Empty;
		}

		private void SetOpacity(float op)
		{
			CurrentOpacity = op;
			CanvasGroup.alpha = op;
			Canvas.enabled = op > 0f;
		}
	}
}
