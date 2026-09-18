using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

namespace AdvancedPeopleSystem
{
	[Serializable]
	public class CharacterCustomizationSetup
	{
		public enum CharacterFileSaveFormat
		{
			Json = 0,
			Xml = 1,
			Binary = 2
		}

		public string settingsName;

		public CharacterSelectedElements selectedElements = new CharacterSelectedElements();

		public List<CharacterBlendshapeData> blendshapes = new List<CharacterBlendshapeData>();

		public int MinLod;

		public int MaxLod;

		public float[] SkinColor;

		public float[] EyeColor;

		public float[] HairColor;

		public float[] UnderpantsColor;

		public float[] TeethColor;

		public float[] OralCavityColor;

		public float Height;

		public float HeadSize;

		public void ApplyToCharacter(CharacterCustomization cc)
		{
			if (cc.Settings == null && settingsName != cc.Settings.name)
			{
				Debug.LogError("Character settings are not compatible with saved data");
				return;
			}
			cc.SetBodyColor(BodyColorPart.Skin, new Color(SkinColor[0], SkinColor[1], SkinColor[2], SkinColor[3]));
			cc.SetBodyColor(BodyColorPart.Eye, new Color(EyeColor[0], EyeColor[1], EyeColor[2], EyeColor[3]));
			cc.SetBodyColor(BodyColorPart.Hair, new Color(HairColor[0], HairColor[1], HairColor[2], HairColor[3]));
			cc.SetBodyColor(BodyColorPart.Underpants, new Color(UnderpantsColor[0], UnderpantsColor[1], UnderpantsColor[2], UnderpantsColor[3]));
			cc.SetBodyColor(BodyColorPart.Teeth, new Color(TeethColor[0], TeethColor[1], TeethColor[2], TeethColor[3]));
			cc.SetBodyColor(BodyColorPart.OralCavity, new Color(OralCavityColor[0], OralCavityColor[1], OralCavityColor[2], OralCavityColor[3]));
			cc.SetElementByIndex(CharacterElementType.Hair, selectedElements.Hair);
			cc.SetElementByIndex(CharacterElementType.Accessory, selectedElements.Accessory);
			cc.SetElementByIndex(CharacterElementType.Hat, selectedElements.Hat);
			cc.SetElementByIndex(CharacterElementType.Pants, selectedElements.Pants);
			cc.SetElementByIndex(CharacterElementType.Shoes, selectedElements.Shoes);
			cc.SetElementByIndex(CharacterElementType.Shirt, selectedElements.Shirt);
			cc.SetElementByIndex(CharacterElementType.Beard, selectedElements.Beard);
			cc.SetElementByIndex(CharacterElementType.Item1, selectedElements.Item1);
			cc.SetHeight(Height);
			cc.SetHeadSize(HeadSize);
			foreach (CharacterBlendshapeData blendshape in blendshapes)
			{
				cc.SetBlendshapeValue(blendshape.type, blendshape.value);
			}
			cc.ApplyPrefab();
		}

		public string Serialize(CharacterFileSaveFormat format)
		{
			string result = string.Empty;
			switch (format)
			{
			case CharacterFileSaveFormat.Json:
				result = JsonUtility.ToJson(this, prettyPrint: true);
				break;
			case CharacterFileSaveFormat.Xml:
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(CharacterCustomizationSetup));
				using (StringWriter stringWriter = new StringWriter())
				{
					xmlSerializer.Serialize(stringWriter, this);
					result = stringWriter.ToString();
				}
				break;
			}
			case CharacterFileSaveFormat.Binary:
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					new BinaryFormatter().Serialize(memoryStream, this);
					result = Convert.ToBase64String(memoryStream.ToArray());
				}
				break;
			}
			}
			return result;
		}

		public static CharacterCustomizationSetup Deserialize(string data, CharacterFileSaveFormat format)
		{
			CharacterCustomizationSetup result = null;
			switch (format)
			{
			case CharacterFileSaveFormat.Json:
				result = JsonUtility.FromJson<CharacterCustomizationSetup>(data);
				break;
			case CharacterFileSaveFormat.Xml:
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(CharacterCustomizationSetup));
				using (StringReader textReader = new StringReader(data))
				{
					result = (CharacterCustomizationSetup)xmlSerializer.Deserialize(textReader);
				}
				break;
			}
			case CharacterFileSaveFormat.Binary:
			{
				using (MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(data)))
				{
					result = (CharacterCustomizationSetup)new BinaryFormatter().Deserialize(serializationStream);
				}
				break;
			}
			}
			return result;
		}
	}
}
