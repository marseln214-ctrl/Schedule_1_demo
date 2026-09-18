using System;
using System.Globalization;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[AttributeUsage(AttributeTargets.Field)]
	public class SlotInfo : Attribute, IComparable
	{
		public enum SlotArrayType
		{
			Unknown = 0,
			Normal = 1,
			Hidden = 2
		}

		[ItemNotNull]
		[NotNull]
		public readonly Type[] DataTypes;

		public string Name;

		private string displayName;

		public string Tooltip;

		public bool Array;

		public SlotArrayType ArrayType = SlotArrayType.Normal;

		public string DisplayName
		{
			get
			{
				return displayName ?? Name;
			}
			set
			{
				displayName = value;
			}
		}

		protected SlotInfo(string name, [ItemNotNull][NotNull] params Type[] type)
		{
			DataTypes = type;
			Name = name;
		}

		protected SlotInfo([ItemNotNull][NotNull] params Type[] type)
			: this(null, type)
		{
		}

		public int CompareTo(object obj)
		{
			return string.Compare(((SlotInfo)obj).Name, Name, StringComparison.Ordinal);
		}

		public void CheckDataTypes()
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			for (int i = 0; i < DataTypes.Length; i++)
			{
				if (!DataTypes[i].IsSubclassOf(typeof(CGData)))
				{
					Debug.LogError(string.Format(invariantCulture, "Slot '{0}': Data type needs to be subclass of CGData!", DisplayName));
				}
			}
		}
	}
}
