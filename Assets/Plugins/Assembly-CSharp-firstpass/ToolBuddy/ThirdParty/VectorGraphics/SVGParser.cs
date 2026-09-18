using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using UnityEngine;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	public class SVGParser
	{
		public struct SceneInfo
		{
			public Scene Scene { get; }

			public Rect SceneViewport { get; }

			public Dictionary<SceneNode, float> NodeOpacity { get; }

			public Dictionary<string, SceneNode> NodeIDs { get; }

			internal SceneInfo(Scene scene, Rect sceneViewport, Dictionary<SceneNode, float> nodeOpacities, Dictionary<string, SceneNode> nodeIDs)
			{
				Scene = scene;
				SceneViewport = sceneViewport;
				NodeOpacity = nodeOpacities;
				NodeIDs = nodeIDs;
			}
		}

		public static SceneInfo ImportSVG(TextReader textReader, float dpi = 0f, float pixelsPerUnit = 1f, int windowWidth = 0, int windowHeight = 0, bool clipViewport = false)
		{
			ViewportOptions viewportOptions = (clipViewport ? ViewportOptions.PreserveViewport : ViewportOptions.DontPreserve);
			return ImportSVG(textReader, viewportOptions, dpi, pixelsPerUnit, windowWidth, windowHeight);
		}

		public static SceneInfo ImportSVG(TextReader textReader, ViewportOptions viewportOptions, float dpi = 0f, float pixelsPerUnit = 1f, int windowWidth = 0, int windowHeight = 0)
		{
			Scene scene = new Scene();
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.IgnoreComments = true;
			xmlReaderSettings.IgnoreProcessingInstructions = true;
			xmlReaderSettings.IgnoreWhitespace = true;
			xmlReaderSettings.DtdProcessing = DtdProcessing.Ignore;
			xmlReaderSettings.ValidationFlags = XmlSchemaValidationFlags.None;
			xmlReaderSettings.ValidationType = ValidationType.None;
			xmlReaderSettings.XmlResolver = null;
			if (dpi == 0f)
			{
				dpi = Screen.dpi;
			}
			SVGDocument sVGDocument;
			Dictionary<SceneNode, float> nodeOpacities;
			Dictionary<string, SceneNode> nodeIDs;
			using (XmlReader docReader = XmlReader.Create(textReader, xmlReaderSettings))
			{
				bool applyRootViewBox = viewportOptions == ViewportOptions.PreserveViewport || viewportOptions == ViewportOptions.OnlyApplyRootViewBox;
				sVGDocument = new SVGDocument(docReader, dpi, scene, windowWidth, windowHeight, applyRootViewBox);
				sVGDocument.Import();
				nodeOpacities = sVGDocument.NodeOpacities;
				nodeIDs = sVGDocument.NodeIDs;
			}
			float num = 1f / pixelsPerUnit;
			if (num != 1f && scene != null && scene.Root != null)
			{
				scene.Root.Transform = scene.Root.Transform * Matrix2D.Scale(new Vector2(num, num));
			}
			if (viewportOptions == ViewportOptions.PreserveViewport && scene != null && scene.Root != null)
			{
				Rect rect = VectorUtils.SceneNodeBounds(scene.Root);
				if (!sVGDocument.sceneViewport.Contains(rect.min) || !sVGDocument.sceneViewport.Contains(rect.max))
				{
					Shape shape = new Shape();
					VectorUtils.MakeRectangleShape(shape, sVGDocument.sceneViewport);
					scene.Root = new SceneNode
					{
						Children = new List<SceneNode> { scene.Root },
						Clipper = new SceneNode
						{
							Shapes = new List<Shape> { shape }
						}
					};
				}
			}
			return new SceneInfo(scene, sVGDocument.sceneViewport, nodeOpacities, nodeIDs);
		}
	}
}
