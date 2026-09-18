using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.Interaction
{
	public class WorldSpaceLabel
	{
		public string text = string.Empty;

		public Color32 color = Color.white;

		public Vector3 position = Vector3.zero;

		public float scale = 1f;

		public RectTransform rect;

		public Text textComp;

		public bool active = true;

		public WorldSpaceLabel(string _text, Vector3 _position)
		{
			text = _text;
			position = _position;
			rect = Object.Instantiate(Singleton<InteractionManager>.Instance.WSLabelPrefab, Singleton<InteractionManager>.Instance.wsLabelContainer).GetComponent<RectTransform>();
			textComp = rect.GetComponent<Text>();
			Singleton<InteractionManager>.Instance.activeWSlabels.Add(this);
			RefreshDisplay();
		}

		public void RefreshDisplay()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.transform.InverseTransformPoint(position).z < -3f || !active)
			{
				rect.gameObject.SetActive(value: false);
				return;
			}
			textComp.text = text;
			textComp.color = color;
			rect.position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(position);
			float num = Mathf.Clamp(1f / Vector3.Distance(position, PlayerSingleton<PlayerCamera>.Instance.transform.position), 0f, 1f) * Singleton<InteractionManager>.Instance.displaySizeMultiplier * scale;
			rect.localScale = new Vector3(num, num, 1f);
			rect.gameObject.SetActive(value: true);
		}

		public void Destroy()
		{
			Singleton<InteractionManager>.Instance.activeWSlabels.Remove(this);
			rect.gameObject.SetActive(value: false);
			Object.Destroy(rect.gameObject);
		}
	}
}
