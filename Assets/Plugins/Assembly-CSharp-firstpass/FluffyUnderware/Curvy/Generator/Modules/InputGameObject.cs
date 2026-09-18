using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Input/GameObjects", ModuleName = "Input GameObjects", Description = "")]
	[HelpURL("https://curvyeditor.com/doclink/cginputgameobject")]
	public class InputGameObject : CGModule
	{
		[HideInInspector]
		[OutputSlotInfo(typeof(CGGameObject), Array = true)]
		public CGModuleOutputSlot OutGameObject = new CGModuleOutputSlot();

		[ArrayEx]
		[SerializeField]
		private List<CGGameObjectProperties> m_GameObjects = new List<CGGameObjectProperties>();

		public List<CGGameObjectProperties> GameObjects => m_GameObjects;

		public bool SupportsIPE => false;

		public override void Reset()
		{
			base.Reset();
			GameObjects.Clear();
		}

		public override void Refresh()
		{
			base.Refresh();
			if (OutGameObject.IsLinked)
			{
				OutGameObject.SetDataToCollection((from go in GameObjects
					where go.Object != null
					select new CGGameObject(go)).ToArray());
			}
		}

		public override void OnTemplateCreated()
		{
			base.OnTemplateCreated();
			GameObjects.Clear();
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void WarnAboutInvalidInputs()
		{
			if (GameObjects.Exists((CGGameObjectProperties g) => g.Object == null))
			{
				UIMessages.Add("Missing Game Object input");
			}
		}
	}
}
