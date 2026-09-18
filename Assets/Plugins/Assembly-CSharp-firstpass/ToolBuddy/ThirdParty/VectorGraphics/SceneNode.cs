using System.Collections.Generic;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	public class SceneNode
	{
		private Matrix2D m_Transform = Matrix2D.identity;

		public List<SceneNode> Children { get; set; }

		public List<Shape> Shapes { get; set; }

		public Matrix2D Transform
		{
			get
			{
				return m_Transform;
			}
			set
			{
				m_Transform = value;
			}
		}

		public SceneNode Clipper { get; set; }
	}
}
