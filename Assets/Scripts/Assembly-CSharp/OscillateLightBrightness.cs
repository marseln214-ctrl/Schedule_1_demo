using UnityEngine;

public class OscillateLightBrightness : MonoBehaviour
{
	private Light lightComponent;

	[SerializeField]
	[Range(0f, 10f)]
	private float lower;

	[SerializeField]
	[Range(0f, 10f)]
	private float upper;

	private void Start()
	{
		lightComponent = GetComponent<Light>();
	}

	private void Update()
	{
		lightComponent.intensity = Random.Range(lower, upper);
	}
}
