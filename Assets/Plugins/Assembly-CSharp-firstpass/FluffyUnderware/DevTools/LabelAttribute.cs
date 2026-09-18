namespace FluffyUnderware.DevTools
{
	public class LabelAttribute : DTPropertyAttribute
	{
		public LabelAttribute()
		{
		}

		public LabelAttribute(string label, string tooltip = "")
			: base(label, tooltip)
		{
		}
	}
}
