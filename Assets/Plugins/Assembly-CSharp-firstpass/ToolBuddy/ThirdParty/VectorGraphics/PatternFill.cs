using UnityEngine;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	public class PatternFill : IFill
	{
		private float m_Opacity = 1f;

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

		public SceneNode Pattern { get; set; }

		public Rect Rect { get; set; }
	}
}
