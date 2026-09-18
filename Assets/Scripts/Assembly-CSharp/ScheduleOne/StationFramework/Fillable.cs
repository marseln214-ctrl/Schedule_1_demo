using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScheduleOne.StationFramework
{
	public class Fillable : MonoBehaviour
	{
		public class Content
		{
			public string Label;

			public float Volume_L;

			public Color Color;
		}

		[Header("References")]
		public LiquidContainer LiquidContainer;

		[Header("Settings")]
		public bool FillableEnabled = true;

		public float LiquidCapacity_L = 1f;

		public List<Content> contents { get; protected set; } = new List<Content>();


		private void Awake()
		{
			LiquidContainer.SetLiquidLevel(0f);
		}

		public void AddLiquid(string label, float volume, Color color)
		{
			Content content = contents.Find((Content c) => c.Label == label);
			if (content == null)
			{
				content = new Content();
				content.Label = label;
				content.Volume_L = 0f;
				content.Color = color;
				contents.Add(content);
			}
			content.Volume_L += volume;
			UpdateLiquid();
		}

		public void ResetContents()
		{
			contents.Clear();
			UpdateLiquid();
		}

		private void UpdateLiquid()
		{
			float totalVolume = contents.Sum((Content x) => x.Volume_L);
			LiquidContainer.SetLiquidLevel(totalVolume / LiquidCapacity_L);
			if (totalVolume > 0f)
			{
				Color color = contents.Aggregate(Color.clear, (Color acc, Content c) => acc + c.Color * c.Volume_L / totalVolume);
				LiquidContainer.SetLiquidColor(color);
			}
		}

		public float GetLiquidVolume(string label)
		{
			return contents.Find((Content c) => c.Label == label)?.Volume_L ?? 0f;
		}

		public float GetTotalLiquidVolume()
		{
			return contents.Sum((Content x) => x.Volume_L);
		}
	}
}
