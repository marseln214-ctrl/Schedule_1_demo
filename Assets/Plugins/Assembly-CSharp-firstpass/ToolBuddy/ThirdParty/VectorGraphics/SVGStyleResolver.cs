using System.Collections.Generic;
using System.Linq;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	internal class SVGStyleResolver
	{
		public struct NodeData
		{
			public XmlReaderIterator.Node node;

			public string name;

			public List<string> classes;

			public string id;
		}

		public class StyleLayer
		{
			public SVGStyleSheet styleSheet;

			public SVGPropertySheet attributeSheet;

			public NodeData nodeData;
		}

		private List<StyleLayer> layers = new List<StyleLayer>();

		private SVGStyleSheet globalStyleSheet = new SVGStyleSheet();

		private Dictionary<SceneNode, StyleLayer> nodeLayers = new Dictionary<SceneNode, StyleLayer>();

		public void PushNode(XmlReaderIterator.Node node)
		{
			NodeData nodeData = default(NodeData);
			nodeData.node = node;
			nodeData.name = node.Name;
			if (node["class"] != null)
			{
				nodeData.classes = (from x in node["class"].Split(' ')
					select x.Trim()).ToList();
			}
			else
			{
				nodeData.classes = new List<string>();
			}
			nodeData.classes = SortedClasses(nodeData.classes).ToList();
			nodeData.id = node["id"];
			StyleLayer styleLayer = new StyleLayer();
			styleLayer.nodeData = nodeData;
			styleLayer.attributeSheet = node.GetAttributes();
			styleLayer.styleSheet = new SVGStyleSheet();
			string text = node["style"];
			if (text != null)
			{
				SVGPropertySheet value = SVGStyleSheetUtils.ParseInline(text);
				styleLayer.styleSheet[node.Name] = value;
			}
			PushLayer(styleLayer);
		}

		public void PopNode()
		{
			PopLayer();
		}

		public void PushLayer(StyleLayer layer)
		{
			layers.Add(layer);
		}

		public void PopLayer()
		{
			if (layers.Count == 0)
			{
				throw SVGFormatException.StackError;
			}
			layers.RemoveAt(layers.Count - 1);
		}

		public StyleLayer PeekLayer()
		{
			if (layers.Count == 0)
			{
				return null;
			}
			return layers[layers.Count - 1];
		}

		public void SaveLayerForSceneNode(SceneNode node)
		{
			nodeLayers[node] = PeekLayer();
		}

		public StyleLayer GetLayerForScenNode(SceneNode node)
		{
			if (!nodeLayers.ContainsKey(node))
			{
				return null;
			}
			return nodeLayers[node];
		}

		public void SetGlobalStyleSheet(SVGStyleSheet sheet)
		{
			foreach (string selector in sheet.selectors)
			{
				globalStyleSheet[selector] = sheet[selector];
			}
		}

		public string Evaluate(string attribName, Inheritance inheritance = Inheritance.None)
		{
			for (int num = layers.Count - 1; num >= 0; num--)
			{
				string attrib = null;
				if (LookupStyleOrAttribute(layers[num], attribName, inheritance, out attrib))
				{
					return attrib;
				}
				if (inheritance == Inheritance.None)
				{
					break;
				}
			}
			return null;
		}

		private bool LookupStyleOrAttribute(StyleLayer layer, string attribName, Inheritance inheritance, out string attrib)
		{
			if (LookupProperty(layer.nodeData, attribName, layer.styleSheet, out attrib))
			{
				return true;
			}
			if (LookupProperty(layer.nodeData, attribName, globalStyleSheet, out attrib))
			{
				return true;
			}
			if (layer.attributeSheet.ContainsKey(attribName))
			{
				attrib = layer.attributeSheet[attribName];
				return true;
			}
			return false;
		}

		private bool LookupProperty(NodeData nodeData, string attribName, SVGStyleSheet sheet, out string val)
		{
			string selector = (string.IsNullOrEmpty(nodeData.id) ? null : ("#" + nodeData.id));
			string selector2 = (string.IsNullOrEmpty(nodeData.name) ? null : nodeData.name);
			if (LookupPropertyInSheet(sheet, attribName, selector, out val))
			{
				return true;
			}
			foreach (string @class in nodeData.classes)
			{
				string selector3 = "." + @class;
				if (LookupPropertyInSheet(sheet, attribName, selector3, out val))
				{
					return true;
				}
			}
			if (LookupPropertyInSheet(sheet, attribName, selector2, out val))
			{
				return true;
			}
			if (LookupPropertyInSheet(sheet, attribName, "*", out val))
			{
				return true;
			}
			val = null;
			return false;
		}

		private bool LookupPropertyInSheet(SVGStyleSheet sheet, string attribName, string selector, out string val)
		{
			if (selector == null)
			{
				val = null;
				return false;
			}
			if (sheet.selectors.Contains(selector))
			{
				SVGPropertySheet sVGPropertySheet = sheet[selector];
				if (sVGPropertySheet.ContainsKey(attribName))
				{
					val = sVGPropertySheet[attribName];
					return true;
				}
			}
			val = null;
			return false;
		}

		private IEnumerable<string> SortedClasses(List<string> classes)
		{
			if (globalStyleSheet.selectors.Count() == 0)
			{
				foreach (string @class in classes)
				{
					yield return @class;
				}
			}
			foreach (string item in globalStyleSheet.selectors.Reverse())
			{
				if (item[0] == '.')
				{
					string text = item.Substring(1);
					if (classes.Contains(text))
					{
						yield return text;
					}
				}
			}
		}
	}
}
