using UnityEngine;

namespace ScheduleOne.Tiles
{
	public class TileAppearance : MonoBehaviour
	{
		[Header("References")]
		[SerializeField]
		protected MeshRenderer tileMesh;

		[Header("Settings")]
		[SerializeField]
		protected Material mat_White;

		[SerializeField]
		protected Material mat_Blue;

		[SerializeField]
		protected Material mat_Red;

		public void Awake()
		{
			SetVisible(visible: false);
		}

		public void SetVisible(bool visible)
		{
			tileMesh.enabled = visible;
		}

		public void SetColor(ETileColor col)
		{
			Material material = mat_White;
			switch (col)
			{
			case ETileColor.White:
				material = mat_White;
				break;
			case ETileColor.Blue:
				material = mat_Blue;
				break;
			case ETileColor.Red:
				material = mat_Red;
				break;
			default:
				Console.LogWarning("GridUnitAppearance: enum type not accounted for.");
				break;
			}
			tileMesh.material = material;
		}
	}
}
