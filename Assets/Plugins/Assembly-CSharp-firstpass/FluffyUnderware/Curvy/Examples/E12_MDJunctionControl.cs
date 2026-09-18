namespace FluffyUnderware.Curvy.Examples
{
	public class E12_MDJunctionControl : CurvyMetadataBase
	{
		public bool UseJunction;

		public void Toggle()
		{
			UseJunction = !UseJunction;
		}
	}
}
