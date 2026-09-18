namespace FluffyUnderware.Curvy.Generator
{
	public abstract class CGDataRequestParameter
	{
		public static implicit operator bool(CGDataRequestParameter a)
		{
			return a != null;
		}
	}
}
