using JetBrains.Annotations;

namespace FluffyUnderware.Curvy.Generator
{
	public interface IOnRequestProcessing
	{
		[NotNull]
		[ItemNotNull]
		CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests);
	}
}
