using System;
using System.Collections.Generic;

namespace AdvancedPeopleSystem
{
	public static class CharacterGenerator
	{
		public static void Generate(CharacterCustomization cc)
		{
			CharacterGeneratorSettings generator = cc.Settings.generator;
			int num = generator.hair.GetRandom(cc.Settings.hairPresets.Count);
			int num2 = generator.beard.GetRandom(cc.Settings.beardPresets.Count);
			int num3 = generator.hat.GetRandom(cc.Settings.hatsPresets.Count);
			int num4 = generator.accessory.GetRandom(cc.Settings.accessoryPresets.Count);
			int num5 = generator.shirt.GetRandom(cc.Settings.shirtsPresets.Count);
			int num6 = generator.pants.GetRandom(cc.Settings.pantsPresets.Count);
			int num7 = generator.shoes.GetRandom(cc.Settings.shoesPresets.Count);
			float random = generator.headSize.GetRandom();
			float random2 = generator.headOffset.GetRandom();
			float random3 = generator.height.GetRandom();
			foreach (GeneratorExclude exclude in generator.excludes)
			{
				if (CheckExclude(num, num2, num3, num4, num5, num6, num7, exclude.exclude) == -1)
				{
					if (exclude.ExcludeItem == ExcludeItem.Hair && num == exclude.targetIndex)
					{
						num = -1;
					}
					if (exclude.ExcludeItem == ExcludeItem.Beard && num2 == exclude.targetIndex)
					{
						num2 = -1;
					}
					if (exclude.ExcludeItem == ExcludeItem.Hat && num3 == exclude.targetIndex)
					{
						num3 = -1;
					}
					if (exclude.ExcludeItem == ExcludeItem.Accessory && num4 == exclude.targetIndex)
					{
						num4 = -1;
					}
					if (exclude.ExcludeItem == ExcludeItem.Shirt && num5 == exclude.targetIndex)
					{
						num5 = -1;
					}
					if (exclude.ExcludeItem == ExcludeItem.Pants && num6 == exclude.targetIndex)
					{
						num6 = -1;
					}
					if (exclude.ExcludeItem == ExcludeItem.Shoes && num7 == exclude.targetIndex)
					{
						num7 = -1;
					}
				}
			}
			cc.SetHeadSize(random);
			cc.SetHeight(random3);
			cc.SetBlendshapeValue(CharacterBlendShapeType.Fat, generator.fat.GetRandom());
			cc.SetElementByIndex(CharacterElementType.Hair, num);
			cc.SetElementByIndex(CharacterElementType.Beard, num2);
			cc.SetElementByIndex(CharacterElementType.Accessory, num4);
			cc.SetElementByIndex(CharacterElementType.Shirt, num5);
			cc.SetElementByIndex(CharacterElementType.Pants, num6);
			cc.SetElementByIndex(CharacterElementType.Shoes, num7);
			cc.SetElementByIndex(CharacterElementType.Hat, num3);
			cc.SetBodyColor(BodyColorPart.Skin, generator.skinColors.GetRandom());
			cc.SetBodyColor(BodyColorPart.Hair, generator.hairColors.GetRandom());
			cc.SetBodyColor(BodyColorPart.Eye, generator.eyeColors.GetRandom());
			cc.SetBlendshapeValue(CharacterBlendShapeType.Head_Offset, random2);
			foreach (MinMaxFacialBlendshapes facialBlendshape in generator.facialBlendshapes)
			{
				if (Enum.TryParse<CharacterBlendShapeType>(facialBlendshape.name, out var result))
				{
					cc.SetBlendshapeValue(result, facialBlendshape.GetRandom());
				}
			}
			static int CheckExclude(int hair, int beard, int hat, int accessory, int shirt, int pants, int shoes, List<ExcludeIndexes> excludeIndexes)
			{
				int result2 = 0;
				if (excludeIndexes.Count == 0)
				{
					result2 = -1;
				}
				else
				{
					foreach (ExcludeIndexes excludeIndex in excludeIndexes)
					{
						if (excludeIndex.item == ExcludeItem.Hair && hair == excludeIndex.index)
						{
							result2 = -1;
							break;
						}
						if (excludeIndex.item == ExcludeItem.Beard && beard == excludeIndex.index)
						{
							result2 = -1;
							break;
						}
						if (excludeIndex.item == ExcludeItem.Hat && hat == excludeIndex.index)
						{
							result2 = -1;
							break;
						}
						if (excludeIndex.item == ExcludeItem.Accessory && accessory == excludeIndex.index)
						{
							result2 = -1;
							break;
						}
						if (excludeIndex.item == ExcludeItem.Shirt && shirt == excludeIndex.index)
						{
							result2 = -1;
							break;
						}
						if (excludeIndex.item == ExcludeItem.Pants && pants == excludeIndex.index)
						{
							result2 = -1;
							break;
						}
						if (excludeIndex.item == ExcludeItem.Shoes && shoes == excludeIndex.index)
						{
							result2 = -1;
							break;
						}
					}
				}
				return result2;
			}
		}
	}
}
