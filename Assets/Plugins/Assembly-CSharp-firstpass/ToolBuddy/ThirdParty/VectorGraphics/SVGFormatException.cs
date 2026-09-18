using System;
using System.Xml;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	internal class SVGFormatException : Exception
	{
		public static SVGFormatException StackError => new SVGFormatException("Vector scene construction mismatch");

		public SVGFormatException()
		{
		}

		public SVGFormatException(string message)
			: base(ComposeMessage(null, message))
		{
		}

		public SVGFormatException(XmlReader reader, string message)
			: base(ComposeMessage(reader, message))
		{
		}

		private static string ComposeMessage(XmlReader reader, string message)
		{
			if (reader is IXmlLineInfo xmlLineInfo)
			{
				return "SVG Error (line " + xmlLineInfo.LineNumber + ", character " + xmlLineInfo.LinePosition + "): " + message;
			}
			return "SVG Error: " + message;
		}
	}
}
