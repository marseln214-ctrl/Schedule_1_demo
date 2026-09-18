using System;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vision
{
	[Serializable]
	public class VisibilityAttribute
	{
		public string name = "Attribute Name";

		public float pointsChange;

		[Range(0f, 5f)]
		public float multiplier = 1f;

		public VisibilityAttribute(string _name, float _pointsChange, float _multiplier = 1f, int attributeIndex = -1)
		{
			name = _name;
			pointsChange = _pointsChange;
			multiplier = _multiplier;
			if (attributeIndex == -1)
			{
				Player.Local.Visibility.activeAttributes.Add(this);
			}
			else
			{
				Player.Local.Visibility.activeAttributes.Insert(attributeIndex, this);
			}
		}

		public void Delete()
		{
			Player.Local.Visibility.activeAttributes.Remove(this);
		}
	}
}
