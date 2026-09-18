namespace ToolBuddy.ThirdParty.VectorGraphics
{
	public struct PathProperties
	{
		public Stroke Stroke { get; set; }

		public PathEnding Head { get; set; }

		public PathEnding Tail { get; set; }

		public PathCorner Corners { get; set; }
	}
}
