using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages
{
	public class MessageBubble : MonoBehaviour
	{
		public enum Alignment
		{
			Center = 0,
			Left = 1,
			Right = 2
		}

		[Header("Settings")]
		public string text = string.Empty;

		public Alignment alignment = Alignment.Left;

		public bool showTriangle;

		public float bubble_MinWidth = 75f;

		public float bubble_MaxWidth = 500f;

		public bool alignTextCenter;

		private string displayedText = string.Empty;

		private bool triangleShown;

		[Header("References")]
		public RectTransform container;

		[SerializeField]
		protected Image bubble;

		[SerializeField]
		protected Text content;

		[SerializeField]
		protected Image triangle_Left;

		[SerializeField]
		protected Image triangle_Right;

		public Button button;

		public float height;

		public float spacingAbove;

		public static Color32 backgroundColor_Left = new Color32(225, 225, 225, byte.MaxValue);

		public static Color32 textColor_Left = new Color32(50, 50, 50, byte.MaxValue);

		public static Color32 backgroundColor_Right = new Color32(75, 175, 225, byte.MaxValue);

		public static Color32 textColor_Right = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public static float baseBubbleSpacing = 5f;

		public void SetupBubble(string _text, Alignment _alignment, bool alignCenter = false)
		{
			alignment = _alignment;
			text = _text;
			alignTextCenter = alignCenter;
			ColorBlock colors = button.colors;
			if (alignment == Alignment.Left)
			{
				container.anchorMin = new Vector2(0f, 1f);
				container.anchorMax = new Vector2(0f, 1f);
				colors.normalColor = backgroundColor_Left;
				colors.disabledColor = backgroundColor_Left;
				content.color = textColor_Left;
			}
			else if (alignment == Alignment.Right)
			{
				container.anchorMin = new Vector2(1f, 1f);
				container.anchorMax = new Vector2(1f, 1f);
				colors.normalColor = backgroundColor_Right;
				colors.disabledColor = backgroundColor_Right;
				content.color = textColor_Right;
			}
			else
			{
				container.anchorMin = new Vector2(0.5f, 1f);
				container.anchorMax = new Vector2(0.5f, 1f);
				colors.normalColor = backgroundColor_Right;
				colors.disabledColor = backgroundColor_Right;
				content.color = textColor_Right;
			}
			button.colors = colors;
			RefreshDisplayedText();
			RefreshTriangle();
		}

		protected virtual void Update()
		{
			if (text != displayedText)
			{
				RefreshDisplayedText();
			}
			if (showTriangle != triangleShown)
			{
				RefreshTriangle();
			}
		}

		protected virtual void RefreshDisplayedText()
		{
			displayedText = text;
			content.text = displayedText;
			if (alignTextCenter)
			{
				content.alignment = TextAnchor.UpperCenter;
			}
			else
			{
				content.alignment = TextAnchor.UpperLeft;
			}
			RectTransform component = GetComponent<RectTransform>();
			component.sizeDelta = new Vector2(Mathf.Clamp(content.preferredWidth + 50f, bubble_MinWidth, bubble_MaxWidth), 75f);
			height = Mathf.Clamp(content.preferredHeight + 25f, 75f, float.MaxValue);
			component.sizeDelta = new Vector2(component.sizeDelta.x, height);
			float num = 1f;
			if (alignment == Alignment.Right)
			{
				num = -1f;
			}
			else if (alignment == Alignment.Center)
			{
				num = 0f;
			}
			component.anchoredPosition = new Vector2((component.sizeDelta.x / 2f + 25f) * num, (0f - height) / 2f);
		}

		protected virtual void RefreshTriangle()
		{
			triangleShown = showTriangle;
			triangle_Left.gameObject.SetActive(value: false);
			triangle_Right.gameObject.SetActive(value: false);
			if (showTriangle)
			{
				triangle_Left.color = button.colors.normalColor;
				triangle_Right.color = button.colors.normalColor;
				if (alignment == Alignment.Left)
				{
					triangle_Left.gameObject.SetActive(value: true);
				}
				else
				{
					triangle_Right.gameObject.SetActive(value: true);
				}
			}
		}
	}
}
