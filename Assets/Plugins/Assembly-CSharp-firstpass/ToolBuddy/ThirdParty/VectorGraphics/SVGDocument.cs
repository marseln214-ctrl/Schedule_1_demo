using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using UnityEngine;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	internal class SVGDocument
	{
		private enum ViewBoxAlign
		{
			Min = 0,
			Mid = 1,
			Max = 2
		}

		private enum ViewBoxAspectRatio
		{
			DontPreserve = 0,
			FitLargestDim = 1,
			FitSmallestDim = 2
		}

		private struct ViewBoxInfo
		{
			public Rect ViewBox;

			public ViewBoxAspectRatio AspectRatio;

			public ViewBoxAlign AlignX;

			public ViewBoxAlign AlignY;

			public bool IsEmpty;
		}

		private struct HierarchyUpdate
		{
			public SceneNode Parent;

			public SceneNode NewNode;

			public SceneNode ReplaceNode;
		}

		private delegate void ElemHandler();

		private class Handlers : Dictionary<string, ElemHandler>
		{
			public Handlers(int capacity)
				: base(capacity)
			{
			}
		}

		private enum DimType
		{
			Width = 0,
			Height = 1,
			Length = 2
		}

		private struct NodeGlobalSceneState
		{
			public Vector2 ContainerSize;
		}

		private class GradientExData
		{
			public bool WorldRelative;

			public Matrix2D FillTransform;
		}

		private class LinearGradientExData : GradientExData
		{
			public string X1;

			public string Y1;

			public string X2;

			public string Y2;
		}

		private class RadialGradientExData : GradientExData
		{
			public bool Parsed;

			public string Cx;

			public string Cy;

			public string Fx;

			public string Fy;

			public string R;
		}

		private struct ClipData
		{
			public bool WorldRelative;
		}

		private struct PatternData
		{
			public bool WorldRelative;

			public bool ContentWorldRelative;

			public Matrix2D PatternTransform;
		}

		private struct MaskData
		{
			public bool WorldRelative;

			public bool ContentWorldRelative;
		}

		private struct NodeWithParent
		{
			public SceneNode node;

			public SceneNode parent;
		}

		private struct NodeReferenceData
		{
			public SceneNode node;

			public Rect viewport;

			public string id;
		}

		private struct PostponedStopData
		{
			public GradientFill fill;
		}

		private struct PostponedClip
		{
			public SceneNode node;
		}

		internal const float SVGLengthFactor = 1.4142135f;

		private static char[] whiteSpaceNumberChars = " \r\n\t,".ToCharArray();

		private XmlReaderIterator docReader;

		private Scene scene;

		private float dpiScale;

		private int windowWidth;

		private int windowHeight;

		private Vector2 scenePos;

		private Vector2 sceneSize;

		private SVGDictionary svgObjects = new SVGDictionary();

		private Dictionary<string, Handlers> subTags = new Dictionary<string, Handlers>();

		private Dictionary<GradientFill, GradientExData> gradientExInfo = new Dictionary<GradientFill, GradientExData>();

		private Dictionary<SceneNode, ViewBoxInfo> symbolViewBoxes = new Dictionary<SceneNode, ViewBoxInfo>();

		private Dictionary<SceneNode, NodeGlobalSceneState> nodeGlobalSceneState = new Dictionary<SceneNode, NodeGlobalSceneState>();

		private Dictionary<SceneNode, float> nodeOpacity = new Dictionary<SceneNode, float>();

		private Dictionary<string, SceneNode> nodeIDs = new Dictionary<string, SceneNode>();

		private Dictionary<SceneNode, SVGStyleResolver.StyleLayer> nodeStyleLayers = new Dictionary<SceneNode, SVGStyleResolver.StyleLayer>();

		private Dictionary<SceneNode, ClipData> clipData = new Dictionary<SceneNode, ClipData>();

		private Dictionary<SceneNode, PatternData> patternData = new Dictionary<SceneNode, PatternData>();

		private Dictionary<SceneNode, MaskData> maskData = new Dictionary<SceneNode, MaskData>();

		private Dictionary<string, List<NodeReferenceData>> postponedSymbolData = new Dictionary<string, List<NodeReferenceData>>();

		private Dictionary<string, List<PostponedStopData>> postponedStopData = new Dictionary<string, List<PostponedStopData>>();

		private Dictionary<string, List<PostponedClip>> postponedClip = new Dictionary<string, List<PostponedClip>>();

		private SVGPostponedFills postponedFills = new SVGPostponedFills();

		private List<NodeWithParent> invisibleNodes = new List<NodeWithParent>();

		private Stack<Vector2> currentContainerSize = new Stack<Vector2>();

		private Stack<Vector2> currentViewBoxSize = new Stack<Vector2>();

		private Stack<SceneNode> currentSceneNode = new Stack<SceneNode>();

		private GradientFill currentGradientFill;

		private string currentGradientId;

		private string currentGradientLink;

		private ElemHandler[] allElems;

		private HashSet<ElemHandler> elemsToAddToHierarchy;

		private SVGStyleResolver styles = new SVGStyleResolver();

		private bool applyRootViewBox;

		internal Rect sceneViewport;

		public Dictionary<SceneNode, float> NodeOpacities => nodeOpacity;

		public Dictionary<string, SceneNode> NodeIDs => nodeIDs;

		internal static string StockBlackNonZeroFillName => "unity_internal_black_nz";

		internal static string StockBlackOddEvenFillName => "unity_internal_black_oe";

		public SVGDocument(XmlReader docReader, float dpi, Scene scene, int windowWidth, int windowHeight, bool applyRootViewBox)
		{
			allElems = new ElemHandler[18]
			{
				circle, defs, ellipse, g, image, line, linearGradient, path, polygon, polyline,
				radialGradient, clipPath, pattern, mask, rect, symbol, use, style
			};
			elemsToAddToHierarchy = new HashSet<ElemHandler>(new ElemHandler[11]
			{
				circle, ellipse, g, image, line, path, polygon, polyline, rect, svg,
				use
			});
			this.docReader = new XmlReaderIterator(docReader);
			this.scene = scene;
			dpiScale = dpi / 90f;
			this.windowWidth = windowWidth;
			this.windowHeight = windowHeight;
			this.applyRootViewBox = applyRootViewBox;
			svgObjects[StockBlackNonZeroFillName] = new SolidFill
			{
				Color = new Color(0f, 0f, 0f),
				Mode = FillMode.NonZero
			};
			svgObjects[StockBlackOddEvenFillName] = new SolidFill
			{
				Color = new Color(0f, 0f, 0f),
				Mode = FillMode.OddEven
			};
		}

		public void Import()
		{
			if (scene == null)
			{
				throw new ArgumentNullException();
			}
			if (!docReader.GoToRoot("svg"))
			{
				throw new SVGFormatException("Document doesn't have 'svg' root");
			}
			currentContainerSize.Push(new Vector2(windowWidth, windowHeight));
			svg();
			currentContainerSize.Pop();
			if (currentContainerSize.Count > 0)
			{
				throw SVGFormatException.StackError;
			}
			PostProcess(scene.Root);
			RemoveInvisibleNodes();
		}

		private void ParseChildren(XmlReaderIterator.Node node, string nodeName)
		{
			SceneNode sceneNode = currentSceneNode.Peek();
			Handlers handlers = subTags[nodeName];
			while (docReader.GoToNextChild(node))
			{
				XmlReaderIterator.Node node2 = docReader.VisitCurrent();
				if (!handlers.TryGetValue(node2.Name, out var value))
				{
					docReader.SkipCurrentChildTree(node2);
					continue;
				}
				bool num = elemsToAddToHierarchy.Contains(value);
				SceneNode sceneNode2 = null;
				if (num)
				{
					if (sceneNode.Children == null)
					{
						sceneNode.Children = new List<SceneNode>();
					}
					sceneNode2 = new SceneNode();
					nodeGlobalSceneState[sceneNode2] = new NodeGlobalSceneState
					{
						ContainerSize = currentContainerSize.Peek()
					};
					sceneNode.Children.Add(sceneNode2);
					currentSceneNode.Push(sceneNode2);
				}
				styles.PushNode(node2);
				if (sceneNode2 != null)
				{
					styles.SaveLayerForSceneNode(sceneNode2);
					if (styles.Evaluate("display") == "none")
					{
						invisibleNodes.Add(new NodeWithParent
						{
							node = sceneNode2,
							parent = sceneNode
						});
					}
				}
				value();
				ParseChildren(node2, node2.Name);
				styles.PopNode();
				if (!num || currentSceneNode.Pop() == sceneNode2)
				{
					continue;
				}
				throw SVGFormatException.StackError;
			}
		}

		private void circle()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = currentSceneNode.Peek();
			ParseID(node, sceneNode);
			ParseOpacity(sceneNode);
			sceneNode.Transform = SVGAttribParser.ParseTransform(node);
			IFill fill = SVGAttribParser.ParseFill(node, svgObjects, postponedFills, styles);
			PathCorner strokeCorner;
			PathEnding strokeEnding;
			Stroke stroke = ParseStrokeAttributeSet(node, out strokeCorner, out strokeEnding);
			float x = AttribLengthVal(node, "cx", 0f, DimType.Width);
			float y = AttribLengthVal(node, "cy", 0f, DimType.Height);
			float radius = AttribLengthVal(node, "r", 0f, DimType.Length);
			Shape shape = new Shape();
			VectorUtils.MakeCircleShape(shape, new Vector2(x, y), radius);
			shape.PathProps = new PathProperties
			{
				Stroke = stroke,
				Head = strokeEnding,
				Tail = strokeEnding,
				Corners = strokeCorner
			};
			shape.Fill = fill;
			sceneNode.Shapes = new List<Shape>(1);
			sceneNode.Shapes.Add(shape);
			ParseClipAndMask(node, sceneNode);
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node);
			}
		}

		private void defs()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = new SceneNode();
			ParseOpacity(sceneNode);
			sceneNode.Transform = SVGAttribParser.ParseTransform(node);
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node, allElems);
			}
			currentSceneNode.Push(sceneNode);
			ParseChildren(node, node.Name);
			if (currentSceneNode.Pop() != sceneNode)
			{
				throw SVGFormatException.StackError;
			}
		}

		private void ellipse()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = currentSceneNode.Peek();
			ParseID(node, sceneNode);
			ParseOpacity(sceneNode);
			sceneNode.Transform = SVGAttribParser.ParseTransform(node);
			IFill fill = SVGAttribParser.ParseFill(node, svgObjects, postponedFills, styles);
			PathCorner strokeCorner;
			PathEnding strokeEnding;
			Stroke stroke = ParseStrokeAttributeSet(node, out strokeCorner, out strokeEnding);
			float x = AttribLengthVal(node, "cx", 0f, DimType.Width);
			float y = AttribLengthVal(node, "cy", 0f, DimType.Height);
			float radiusX = AttribLengthVal(node, "rx", 0f, DimType.Length);
			float radiusY = AttribLengthVal(node, "ry", 0f, DimType.Length);
			Shape shape = new Shape();
			VectorUtils.MakeEllipseShape(shape, new Vector2(x, y), radiusX, radiusY);
			shape.PathProps = new PathProperties
			{
				Stroke = stroke,
				Corners = strokeCorner,
				Head = strokeEnding,
				Tail = strokeEnding
			};
			shape.Fill = fill;
			sceneNode.Shapes = new List<Shape>(1);
			sceneNode.Shapes.Add(shape);
			ParseClipAndMask(node, sceneNode);
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node);
			}
		}

		private void g()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = currentSceneNode.Peek();
			ParseID(node, sceneNode);
			ParseOpacity(sceneNode);
			sceneNode.Transform = SVGAttribParser.ParseTransform(node);
			ParseClipAndMask(node, sceneNode);
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node, allElems);
			}
		}

		private void image()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = currentSceneNode.Peek();
			string text = node["xlink:href"];
			if (text != null)
			{
				TextureFill textureFill = new TextureFill();
				textureFill.Mode = FillMode.NonZero;
				textureFill.Addressing = AddressMode.Clamp;
				string text2 = text.ToLower();
				if (text2.StartsWith("data:"))
				{
					textureFill.Texture = DecodeTextureData(text);
				}
				else if (text2.Contains("://"))
				{
					if (text2.StartsWith("http://") || text2.StartsWith("https://"))
					{
						using WWW wWW = new WWW(text);
						while (wWW.keepWaiting)
						{
							Thread.Sleep(10);
						}
						textureFill.Texture = wWW.texture;
					}
					else
					{
						Debug.LogWarning("Unsupported URL scheme for <image> (only http/https is supported): " + text);
					}
				}
				if (textureFill.Texture != null)
				{
					ParseID(node, sceneNode);
					ParseOpacity(sceneNode);
					sceneNode.Transform = SVGAttribParser.ParseTransform(node);
					Rect rect = ParseViewport(node, sceneNode, currentContainerSize.Peek());
					sceneNode.Transform *= Matrix2D.Translate(rect.position);
					ViewBoxInfo viewBoxInfo = default(ViewBoxInfo);
					viewBoxInfo.ViewBox = new Rect(0f, 0f, textureFill.Texture.width, textureFill.Texture.height);
					ParseViewBoxAspectRatio(node, ref viewBoxInfo);
					ApplyViewBox(sceneNode, viewBoxInfo, rect);
					Shape shape = new Shape();
					VectorUtils.MakeRectangleShape(shape, new Rect(0f, 0f, textureFill.Texture.width, textureFill.Texture.height));
					shape.Fill = textureFill;
					sceneNode.Shapes = new List<Shape>(1);
					sceneNode.Shapes.Add(shape);
					ParseClipAndMask(node, sceneNode);
				}
			}
			string text3 = node["id"];
			if (!string.IsNullOrEmpty(text3) && postponedSymbolData.TryGetValue(text3, out var value))
			{
				foreach (NodeReferenceData item in value)
				{
					ResolveReferencedNode(sceneNode, item, isDeferred: true);
				}
			}
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node);
			}
		}

		private void line()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = currentSceneNode.Peek();
			ParseID(node, sceneNode);
			ParseOpacity(sceneNode);
			sceneNode.Transform = SVGAttribParser.ParseTransform(node);
			PathCorner strokeCorner;
			PathEnding strokeEnding;
			Stroke stroke = ParseStrokeAttributeSet(node, out strokeCorner, out strokeEnding);
			float x = AttribLengthVal(node, "x1", 0f, DimType.Width);
			float y = AttribLengthVal(node, "y1", 0f, DimType.Height);
			float x2 = AttribLengthVal(node, "x2", 0f, DimType.Width);
			float y2 = AttribLengthVal(node, "y2", 0f, DimType.Height);
			Shape shape = new Shape();
			shape.PathProps = new PathProperties
			{
				Stroke = stroke,
				Head = strokeEnding,
				Tail = strokeEnding
			};
			shape.Contours = new BezierContour[1]
			{
				new BezierContour
				{
					Segments = VectorUtils.BezierSegmentToPath(VectorUtils.MakeLine(new Vector2(x, y), new Vector2(x2, y2)))
				}
			};
			sceneNode.Shapes = new List<Shape>(1);
			sceneNode.Shapes.Add(shape);
			ParseClipAndMask(node, sceneNode);
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node);
			}
		}

		private void linearGradient()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			string text = node["xlink:href"];
			GradientFill gradientFill = SVGAttribParser.ParseRelativeRef(text, svgObjects) as GradientFill;
			bool worldRelative = ((gradientFill != null) ? (gradientExInfo[gradientFill] as LinearGradientExData) : null)?.WorldRelative ?? false;
			switch (node["gradientUnits"])
			{
			case "objectBoundingBox":
				worldRelative = false;
				break;
			case "userSpaceOnUse":
				worldRelative = true;
				break;
			default:
				throw node.GetUnsupportedAttribValException("gradientUnits");
			case null:
				break;
			}
			AddressMode addressing = gradientFill?.Addressing ?? AddressMode.Clamp;
			switch (node["spreadMethod"])
			{
			case "pad":
				addressing = AddressMode.Clamp;
				break;
			case "reflect":
				addressing = AddressMode.Mirror;
				break;
			case "repeat":
				addressing = AddressMode.Wrap;
				break;
			default:
				throw node.GetUnsupportedAttribValException("spreadMethod");
			case null:
				break;
			}
			Matrix2D fillTransform = SVGAttribParser.ParseTransform(node, "gradientTransform");
			GradientFill gradientFill2 = CloneGradientFill(gradientFill);
			if (gradientFill2 == null)
			{
				gradientFill2 = new GradientFill
				{
					Addressing = addressing,
					Type = GradientFillType.Linear
				};
			}
			LinearGradientExData linearGradientExData = new LinearGradientExData
			{
				WorldRelative = worldRelative,
				FillTransform = fillTransform
			};
			gradientExInfo[gradientFill2] = linearGradientExData;
			currentContainerSize.Push(Vector2.one);
			linearGradientExData.X1 = node["x1"];
			linearGradientExData.Y1 = node["y1"];
			linearGradientExData.X2 = node["x2"];
			linearGradientExData.Y2 = node["y2"];
			AttribLengthVal(linearGradientExData.X1, node, "x1", 0f, DimType.Width);
			AttribLengthVal(linearGradientExData.Y1, node, "y1", 0f, DimType.Height);
			AttribLengthVal(linearGradientExData.X2, node, "x2", 1f, DimType.Width);
			AttribLengthVal(linearGradientExData.Y2, node, "y2", 0f, DimType.Height);
			currentContainerSize.Pop();
			currentGradientFill = gradientFill2;
			currentGradientId = node["id"];
			currentGradientLink = SVGAttribParser.CleanIri(text);
			if (!string.IsNullOrEmpty(text) && !svgObjects.ContainsKey(text))
			{
				if (!postponedStopData.ContainsKey(currentGradientLink))
				{
					postponedStopData.Add(currentGradientLink, new List<PostponedStopData>());
				}
				postponedStopData[currentGradientLink].Add(new PostponedStopData
				{
					fill = gradientFill2
				});
			}
			AddToSVGDictionaryIfPossible(node, gradientFill2);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node, stop);
			}
		}

		private void path()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = currentSceneNode.Peek();
			ParseID(node, sceneNode);
			ParseOpacity(sceneNode);
			sceneNode.Transform = SVGAttribParser.ParseTransform(node);
			IFill fill = SVGAttribParser.ParseFill(node, svgObjects, postponedFills, styles);
			PathCorner strokeCorner;
			PathEnding strokeEnding;
			Stroke stroke = ParseStrokeAttributeSet(node, out strokeCorner, out strokeEnding);
			PathProperties pathProperties = default(PathProperties);
			pathProperties.Stroke = stroke;
			pathProperties.Corners = strokeCorner;
			pathProperties.Head = strokeEnding;
			pathProperties.Tail = strokeEnding;
			PathProperties pathProps = pathProperties;
			List<BezierContour> list = SVGAttribParser.ParsePath(node);
			if (list != null && list.Count > 0)
			{
				sceneNode.Shapes = new List<Shape>(1);
				sceneNode.Shapes.Add(new Shape
				{
					Contours = list.ToArray(),
					Fill = fill,
					PathProps = pathProps
				});
				AddToSVGDictionaryIfPossible(node, sceneNode);
			}
			ParseClipAndMask(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node);
			}
		}

		private void polygon()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = currentSceneNode.Peek();
			ParseID(node, sceneNode);
			ParseOpacity(sceneNode);
			sceneNode.Transform = SVGAttribParser.ParseTransform(node);
			IFill fill = SVGAttribParser.ParseFill(node, svgObjects, postponedFills, styles);
			PathCorner strokeCorner;
			PathEnding strokeEnding;
			Stroke stroke = ParseStrokeAttributeSet(node, out strokeCorner, out strokeEnding);
			string[] array = node["points"]?.Split(whiteSpaceNumberChars, StringSplitOptions.RemoveEmptyEntries);
			if (array != null)
			{
				if ((array.Length & 1) == 1)
				{
					throw node.GetException("polygon 'points' must specify x,y for each coordinate");
				}
				if (array.Length < 4)
				{
					throw node.GetException("polygon 'points' do not even specify one triangle");
				}
				PathProperties pathProperties = default(PathProperties);
				pathProperties.Stroke = stroke;
				pathProperties.Corners = strokeCorner;
				pathProperties.Head = strokeEnding;
				pathProperties.Tail = strokeEnding;
				PathProperties pathProps = pathProperties;
				BezierContour bezierContour = default(BezierContour);
				bezierContour.Closed = true;
				BezierContour bezierContour2 = bezierContour;
				Vector2 vector = new Vector2(AttribLengthVal(array[0], node, "points", 0f, DimType.Width), AttribLengthVal(array[1], node, "points", 0f, DimType.Height));
				int num = array.Length / 2;
				List<BezierPathSegment> list = new List<BezierPathSegment>(num);
				for (int i = 1; i < num; i++)
				{
					Vector2 vector2 = new Vector2(AttribLengthVal(array[i * 2], node, "points", 0f, DimType.Width), AttribLengthVal(array[i * 2 + 1], node, "points", 0f, DimType.Height));
					if (!(vector2 == vector))
					{
						BezierSegment bezierSegment = VectorUtils.MakeLine(vector, vector2);
						list.Add(new BezierPathSegment
						{
							P0 = bezierSegment.P0,
							P1 = bezierSegment.P1,
							P2 = bezierSegment.P2
						});
						vector = vector2;
					}
				}
				if (list.Count > 0)
				{
					BezierSegment bezierSegment2 = VectorUtils.MakeLine(vector, list[0].P0);
					list.Add(new BezierPathSegment
					{
						P0 = bezierSegment2.P0,
						P1 = bezierSegment2.P1,
						P2 = bezierSegment2.P2
					});
					bezierContour2.Segments = list.ToArray();
					Shape shape = new Shape();
					shape.Contours = new BezierContour[1] { bezierContour2 };
					shape.PathProps = pathProps;
					shape.Fill = fill;
					Shape item = shape;
					sceneNode.Shapes = new List<Shape>(1);
					sceneNode.Shapes.Add(item);
				}
			}
			ParseClipAndMask(node, sceneNode);
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node);
			}
		}

		private void polyline()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = currentSceneNode.Peek();
			ParseID(node, sceneNode);
			ParseOpacity(sceneNode);
			sceneNode.Transform = SVGAttribParser.ParseTransform(node);
			IFill fill = SVGAttribParser.ParseFill(node, svgObjects, postponedFills, styles);
			PathCorner strokeCorner;
			PathEnding strokeEnding;
			Stroke stroke = ParseStrokeAttributeSet(node, out strokeCorner, out strokeEnding);
			string[] array = node["points"]?.Split(whiteSpaceNumberChars, StringSplitOptions.RemoveEmptyEntries);
			if (array != null)
			{
				if ((array.Length & 1) == 1)
				{
					throw node.GetException("polyline 'points' must specify x,y for each coordinate");
				}
				if (array.Length < 4)
				{
					throw node.GetException("polyline 'points' do not even specify one line");
				}
				Shape shape = new Shape
				{
					Fill = fill
				};
				shape.PathProps = new PathProperties
				{
					Stroke = stroke,
					Corners = strokeCorner,
					Head = strokeEnding,
					Tail = strokeEnding
				};
				Vector2 vector = new Vector2(AttribLengthVal(array[0], node, "points", 0f, DimType.Width), AttribLengthVal(array[1], node, "points", 0f, DimType.Height));
				int num = array.Length / 2;
				List<BezierPathSegment> list = new List<BezierPathSegment>(num);
				for (int i = 1; i < num; i++)
				{
					Vector2 vector2 = new Vector2(AttribLengthVal(array[i * 2], node, "points", 0f, DimType.Width), AttribLengthVal(array[i * 2 + 1], node, "points", 0f, DimType.Height));
					if (!(vector2 == vector))
					{
						BezierSegment bezierSegment = VectorUtils.MakeLine(vector, vector2);
						list.Add(new BezierPathSegment
						{
							P0 = bezierSegment.P0,
							P1 = bezierSegment.P1,
							P2 = bezierSegment.P2
						});
						vector = vector2;
					}
				}
				if (list.Count > 0)
				{
					BezierSegment bezierSegment2 = VectorUtils.MakeLine(vector, list[0].P0);
					list.Add(new BezierPathSegment
					{
						P0 = bezierSegment2.P0,
						P1 = bezierSegment2.P1,
						P2 = bezierSegment2.P2
					});
					shape.Contours = new BezierContour[1]
					{
						new BezierContour
						{
							Segments = list.ToArray()
						}
					};
					sceneNode.Shapes = new List<Shape>(1);
					sceneNode.Shapes.Add(shape);
				}
			}
			ParseClipAndMask(node, sceneNode);
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node);
			}
		}

		private void radialGradient()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			string text = node["xlink:href"];
			GradientFill gradientFill = SVGAttribParser.ParseRelativeRef(text, svgObjects) as GradientFill;
			bool worldRelative = ((gradientFill != null) ? (gradientExInfo[gradientFill] as RadialGradientExData) : null)?.WorldRelative ?? false;
			switch (node["gradientUnits"])
			{
			case "objectBoundingBox":
				worldRelative = false;
				break;
			case "userSpaceOnUse":
				worldRelative = true;
				break;
			default:
				throw node.GetUnsupportedAttribValException("gradientUnits");
			case null:
				break;
			}
			AddressMode addressing = gradientFill?.Addressing ?? AddressMode.Clamp;
			switch (node["spreadMethod"])
			{
			case "pad":
				addressing = AddressMode.Clamp;
				break;
			case "reflect":
				addressing = AddressMode.Mirror;
				break;
			case "repeat":
				addressing = AddressMode.Wrap;
				break;
			default:
				throw node.GetUnsupportedAttribValException("spreadMethod");
			case null:
				break;
			}
			Matrix2D fillTransform = SVGAttribParser.ParseTransform(node, "gradientTransform");
			GradientFill gradientFill2 = CloneGradientFill(gradientFill);
			if (gradientFill2 == null)
			{
				gradientFill2 = new GradientFill
				{
					Addressing = addressing,
					Type = GradientFillType.Radial
				};
			}
			RadialGradientExData radialGradientExData = new RadialGradientExData
			{
				WorldRelative = worldRelative,
				FillTransform = fillTransform
			};
			gradientExInfo[gradientFill2] = radialGradientExData;
			currentContainerSize.Push(Vector2.one);
			radialGradientExData.Cx = node["cx"];
			radialGradientExData.Cy = node["cy"];
			radialGradientExData.Fx = node["fx"];
			radialGradientExData.Fy = node["fy"];
			radialGradientExData.R = node["r"];
			AttribLengthVal(radialGradientExData.Cx, node, "cx", 0.5f, DimType.Width);
			AttribLengthVal(radialGradientExData.Cy, node, "cy", 0.5f, DimType.Height);
			AttribLengthVal(radialGradientExData.Fx, node, "fx", 0.5f, DimType.Width);
			AttribLengthVal(radialGradientExData.Fy, node, "fy", 0.5f, DimType.Height);
			AttribLengthVal(radialGradientExData.R, node, "r", 0.5f, DimType.Length);
			currentContainerSize.Pop();
			currentGradientFill = gradientFill2;
			currentGradientId = node["id"];
			currentGradientLink = SVGAttribParser.CleanIri(text);
			if (!string.IsNullOrEmpty(text) && !svgObjects.ContainsKey(text))
			{
				if (!postponedStopData.ContainsKey(currentGradientLink))
				{
					postponedStopData.Add(currentGradientLink, new List<PostponedStopData>());
				}
				postponedStopData[currentGradientLink].Add(new PostponedStopData
				{
					fill = gradientFill2
				});
			}
			AddToSVGDictionaryIfPossible(node, gradientFill2);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node, stop);
			}
		}

		private void clipPath()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			string text = node["id"];
			SceneNode sceneNode = new SceneNode
			{
				Transform = SVGAttribParser.ParseTransform(node)
			};
			bool worldRelative;
			switch (node["clipPathUnits"])
			{
			case null:
			case "userSpaceOnUse":
				worldRelative = true;
				break;
			case "objectBoundingBox":
				worldRelative = false;
				break;
			default:
				throw node.GetUnsupportedAttribValException("clipPathUnits");
			}
			clipData[sceneNode] = new ClipData
			{
				WorldRelative = worldRelative
			};
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node, allElems);
			}
			currentSceneNode.Push(sceneNode);
			ParseChildren(node, node.Name);
			if (currentSceneNode.Pop() != sceneNode)
			{
				throw SVGFormatException.StackError;
			}
			if (string.IsNullOrEmpty(text) || !postponedClip.TryGetValue(text, out var value))
			{
				return;
			}
			foreach (PostponedClip item in value)
			{
				ApplyClipper(sceneNode, item.node, worldRelative);
			}
		}

		private void pattern()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = new SceneNode
			{
				Transform = Matrix2D.identity
			};
			bool flag = false;
			switch (node["patternUnits"])
			{
			case null:
			case "objectBoundingBox":
				flag = false;
				break;
			case "userSpaceOnUse":
				flag = true;
				break;
			default:
				throw node.GetUnsupportedAttribValException("patternUnits");
			}
			bool flag2 = true;
			switch (node["patternContentUnits"])
			{
			case null:
			case "userSpaceOnUse":
				flag2 = true;
				break;
			case "objectBoundingBox":
				flag2 = false;
				break;
			default:
				throw node.GetUnsupportedAttribValException("patternContentUnits");
			}
			float x = AttribLengthVal(node["x"], node, "x", 0f, DimType.Width);
			float y = AttribLengthVal(node["y"], node, "y", 0f, DimType.Height);
			float width = AttribLengthVal(node["width"], node, "width", 0f, DimType.Width);
			float height = AttribLengthVal(node["height"], node, "height", 0f, DimType.Height);
			Matrix2D patternTransform = SVGAttribParser.ParseTransform(node, "patternTransform");
			patternData[sceneNode] = new PatternData
			{
				WorldRelative = flag,
				ContentWorldRelative = flag2,
				PatternTransform = patternTransform
			};
			PatternFill vectorElement = new PatternFill
			{
				Pattern = sceneNode,
				Rect = new Rect(x, y, width, height)
			};
			AddToSVGDictionaryIfPossible(node, vectorElement);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node, allElems);
			}
			currentSceneNode.Push(sceneNode);
			ParseChildren(node, node.Name);
			if (currentSceneNode.Pop() != sceneNode)
			{
				throw SVGFormatException.StackError;
			}
		}

		private void mask()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = new SceneNode
			{
				Transform = Matrix2D.identity
			};
			bool worldRelative;
			switch (node["maskUnits"])
			{
			case null:
			case "userSpaceOnUse":
				worldRelative = true;
				break;
			case "objectBoundingBox":
				worldRelative = false;
				break;
			default:
				throw node.GetUnsupportedAttribValException("maskUnits");
			}
			bool contentWorldRelative;
			switch (node["maskContentUnits"])
			{
			case null:
			case "userSpaceOnUse":
				contentWorldRelative = true;
				break;
			case "objectBoundingBox":
				contentWorldRelative = false;
				break;
			default:
				throw node.GetUnsupportedAttribValException("maskContentUnits");
			}
			maskData[sceneNode] = new MaskData
			{
				WorldRelative = worldRelative,
				ContentWorldRelative = contentWorldRelative
			};
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node, allElems);
			}
			currentSceneNode.Push(sceneNode);
			ParseChildren(node, node.Name);
			if (currentSceneNode.Pop() != sceneNode)
			{
				throw SVGFormatException.StackError;
			}
		}

		private void rect()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = currentSceneNode.Peek();
			ParseID(node, sceneNode);
			ParseOpacity(sceneNode);
			sceneNode.Transform = SVGAttribParser.ParseTransform(node);
			IFill fill = SVGAttribParser.ParseFill(node, svgObjects, postponedFills, styles);
			PathCorner strokeCorner;
			PathEnding strokeEnding;
			Stroke stroke = ParseStrokeAttributeSet(node, out strokeCorner, out strokeEnding);
			float x = AttribLengthVal(node, "x", 0f, DimType.Width);
			float y = AttribLengthVal(node, "y", 0f, DimType.Height);
			float num = AttribLengthVal(node, "rx", -1f, DimType.Length);
			float num2 = AttribLengthVal(node, "ry", -1f, DimType.Length);
			float num3 = AttribLengthVal(node, "width", 0f, DimType.Length);
			float num4 = AttribLengthVal(node, "height", 0f, DimType.Length);
			if (num < 0f && num2 >= 0f)
			{
				num = num2;
			}
			else if (num2 < 0f && num >= 0f)
			{
				num2 = num;
			}
			else if (num2 < 0f && num < 0f)
			{
				num = (num2 = 0f);
			}
			num = Mathf.Min(num, num3 * 0.5f);
			num2 = Mathf.Min(num2, num4 * 0.5f);
			Vector2 vector = new Vector2(num, num2);
			Shape shape = new Shape();
			VectorUtils.MakeRectangleShape(shape, new Rect(x, y, num3, num4), vector, vector, vector, vector);
			shape.Fill = fill;
			shape.PathProps = new PathProperties
			{
				Stroke = stroke,
				Head = strokeEnding,
				Tail = strokeEnding,
				Corners = strokeCorner
			};
			sceneNode.Shapes = new List<Shape>(1);
			sceneNode.Shapes.Add(shape);
			ParseClipAndMask(node, sceneNode);
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node);
			}
		}

		private void stop()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			GradientStop gradientStop = default(GradientStop);
			string text = styles.Evaluate("stop-color");
			Color color = ((text != null) ? SVGAttribParser.ParseColor(text) : Color.black);
			color.a = AttribFloatVal("stop-opacity", 1f);
			gradientStop.Color = color;
			string text2 = styles.Evaluate("offset");
			if (!string.IsNullOrEmpty(text2))
			{
				bool num = text2.EndsWith("%");
				if (num)
				{
					text2 = text2.Substring(0, text2.Length - 1);
				}
				gradientStop.StopPercentage = SVGAttribParser.ParseFloat(text2);
				if (num)
				{
					gradientStop.StopPercentage /= 100f;
				}
				gradientStop.StopPercentage = Mathf.Max(0f, gradientStop.StopPercentage);
				gradientStop.StopPercentage = Mathf.Min(1f, gradientStop.StopPercentage);
			}
			GradientStop[] array;
			if (currentGradientFill.Stops == null || currentGradientFill.Stops.Length == 0)
			{
				array = new GradientStop[1];
			}
			else
			{
				array = new GradientStop[currentGradientFill.Stops.Length + 1];
				currentGradientFill.Stops.CopyTo(array, 0);
			}
			array[^1] = gradientStop;
			currentGradientFill.Stops = array;
			if (!string.IsNullOrEmpty(currentGradientId) && postponedStopData.ContainsKey(currentGradientId))
			{
				foreach (PostponedStopData item in postponedStopData[currentGradientId])
				{
					item.fill.Stops = array;
				}
			}
			if (!string.IsNullOrEmpty(currentGradientLink) && postponedStopData.ContainsKey(currentGradientLink))
			{
				List<PostponedStopData> list = postponedStopData[currentGradientLink];
				foreach (PostponedStopData item2 in list)
				{
					if (item2.fill == currentGradientFill)
					{
						list.Remove(item2);
						break;
					}
				}
			}
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node);
			}
		}

		private void svg()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = new SceneNode();
			if (scene.Root == null)
			{
				scene.Root = sceneNode;
			}
			styles.PushNode(node);
			ParseID(node, sceneNode);
			ParseOpacity(sceneNode);
			sceneViewport = ParseViewport(node, sceneNode, new Vector2(windowWidth, windowHeight));
			ViewBoxInfo viewBoxInfo = ParseViewBox(node, sceneNode, sceneViewport);
			if (applyRootViewBox)
			{
				ApplyViewBox(sceneNode, viewBoxInfo, sceneViewport);
			}
			currentContainerSize.Push(sceneViewport.size);
			if (!viewBoxInfo.IsEmpty)
			{
				currentViewBoxSize.Push(viewBoxInfo.ViewBox.size);
			}
			currentSceneNode.Push(sceneNode);
			nodeGlobalSceneState[sceneNode] = new NodeGlobalSceneState
			{
				ContainerSize = currentContainerSize.Peek()
			};
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node, allElems);
			}
			ParseChildren(node, "svg");
			if (currentSceneNode.Pop() != sceneNode)
			{
				throw SVGFormatException.StackError;
			}
			if (!viewBoxInfo.IsEmpty)
			{
				currentViewBoxSize.Pop();
			}
			currentContainerSize.Pop();
			styles.PopNode();
		}

		private void symbol()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = new SceneNode();
			string text = node["id"];
			ParseID(node, sceneNode);
			ParseOpacity(sceneNode);
			sceneNode.Transform = Matrix2D.identity;
			Rect rect = new Rect(Vector2.zero, currentContainerSize.Peek());
			ViewBoxInfo value = ParseViewBox(node, sceneNode, rect);
			if (!value.IsEmpty)
			{
				currentViewBoxSize.Push(value.ViewBox.size);
			}
			symbolViewBoxes[sceneNode] = value;
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node, allElems);
			}
			currentSceneNode.Push(sceneNode);
			ParseChildren(node, node.Name);
			if (currentSceneNode.Pop() != sceneNode)
			{
				throw SVGFormatException.StackError;
			}
			if (!value.IsEmpty)
			{
				currentViewBoxSize.Pop();
			}
			ParseClipAndMask(node, sceneNode);
			if (string.IsNullOrEmpty(text) || !postponedSymbolData.TryGetValue(text, out var value2))
			{
				return;
			}
			foreach (NodeReferenceData item in value2)
			{
				ResolveReferencedNode(sceneNode, item, isDeferred: true);
			}
		}

		private void use()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			SceneNode sceneNode = currentSceneNode.Peek();
			ParseOpacity(sceneNode);
			Rect viewport = ParseViewport(node, sceneNode, Vector2.zero);
			NodeReferenceData nodeReferenceData = default(NodeReferenceData);
			nodeReferenceData.node = sceneNode;
			nodeReferenceData.viewport = viewport;
			nodeReferenceData.id = node["id"];
			NodeReferenceData nodeReferenceData2 = nodeReferenceData;
			string text = node["xlink:href"];
			SceneNode sceneNode2 = SVGAttribParser.ParseRelativeRef(text, svgObjects) as SceneNode;
			if (sceneNode2 == null && !string.IsNullOrEmpty(text) && text.StartsWith("#"))
			{
				text = text.Substring(1);
				if (!postponedSymbolData.TryGetValue(text, out var value))
				{
					value = new List<NodeReferenceData>();
					postponedSymbolData[text] = value;
				}
				value.Add(nodeReferenceData2);
			}
			sceneNode.Transform = SVGAttribParser.ParseTransform(node);
			sceneNode.Transform *= Matrix2D.Translate(viewport.position);
			if (sceneNode2 != null)
			{
				ResolveReferencedNode(sceneNode2, nodeReferenceData2, isDeferred: false);
			}
			ParseClipAndMask(node, sceneNode);
			AddToSVGDictionaryIfPossible(node, sceneNode);
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node);
			}
		}

		private void style()
		{
			XmlReaderIterator.Node node = docReader.VisitCurrent();
			string text = docReader.ReadTextWithinElement();
			if (text.Length > 0)
			{
				styles.SetGlobalStyleSheet(SVGStyleSheetUtils.Parse(text));
			}
			if (ShouldDeclareSupportedChildren(node))
			{
				SupportElems(node);
			}
		}

		private void ResolveReferencedNode(SceneNode referencedNode, NodeReferenceData refData, bool isDeferred)
		{
			if (symbolViewBoxes.TryGetValue(referencedNode, out var value))
			{
				ApplyViewBox(refData.node, value, refData.viewport);
			}
			if (refData.node.Children == null)
			{
				refData.node.Children = new List<SceneNode>();
			}
			SVGStyleResolver.StyleLayer styleLayer = null;
			if (isDeferred)
			{
				styleLayer = styles.GetLayerForScenNode(refData.node);
				if (styleLayer != null)
				{
					styles.PushLayer(styleLayer);
				}
			}
			SVGStyleResolver.StyleLayer styleLayer2 = nodeStyleLayers[referencedNode];
			if (styleLayer2 != null)
			{
				styles.PushLayer(styleLayer2);
			}
			List<SceneNode> list = new List<SceneNode>(10);
			foreach (SceneNode item in VectorUtils.SceneNodes(referencedNode))
			{
				list.Add(item);
			}
			SceneNode sceneNode = CloneSceneNode(referencedNode);
			int num = 0;
			foreach (SceneNode item2 in VectorUtils.SceneNodes(sceneNode))
			{
				int index = num++;
				if (item2.Shapes == null)
				{
					continue;
				}
				SceneNode node = list[index];
				SVGStyleResolver.StyleLayer layerForScenNode = styles.GetLayerForScenNode(node);
				if (layerForScenNode != null)
				{
					styles.PushLayer(layerForScenNode);
				}
				bool isDefaultFill;
				IFill fill = SVGAttribParser.ParseFill(null, svgObjects, postponedFills, styles, Inheritance.Inherited, out isDefaultFill);
				PathCorner strokeCorner;
				PathEnding strokeEnding;
				Stroke stroke = ParseStrokeAttributeSet(null, out strokeCorner, out strokeEnding);
				foreach (Shape shape in item2.Shapes)
				{
					PathProperties pathProps = shape.PathProps;
					pathProps.Stroke = stroke;
					pathProps.Corners = strokeCorner;
					pathProps.Head = strokeEnding;
					shape.PathProps = pathProps;
					shape.Fill = (isDefaultFill ? shape.Fill : fill);
				}
				if (layerForScenNode != null)
				{
					styles.PopLayer();
				}
			}
			if (styleLayer2 != null)
			{
				styles.PopLayer();
			}
			if (styleLayer != null)
			{
				styles.PopLayer();
			}
			if (!string.IsNullOrEmpty(refData.id))
			{
				nodeIDs[refData.id] = sceneNode;
			}
			refData.node.Children.Add(sceneNode);
		}

		private SceneNode CloneSceneNode(SceneNode node)
		{
			if (node == null)
			{
				return null;
			}
			List<SceneNode> list = null;
			if (node.Children != null)
			{
				list = new List<SceneNode>(node.Children.Count);
				foreach (SceneNode child in node.Children)
				{
					list.Add(CloneSceneNode(child));
				}
			}
			List<Shape> list2 = null;
			if (node.Shapes != null)
			{
				list2 = new List<Shape>(node.Shapes.Count);
				foreach (Shape shape in node.Shapes)
				{
					list2.Add(CloneShape(shape));
				}
			}
			SceneNode sceneNode = new SceneNode
			{
				Children = list,
				Shapes = list2,
				Transform = node.Transform,
				Clipper = CloneSceneNode(node.Clipper)
			};
			if (nodeGlobalSceneState.ContainsKey(node))
			{
				nodeGlobalSceneState[sceneNode] = nodeGlobalSceneState[node];
			}
			if (nodeOpacity.ContainsKey(node))
			{
				nodeOpacity[sceneNode] = nodeOpacity[node];
			}
			return sceneNode;
		}

		private Shape CloneShape(Shape shape)
		{
			if (shape == null)
			{
				return null;
			}
			BezierContour[] array = null;
			if (shape.Contours != null)
			{
				array = new BezierContour[shape.Contours.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = CloneContour(shape.Contours[i]);
				}
			}
			return new Shape
			{
				Fill = CloneFill(shape.Fill),
				FillTransform = shape.FillTransform,
				PathProps = ClonePathProps(shape.PathProps),
				Contours = array,
				IsConvex = shape.IsConvex
			};
		}

		private BezierContour CloneContour(BezierContour c)
		{
			BezierPathSegment[] array = null;
			if (c.Segments != null)
			{
				array = new BezierPathSegment[c.Segments.Length];
				for (int i = 0; i < array.Length; i++)
				{
					BezierPathSegment bezierPathSegment = c.Segments[i];
					array[i] = new BezierPathSegment
					{
						P0 = bezierPathSegment.P0,
						P1 = bezierPathSegment.P1,
						P2 = bezierPathSegment.P2
					};
				}
			}
			BezierContour result = default(BezierContour);
			result.Segments = array;
			result.Closed = c.Closed;
			return result;
		}

		private IFill CloneFill(IFill fill)
		{
			if (fill == null)
			{
				return null;
			}
			IFill result = null;
			if (fill is SolidFill)
			{
				SolidFill solidFill = fill as SolidFill;
				result = new SolidFill
				{
					Color = solidFill.Color,
					Opacity = solidFill.Opacity,
					Mode = solidFill.Mode
				};
			}
			else if (fill is GradientFill)
			{
				GradientFill gradientFill = fill as GradientFill;
				GradientStop[] array = null;
				if (gradientFill.Stops != null)
				{
					array = new GradientStop[gradientFill.Stops.Length];
					for (int i = 0; i < array.Length; i++)
					{
						GradientStop gradientStop = gradientFill.Stops[i];
						array[i] = new GradientStop
						{
							Color = gradientStop.Color,
							StopPercentage = gradientStop.StopPercentage
						};
					}
				}
				GradientFill gradientFill2 = new GradientFill
				{
					Type = gradientFill.Type,
					Stops = array,
					Mode = gradientFill.Mode,
					Opacity = gradientFill.Opacity,
					Addressing = gradientFill.Addressing,
					RadialFocus = gradientFill.RadialFocus
				};
				gradientExInfo[gradientFill2] = gradientExInfo[gradientFill];
				result = gradientFill2;
			}
			else if (fill is TextureFill)
			{
				TextureFill textureFill = fill as TextureFill;
				result = new TextureFill
				{
					Texture = textureFill.Texture,
					Mode = textureFill.Mode,
					Opacity = textureFill.Opacity,
					Addressing = textureFill.Addressing
				};
			}
			else if (fill is PatternFill)
			{
				PatternFill patternFill = fill as PatternFill;
				result = new PatternFill
				{
					Mode = patternFill.Mode,
					Opacity = patternFill.Opacity,
					Pattern = CloneSceneNode(patternFill.Pattern),
					Rect = patternFill.Rect
				};
			}
			return result;
		}

		private PathProperties ClonePathProps(PathProperties props)
		{
			Stroke stroke = null;
			if (props.Stroke != null)
			{
				float[] array = null;
				if (props.Stroke.Pattern != null)
				{
					array = new float[props.Stroke.Pattern.Length];
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = props.Stroke.Pattern[i];
					}
				}
				stroke = new Stroke
				{
					Fill = CloneFill(props.Stroke.Fill),
					FillTransform = props.Stroke.FillTransform,
					HalfThickness = props.Stroke.HalfThickness,
					Pattern = array,
					PatternOffset = props.Stroke.PatternOffset,
					TippedCornerLimit = props.Stroke.TippedCornerLimit
				};
			}
			PathProperties result = default(PathProperties);
			result.Stroke = stroke;
			result.Head = props.Head;
			result.Tail = props.Tail;
			result.Corners = props.Corners;
			return result;
		}

		private GradientFill CloneGradientFill(GradientFill other)
		{
			if (other == null)
			{
				return null;
			}
			return new GradientFill
			{
				Type = other.Type,
				Stops = other.Stops,
				Mode = other.Mode,
				Opacity = other.Opacity,
				Addressing = other.Addressing,
				RadialFocus = other.RadialFocus
			};
		}

		private int AttribIntVal(string attribName)
		{
			return AttribIntVal(attribName, 0);
		}

		private int AttribIntVal(string attribName, int defaultVal)
		{
			string text = styles.Evaluate(attribName);
			if (text == null)
			{
				return defaultVal;
			}
			return int.Parse(text);
		}

		private float AttribFloatVal(string attribName)
		{
			return AttribFloatVal(attribName, 0f);
		}

		private float AttribFloatVal(string attribName, float defaultVal)
		{
			string text = styles.Evaluate(attribName);
			if (text == null)
			{
				return defaultVal;
			}
			return SVGAttribParser.ParseFloat(text);
		}

		private float AttribLengthVal(XmlReaderIterator.Node node, string attribName, DimType dimType)
		{
			return AttribLengthVal(node, attribName, 0f, dimType);
		}

		private float AttribLengthVal(XmlReaderIterator.Node node, string attribName, float defaultUnitVal, DimType dimType)
		{
			string val = styles.Evaluate(attribName);
			return AttribLengthVal(val, node, attribName, defaultUnitVal, dimType);
		}

		private float AttribLengthVal(string val, XmlReaderIterator.Node node, string attribName, float defaultUnitVal, DimType dimType)
		{
			if (val == null)
			{
				return defaultUnitVal;
			}
			val = val.Trim();
			string text = "px";
			char c = val[val.Length - 1];
			if (c == '%')
			{
				float num = SVGAttribParser.ParseFloat(val.Substring(0, val.Length - 1));
				if (num < 0f)
				{
					throw node.GetException("Number in " + attribName + " cannot be negative");
				}
				num /= 100f;
				Vector2 vector = ((currentViewBoxSize.Count > 0) ? currentViewBoxSize.Peek() : currentContainerSize.Peek());
				switch (dimType)
				{
				case DimType.Width:
					return num * vector.x;
				case DimType.Height:
					return num * vector.y;
				case DimType.Length:
					return num * vector.magnitude / 1.4142135f;
				}
			}
			else if (val.Length >= 2)
			{
				text = val.Substring(val.Length - 2);
			}
			if (char.IsDigit(c) || c == '.')
			{
				return SVGAttribParser.ParseFloat(val);
			}
			float num2 = SVGAttribParser.ParseFloat(val.Substring(0, val.Length - 2));
			return text switch
			{
				"em" => throw new NotImplementedException(), 
				"ex" => throw new NotImplementedException(), 
				"px" => num2, 
				"in" => 90f * num2 * dpiScale, 
				"cm" => 35.43307f * num2 * dpiScale, 
				"mm" => 3.543307f * num2 * dpiScale, 
				"pt" => 1.25f * num2 * dpiScale, 
				"pc" => 15f * num2 * dpiScale, 
				_ => throw new FormatException("Unknown length unit type (" + text + ")"), 
			};
		}

		private void AddToSVGDictionaryIfPossible(XmlReaderIterator.Node node, object vectorElement)
		{
			string text = node["id"];
			if (!string.IsNullOrEmpty(text))
			{
				svgObjects[text] = vectorElement;
			}
		}

		private Rect ParseViewport(XmlReaderIterator.Node node, SceneNode sceneNode, Vector2 defaultViewportSize)
		{
			scenePos.x = AttribLengthVal(node, "x", DimType.Width);
			scenePos.y = AttribLengthVal(node, "y", DimType.Height);
			sceneSize.x = AttribLengthVal(node, "width", defaultViewportSize.x, DimType.Width);
			sceneSize.y = AttribLengthVal(node, "height", defaultViewportSize.y, DimType.Height);
			return new Rect(scenePos, sceneSize);
		}

		private ViewBoxInfo ParseViewBox(XmlReaderIterator.Node node, SceneNode sceneNode, Rect sceneViewport)
		{
			ViewBoxInfo viewBoxInfo = default(ViewBoxInfo);
			viewBoxInfo.IsEmpty = true;
			ViewBoxInfo viewBoxInfo2 = viewBoxInfo;
			string text = node["viewBox"]?.Trim();
			if (string.IsNullOrEmpty(text))
			{
				return viewBoxInfo2;
			}
			string[] array = text.Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 4)
			{
				throw node.GetException("Invalid viewBox specification");
			}
			Vector2 position = new Vector2(AttribLengthVal(array[0], node, "viewBox", 0f, DimType.Width), AttribLengthVal(array[1], node, "viewBox", 0f, DimType.Height));
			Vector2 size = new Vector2(AttribLengthVal(array[2], node, "viewBox", sceneViewport.width, DimType.Width), AttribLengthVal(array[3], node, "viewBox", sceneViewport.height, DimType.Height));
			viewBoxInfo2.ViewBox = new Rect(position, size);
			ParseViewBoxAspectRatio(node, ref viewBoxInfo2);
			viewBoxInfo2.IsEmpty = false;
			return viewBoxInfo2;
		}

		private void ParseViewBoxAspectRatio(XmlReaderIterator.Node node, ref ViewBoxInfo viewBoxInfo)
		{
			viewBoxInfo.AspectRatio = ViewBoxAspectRatio.FitLargestDim;
			viewBoxInfo.AlignX = ViewBoxAlign.Mid;
			viewBoxInfo.AlignY = ViewBoxAlign.Mid;
			string text = node["preserveAspectRatio"]?.Trim();
			bool flag = false;
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					switch (array[i])
					{
					case "none":
						flag = true;
						break;
					case "xMinYMin":
						viewBoxInfo.AlignX = ViewBoxAlign.Min;
						viewBoxInfo.AlignY = ViewBoxAlign.Min;
						break;
					case "xMidYMin":
						viewBoxInfo.AlignX = ViewBoxAlign.Mid;
						viewBoxInfo.AlignY = ViewBoxAlign.Min;
						break;
					case "xMaxYMin":
						viewBoxInfo.AlignX = ViewBoxAlign.Max;
						viewBoxInfo.AlignY = ViewBoxAlign.Min;
						break;
					case "xMinYMid":
						viewBoxInfo.AlignX = ViewBoxAlign.Min;
						viewBoxInfo.AlignY = ViewBoxAlign.Mid;
						break;
					case "xMidYMid":
						viewBoxInfo.AlignX = ViewBoxAlign.Mid;
						viewBoxInfo.AlignY = ViewBoxAlign.Mid;
						break;
					case "xMaxYMid":
						viewBoxInfo.AlignX = ViewBoxAlign.Max;
						viewBoxInfo.AlignY = ViewBoxAlign.Mid;
						break;
					case "xMinYMax":
						viewBoxInfo.AlignX = ViewBoxAlign.Min;
						viewBoxInfo.AlignY = ViewBoxAlign.Max;
						break;
					case "xMidYMax":
						viewBoxInfo.AlignX = ViewBoxAlign.Mid;
						viewBoxInfo.AlignY = ViewBoxAlign.Max;
						break;
					case "xMaxYMax":
						viewBoxInfo.AlignX = ViewBoxAlign.Max;
						viewBoxInfo.AlignY = ViewBoxAlign.Max;
						break;
					case "meet":
						viewBoxInfo.AspectRatio = ViewBoxAspectRatio.FitLargestDim;
						break;
					case "slice":
						viewBoxInfo.AspectRatio = ViewBoxAspectRatio.FitSmallestDim;
						break;
					}
				}
			}
			if (flag)
			{
				viewBoxInfo.AspectRatio = ViewBoxAspectRatio.DontPreserve;
			}
		}

		private void ApplyViewBox(SceneNode sceneNode, ViewBoxInfo viewBoxInfo, Rect sceneViewport)
		{
			if (viewBoxInfo.ViewBox.size == Vector2.zero || sceneViewport.size == Vector2.zero)
			{
				return;
			}
			Vector2 vector = Vector2.one;
			Vector2 vector2 = -viewBoxInfo.ViewBox.position;
			if (viewBoxInfo.AspectRatio == ViewBoxAspectRatio.DontPreserve)
			{
				vector = sceneViewport.size / viewBoxInfo.ViewBox.size;
			}
			else
			{
				vector.x = (vector.y = sceneViewport.width / viewBoxInfo.ViewBox.width);
				bool flag = ((viewBoxInfo.AspectRatio != ViewBoxAspectRatio.FitLargestDim) ? (viewBoxInfo.ViewBox.height * vector.y > sceneViewport.height) : (viewBoxInfo.ViewBox.height * vector.y <= sceneViewport.height));
				Vector2 zero = Vector2.zero;
				if (flag)
				{
					if (viewBoxInfo.AlignY == ViewBoxAlign.Mid)
					{
						zero.y = (sceneViewport.height - viewBoxInfo.ViewBox.height * vector.y) * 0.5f;
					}
					else if (viewBoxInfo.AlignY == ViewBoxAlign.Max)
					{
						zero.y = sceneViewport.height - viewBoxInfo.ViewBox.height * vector.y;
					}
				}
				else
				{
					vector.x = (vector.y = sceneViewport.height / viewBoxInfo.ViewBox.height);
					if (viewBoxInfo.AlignX == ViewBoxAlign.Mid)
					{
						zero.x = (sceneViewport.width - viewBoxInfo.ViewBox.width * vector.x) * 0.5f;
					}
					else if (viewBoxInfo.AlignX == ViewBoxAlign.Max)
					{
						zero.x = sceneViewport.width - viewBoxInfo.ViewBox.width * vector.x;
					}
				}
				vector2 += zero / vector;
			}
			sceneNode.Transform = sceneNode.Transform * Matrix2D.Scale(vector) * Matrix2D.Translate(vector2);
		}

		private Stroke ParseStrokeAttributeSet(XmlReaderIterator.Node node, out PathCorner strokeCorner, out PathEnding strokeEnding, Inheritance inheritance = Inheritance.Inherited)
		{
			Stroke stroke = SVGAttribParser.ParseStrokeAndOpacity(node, svgObjects, styles, inheritance);
			strokeCorner = PathCorner.Tipped;
			strokeEnding = PathEnding.Chop;
			if (stroke != null)
			{
				string val = styles.Evaluate("stroke-width", inheritance);
				stroke.HalfThickness = AttribLengthVal(val, node, "stroke-width", 1f, DimType.Length) * 0.5f;
				switch (styles.Evaluate("stroke-linecap", inheritance))
				{
				case "butt":
					strokeEnding = PathEnding.Chop;
					break;
				case "square":
					strokeEnding = PathEnding.Square;
					break;
				case "round":
					strokeEnding = PathEnding.Round;
					break;
				}
				switch (styles.Evaluate("stroke-linejoin", inheritance))
				{
				case "miter":
					strokeCorner = PathCorner.Tipped;
					break;
				case "round":
					strokeCorner = PathCorner.Round;
					break;
				case "bevel":
					strokeCorner = PathCorner.Beveled;
					break;
				}
				string text = styles.Evaluate("stroke-dasharray", inheritance);
				if (text != null && text != "none")
				{
					string[] array = text.Split(whiteSpaceNumberChars, StringSplitOptions.RemoveEmptyEntries);
					int num = (((array.Length & 1) == 1) ? (array.Length * 2) : array.Length);
					stroke.Pattern = new float[num];
					for (int i = 0; i < array.Length; i++)
					{
						stroke.Pattern[i] = AttribLengthVal(array[i], node, "stroke-dasharray", 0f, DimType.Length);
					}
					if (num > array.Length)
					{
						for (int j = 0; j < array.Length; j++)
						{
							stroke.Pattern[j + array.Length] = stroke.Pattern[j];
						}
					}
					string val2 = styles.Evaluate("stroke-dashoffset", inheritance);
					stroke.PatternOffset = AttribLengthVal(val2, node, "stroke-dashoffset", 0f, DimType.Length);
				}
				string val3 = styles.Evaluate("stroke-miterlimit", inheritance);
				stroke.TippedCornerLimit = AttribLengthVal(val3, node, "stroke-miterlimit", 4f, DimType.Length);
				if (stroke.TippedCornerLimit < 1f)
				{
					throw node.GetException("'stroke-miterlimit' should be greater or equal to 1");
				}
			}
			return stroke;
		}

		private void ParseID(XmlReaderIterator.Node node, SceneNode sceneNode)
		{
			string text = node["id"];
			if (!string.IsNullOrEmpty(text))
			{
				nodeIDs[text] = sceneNode;
				nodeStyleLayers[sceneNode] = styles.PeekLayer();
			}
		}

		private float ParseOpacity(SceneNode sceneNode)
		{
			float num = AttribFloatVal("opacity", 1f);
			if (num != 1f && sceneNode != null)
			{
				nodeOpacity[sceneNode] = num;
			}
			return num;
		}

		private void ParseClipAndMask(XmlReaderIterator.Node node, SceneNode sceneNode)
		{
			ParseClip(node, sceneNode);
			ParseMask(node, sceneNode);
		}

		private void ParseClip(XmlReaderIterator.Node node, SceneNode sceneNode)
		{
			string text = null;
			string text2 = styles.Evaluate("clip-path");
			if (text2 != null)
			{
				text = SVGAttribParser.ParseURLRef(text2);
			}
			if (text == null)
			{
				return;
			}
			SceneNode sceneNode2 = SVGAttribParser.ParseRelativeRef(text, svgObjects) as SceneNode;
			if (sceneNode2 == null && text.Length > 1 && text.StartsWith("#"))
			{
				if (!postponedClip.TryGetValue(text, out var value))
				{
					value = new List<PostponedClip>(1);
				}
				value.Add(new PostponedClip
				{
					node = sceneNode
				});
				postponedClip[text.Substring(1)] = value;
			}
			else
			{
				bool worldRelative = true;
				if (clipData.TryGetValue(sceneNode2, out var value2))
				{
					worldRelative = value2.WorldRelative;
				}
				ApplyClipper(sceneNode2, sceneNode, worldRelative);
			}
		}

		private void ApplyClipper(SceneNode clipper, SceneNode target, bool worldRelative)
		{
			SceneNode clipper2 = clipper;
			if (!worldRelative)
			{
				Rect rect = VectorUtils.SceneNodeBounds(target);
				Matrix2D transform = Matrix2D.Translate(rect.position) * Matrix2D.Scale(rect.size);
				clipper2 = new SceneNode
				{
					Children = new List<SceneNode> { clipper },
					Transform = transform
				};
			}
			target.Clipper = clipper2;
		}

		private void ParseMask(XmlReaderIterator.Node node, SceneNode sceneNode)
		{
			string text = null;
			string text2 = node["mask"];
			if (text2 != null)
			{
				text = SVGAttribParser.ParseURLRef(text2);
			}
			if (text != null)
			{
				SceneNode sceneNode2 = SVGAttribParser.ParseRelativeRef(text, svgObjects) as SceneNode;
				SceneNode clipper = sceneNode2;
				if (maskData.TryGetValue(sceneNode2, out var value) && !value.ContentWorldRelative)
				{
					Rect rect = VectorUtils.SceneNodeBounds(sceneNode);
					Matrix2D transform = Matrix2D.Translate(rect.position) * Matrix2D.Scale(rect.size);
					clipper = new SceneNode
					{
						Children = new List<SceneNode> { sceneNode2 },
						Transform = transform
					};
				}
				sceneNode.Clipper = clipper;
			}
		}

		private Texture2D DecodeTextureData(string dataURI)
		{
			int i = 5;
			int length = dataURI.Length;
			int num = i;
			for (; i < length && dataURI[i] != ';' && dataURI[i] != ','; i++)
			{
			}
			string text = dataURI.Substring(num, i - num);
			if (text != "image/png" && text != "image/jpeg")
			{
				return null;
			}
			for (; i < length && dataURI[i] != ','; i++)
			{
			}
			i++;
			if (i >= length)
			{
				return null;
			}
			byte[] data = Convert.FromBase64String(dataURI.Substring(i));
			Texture2D texture2D = new Texture2D(1, 1);
			if (texture2D.LoadImage(data))
			{
				return texture2D;
			}
			return null;
		}

		private void PostProcess(SceneNode root)
		{
			AdjustFills(root);
		}

		private void AdjustFills(SceneNode root)
		{
			List<HierarchyUpdate> list = new List<HierarchyUpdate>();
			foreach (VectorUtils.SceneNodeWorldTransform item in VectorUtils.WorldTransformedSceneNodes(root, nodeOpacity))
			{
				if (item.Node.Shapes == null)
				{
					continue;
				}
				foreach (Shape shape in item.Node.Shapes)
				{
					if (shape.Fill != null && postponedFills.TryGetValue(shape.Fill, out var value) && SVGAttribParser.ParseRelativeRef(value, svgObjects) is IFill fill)
					{
						shape.Fill = fill;
					}
					Stroke stroke = shape.PathProps.Stroke;
					if (stroke != null && stroke.Fill is GradientFill)
					{
						Matrix2D computedTransform = Matrix2D.identity;
						AdjustGradientFill(item.Node, item.WorldTransform, stroke.Fill, shape.Contours, ref computedTransform);
						stroke.FillTransform = computedTransform;
					}
					if (shape.Fill is GradientFill)
					{
						Matrix2D computedTransform2 = Matrix2D.identity;
						AdjustGradientFill(item.Node, item.WorldTransform, shape.Fill, shape.Contours, ref computedTransform2);
						shape.FillTransform = computedTransform2;
					}
					else if (shape.Fill is PatternFill)
					{
						SceneNode sceneNode = AdjustPatternFill(item.Node, item.WorldTransform, shape);
						if (sceneNode != null)
						{
							list.Add(new HierarchyUpdate
							{
								Parent = item.Parent,
								NewNode = sceneNode,
								ReplaceNode = item.Node
							});
						}
					}
				}
			}
			foreach (HierarchyUpdate item2 in list)
			{
				int index = item2.Parent.Children.IndexOf(item2.ReplaceNode);
				item2.Parent.Children.RemoveAt(index);
				item2.Parent.Children.Insert(index, item2.NewNode);
			}
		}

		private void AdjustGradientFill(SceneNode node, Matrix2D worldTransform, IFill fill, BezierContour[] contours, ref Matrix2D computedTransform)
		{
			GradientFill gradientFill = fill as GradientFill;
			if (fill == null || contours == null || contours.Length == 0)
			{
				return;
			}
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			foreach (BezierContour bezierContour in contours)
			{
				Rect rect = VectorUtils.Bounds(bezierContour.Segments);
				vector = Vector2.Min(vector, rect.min);
				vector2 = Vector2.Max(vector2, rect.max);
			}
			Rect rect2 = new Rect(vector, vector2 - vector);
			GradientExData gradientExData = gradientExInfo[gradientFill];
			Vector2 containerSize = nodeGlobalSceneState[node].ContainerSize;
			Matrix2D matrix2D = Matrix2D.identity;
			currentContainerSize.Push(gradientExData.WorldRelative ? containerSize : Vector2.one);
			if (gradientExData is LinearGradientExData)
			{
				LinearGradientExData linearGradientExData = (LinearGradientExData)gradientExData;
				Vector2 vector3 = new Vector2(AttribLengthVal(linearGradientExData.X1, null, null, 0f, DimType.Width), AttribLengthVal(linearGradientExData.Y1, null, null, 0f, DimType.Height));
				Vector2 vector4 = new Vector2(AttribLengthVal(linearGradientExData.X2, null, null, currentContainerSize.Peek().x, DimType.Width), AttribLengthVal(linearGradientExData.Y2, null, null, 0f, DimType.Height)) - vector3;
				float num = 1f / vector4.magnitude;
				Matrix2D matrix2D2 = Matrix2D.Scale(new Vector2(rect2.width * num, rect2.height * num));
				Matrix2D matrix2D3 = Matrix2D.RotateLH(Mathf.Atan2(vector4.y, vector4.x));
				Matrix2D matrix2D4 = Matrix2D.Translate(-vector3);
				matrix2D = matrix2D2 * matrix2D3 * matrix2D4;
			}
			else if (gradientExData is RadialGradientExData)
			{
				RadialGradientExData radialGradientExData = (RadialGradientExData)gradientExData;
				Vector2 vector5 = currentContainerSize.Peek() * 0.5f;
				Vector2 vector6 = new Vector2(AttribLengthVal(radialGradientExData.Cx, null, null, vector5.x, DimType.Width), AttribLengthVal(radialGradientExData.Cy, null, null, vector5.y, DimType.Height));
				Vector2 vector7 = new Vector2(AttribLengthVal(radialGradientExData.Fx, null, null, vector6.x, DimType.Width), AttribLengthVal(radialGradientExData.Fy, null, null, vector6.y, DimType.Height));
				float num2 = AttribLengthVal(radialGradientExData.R, null, null, vector5.magnitude / 1.4142135f, DimType.Length);
				if (!radialGradientExData.Parsed)
				{
					gradientFill.RadialFocus = (vector7 - vector6) / num2;
					if (gradientFill.RadialFocus.sqrMagnitude > 1f - VectorUtils.Epsilon)
					{
						gradientFill.RadialFocus = gradientFill.RadialFocus.normalized * (1f - VectorUtils.Epsilon);
					}
					radialGradientExData.Parsed = true;
				}
				matrix2D = Matrix2D.Scale(rect2.size * 0.5f / num2) * Matrix2D.Translate(new Vector2(num2, num2) - vector6);
			}
			else
			{
				Debug.LogError("Unsupported gradient type: " + gradientExData);
			}
			currentContainerSize.Pop();
			Matrix2D matrix2D5 = (gradientExData.WorldRelative ? (Matrix2D.Translate(rect2.min) * Matrix2D.Scale(rect2.size)) : Matrix2D.identity);
			Vector2 vector8 = new Vector2(1f / rect2.width, 1f / rect2.height);
			computedTransform = Matrix2D.Scale(vector8) * matrix2D * gradientExData.FillTransform.Inverse() * matrix2D5;
		}

		private SceneNode AdjustPatternFill(SceneNode node, Matrix2D worldTransform, Shape shape)
		{
			if (!(shape.Fill is PatternFill { Rect: var rect } patternFill) || Mathf.Abs(rect.width) < VectorUtils.Epsilon || Mathf.Abs(patternFill.Rect.height) < VectorUtils.Epsilon)
			{
				return null;
			}
			PatternData patternData = this.patternData[patternFill.Pattern];
			Rect rect2 = VectorUtils.SceneNodeBounds(node);
			Rect rect3 = patternFill.Rect;
			if (!patternData.WorldRelative)
			{
				rect3.position *= rect2.size;
				rect3.size *= rect2.size;
			}
			SceneNode sceneNode = new SceneNode
			{
				Transform = node.Transform,
				Children = new List<SceneNode>(2)
			};
			node.Transform = Matrix2D.identity;
			SceneNode sceneNode2 = patternFill.Pattern;
			if (!patternData.ContentWorldRelative)
			{
				sceneNode2 = new SceneNode
				{
					Transform = Matrix2D.Scale(rect2.size),
					Children = new List<SceneNode> { patternFill.Pattern }
				};
			}
			PostProcess(sceneNode2);
			SceneNode sceneNode3 = new SceneNode
			{
				Transform = patternData.PatternTransform,
				Children = new List<SceneNode>(20)
			};
			SceneNode item = new SceneNode
			{
				Transform = Matrix2D.identity,
				Children = new List<SceneNode> { sceneNode3 },
				Clipper = node
			};
			Shape shape2 = new Shape();
			VectorUtils.MakeRectangleShape(shape2, new Rect(0f, 0f, rect3.width, rect3.height));
			SceneNode clipper = new SceneNode
			{
				Transform = Matrix2D.identity,
				Shapes = new List<Shape> { shape2 }
			};
			Rect rect4 = VectorUtils.SceneNodeBounds(node);
			Matrix2D matrix2D = patternData.PatternTransform.Inverse();
			rect4 = VectorUtils.Bounds(new Vector2[4]
			{
				matrix2D * new Vector2(rect4.xMin, rect4.yMin),
				matrix2D * new Vector2(rect4.xMax, rect4.yMin),
				matrix2D * new Vector2(rect4.xMax, rect4.yMax),
				matrix2D * new Vector2(rect4.xMin, rect4.yMax)
			});
			float num = rect4.xMax / rect3.width;
			float num2 = rect4.yMax / rect3.height;
			if (Mathf.Abs(rect3.width) < VectorUtils.Epsilon || Mathf.Abs(rect3.height) < VectorUtils.Epsilon || num * num2 > 5000f)
			{
				Debug.LogWarning("Ignoring pattern which would result in too many repetitions");
				return null;
			}
			Vector2 position = rect3.position;
			float num3 = (float)(int)(rect4.x / rect3.width) * rect3.width - rect3.width;
			for (float num4 = (float)(int)(rect4.y / rect3.height) * rect3.height - rect3.height; num4 < rect4.yMax; num4 += rect3.height)
			{
				for (float num5 = num3; num5 < rect4.xMax; num5 += rect3.width)
				{
					SceneNode item2 = new SceneNode
					{
						Transform = Matrix2D.Translate(new Vector2(num5, num4) + position),
						Children = new List<SceneNode> { sceneNode2 },
						Clipper = clipper
					};
					sceneNode3.Children.Add(item2);
				}
			}
			sceneNode.Children.Add(item);
			sceneNode.Children.Add(node);
			return sceneNode;
		}

		private void RemoveInvisibleNodes()
		{
			foreach (NodeWithParent invisibleNode in invisibleNodes)
			{
				if (invisibleNode.parent.Children != null)
				{
					invisibleNode.parent.Children.Remove(invisibleNode.node);
				}
			}
		}

		private bool ShouldDeclareSupportedChildren(XmlReaderIterator.Node node)
		{
			return !subTags.ContainsKey(node.Name);
		}

		private void SupportElems(XmlReaderIterator.Node node, params ElemHandler[] handlers)
		{
			Handlers handlers2 = new Handlers(handlers.Length);
			foreach (ElemHandler elemHandler in handlers)
			{
				handlers2[elemHandler.Method.Name] = elemHandler;
			}
			subTags[node.Name] = handlers2;
		}
	}
}
