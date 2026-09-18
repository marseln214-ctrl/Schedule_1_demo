using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Input/Meshes", ModuleName = "Input Meshes", Description = "Create VMeshes")]
	[HelpURL("https://curvyeditor.com/doclink/cginputmesh")]
	public class InputMesh : CGModule, IExternalInput
	{
		[HideInInspector]
		[OutputSlotInfo(typeof(CGVMesh), Array = true)]
		public CGModuleOutputSlot OutVMesh = new CGModuleOutputSlot();

		[SerializeField]
		[ArrayEx]
		private List<CGMeshProperties> m_Meshes = new List<CGMeshProperties>(new CGMeshProperties[1]
		{
			new CGMeshProperties()
		});

		public List<CGMeshProperties> Meshes => m_Meshes;

		public bool SupportsIPE => false;

		protected override void OnValidate()
		{
			base.OnValidate();
			foreach (CGMeshProperties mesh in Meshes)
			{
				_ = mesh;
			}
		}

		public override void Reset()
		{
			base.Reset();
			Meshes.Clear();
		}

		public override void Refresh()
		{
			base.Refresh();
			if (OutVMesh.IsLinked)
			{
				OutVMesh.SetDataToCollection((from p in Meshes
					where p.Mesh != null
					select new CGVMesh(p)).ToArray());
			}
		}

		public override void OnTemplateCreated()
		{
			base.OnTemplateCreated();
			Meshes.Clear();
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void WarnAboutInvalidInputs()
		{
			if (Meshes.Exists((CGMeshProperties m) => m.Mesh == null))
			{
				UIMessages.Add("Missing Mesh input");
			}
			(from p in Meshes
				select p.Mesh into m
				where m != null && !m.isReadable
				select m).ForEach(delegate(Mesh m)
			{
				UIMessages.Add("Input mesh '" + m.name + "' is not readable. Please set the 'Read/Write Enabled' parameter to true in the mesh model import settings");
			});
		}
	}
}
