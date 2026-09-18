namespace FluffyUnderware.DevTools
{
	public class NoSectionAttribute : SectionAttribute
	{
		public NoSectionAttribute()
			: base("")
		{
			base.TypeSort = 10;
		}
	}
}
