using System.Collections.Generic;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace FluffyUnderware.Curvy.Controllers
{
	[RequireComponent(typeof(Text))]
	[AddComponentMenu("Curvy/Controllers/UI Text Spline Controller")]
	[HelpURL("https://curvyeditor.com/doclink/uitextsplinecontroller")]
	public class UITextSplineController : SplineController, IMeshModifier
	{
		protected class GlyphPlain : IGlyph
		{
			public Vector3[] V = new Vector3[4];

			public Rect Rect;

			public Vector3 Center => Rect.center;

			public void Load(ref Vector3[] verts, int index)
			{
				V[0] = verts[index];
				V[1] = verts[index + 1];
				V[2] = verts[index + 2];
				V[3] = verts[index + 3];
				calcRect();
			}

			public void calcRect()
			{
				Rect = new Rect(V[0].x, V[2].y, V[2].x - V[0].x, V[0].y - V[2].y);
			}

			public void Save(ref Vector3[] verts, int index)
			{
				verts[index] = V[0];
				verts[index + 1] = V[1];
				verts[index + 2] = V[2];
				verts[index + 3] = V[3];
			}

			public void Transpose(Vector3 v)
			{
				for (int i = 0; i < 4; i++)
				{
					V[i] += v;
				}
			}

			public void Rotate(Quaternion rotation)
			{
				for (int i = 0; i < 4; i++)
				{
					V[i] = V[i].RotateAround(Center, rotation);
				}
			}
		}

		protected class GlyphQuad : IGlyph
		{
			public UIVertex[] V = new UIVertex[4];

			public Rect Rect;

			public Vector3 Center => Rect.center;

			public void Load(List<UIVertex> verts, int index)
			{
				V[0] = verts[index];
				V[1] = verts[index + 1];
				V[2] = verts[index + 2];
				V[3] = verts[index + 3];
				calcRect();
			}

			public void LoadTris(List<UIVertex> verts, int index)
			{
				V[0] = verts[index];
				V[1] = verts[index + 1];
				V[2] = verts[index + 2];
				V[3] = verts[index + 4];
				calcRect();
			}

			public void calcRect()
			{
				Rect = new Rect(V[0].position.x, V[2].position.y, V[2].position.x - V[0].position.x, V[0].position.y - V[2].position.y);
			}

			public void Save(List<UIVertex> verts, int index)
			{
				verts[index] = V[0];
				verts[index + 1] = V[1];
				verts[index + 2] = V[2];
				verts[index + 3] = V[3];
			}

			public void Save(VertexHelper vh)
			{
				vh.AddUIVertexQuad(V);
			}

			public void Transpose(Vector3 v)
			{
				for (int i = 0; i < 4; i++)
				{
					V[i].position += v;
				}
			}

			public void Rotate(Quaternion rotation)
			{
				for (int i = 0; i < 4; i++)
				{
					V[i].position = V[i].position.RotateAround(Center, rotation);
				}
			}
		}

		protected interface IGlyph
		{
			Vector3 Center { get; }

			void Transpose(Vector3 v);

			void Rotate(Quaternion rotation);
		}

		[Section("Orientation", true, false, 100)]
		[Tooltip("If true, the text characters will keep the same orientation regardless of the spline they follow")]
		[SerializeField]
		private bool staticOrientation;

		private Graphic m_Graphic;

		private RectTransform rectTransform;

		private Text text;

		public bool StaticOrientation
		{
			get
			{
				return staticOrientation;
			}
			set
			{
				staticOrientation = value;
			}
		}

		protected override bool ShowOrientationSection => false;

		protected override bool ShowOffsetSection => false;

		protected Text Text
		{
			get
			{
				if (text == null)
				{
					text = GetComponent<Text>();
				}
				return text;
			}
		}

		protected RectTransform Rect
		{
			get
			{
				if (rectTransform == null)
				{
					rectTransform = GetComponent<RectTransform>();
				}
				return rectTransform;
			}
		}

		protected Graphic graphic
		{
			get
			{
				if (m_Graphic == null)
				{
					m_Graphic = GetComponent<Graphic>();
				}
				return m_Graphic;
			}
		}

		public override CurvySpline Spline
		{
			get
			{
				return m_Spline;
			}
			set
			{
				if (m_Spline != value)
				{
					UnbindSplineRelatedEvents();
					m_Spline = value;
					if (base.IsActiveAndEnabled)
					{
						BindSplineRelatedEvents();
					}
				}
			}
		}

		protected override void InitializedApplyDeltaTime(float deltaTime)
		{
			base.InitializedApplyDeltaTime(deltaTime);
			graphic.SetVerticesDirty();
		}

		public void ModifyMesh(Mesh verts)
		{
			if (base.IsActiveAndEnabled && base.isInitialized)
			{
				Vector3[] verts2 = verts.vertices;
				GlyphPlain glyphPlain = new GlyphPlain();
				for (int i = 0; i < Text.text.Length; i++)
				{
					glyphPlain.Load(ref verts2, i * 4);
					UpdateGlyph(glyphPlain);
					glyphPlain.Save(ref verts2, i * 4);
				}
				verts.vertices = verts2;
				ArrayPools.Vector3.Free(verts2);
			}
		}

		public void ModifyMesh(VertexHelper vertexHelper)
		{
			if (!base.IsActiveAndEnabled || !base.isInitialized)
			{
				return;
			}
			List<UIVertex> list = new List<UIVertex>();
			GlyphQuad glyphQuad = new GlyphQuad();
			vertexHelper.GetUIVertexStream(list);
			vertexHelper.Clear();
			int num = 0;
			for (int i = 0; i < Text.text.Length; i++)
			{
				if (Text.text[i] != ' ')
				{
					glyphQuad.LoadTris(list, num * 6);
					num++;
					UpdateGlyph(glyphQuad);
					glyphQuad.Save(vertexHelper);
				}
			}
		}

		[UsedImplicitly]
		private void UpdateGlyph(IGlyph glyph)
		{
			float tf = AbsoluteToRelative(CurvyController.GetClampedPosition(base.AbsolutePosition + glyph.Center.x, CurvyPositionMode.WorldUnits, base.Clamping, Length));
			glyph.Transpose(new Vector3(0f, glyph.Center.y, 0f));
			if (!StaticOrientation)
			{
				Vector3 tangent = GetTangent(tf);
				glyph.Rotate(Quaternion.AngleAxis(Mathf.Atan2(tangent.x, 0f - tangent.y) * 57.29578f - 90f, Vector3.forward));
			}
			glyph.Transpose(-glyph.Center);
			float tf2 = AbsoluteToRelative(CurvyController.GetClampedPosition(base.AbsolutePosition, CurvyPositionMode.WorldUnits, base.Clamping, Length));
			Vector3 vector = (base.UseCache ? Spline.InterpolateFast(tf2) : Spline.Interpolate(tf2));
			Vector3 vector2 = (base.UseCache ? Spline.InterpolateFast(tf) : Spline.Interpolate(tf));
			glyph.Transpose(Spline.transform.TransformDirection(vector2 - vector));
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			BindSplineRelatedEvents();
			if (graphic != null)
			{
				graphic.SetVerticesDirty();
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			UnbindSplineRelatedEvents();
			if (graphic != null)
			{
				graphic.SetVerticesDirty();
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (base.IsActiveAndEnabled)
			{
				BindSplineRelatedEvents();
				if (graphic != null)
				{
					graphic.SetVerticesDirty();
				}
			}
		}

		protected override void BindEvents()
		{
			base.BindEvents();
			BindSplineRelatedEvents();
		}

		protected override void UnbindEvents()
		{
			base.UnbindEvents();
			UnbindSplineRelatedEvents();
		}

		private void BindSplineRelatedEvents()
		{
			if ((bool)Spline)
			{
				UnbindSplineRelatedEvents();
				Spline.OnRefresh.AddListener(OnSplineRefreshed);
			}
		}

		private void UnbindSplineRelatedEvents()
		{
			if ((bool)Spline)
			{
				Spline.OnRefresh.RemoveListener(OnSplineRefreshed);
			}
		}

		private void OnSplineRefreshed(CurvySplineEventArgs e)
		{
			CurvySpline curvySpline = e.Sender as CurvySpline;
			if (curvySpline != Spline)
			{
				curvySpline.OnRefresh.RemoveListener(OnSplineRefreshed);
			}
			else
			{
				graphic.SetVerticesDirty();
			}
		}
	}
}
