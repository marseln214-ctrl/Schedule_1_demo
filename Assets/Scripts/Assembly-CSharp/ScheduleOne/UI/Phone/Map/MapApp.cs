using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Map
{
	public class MapApp : App<MapApp>
	{
		public const float KeyMoveSpeed = 1.25f;

		public RectTransform ContentRect;

		public RectTransform PoIContainer;

		public Scrollbar HorizontalScrollbar;

		public Scrollbar VerticalScrollbar;

		public Image BackgroundImage;

		public CanvasGroup LabelGroup;

		[Header("Settings")]
		public Sprite MainMapSprite;

		public Sprite TutorialMapSprite;

		public float LabelScrollMin = 1.2f;

		public float LabelScrollMax = 1.5f;

		[HideInInspector]
		public bool SkipFocusPlayer;

		private Coroutine contentMoveRoutine;

		private bool opened;

		protected override void Start()
		{
			base.Start();
			BackgroundImage.sprite = (NetworkSingleton<GameManager>.Instance.IsTutorial ? TutorialMapSprite : MainMapSprite);
		}

		public override void SetOpen(bool open)
		{
			base.SetOpen(open);
			if (NetworkSingleton<VariableDatabase>.InstanceExists)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("MapAppOpen", open.ToString(), network: false);
			}
			if (open)
			{
				if (!opened && !SkipFocusPlayer)
				{
					opened = true;
					Player.Local.PoI.UpdatePosition();
					FocusPosition(Player.Local.PoI.UI.anchoredPosition);
				}
				if (Player.Local != null && Player.Local.PoI.UI != null)
				{
					Player.Local.PoI.UI.GetComponentInChildren<Animation>().Play();
				}
			}
		}

		protected override void Update()
		{
			base.Update();
			if (base.isOpen)
			{
				GameInput.GetButton(GameInput.ButtonCode.Right);
				GameInput.GetButton(GameInput.ButtonCode.Left);
				GameInput.GetButton(GameInput.ButtonCode.Forward);
				GameInput.GetButton(GameInput.ButtonCode.Backward);
				float x = ContentRect.localScale.x;
				if (x >= LabelScrollMin)
				{
					LabelGroup.alpha = Mathf.Clamp01((x - LabelScrollMin) / (LabelScrollMax - LabelScrollMin));
				}
				else
				{
					LabelGroup.alpha = 0f;
				}
			}
		}

		public void FocusPosition(Vector2 anchoredPosition)
		{
			ContentRect.pivot = new Vector2(0f, 1f);
			float num = 1.3f;
			Vector2 vector = new Vector2((0f - ContentRect.sizeDelta.x) / 2f, ContentRect.sizeDelta.y / 2f);
			vector.x -= anchoredPosition.x;
			vector.y -= anchoredPosition.y;
			ContentRect.localScale = new Vector3(num, num, num);
			ContentRect.anchoredPosition = vector * num;
		}
	}
}
