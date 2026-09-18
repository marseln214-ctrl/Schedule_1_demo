namespace ToolBuddy.ThirdParty.VectorGraphics
{
	public class Shape
	{
		private Matrix2D m_FillTransform = Matrix2D.identity;

		public BezierContour[] Contours { get; set; }

		public IFill Fill { get; set; }

		public Matrix2D FillTransform
		{
			get
			{
				return m_FillTransform;
			}
			set
			{
				m_FillTransform = value;
			}
		}

		public PathProperties PathProps { get; set; }

		public bool IsConvex { get; set; }
	}
}
