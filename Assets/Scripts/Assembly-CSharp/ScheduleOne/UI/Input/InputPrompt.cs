using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ScheduleOne.UI.Input
{
	[ExecuteInEditMode]
	public class InputPrompt : MonoBehaviour
	{
		public enum EInputPromptAlignment
		{
			Left = 0,
			Middle = 1,
			Right = 2
		}

		public static float Spacing = 10f;

		[Header("Settings")]
		public List<InputActionReference> Actions = new List<InputActionReference>();

		public string Label;

		public EInputPromptAlignment Alignment;

		[Header("References")]
		public RectTransform Container;

		public RectTransform ImagesContainer;

		public TextMeshProUGUI LabelComponent;

		public RectTransform Shade;

		[Header("Settings")]
		public bool OverridePromptImageColor;

		public Color PromptImageColor = Color.white;

		[SerializeField]
		private List<PromptImage> promptImages = new List<PromptImage>();

		private List<InputActionReference> displayedActions = new List<InputActionReference>();

		private EInputPromptAlignment AppliedAlignment;

		private InputPromptsManager manager
		{
			get
			{
				if (!Singleton<InputPromptsManager>.InstanceExists)
				{
					return GameObject.Find("@InputPromptsManager").GetComponent<InputPromptsManager>();
				}
				return Singleton<InputPromptsManager>.Instance;
			}
		}

		private void OnEnable()
		{
			RefreshPromptImages();
			Container.gameObject.SetActive(value: true);
		}

		private void OnDisable()
		{
			Container.gameObject.SetActive(value: false);
		}

		private void Update()
		{
		}

		private void RefreshPromptImages()
		{
			AppliedAlignment = Alignment;
			displayedActions.Clear();
			displayedActions.AddRange(Actions);
			int childCount = ImagesContainer.childCount;
			Transform[] array = new Transform[childCount];
			for (int i = 0; i < childCount; i++)
			{
				array[i] = ImagesContainer.GetChild(i);
			}
			for (int j = 0; j < childCount; j++)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(array[j].gameObject);
				}
				else
				{
					Object.DestroyImmediate(array[j].gameObject);
				}
			}
			promptImages.Clear();
			float num = 0f;
			for (int k = 0; k < Actions.Count; k++)
			{
				Actions[k].action.GetBindingDisplayString(0, out var _, out var controlPath);
				PromptImage promptImage = manager.GetPromptImage(controlPath, ImagesContainer);
				if (promptImage == null)
				{
					continue;
				}
				num += promptImage.Width;
				Image[] componentsInChildren = promptImage.transform.GetComponentsInChildren<Image>();
				foreach (Image image in componentsInChildren)
				{
					if (OverridePromptImageColor)
					{
						image.color = PromptImageColor;
					}
				}
				promptImages.Add(promptImage);
			}
			num += Spacing * (float)Actions.Count;
			LabelComponent.text = Label;
			LabelComponent.ForceMeshUpdate();
			num += LabelComponent.preferredWidth;
			float num2 = 0f;
			if (Alignment == EInputPromptAlignment.Left)
			{
				num2 = 0f - Spacing;
			}
			else if (Alignment == EInputPromptAlignment.Middle)
			{
				num2 = (0f - num) / 2f;
			}
			else if (Alignment == EInputPromptAlignment.Right)
			{
				num2 = Spacing;
			}
			float num3 = 1f;
			if (Alignment == EInputPromptAlignment.Left)
			{
				LabelComponent.alignment = TextAlignmentOptions.CaplineRight;
				num3 = -1f;
			}
			else
			{
				LabelComponent.alignment = TextAlignmentOptions.CaplineLeft;
			}
			float num4 = 0f;
			for (int m = 0; m < promptImages.Count; m++)
			{
				promptImages[m].GetComponent<RectTransform>().anchoredPosition = new Vector2(num2 + num4 * num3 + promptImages[m].Width * 0.5f * num3, 0f);
				num4 += promptImages[m].Width + Spacing;
			}
			LabelComponent.GetComponent<RectTransform>().anchoredPosition = new Vector2(num2 + num4 * num3 + LabelComponent.GetComponent<RectTransform>().sizeDelta.x * 0.5f * num3, 0f);
			UpdateShade();
		}

		public void SetLabel(string label)
		{
			Label = label;
			LabelComponent.text = Label;
			UpdateShade();
		}

		private void UpdateShade()
		{
			float num = LabelComponent.preferredWidth + 90f;
			Shade.sizeDelta = new Vector2(num, Shade.sizeDelta.y);
			if (Alignment == EInputPromptAlignment.Left)
			{
				Shade.anchoredPosition = new Vector2((0f - num) / 2f, 0f);
			}
			else if (Alignment == EInputPromptAlignment.Middle)
			{
				Shade.anchoredPosition = new Vector2(0f, 0f);
			}
			else if (Alignment == EInputPromptAlignment.Right)
			{
				Shade.anchoredPosition = new Vector2(num / 2f, 0f);
			}
		}
	}
}
