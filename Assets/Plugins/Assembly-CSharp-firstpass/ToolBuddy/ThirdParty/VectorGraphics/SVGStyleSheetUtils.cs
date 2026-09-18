using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	internal static class SVGStyleSheetUtils
	{
		public static SVGStyleSheet Parse(string cssText)
		{
			SVGStyleSheet sVGStyleSheet = new SVGStyleSheet();
			List<string> tokens = Tokenize(cssText);
			SVGStyleSheet sVGStyleSheet2 = new SVGStyleSheet();
			while (ParseSelector(tokens, sVGStyleSheet2))
			{
				foreach (string selector in sVGStyleSheet2.selectors)
				{
					if (sVGStyleSheet.selectors.Contains(selector))
					{
						CombineProperties(sVGStyleSheet[selector], sVGStyleSheet2[selector]);
					}
					else
					{
						sVGStyleSheet[selector] = sVGStyleSheet2[selector];
					}
				}
				sVGStyleSheet2.Clear();
			}
			return sVGStyleSheet;
		}

		public static SVGPropertySheet ParseInline(string cssText)
		{
			List<string> tokens = Tokenize(cssText);
			SVGPropertySheet sVGPropertySheet = new SVGPropertySheet();
			ParseProperties(tokens, sVGPropertySheet);
			return sVGPropertySheet;
		}

		private static bool ParseSelector(List<string> tokens, SVGStyleSheet sheet)
		{
			if (tokens.Count == 0)
			{
				return false;
			}
			SVGStyleSheet sVGStyleSheet = new SVGStyleSheet();
			do
			{
				string key = PopToken(tokens);
				sVGStyleSheet[key] = new SVGPropertySheet();
				while (PeekToken(tokens) == ",")
				{
					PopToken(tokens);
				}
			}
			while (!(PeekToken(tokens) == "") && !(PeekToken(tokens) == "{"));
			string text = PopToken(tokens);
			if (text != "{")
			{
				Debug.LogError("Invalid CSS selector opening bracket: \"" + text + "\"");
				return false;
			}
			SVGPropertySheet props = new SVGPropertySheet();
			ParseProperties(tokens, props);
			foreach (string selector in sVGStyleSheet.selectors)
			{
				sheet[selector] = CopyProperties(props);
			}
			text = PopToken(tokens);
			if (text != "}")
			{
				Debug.LogError("Invalid CSS selector closing bracket: \"" + text + "\"");
				return false;
			}
			return true;
		}

		private static void CombineProperties(SVGPropertySheet first, SVGPropertySheet second)
		{
			foreach (string key in second.Keys)
			{
				first[key] = second[key];
			}
		}

		private static SVGPropertySheet CopyProperties(SVGPropertySheet props)
		{
			SVGPropertySheet sVGPropertySheet = new SVGPropertySheet();
			foreach (KeyValuePair<string, string> prop in props)
			{
				sVGPropertySheet[prop.Key] = prop.Value;
			}
			return sVGPropertySheet;
		}

		private static bool ParseProperties(List<string> tokens, SVGPropertySheet props)
		{
			string name;
			string value;
			while (ParseProperty(tokens, out name, out value))
			{
				props[name] = value;
				while (PeekToken(tokens) == ";")
				{
					PopToken(tokens);
				}
			}
			return true;
		}

		private static bool ParseProperty(List<string> tokens, out string name, out string value)
		{
			name = null;
			value = null;
			if (PeekToken(tokens) == "" || PeekToken(tokens) == "}")
			{
				return false;
			}
			name = PopToken(tokens);
			string text = PopToken(tokens);
			if (text != ":")
			{
				Debug.LogError("Invalid CSS property separator: \"" + text + "\"");
				return false;
			}
			value = "";
			while (PeekToken(tokens) != "" && PeekToken(tokens) != ";" && PeekToken(tokens) != "}")
			{
				value = ((value == "") ? PopToken(tokens) : (value + " " + PopToken(tokens)));
				if (PeekToken(tokens) == "(")
				{
					value += ParseParenValue(tokens);
				}
			}
			return true;
		}

		private static string ParseParenValue(List<string> tokens)
		{
			string text = PopToken(tokens);
			if (text != "(")
			{
				Debug.LogError("Invaid CSS value opening");
				return "";
			}
			string text2 = text;
			while (PeekToken(tokens) != "" && PeekToken(tokens) != ")")
			{
				text2 += PopToken(tokens);
			}
			if (PeekToken(tokens) != ")")
			{
				Debug.LogError("Invaid CSS value closing");
				return "";
			}
			return text2 + PopToken(tokens);
		}

		public static List<string> Tokenize(string cssText)
		{
			List<string> list = new List<string>();
			cssText = cssText.Replace(Environment.NewLine, "");
			cssText = Regex.Replace(cssText, "/\\*.*?\\*/", "");
			cssText = Regex.Replace(cssText, "<!--.*?-->", "");
			int i = 0;
			int num = 0;
			while (i < cssText.Length)
			{
				for (; i < cssText.Length && IsWhitespace(cssText[i]); i++)
				{
				}
				for (num = i; num < cssText.Length && !IsSeparator(cssText[num]); num++)
				{
				}
				if (i == num)
				{
					if (i < cssText.Length)
					{
						list.Add(cssText[i].ToString());
					}
					num++;
				}
				else
				{
					list.Add(cssText.Substring(i, num - i));
				}
				i = num;
			}
			return list;
		}

		private static string PeekToken(List<string> tokens)
		{
			if (tokens.Count == 0)
			{
				return "";
			}
			return tokens[0];
		}

		private static string PopToken(List<string> tokens)
		{
			if (tokens.Count == 0)
			{
				return "";
			}
			string result = tokens[0];
			tokens.RemoveAt(0);
			return result;
		}

		private static bool IsSeparator(char ch)
		{
			if (!IsWhitespace(ch) && ch != ';' && ch != ':' && ch != '{' && ch != '}' && ch != '(' && ch != ')')
			{
				return ch == ',';
			}
			return true;
		}

		private static bool IsWhitespace(char ch)
		{
			if (ch != ' ' && ch != '\n')
			{
				return ch == '\t';
			}
			return true;
		}
	}
}
