using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.TV
{
	public class SnakeTile : MonoBehaviour
	{
		public enum TileType
		{
			Empty = 0,
			Snake = 1,
			Food = 2
		}

		public Vector2 Position = Vector2.zero;

		public Color SnakeColor;

		public Color FoodColor;

		public RectTransform RectTransform;

		public Image Image;

		public TileType Type { get; private set; }

		public void SetType(TileType type, int index = 0)
		{
			Type = type;
			switch (Type)
			{
			case TileType.Empty:
				base.gameObject.SetActive(value: false);
				break;
			case TileType.Snake:
				Image.color = SnakeColor;
				if (index > 0)
				{
					float a = 1f - 0.8f * Mathf.Sqrt((float)index / 240f);
					Image.color = new Color(SnakeColor.r, SnakeColor.g, SnakeColor.b, a);
				}
				base.gameObject.SetActive(value: true);
				break;
			case TileType.Food:
				Image.color = FoodColor;
				base.gameObject.SetActive(value: true);
				break;
			}
		}

		public void SetPosition(Vector2 position, float tileSize)
		{
			Position = position;
			RectTransform.anchoredPosition = new Vector2((0.5f + position.x) * tileSize, (0.5f + position.y) * tileSize);
			base.gameObject.name = $"Tile {position.x}, {position.y}";
			RectTransform.sizeDelta = new Vector2(tileSize, tileSize);
		}
	}
}
