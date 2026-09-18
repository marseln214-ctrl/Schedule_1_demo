using System.Collections.Generic;
using System.Linq;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	internal class SVGStyleSheet
	{
		private List<KeyValuePair<string, SVGPropertySheet>> m_Selectors = new List<KeyValuePair<string, SVGPropertySheet>>();

		public SVGPropertySheet this[string key]
		{
			get
			{
				int num = m_Selectors.FindIndex((KeyValuePair<string, SVGPropertySheet> x) => x.Key == key);
				if (num != -1)
				{
					return m_Selectors[num].Value;
				}
				return null;
			}
			set
			{
				KeyValuePair<string, SVGPropertySheet> keyValuePair = new KeyValuePair<string, SVGPropertySheet>(key, value);
				int num = m_Selectors.FindIndex((KeyValuePair<string, SVGPropertySheet> x) => x.Key == key);
				if (num != -1)
				{
					m_Selectors[num] = keyValuePair;
				}
				m_Selectors.Add(keyValuePair);
			}
		}

		public IEnumerable<string> selectors => m_Selectors.Select((KeyValuePair<string, SVGPropertySheet> x) => x.Key);

		public int Count => m_Selectors.Count;

		public void Clear()
		{
			m_Selectors.Clear();
		}
	}
}
