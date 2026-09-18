using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class Flipboard : MonoBehaviour
	{
		public Sprite[] Sprites;

		public Image Image;

		public float FlipTime = 0.2f;

		public float SpeedMultiplier = 1f;

		private float time;

		private int index;

		public void Update()
		{
			time += Time.deltaTime * SpeedMultiplier;
			if (time >= FlipTime)
			{
				time = 0f;
				index = (index + 1) % Sprites.Length;
				Image.sprite = Sprites[index];
			}
		}

		public void SetIndex(int index)
		{
			this.index = index;
			time = 0f;
			Image.sprite = Sprites[index];
		}
	}
}
