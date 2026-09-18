using HSVPicker;
using UnityEngine;
using UnityEngine.Events;

public class SColorPicker : HSVPicker.ColorPicker
{
	public int PropertyIndex;

	public UnityEvent<Color, int> onValueChangeWithIndex;

	private void Start()
	{
		onValueChanged.AddListener(ValueChanged);
	}

	private void ValueChanged(Color col)
	{
		onValueChangeWithIndex.Invoke(col, PropertyIndex);
	}
}
