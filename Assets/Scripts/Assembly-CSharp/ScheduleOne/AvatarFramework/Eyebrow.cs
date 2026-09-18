using UnityEngine;

namespace ScheduleOne.AvatarFramework
{
	public class Eyebrow : MonoBehaviour
	{
		public enum ESide
		{
			Right = 0,
			Left = 1
		}

		private static Vector3 eyebrowDefaultScale = new Vector3(0.28f, 0.28f, 0.28f);

		[SerializeField]
		protected ESide Side;

		[SerializeField]
		protected Transform Model;

		[SerializeField]
		protected MeshRenderer Rend;

		[Header("Eyebrow Data - Readonly")]
		[SerializeField]
		private Color col;

		[SerializeField]
		private float scale = 1f;

		[SerializeField]
		private float thickness = 1f;

		[SerializeField]
		private float restingAngle;

		public void SetScale(float _scale)
		{
			scale = _scale;
			Model.localScale = new Vector3(eyebrowDefaultScale.x, eyebrowDefaultScale.y, eyebrowDefaultScale.z * thickness) * scale;
		}

		public void SetThickness(float thickness)
		{
			this.thickness = thickness;
			SetScale(scale);
		}

		public void SetRestingAngle(float _angle)
		{
			restingAngle = _angle;
			base.transform.localRotation = Quaternion.Euler(0f, 0f, restingAngle * ((Side == ESide.Left) ? (-1f) : 1f));
		}

		public void SetColor(Color _col)
		{
			col = _col;
			Rend.material.color = col;
		}
	}
}
