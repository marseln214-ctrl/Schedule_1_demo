using UnityEngine;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	public class GradientFill : IFill
	{
		private float m_Opacity = 1f;

		public GradientFillType Type { get; set; }

		public GradientStop[] Stops { get; set; }

		public FillMode Mode { get; set; }

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

		public AddressMode Addressing { get; set; }

		public Vector2 RadialFocus { get; set; }
	}
}
