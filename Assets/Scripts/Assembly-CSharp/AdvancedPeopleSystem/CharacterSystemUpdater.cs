using UnityEngine;

namespace AdvancedPeopleSystem
{
	public static class CharacterSystemUpdater
	{
		[RuntimeInitializeOnLoadMethod]
		private static void updateCharacters()
		{
			UpdateCharactersOnScene();
		}

		public static void UpdateCharactersOnScene(bool revertPrefabs = false, CharacterCustomization reverbObject = null)
		{
			CharacterCustomization[] array = Object.FindObjectsOfType<CharacterCustomization>();
			if (array == null)
			{
				return;
			}
			CharacterCustomization[] array2 = array;
			foreach (CharacterCustomization characterCustomization in array2)
			{
				if (!(characterCustomization == null))
				{
					characterCustomization.InitColors();
				}
			}
		}
	}
}
