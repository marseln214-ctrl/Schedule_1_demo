using UnityEngine;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	public class TextureFill : IFill
	{
		private float m_Opacity = 1f;

		public Texture2D Texture { get; set; }

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
	}
}
