using System.Xml;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	internal class XmlReaderIterator
	{
		internal class Node
		{
			private XmlReader reader;

			private int depth;

			private string name;

			public string Name => name;

			public string this[string attrib] => reader.GetAttribute(attrib);

			public int Depth => depth;

			public Node(XmlReader reader)
			{
				this.reader = reader;
				name = reader.Name;
				depth = reader.Depth;
			}

			public SVGPropertySheet GetAttributes()
			{
				SVGPropertySheet sVGPropertySheet = new SVGPropertySheet();
				for (int i = 0; i < reader.AttributeCount; i++)
				{
					reader.MoveToAttribute(i);
					sVGPropertySheet[reader.Name] = reader.Value;
				}
				reader.MoveToElement();
				return sVGPropertySheet;
			}

			public SVGFormatException GetException(string message)
			{
				return new SVGFormatException(reader, message);
			}

			public SVGFormatException GetUnsupportedAttribValException(string attrib)
			{
				return new SVGFormatException(reader, "Value '" + this[attrib] + "' is invalid for attribute '" + attrib + "'");
			}
		}

		private XmlReader reader;

		private bool currentElementVisited;

		public XmlReaderIterator(XmlReader reader)
		{
			this.reader = reader;
		}

		public bool GoToRoot(string tagName)
		{
			if (reader.ReadToFollowing(tagName))
			{
				return reader.Depth == 0;
			}
			return false;
		}

		public Node VisitCurrent()
		{
			currentElementVisited = true;
			return new Node(reader);
		}

		public bool IsEmptyElement()
		{
			return reader.IsEmptyElement;
		}

		public bool GoToNextChild(Node node)
		{
			if (!currentElementVisited)
			{
				return reader.Depth == node.Depth + 1;
			}
			reader.Read();
			while (reader.NodeType != 0 && reader.NodeType != XmlNodeType.Element)
			{
				reader.Read();
			}
			if (reader.NodeType != XmlNodeType.Element)
			{
				return false;
			}
			currentElementVisited = false;
			return reader.Depth == node.Depth + 1;
		}

		public void SkipCurrentChildTree(Node node)
		{
			while (GoToNextChild(node))
			{
				SkipCurrentChildTree(VisitCurrent());
			}
		}

		public string ReadTextWithinElement()
		{
			if (reader.IsEmptyElement)
			{
				return "";
			}
			string text = "";
			while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
			{
				text += reader.Value;
			}
			return text;
		}
	}
}
