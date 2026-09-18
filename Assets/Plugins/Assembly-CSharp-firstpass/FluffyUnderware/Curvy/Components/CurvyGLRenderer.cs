using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Components
{
	[HelpURL("https://curvyeditor.com/doclink/curvyglrenderer")]
	[AddComponentMenu("Curvy/Misc/Curvy GL Renderer")]
	public class CurvyGLRenderer : DTVersionedMonoBehaviour
	{
		[ArrayEx(ShowAdd = false, Draggable = false)]
		public List<GLSlotData> Splines = new List<GLSlotData>();

		private readonly Lazy<Material> lineMaterial = new Lazy<Material>(() => new Material(Shader.Find("Hidden/Internal-Colored"))
		{
			hideFlags = HideFlags.HideAndDontSave,
			shader = 
			{
				hideFlags = HideFlags.HideAndDontSave
			}
		});

		protected override void OnValidate()
		{
			base.OnValidate();
			if (base.IsActiveAndEnabled)
			{
				sanitize();
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			sanitize();
		}

		[UsedImplicitly]
		private void OnPostRender()
		{
			sanitize();
			for (int num = Splines.Count - 1; num >= 0; num--)
			{
				Splines[num].Spline.OnRefresh.AddListenerOnce(OnSplineRefresh);
				if (Splines[num].VertexData.Count == 0)
				{
					Splines[num].GetVertexData();
				}
				Splines[num].Render(lineMaterial.Value);
			}
		}

		private void sanitize()
		{
			for (int num = Splines.Count - 1; num >= 0; num--)
			{
				if (Splines[num] == null || Splines[num].Spline == null)
				{
					Splines.RemoveAt(num);
				}
			}
		}

		private void OnSplineRefresh(CurvySplineEventArgs e)
		{
			GLSlotData slot = getSlot((CurvySpline)e.Sender);
			if (slot == null)
			{
				((CurvySpline)e.Sender).OnRefresh.RemoveListener(OnSplineRefresh);
			}
			else
			{
				slot.VertexData.Clear();
			}
		}

		private GLSlotData getSlot(CurvySpline spline)
		{
			if ((bool)spline)
			{
				foreach (GLSlotData spline2 in Splines)
				{
					if (spline2.Spline == spline)
					{
						return spline2;
					}
				}
			}
			return null;
		}

		public void Add(CurvySpline spline)
		{
			if (spline != null)
			{
				Splines.Add(new GLSlotData
				{
					Spline = spline
				});
			}
		}

		[UsedImplicitly]
		[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
		public void Remove(CurvySpline spline)
		{
			for (int num = Splines.Count - 1; num >= 0; num--)
			{
				if (Splines[num].Spline == spline)
				{
					Splines.RemoveAt(num);
				}
			}
		}
	}
}
