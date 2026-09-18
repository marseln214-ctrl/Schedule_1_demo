using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[HelpURL("https://curvyeditor.com/doclink/dtinspectornode")]
	public class InspectorNote : DTVersionedMonoBehaviour
	{
		[TextArea(5, 20)]
		[SerializeField]
		private string m_Note;
	}
}
