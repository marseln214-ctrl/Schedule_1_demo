using System;

namespace AdvancedPeopleSystem
{
	[Serializable]
	public class CharacterSelectedElements : ICloneable
	{
		public int Hair = -1;

		public int Beard = -1;

		public int Hat = -1;

		public int Shirt = -1;

		public int Pants = -1;

		public int Shoes = -1;

		public int Accessory = -1;

		public int Item1 = -1;

		public object Clone()
		{
			return new CharacterSelectedElements
			{
				Hair = Hair,
				Beard = Beard,
				Hat = Hat,
				Shirt = Shirt,
				Pants = Pants,
				Shoes = Shoes,
				Accessory = Accessory,
				Item1 = Item1
			};
		}

		public int GetSelectedIndex(CharacterElementType type)
		{
			return type switch
			{
				CharacterElementType.Hat => Hat, 
				CharacterElementType.Shirt => Shirt, 
				CharacterElementType.Pants => Pants, 
				CharacterElementType.Shoes => Shoes, 
				CharacterElementType.Accessory => Accessory, 
				CharacterElementType.Hair => Hair, 
				CharacterElementType.Beard => Beard, 
				CharacterElementType.Item1 => Item1, 
				_ => -1, 
			};
		}

		public void SetSelectedIndex(CharacterElementType type, int newIndex)
		{
			switch (type)
			{
			case CharacterElementType.Hat:
				Hat = newIndex;
				break;
			case CharacterElementType.Shirt:
				Shirt = newIndex;
				break;
			case CharacterElementType.Pants:
				Pants = newIndex;
				break;
			case CharacterElementType.Shoes:
				Shoes = newIndex;
				break;
			case CharacterElementType.Accessory:
				Accessory = newIndex;
				break;
			case CharacterElementType.Hair:
				Hair = newIndex;
				break;
			case CharacterElementType.Beard:
				Beard = newIndex;
				break;
			case CharacterElementType.Item1:
				Item1 = newIndex;
				break;
			}
		}
	}
}
