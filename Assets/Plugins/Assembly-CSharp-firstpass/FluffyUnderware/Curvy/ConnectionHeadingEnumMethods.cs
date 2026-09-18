namespace FluffyUnderware.Curvy
{
	public static class ConnectionHeadingEnumMethods
	{
		public static ConnectionHeadingEnum ResolveAuto(this ConnectionHeadingEnum heading, CurvySplineSegment followUp)
		{
			if (heading == ConnectionHeadingEnum.Auto)
			{
				heading = (CurvySplineSegment.CanFollowUpHeadToEnd(followUp) ? ConnectionHeadingEnum.Plus : (CurvySplineSegment.CanFollowUpHeadToStart(followUp) ? ConnectionHeadingEnum.Minus : ConnectionHeadingEnum.Sharp));
			}
			return heading;
		}
	}
}
