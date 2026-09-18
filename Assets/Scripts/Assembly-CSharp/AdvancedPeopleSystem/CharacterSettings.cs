using System.Collections.Generic;
using UnityEngine;

namespace AdvancedPeopleSystem
{
	[CreateAssetMenu(fileName = "NewCharacterSettings", menuName = "Advanced People Pack/Settings", order = 1)]
	public class CharacterSettings : ScriptableObject
	{
		public GameObject OriginalMesh;

		public Material bodyMaterial;

		[Space(20f)]
		public List<CharacterAnimationPreset> characterAnimationPresets = new List<CharacterAnimationPreset>();

		[Space(20f)]
		public List<CharacterBlendshapeData> characterBlendshapeDatas = new List<CharacterBlendshapeData>();

		[Space(20f)]
		public List<CharacterElementsPreset> hairPresets = new List<CharacterElementsPreset>();

		public List<CharacterElementsPreset> beardPresets = new List<CharacterElementsPreset>();

		public List<CharacterElementsPreset> hatsPresets = new List<CharacterElementsPreset>();

		public List<CharacterElementsPreset> accessoryPresets = new List<CharacterElementsPreset>();

		public List<CharacterElementsPreset> shirtsPresets = new List<CharacterElementsPreset>();

		public List<CharacterElementsPreset> pantsPresets = new List<CharacterElementsPreset>();

		public List<CharacterElementsPreset> shoesPresets = new List<CharacterElementsPreset>();

		public List<CharacterElementsPreset> item1Presets = new List<CharacterElementsPreset>();

		[Space(20f)]
		public List<CharacterSettingsSelector> settingsSelectors = new List<CharacterSettingsSelector>();

		[Space(20f)]
		public RuntimeAnimatorController Animator;

		public Avatar Avatar;

		[Space(20f)]
		public CharacterGeneratorSettings generator;

		[Space(20f)]
		public CharacterSelectedElements DefaultSelectedElements = new CharacterSelectedElements();

		[Space(20f)]
		public bool DisableBlendshapeModifier;
	}
}
