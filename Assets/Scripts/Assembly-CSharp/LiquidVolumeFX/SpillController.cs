using System.Collections;
using UnityEngine;

namespace LiquidVolumeFX
{
	public class SpillController : MonoBehaviour
	{
		public GameObject spill;

		private LiquidVolume lv;

		private GameObject[] dropTemplates;

		private const int DROP_TEMPLATES_COUNT = 10;

		private void Start()
		{
			lv = GetComponent<LiquidVolume>();
			dropTemplates = new GameObject[10];
			for (int i = 0; i < 10; i++)
			{
				GameObject gameObject = Object.Instantiate(spill);
				gameObject.transform.localScale *= Random.Range(0.45f, 0.65f);
				gameObject.GetComponent<Renderer>().material.color = Color.Lerp(lv.liquidColor1, lv.liquidColor2, Random.value);
				gameObject.SetActive(value: false);
				dropTemplates[i] = gameObject;
			}
		}

		private void Update()
		{
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				base.transform.Rotate(Vector3.forward * Time.deltaTime * 10f);
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				base.transform.Rotate(-Vector3.forward * Time.deltaTime * 10f);
			}
		}

		private void FixedUpdate()
		{
			if (lv.GetSpillPoint(out var spillPosition, out var spillAmount))
			{
				for (int i = 0; i < 15; i++)
				{
					int num = Random.Range(0, 10);
					GameObject gameObject = Object.Instantiate(dropTemplates[num]);
					gameObject.SetActive(value: true);
					Rigidbody component = gameObject.GetComponent<Rigidbody>();
					component.transform.position = spillPosition + Random.insideUnitSphere * 0.01f;
					component.AddForce(new Vector3(Random.value - 0.5f, Random.value * 0.1f - 0.2f, Random.value - 0.5f));
					StartCoroutine(DestroySpill(gameObject));
				}
				lv.level -= spillAmount / 10f + 0.001f;
			}
		}

		private IEnumerator DestroySpill(GameObject spill)
		{
			yield return new WaitForSeconds(1f);
			Object.Destroy(spill);
		}
	}
}
