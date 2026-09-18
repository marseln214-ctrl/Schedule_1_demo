using UnityEngine;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	public class SolidFill : IFill
	{
		private float m_Opacity = 1f;

		public Color Color { get; set; }

		public float Opacity
		{
			get
			{
				return m_Opacity;
			}
			set
			{
				m_Opacity = value;
			}
		}

		public FillMode Mode { get; set; }
	}
}
