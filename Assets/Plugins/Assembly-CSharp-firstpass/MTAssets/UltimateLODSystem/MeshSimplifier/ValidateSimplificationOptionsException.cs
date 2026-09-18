using System;

namespace MTAssets.UltimateLODSystem.MeshSimplifier
{
	public sealed class ValidateSimplificationOptionsException : Exception
	{
		private readonly string propertyName;

		public string PropertyName => propertyName;

		public override string Message => base.Message + Environment.NewLine + "Property name: " + propertyName;

		public ValidateSimplificationOptionsException(string propertyName, string message)
			: base(message)
		{
			this.propertyName = propertyName;
		}

		public ValidateSimplificationOptionsException(string propertyName, string message, Exception innerException)
			: base(message, innerException)
		{
			this.propertyName = propertyName;
		}
	}
}
