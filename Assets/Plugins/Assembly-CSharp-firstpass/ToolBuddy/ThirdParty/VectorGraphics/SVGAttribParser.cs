using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	internal class SVGAttribParser
	{
		private enum AttribPath
		{
			Path = 0
		}

		private enum AttribTransform
		{
			Transform = 0
		}

		private enum AttribStroke
		{
			Stroke = 0
		}

		private LinkedList<BezierSegment> currentContour = new LinkedList<BezierSegment>();

		private List<BezierContour> contours = new List<BezierContour>();

		private Vector2 penPos;

		private string attribString;

		private char pathCommand;

		private Matrix2D transform;

		private IFill fill;

		private string attribName;

		private int stringPos;

		private static NamedWebColorDictionary namedColors;

		public static List<BezierContour> ParsePath(XmlReaderIterator.Node node)
		{
			string text = node["d"];
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			try
			{
				return new SVGAttribParser(text, AttribPath.Path).contours;
			}
			catch (Exception ex)
			{
				throw node.GetException(ex.Message);
			}
		}

		public static Matrix2D ParseTransform(XmlReaderIterator.Node node)
		{
			return ParseTransform(node, "transform");
		}

		public static Matrix2D ParseTransform(XmlReaderIterator.Node node, string attribName)
		{
			string text = node[attribName];
			if (string.IsNullOrEmpty(text))
			{
				return Matrix2D.identity;
			}
			try
			{
				return new SVGAttribParser(text, attribName, AttribTransform.Transform).transform;
			}
			catch (Exception ex)
			{
				throw node.GetException(ex.Message);
			}
		}

		public static IFill ParseFill(XmlReaderIterator.Node node, SVGDictionary dict, SVGPostponedFills postponedFills, SVGStyleResolver styles, Inheritance inheritance = Inheritance.Inherited)
		{
			bool isDefaultFill;
			return ParseFill(node, dict, postponedFills, styles, inheritance, out isDefaultFill);
		}

		public static IFill ParseFill(XmlReaderIterator.Node node, SVGDictionary dict, SVGPostponedFills postponedFills, SVGStyleResolver styles, Inheritance inheritance, out bool isDefaultFill)
		{
			string text = styles.Evaluate("fill-opacity", inheritance);
			float opacity = ((text != null) ? ParseFloat(text) : 1f);
			string text2 = styles.Evaluate("fill-rule", inheritance);
			FillMode mode = FillMode.NonZero;
			switch (text2)
			{
			case "nonzero":
				mode = FillMode.NonZero;
				break;
			case "evenodd":
				mode = FillMode.OddEven;
				break;
			default:
				throw new Exception("Unknown fill-rule: " + text2);
			case null:
				break;
			}
			try
			{
				string text3 = styles.Evaluate("fill", inheritance);
				isDefaultFill = text3 == null && text == null;
				return new SVGAttribParser(text3, "fill", opacity, mode, dict, postponedFills).fill;
			}
			catch (Exception ex)
			{
				throw node.GetException(ex.Message);
			}
		}

		public static Stroke ParseStrokeAndOpacity(XmlReaderIterator.Node node, SVGDictionary dict, SVGStyleResolver styles, Inheritance inheritance = Inheritance.Inherited)
		{
			string text = styles.Evaluate("stroke", inheritance);
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			string text2 = styles.Evaluate("stroke-opacity", inheritance);
			float opacity = ((text2 != null) ? ParseFloat(text2) : 1f);
			IFill fill = null;
			try
			{
				fill = new SVGAttribParser(text, "stroke", opacity, FillMode.NonZero, dict, null).fill;
			}
			catch (Exception ex)
			{
				throw node.GetException(ex.Message);
			}
			if (fill == null)
			{
				return null;
			}
			return new Stroke
			{
				Fill = fill
			};
		}

		public static Color ParseColor(string colorString)
		{
			if (colorString[0] == '#')
			{
				uint num = uint.Parse(colorString.Substring(1), NumberStyles.HexNumber);
				if (colorString.Length == 4)
				{
					return new Color((float)(((num >> 8) & 0xFu) | (((num >> 8) & 0xF) << 4)) / 255f, (float)(((num >> 4) & 0xFu) | (((num >> 4) & 0xF) << 4)) / 255f, (float)((num & 0xFu) | ((num & 0xF) << 4)) / 255f);
				}
				return new Color((float)((num >> 16) & 0xFFu) / 255f, (float)((num >> 8) & 0xFFu) / 255f, (float)(num & 0xFFu) / 255f);
			}
			if (colorString.StartsWith("rgb(") && colorString.EndsWith(")"))
			{
				string[] array = colorString.Substring(4, colorString.Length - 5).Split(new char[2] { ',', '%' }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length != 3)
				{
					throw new Exception("Invalid rgb() color specification");
				}
				float num2 = (colorString.Contains("%") ? 100f : 255f);
				return new Color((float)(int)byte.Parse(array[0]) / num2, (float)(int)byte.Parse(array[1]) / num2, (float)(int)byte.Parse(array[2]) / num2);
			}
			if (colorString.StartsWith("rgba(") && colorString.EndsWith(")"))
			{
				string[] array2 = colorString.Substring(5, colorString.Length - 6).Split(new char[2] { ',', '%' }, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length != 4)
				{
					throw new Exception("Invalid rgba() color specification");
				}
				float num3 = (colorString.Contains("%") ? 100f : 255f);
				return new Color((float)(int)byte.Parse(array2[0]) / num3, (float)(int)byte.Parse(array2[1]) / num3, (float)(int)byte.Parse(array2[2]) / num3, (num3 == 100f) ? ((float)(int)byte.Parse(array2[3]) / num3) : ParseFloat(array2[3]));
			}
			if (namedColors == null)
			{
				namedColors = new NamedWebColorDictionary();
			}
			return namedColors[colorString.ToLower()];
		}

		public static string ParseURLRef(string url)
		{
			if (url.StartsWith("url(") && url.EndsWith(")"))
			{
				return url.Substring(4, url.Length - 5);
			}
			return null;
		}

		public static object ParseRelativeRef(string iri, SVGDictionary dict)
		{
			if (iri == null)
			{
				return null;
			}
			if (!iri.StartsWith("#"))
			{
				throw new Exception("Unsupported reference type (" + iri + ")");
			}
			iri = iri.Substring(1);
			dict.TryGetValue(iri, out var value);
			return value;
		}

		public static string CleanIri(string iri)
		{
			if (iri == null)
			{
				return null;
			}
			if (!iri.StartsWith("#"))
			{
				throw new Exception("Unsupported reference type (" + iri + ")");
			}
			iri = iri.Substring(1);
			return iri;
		}

		private SVGAttribParser(string attrib, AttribPath attribPath)
		{
			attribName = "path";
			attribString = attrib;
			NextPathCommand(noCommandInheritance: true);
			if (pathCommand != 'm' && pathCommand != 'M')
			{
				throw new Exception("Path must start with a MoveTo pathCommand");
			}
			char c = '\0';
			Vector2 vector = Vector2.zero;
			while (NextPathCommand() != 0)
			{
				bool flag = pathCommand >= 'a' && pathCommand <= 'z';
				char c2 = char.ToLower(pathCommand);
				switch (c2)
				{
				case 'm':
					penPos = NextVector2(flag);
					pathCommand = (flag ? 'l' : 'L');
					ConcludePath(joinEnds: false);
					break;
				case 'z':
					if (currentContour.First != null)
					{
						penPos = currentContour.First.Value.P0;
					}
					ConcludePath(joinEnds: true);
					break;
				case 'l':
				{
					Vector2 vector6 = NextVector2(flag);
					if ((vector6 - penPos).magnitude > VectorUtils.Epsilon)
					{
						currentContour.AddLast(VectorUtils.MakeLine(penPos, vector6));
					}
					penPos = vector6;
					break;
				}
				case 'h':
				{
					float x = (flag ? (penPos.x + NextFloat()) : NextFloat());
					Vector2 vector4 = new Vector2(x, penPos.y);
					if ((vector4 - penPos).magnitude > VectorUtils.Epsilon)
					{
						currentContour.AddLast(VectorUtils.MakeLine(penPos, vector4));
					}
					penPos = vector4;
					break;
				}
				case 'v':
				{
					float y = (flag ? (penPos.y + NextFloat()) : NextFloat());
					Vector2 vector5 = new Vector2(penPos.x, y);
					if ((vector5 - penPos).magnitude > VectorUtils.Epsilon)
					{
						currentContour.AddLast(VectorUtils.MakeLine(penPos, vector5));
					}
					penPos = vector5;
					break;
				}
				case 'c':
				case 'q':
				{
					BezierSegment bezierSegment2 = new BezierSegment
					{
						P0 = penPos,
						P1 = NextVector2(flag)
					};
					if (c2 == 'c')
					{
						bezierSegment2.P2 = NextVector2(flag);
					}
					bezierSegment2.P3 = NextVector2(flag);
					if (c2 == 'q')
					{
						vector = bezierSegment2.P1;
						float num3 = 2f / 3f;
						bezierSegment2.P1 = bezierSegment2.P0 + num3 * (vector - bezierSegment2.P0);
						bezierSegment2.P2 = bezierSegment2.P3 + num3 * (vector - bezierSegment2.P3);
					}
					penPos = bezierSegment2.P3;
					if (!VectorUtils.IsEmptySegment(bezierSegment2))
					{
						currentContour.AddLast(bezierSegment2);
					}
					break;
				}
				case 's':
				case 't':
				{
					Vector2 p = penPos;
					if (currentContour.Count > 0 && (c == 'c' || c == 'q' || c == 's' || c == 't'))
					{
						p += currentContour.Last.Value.P3 - ((c == 'q' || c == 't') ? vector : currentContour.Last.Value.P2);
					}
					BezierSegment bezierSegment = new BezierSegment
					{
						P0 = penPos,
						P1 = p
					};
					if (c2 == 's')
					{
						bezierSegment.P2 = NextVector2(flag);
					}
					bezierSegment.P3 = NextVector2(flag);
					if (c2 == 't')
					{
						vector = bezierSegment.P1;
						float num2 = 2f / 3f;
						bezierSegment.P1 = bezierSegment.P0 + num2 * (vector - bezierSegment.P0);
						bezierSegment.P2 = bezierSegment.P3 + num2 * (vector - bezierSegment.P3);
					}
					penPos = bezierSegment.P3;
					if (!VectorUtils.IsEmptySegment(bezierSegment))
					{
						currentContour.AddLast(bezierSegment);
					}
					break;
				}
				case 'a':
				{
					Vector2 vector2 = NextVector2();
					float num = NextFloat();
					bool largeArc = NextBool();
					bool sweep = NextBool();
					Vector2 vector3 = NextVector2(flag);
					if (vector2.magnitude <= VectorUtils.Epsilon)
					{
						if ((vector3 - penPos).magnitude > VectorUtils.Epsilon)
						{
							currentContour.AddLast(VectorUtils.MakeLine(penPos, vector3));
						}
					}
					else
					{
						foreach (BezierSegment item in VectorUtils.SegmentsInPath(VectorUtils.BuildEllipsePath(penPos, vector3, (0f - num) * (MathF.PI / 180f), vector2.x, vector2.y, largeArc, sweep)))
						{
							currentContour.AddLast(item);
						}
					}
					penPos = vector3;
					break;
				}
				}
				c = c2;
			}
			ConcludePath(joinEnds: false);
		}

		private SVGAttribParser(string attrib, string attribNameVal, AttribTransform attribTransform)
		{
			attribString = attrib;
			attribName = attribNameVal;
			transform = Matrix2D.identity;
			while (stringPos < attribString.Length)
			{
				int num = stringPos;
				string text = NextStringCommand();
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				SkipSymbol('(');
				switch (text)
				{
				case "matrix":
					transform *= new Matrix2D
					{
						m00 = NextFloat(),
						m10 = NextFloat(),
						m01 = NextFloat(),
						m11 = NextFloat(),
						m02 = NextFloat(),
						m12 = NextFloat()
					};
					break;
				case "translate":
				{
					float x = NextFloat();
					float y2 = 0f;
					if (!PeekSymbol(')'))
					{
						y2 = NextFloat();
					}
					transform *= Matrix2D.Translate(new Vector2(x, y2));
					break;
				}
				case "scale":
				{
					float num3 = NextFloat();
					float y = num3;
					if (!PeekSymbol(')'))
					{
						y = NextFloat();
					}
					transform *= Matrix2D.Scale(new Vector2(num3, y));
					break;
				}
				case "rotate":
				{
					float num4 = NextFloat() * (MathF.PI / 180f);
					float num5 = 0f;
					float num6 = 0f;
					if (!PeekSymbol(')'))
					{
						num5 = NextFloat();
						num6 = NextFloat();
					}
					transform *= Matrix2D.Translate(new Vector2(num5, num6)) * Matrix2D.RotateLH(0f - num4) * Matrix2D.Translate(new Vector2(0f - num5, 0f - num6));
					break;
				}
				case "skewX":
				case "skewY":
				{
					float num2 = Mathf.Tan(NextFloat() * (MathF.PI / 180f));
					Matrix2D identity = Matrix2D.identity;
					if (text == "skewY")
					{
						identity.m10 = num2;
					}
					else
					{
						identity.m01 = num2;
					}
					transform *= identity;
					break;
				}
				default:
					throw new Exception("Unknown transform command at " + num + " in trasform specification");
				}
				SkipSymbol(')');
			}
		}

		private SVGAttribParser(string attrib, string attribName, float opacity, FillMode mode, SVGDictionary dict, SVGPostponedFills postponedFills, bool allowReference = true)
		{
			this.attribName = attribName;
			if (string.IsNullOrEmpty(attrib))
			{
				if (opacity < 1f)
				{
					fill = new SolidFill
					{
						Color = new Color(0f, 0f, 0f, opacity)
					};
				}
				else
				{
					fill = dict[(mode == FillMode.NonZero) ? SVGDocument.StockBlackNonZeroFillName : SVGDocument.StockBlackOddEvenFillName] as IFill;
				}
				return;
			}
			switch (attrib)
			{
			case "transparent":
				return;
			case "currentColor":
				Debug.LogError("currentColor is not supported as a " + attribName + " value");
				return;
			}
			string[] array = attrib.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (allowReference)
			{
				string text = ParseURLRef(array[0]);
				if (text != null)
				{
					fill = ParseRelativeRef(text, dict) as IFill;
					if (fill == null)
					{
						if (array.Length > 1)
						{
							fill = new SVGAttribParser(array[1], attribName, opacity, mode, dict, postponedFills, allowReference: false).fill;
						}
						else if (postponedFills != null)
						{
							fill = new SolidFill
							{
								Color = Color.clear
							};
							postponedFills[fill] = text;
						}
					}
					if (fill != null)
					{
						fill.Opacity = opacity;
					}
					return;
				}
			}
			Color color = ParseColor(string.Join("", array));
			color.a *= opacity;
			_ = array.Length;
			_ = 1;
			fill = new SolidFill
			{
				Color = color,
				Mode = mode
			};
		}

		private void ConcludePath(bool joinEnds)
		{
			if (currentContour.Count > 0)
			{
				BezierContour item = default(BezierContour);
				item.Closed = joinEnds && currentContour.Count >= 1;
				item.Segments = new BezierPathSegment[currentContour.Count + 1];
				int num = 0;
				foreach (BezierSegment item2 in currentContour)
				{
					item.Segments[num++] = new BezierPathSegment
					{
						P0 = item2.P0,
						P1 = item2.P1,
						P2 = item2.P2
					};
				}
				BezierSegment bezierSegment = VectorUtils.MakeLine(currentContour.Last.Value.P3, item.Segments[0].P0);
				item.Segments[num] = new BezierPathSegment
				{
					P0 = bezierSegment.P0,
					P1 = bezierSegment.P1,
					P2 = bezierSegment.P2
				};
				contours.Add(item);
			}
			currentContour.Clear();
		}

		private Vector2 NextVector2(bool relative = false)
		{
			Vector2 vector = new Vector2(NextFloat(), NextFloat());
			if (!relative)
			{
				return vector;
			}
			return vector + penPos;
		}

		private float NextFloat()
		{
			SkipWhitespaces();
			if (stringPos >= attribString.Length)
			{
				throw new Exception(attribName + " specification ended before sufficing numbers required by the last pathCommand");
			}
			int num = stringPos;
			if (attribString[stringPos] == '-')
			{
				stringPos++;
			}
			bool flag = false;
			bool flag2 = false;
			while (stringPos < attribString.Length)
			{
				char c = attribString[stringPos];
				if (!flag && c == '.')
				{
					flag = true;
					stringPos++;
				}
				else if (!flag2 && (c == 'e' || c == 'E'))
				{
					flag2 = true;
					stringPos++;
					if (stringPos < attribString.Length && attribString[stringPos] == '-')
					{
						stringPos++;
					}
				}
				else
				{
					if (!char.IsDigit(c))
					{
						break;
					}
					stringPos++;
				}
			}
			if (stringPos - num == 0 || (stringPos - num == 1 && attribString[num] == '-'))
			{
				throw new Exception("Missing number at " + num + " in " + attribName + " specification");
			}
			return ParseFloat(attribString.Substring(num, stringPos - num));
		}

		internal static float ParseFloat(string s)
		{
			return float.Parse(s, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
		}

		private bool NextBool()
		{
			return Mathf.Abs(NextFloat()) > VectorUtils.Epsilon;
		}

		private char NextPathCommand(bool noCommandInheritance = false)
		{
			SkipWhitespaces();
			if (stringPos >= attribString.Length)
			{
				return '\0';
			}
			char c = attribString[stringPos];
			if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
			{
				pathCommand = c;
				stringPos++;
				return c;
			}
			if (!noCommandInheritance && (char.IsDigit(c) || c == '.' || c == '-'))
			{
				return pathCommand;
			}
			throw new Exception("Unexpected character at " + stringPos + " in path specification");
		}

		private string NextStringCommand()
		{
			SkipWhitespaces();
			if (stringPos >= attribString.Length)
			{
				return null;
			}
			int num = stringPos;
			while (stringPos < attribString.Length)
			{
				char c = attribString[stringPos];
				if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z'))
				{
					break;
				}
				stringPos++;
			}
			if (stringPos - num == 0)
			{
				throw new Exception("Unexpected character at " + stringPos + " in " + attribName + " specification");
			}
			return attribString.Substring(num, stringPos - num);
		}

		private void SkipSymbol(char s)
		{
			SkipWhitespaces();
			if (stringPos >= attribString.Length || attribString[stringPos] != s)
			{
				throw new Exception("Expected " + s + " at " + stringPos + " of " + attribName + " specification");
			}
			stringPos++;
		}

		private bool PeekSymbol(char s)
		{
			SkipWhitespaces();
			if (stringPos < attribString.Length)
			{
				return attribString[stringPos] == s;
			}
			return false;
		}

		private void SkipWhitespaces()
		{
			while (stringPos < attribString.Length)
			{
				switch (attribString[stringPos])
				{
				case '\t':
				case '\n':
				case '\r':
				case ' ':
				case ',':
					break;
				default:
					return;
				}
				stringPos++;
			}
		}
	}
}
