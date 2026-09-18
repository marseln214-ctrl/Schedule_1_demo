using UnityEngine;

namespace ScheduleOne.NPCs.Relation
{
	public class RelationshipCategory
	{
		public static Color32 Hostile_Color = new Color32(173, 63, 63, byte.MaxValue);

		public static Color32 Unfriendly_Color = new Color32(227, 136, 55, byte.MaxValue);

		public static Color32 Neutral_Color = new Color32(208, 208, 208, byte.MaxValue);

		public static Color32 Friendly_Color = new Color32(61, 181, 243, byte.MaxValue);

		public static Color32 Loyal_Color = new Color32(63, 211, 63, byte.MaxValue);

		public static ERelationshipCategory GetCategory(float delta)
		{
			if (delta >= 4f)
			{
				return ERelationshipCategory.Loyal;
			}
			if (delta >= 3f)
			{
				return ERelationshipCategory.Friendly;
			}
			if (delta >= 2f)
			{
				return ERelationshipCategory.Neutral;
			}
			if (delta >= 1f)
			{
				return ERelationshipCategory.Unfriendly;
			}
			return ERelationshipCategory.Hostile;
		}

		public static Color32 GetColor(ERelationshipCategory category)
		{
			switch (category)
			{
			case ERelationshipCategory.Hostile:
				return Hostile_Color;
			case ERelationshipCategory.Unfriendly:
				return Unfriendly_Color;
			case ERelationshipCategory.Neutral:
				return Neutral_Color;
			case ERelationshipCategory.Friendly:
				return Friendly_Color;
			case ERelationshipCategory.Loyal:
				return Loyal_Color;
			default:
				Console.LogError("Failed to find relationship category color");
				return Color.white;
			}
		}
	}
}
