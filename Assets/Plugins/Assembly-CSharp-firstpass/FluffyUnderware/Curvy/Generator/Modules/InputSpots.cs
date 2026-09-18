using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Input/Spots", ModuleName = "Input Spots", Description = "Defines an array of placement spots")]
	[HelpURL("https://curvyeditor.com/doclink/cginputspots")]
	public class InputSpots : CGModule
	{
		[HideInInspector]
		[OutputSlotInfo(typeof(CGSpots))]
		public CGModuleOutputSlot OutSpots = new CGModuleOutputSlot();

		[ArrayEx]
		[SerializeField]
		private List<CGSpot> m_Spots = new List<CGSpot>();

		public List<CGSpot> Spots
		{
			get
			{
				return m_Spots;
			}
			set
			{
				if (m_Spots != value)
				{
					m_Spots = value;
					base.Dirty = true;
				}
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 250f;
		}

		public override void Reset()
		{
			base.Reset();
			Spots.Clear();
		}

		public override void Refresh()
		{
			base.Refresh();
			if (OutSpots.IsLinked)
			{
				OutSpots.SetDataToElement(new CGSpots(Spots));
			}
		}
	}
}
