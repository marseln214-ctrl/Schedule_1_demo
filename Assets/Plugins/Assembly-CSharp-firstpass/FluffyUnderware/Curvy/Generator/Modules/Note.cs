using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Note", ModuleName = "Note", Description = "Creates a note")]
	[HelpURL("https://curvyeditor.com/doclink/cgnote")]
	public class Note : CGModule, INoProcessing
	{
		[SerializeField]
		[TextArea(3, 10)]
		private string m_Note;

		public string NoteText
		{
			get
			{
				return m_Note;
			}
			set
			{
				m_Note = value;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 250f;
			Properties.LabelWidth = 50f;
		}

		public override void Reset()
		{
			base.Reset();
			m_Note = null;
		}
	}
}
