using ScheduleOne;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
	private Image mainImage;

	public RectTransform pickerIcon;

	public Image colorPreview;

	private bool _activeCursor;

	public Vector2 offset;

	public UIControllerDEMO UIControllerDEMO;

	public Canvas Canvas;

	private Color _findColor;

	public Vector2 realSize;

	private void Awake()
	{
		mainImage = GetComponent<Image>();
	}

	public void CursorEnter()
	{
		if (!pickerIcon.gameObject.activeSelf)
		{
			pickerIcon.gameObject.SetActive(value: true);
			_activeCursor = true;
		}
	}

	private void Update()
	{
		if (_activeCursor)
		{
			CursorMove();
		}
	}

	public void CursorMove()
	{
		pickerIcon.position = GameInput.MousePosition;
		realSize = mainImage.rectTransform.rect.size * Canvas.scaleFactor;
		offset = mainImage.rectTransform.position - GameInput.MousePosition;
		offset = new Vector2(256f / realSize.x * offset.x, 256f / realSize.y * offset.y);
		_findColor = mainImage.sprite.texture.GetPixel((int)(0f - offset.x), 256 - (int)offset.y);
		if (_findColor.a == 1f)
		{
			colorPreview.color = _findColor;
		}
	}

	public void CursorPickSkin()
	{
		if (_findColor.a == 1f)
		{
			UIControllerDEMO.SetNewSkinColor(_findColor);
		}
	}

	public void CursorPickEye()
	{
		if (_findColor.a == 1f)
		{
			UIControllerDEMO.SetNewEyeColor(_findColor);
		}
	}

	public void CursorPickHair()
	{
		if (_findColor.a == 1f)
		{
			UIControllerDEMO.SetNewHairColor(_findColor);
		}
	}

	public void CursorPickUnderpants()
	{
		if (_findColor.a == 1f)
		{
			UIControllerDEMO.SetNewUnderpantsColor(_findColor);
		}
	}

	public void CursorExit()
	{
		if (pickerIcon.gameObject.activeSelf)
		{
			pickerIcon.gameObject.SetActive(value: false);
			_activeCursor = false;
		}
	}
}
