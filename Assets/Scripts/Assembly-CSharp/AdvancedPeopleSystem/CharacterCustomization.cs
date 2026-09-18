using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AdvancedPeopleSystem
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Advanced People Pack/Character Customizable", -1)]
	public class CharacterCustomization : MonoBehaviour
	{
		[SerializeField]
		public bool isSettingsExpanded;

		public CharacterSettings selectedsettings;

		[SerializeField]
		private CharacterSettings _settings;

		public List<CharacterPart> characterParts = new List<CharacterPart>();

		public string prefabPath = string.Empty;

		[SerializeField]
		public CharacterInstanceStatus instanceStatus;

		public Transform originHip;

		public Transform headHip;

		public List<ClothesAnchor> clothesAnchors = new List<ClothesAnchor>();

		public Animator animator;

		public CharacterSelectedElements characterSelectedElements = new CharacterSelectedElements();

		public float heightValue;

		public float headSizeValue;

		public float feetOffset;

		public List<CharacterBlendshapeData> characterBlendshapeDatas = new List<CharacterBlendshapeData>();

		public Color Skin;

		public Color Eye;

		public Color Hair;

		public Color Underpants;

		public Color OralCavity;

		public Color Teeth;

		public MaterialPropertyBlock bodyPropertyBlock;

		public CurrentBlendshapeAnimation currentBlendshapeAnimation;

		public CombinerState CurrentCombinerState;

		public CharacterPreBuilt combinedCharacter;

		public Transform ProbesAnchorOverride;

		public CharacterGeneratorSettings CharacterGenerator_settings;

		public bool UpdateWhenOffscreenMeshes = true;

		[SerializeField]
		public int MinLODLevels;

		[SerializeField]
		public int MaxLODLevels = 3;

		private LODGroup _lodGroup;

		public Transform _transform;

		public bool applyFeetOffset = true;

		public bool notAPP2Shader;

		private GameObject prebuiltPrefab;

		public CharacterSettings Settings => _settings;

		private void Awake()
		{
			_transform = base.transform;
			_lodGroup = GetComponent<LODGroup>();
			UpdateSkinnedMeshesOffscreenBounds();
		}

		private void Update()
		{
			AnimationTick();
		}

		private void LateUpdate()
		{
			if (feetOffset != 0f && applyFeetOffset)
			{
				SetFeetOffset(new Vector3(0f, feetOffset, 0f));
			}
		}

		public void AnimationTick()
		{
			if (currentBlendshapeAnimation == null)
			{
				return;
			}
			currentBlendshapeAnimation.timer += Time.deltaTime * currentBlendshapeAnimation.preset.AnimationPlayDuration;
			for (int i = 0; i < currentBlendshapeAnimation.preset.blendshapes.Count; i++)
			{
				if (currentBlendshapeAnimation.preset.UseGlobalBlendCurve)
				{
					SetBlendshapeValue(currentBlendshapeAnimation.preset.blendshapes[i].BlendType, currentBlendshapeAnimation.preset.blendshapes[i].BlendValue * currentBlendshapeAnimation.preset.weightPower * currentBlendshapeAnimation.preset.GlobalBlendAnimationCurve.Evaluate(currentBlendshapeAnimation.timer));
				}
				else
				{
					SetBlendshapeValue(currentBlendshapeAnimation.preset.blendshapes[i].BlendType, currentBlendshapeAnimation.preset.blendshapes[i].BlendValue * currentBlendshapeAnimation.preset.weightPower * currentBlendshapeAnimation.preset.blendshapes[i].BlendAnimationCurve.Evaluate(currentBlendshapeAnimation.timer));
				}
			}
			if (currentBlendshapeAnimation.timer >= 1f)
			{
				currentBlendshapeAnimation = null;
			}
		}

		public void SwitchCharacterSettings(int settingsIndex)
		{
			if (Settings.settingsSelectors.Count - 1 >= settingsIndex)
			{
				CharacterSettingsSelector characterSettingsSelector = Settings.settingsSelectors[settingsIndex];
				InitializeMeshes(characterSettingsSelector.settings);
			}
		}

		public void SwitchCharacterSettings(string selectorName)
		{
			for (int i = 0; i < Settings.settingsSelectors.Count; i++)
			{
				if (Settings.settingsSelectors[i].name == selectorName)
				{
					SwitchCharacterSettings(i);
					break;
				}
			}
		}

		public void InitializeMeshes(CharacterSettings newSettings = null, bool resetAll = true)
		{
			_transform = base.transform;
			if (selectedsettings == null && newSettings == null)
			{
				Debug.LogError("_settings = null, Unable to initialize character");
			}
			else
			{
				_settings = ((newSettings != null) ? newSettings : selectedsettings);
				if (newSettings != null)
				{
					selectedsettings = newSettings;
				}
			}
			UnlockPrefab();
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < _transform.childCount; i++)
			{
				list.Add(_transform.GetChild(i).gameObject);
			}
			UnityEngine.Object[] objects;
			if (list.Count > 0)
			{
				objects = list.ToArray();
				DestroyObjects(objects);
			}
			characterBlendshapeDatas.Clear();
			foreach (CharacterBlendshapeData characterBlendshapeData in _settings.characterBlendshapeDatas)
			{
				characterBlendshapeDatas.Add(new CharacterBlendshapeData(characterBlendshapeData.blendshapeName, characterBlendshapeData.type, characterBlendshapeData.group));
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(_settings.OriginalMesh, _transform);
			gameObject.name = "Character";
			ProbesAnchorOverride = new GameObject("Probes Anchor").GetComponent<Transform>();
			ProbesAnchorOverride.parent = _transform;
			ProbesAnchorOverride.localPosition = Vector3.up * 1.5f;
			ProbesAnchorOverride.localRotation = Quaternion.identity;
			List<Transform> list2 = new List<Transform>();
			new List<Transform>();
			for (int j = 0; j < gameObject.transform.childCount; j++)
			{
				Transform child = gameObject.transform.GetChild(j);
				list2.Add(child);
			}
			characterParts.Clear();
			clothesAnchors.Clear();
			foreach (Transform item in list2)
			{
				item.SetParent(_transform);
				SkinnedMeshRenderer component = item.GetComponent<SkinnedMeshRenderer>();
				string[] array = item.name.Split('_');
				string objType = array[0];
				string input = ((array.Length == 3) ? array[2] : "-");
				int num = -1;
				Match match = Regex.Match(input, "(\\d+)");
				if (match.Success)
				{
					num = int.Parse(match.Groups[1].Value);
				}
				if ((num != -1 && num < MinLODLevels) || num > MaxLODLevels)
				{
					objects = new GameObject[1] { item.gameObject };
					DestroyObjects(objects);
					continue;
				}
				if (component != null)
				{
					component.updateWhenOffscreen = true;
					component.probeAnchor = ProbesAnchorOverride;
				}
				if (objType != "ACCESSORY" && objType != "HAT" && objType != "PANTS" && objType != "SHIRT" && objType != "SHOES" && objType != "ITEM1" && objType.ToLowerInvariant() != "hips")
				{
					if (!(component == null))
					{
						component.sharedMaterials = new Material[0];
						if (objType == "COMBINED")
						{
							component.gameObject.SetActive(value: false);
						}
						else
						{
							component.sharedMaterials = new Material[1] { _settings.bodyMaterial };
						}
						CharacterPart characterPart = characterParts.Find((CharacterPart f) => f.name == objType);
						if (characterPart == null)
						{
							CharacterPart characterPart2 = new CharacterPart();
							characterPart2.name = objType;
							characterPart2.skinnedMesh.Add(component);
							characterParts.Add(characterPart2);
						}
						else
						{
							characterPart.skinnedMesh.Add(component);
						}
					}
				}
				else if (objType.ToLowerInvariant() == "hips")
				{
					item.SetSiblingIndex(0);
					originHip = item;
					Transform[] componentsInChildren = originHip.GetComponentsInChildren<Transform>();
					headHip = componentsInChildren.First((Transform f) => f.name.ToLowerInvariant() == "head");
				}
				else if ((objType == "HAT" || objType == "SHIRT" || objType == "PANTS" || objType == "SHOES" || objType == "ACCESSORY" || objType == "ITEM1") && !(component == null))
				{
					component.gameObject.SetActive(value: false);
					ClothesAnchor clothesAnchor = clothesAnchors.Find((ClothesAnchor f) => f.partType.ToString().ToLowerInvariant() == objType.ToLowerInvariant());
					if (clothesAnchor == null)
					{
						ClothesAnchor clothesAnchor2 = new ClothesAnchor();
						clothesAnchor2.partType = (CharacterElementType)Enum.Parse(typeof(CharacterElementType), objType.ToLowerInvariant(), ignoreCase: true);
						clothesAnchor2.skinnedMesh.Add(component);
						clothesAnchors.Add(clothesAnchor2);
					}
					else
					{
						clothesAnchor.skinnedMesh.Add(component);
					}
				}
			}
			objects = new GameObject[1] { gameObject };
			DestroyObjects(objects);
			_lodGroup = GetComponent<LODGroup>();
			animator = GetComponent<Animator>();
			if (animator == null)
			{
				animator = base.gameObject.AddComponent<Animator>();
			}
			if (_lodGroup != null && MinLODLevels == MaxLODLevels)
			{
				UnityEngine.Object.DestroyImmediate(_lodGroup);
			}
			else if (_lodGroup == null)
			{
				_lodGroup = base.gameObject.AddComponent<LODGroup>();
			}
			animator.avatar = _settings.Avatar;
			animator.runtimeAnimatorController = _settings.Animator;
			animator.Rebind();
			if (resetAll)
			{
				ResetAll(ignore_settingsDefaultElements: false);
			}
			RecalculateLOD();
			if (!_settings.bodyMaterial.HasProperty("_SkinColor"))
			{
				notAPP2Shader = true;
			}
			else
			{
				Skin = _settings.bodyMaterial.GetColor("_SkinColor");
				Eye = _settings.bodyMaterial.GetColor("_EyeColor");
				Hair = _settings.bodyMaterial.GetColor("_HairColor");
				Underpants = _settings.bodyMaterial.GetColor("_UnderpantsColor");
				OralCavity = _settings.bodyMaterial.GetColor("_OralCavityColor");
				Teeth = _settings.bodyMaterial.GetColor("_TeethColor");
			}
			LockPrefab();
		}

		public void UpdateSkinnedMeshesOffscreenBounds()
		{
			foreach (SkinnedMeshRenderer allMesh in GetAllMeshes())
			{
				SkinnedMeshRenderer mesh = allMesh;
				mesh.updateWhenOffscreen = UpdateWhenOffscreenMeshes;
				if (!UpdateWhenOffscreenMeshes)
				{
					if (GetCharacterInstanceStatus() != CharacterInstanceStatus.PrefabEditingInProjectView && GetCharacterInstanceStatus() != CharacterInstanceStatus.PrefabStageSceneOpened)
					{
						StartCoroutine(UpdateBounds());
					}
					else
					{
						UpdateBounds();
					}
				}
				IEnumerator UpdateBounds()
				{
					mesh.updateWhenOffscreen = true;
					if (Application.isPlaying)
					{
						yield return new WaitForEndOfFrame();
					}
					else
					{
						yield return new WaitForSeconds(0.1f);
					}
					Bounds localBounds = default(Bounds);
					Vector3 center = mesh.localBounds.center;
					Vector3 extents = mesh.localBounds.extents * 1.4f;
					localBounds.center = center;
					localBounds.extents = extents;
					mesh.updateWhenOffscreen = false;
					mesh.localBounds = localBounds;
					if (_lodGroup != null)
					{
						_lodGroup.RecalculateBounds();
					}
				}
			}
		}

		public List<CharacterSettingsSelector> GetCharacterSettingsSelectors()
		{
			return Settings.settingsSelectors;
		}

		public void ResetBodyMaterial()
		{
			foreach (CharacterPart characterPart3 in characterParts)
			{
				foreach (SkinnedMeshRenderer item in characterPart3.skinnedMesh)
				{
					item.sharedMaterial = _settings.bodyMaterial;
				}
			}
			CharacterPart characterPart = GetCharacterPart("HAIR");
			CharacterElementsPreset elementsPreset = GetElementsPreset(CharacterElementType.Hair, characterSelectedElements.GetSelectedIndex(CharacterElementType.Hair));
			if (elementsPreset != null)
			{
				List<Material> list = elementsPreset.mats.ToList();
				if (elementsPreset.mats != null && elementsPreset.mats.Length != 0)
				{
					for (int i = 0; i < characterPart.skinnedMesh.Count; i++)
					{
						characterPart.skinnedMesh[i].sharedMaterials = list.ToArray();
						for (int j = 0; j < list.Count; j++)
						{
							if (list[j].name == _settings.bodyMaterial.name)
							{
								characterPart.skinnedMesh[i].SetPropertyBlock(bodyPropertyBlock, j);
							}
						}
					}
				}
			}
			CharacterPart characterPart2 = GetCharacterPart("BEARD");
			CharacterElementsPreset elementsPreset2 = GetElementsPreset(CharacterElementType.Beard, characterSelectedElements.GetSelectedIndex(CharacterElementType.Beard));
			if (elementsPreset2 != null)
			{
				List<Material> list2 = elementsPreset2.mats.ToList();
				if (elementsPreset2.mats != null && elementsPreset2.mats.Length != 0)
				{
					for (int k = 0; k < characterPart2.skinnedMesh.Count; k++)
					{
						characterPart2.skinnedMesh[k].sharedMaterials = list2.ToArray();
						for (int l = 0; l < list2.Count; l++)
						{
							if (list2[l].name == _settings.bodyMaterial.name)
							{
								characterPart2.skinnedMesh[k].SetPropertyBlock(bodyPropertyBlock, l);
							}
						}
					}
				}
			}
			ClothesAnchor clothesAnchor = GetClothesAnchor(CharacterElementType.Shoes);
			for (int m = 0; m < clothesAnchor.skinnedMesh.Count; m++)
			{
				List<Material> list3 = clothesAnchor.skinnedMesh[m].sharedMaterials.ToList();
				for (int n = 0; n < list3.Count; n++)
				{
					if (list3[n].name == _settings.bodyMaterial.name)
					{
						list3[n] = _settings.bodyMaterial;
						clothesAnchor.skinnedMesh[m].sharedMaterials = list3.ToArray();
					}
				}
			}
			if (CurrentCombinerState != CombinerState.Combined && CurrentCombinerState != CombinerState.UsedPreBuitMeshes)
			{
				return;
			}
			List<SkinnedMeshRenderer> skinnedMesh = GetCharacterPart("COMBINED").skinnedMesh;
			for (int num = 0; num < skinnedMesh.Count; num++)
			{
				if (!(skinnedMesh[num] != null))
				{
					continue;
				}
				List<Material> list4 = skinnedMesh[num].sharedMaterials.ToList();
				for (int num2 = 0; num2 < list4.Count; num2++)
				{
					if (list4[num2].name == _settings.bodyMaterial.name)
					{
						list4[num2] = _settings.bodyMaterial;
						skinnedMesh[num].sharedMaterials = list4.ToArray();
					}
				}
			}
		}

		public void InitColors()
		{
			if (!(Settings == null))
			{
				if (bodyPropertyBlock == null)
				{
					bodyPropertyBlock = new MaterialPropertyBlock();
				}
				SetBodyColor(BodyColorPart.Skin, Skin);
				SetBodyColor(BodyColorPart.Eye, Eye);
				SetBodyColor(BodyColorPart.Hair, Hair);
				SetBodyColor(BodyColorPart.Underpants, Underpants);
				SetBodyColor(BodyColorPart.Teeth, Teeth);
				SetBodyColor(BodyColorPart.OralCavity, OralCavity);
			}
		}

		public void ResetBodyColors()
		{
			if (!notAPP2Shader)
			{
				if (_settings.bodyMaterial.HasProperty("_SkinColor"))
				{
					SetBodyColor(BodyColorPart.Skin, _settings.bodyMaterial.GetColor("_SkinColor"));
				}
				if (_settings.bodyMaterial.HasProperty("_EyeColor"))
				{
					SetBodyColor(BodyColorPart.Eye, _settings.bodyMaterial.GetColor("_EyeColor"));
				}
				if (_settings.bodyMaterial.HasProperty("_HairColor"))
				{
					SetBodyColor(BodyColorPart.Hair, _settings.bodyMaterial.GetColor("_HairColor"));
				}
				if (_settings.bodyMaterial.HasProperty("_UnderpantsColor"))
				{
					SetBodyColor(BodyColorPart.Underpants, _settings.bodyMaterial.GetColor("_UnderpantsColor"));
				}
				if (_settings.bodyMaterial.HasProperty("_OralCavityColor"))
				{
					SetBodyColor(BodyColorPart.OralCavity, _settings.bodyMaterial.GetColor("_OralCavityColor"));
				}
				if (_settings.bodyMaterial.HasProperty("_TeethColor"))
				{
					SetBodyColor(BodyColorPart.Teeth, _settings.bodyMaterial.GetColor("_TeethColor"));
				}
			}
		}

		public void SetBlendshapeValue(CharacterBlendShapeType type, float weight, string[] forPart = null, CharacterElementType[] forClothPart = null)
		{
			try
			{
				string text = type.ToString();
				if (CurrentCombinerState != CombinerState.Combined && CurrentCombinerState != CombinerState.UsedPreBuitMeshes)
				{
					foreach (CharacterPart characterPart in characterParts)
					{
						if (forPart != null && !forPart.Contains(characterPart.name))
						{
							continue;
						}
						foreach (SkinnedMeshRenderer item in characterPart.skinnedMesh)
						{
							if (!(item != null) || !(item.sharedMesh != null))
							{
								continue;
							}
							for (int i = 0; i < item.sharedMesh.blendShapeCount; i++)
							{
								if (text == item.sharedMesh.GetBlendShapeName(i))
								{
									int blendShapeIndex = item.sharedMesh.GetBlendShapeIndex(text);
									if (blendShapeIndex != -1 && !Settings.DisableBlendshapeModifier)
									{
										item.SetBlendShapeWeight(blendShapeIndex, weight);
									}
								}
							}
						}
					}
					foreach (ClothesAnchor clothesAnchor in clothesAnchors)
					{
						if (forClothPart != null && !forClothPart.Contains(clothesAnchor.partType))
						{
							continue;
						}
						foreach (SkinnedMeshRenderer item2 in clothesAnchor.skinnedMesh)
						{
							if (!(item2 != null) || !(item2.sharedMesh != null))
							{
								continue;
							}
							for (int j = 0; j < item2.sharedMesh.blendShapeCount; j++)
							{
								if (text == item2.sharedMesh.GetBlendShapeName(j))
								{
									int blendShapeIndex2 = item2.sharedMesh.GetBlendShapeIndex(text);
									if (blendShapeIndex2 != -1 && !Settings.DisableBlendshapeModifier)
									{
										item2.SetBlendShapeWeight(blendShapeIndex2, weight);
									}
								}
							}
						}
					}
				}
				else
				{
					foreach (SkinnedMeshRenderer item3 in GetCharacterPart("COMBINED").skinnedMesh)
					{
						if (!(item3.sharedMesh != null))
						{
							continue;
						}
						for (int k = 0; k < item3.sharedMesh.blendShapeCount; k++)
						{
							if (!Settings.DisableBlendshapeModifier && text == item3.sharedMesh.GetBlendShapeName(k))
							{
								item3.SetBlendShapeWeight(item3.sharedMesh.GetBlendShapeIndex(text), weight);
							}
						}
					}
				}
				GetBlendshapeData(type).value = weight;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public void ForceLOD(int lodLevel)
		{
			if (lodLevel <= MaxLODLevels - MinLODLevels)
			{
				if (lodLevel != 0)
				{
					_lodGroup.ForceLOD(lodLevel);
				}
				else
				{
					_lodGroup.ForceLOD(-1);
				}
			}
		}

		public void SetElementByIndex(CharacterElementType type, int index)
		{
			if (Settings == null)
			{
				Debug.LogError("settings = null");
				return;
			}
			switch (type)
			{
			case CharacterElementType.Hair:
				SetHairByIndex(index);
				RecalculateShapes();
				return;
			case CharacterElementType.Beard:
				SetBeardByIndex(index);
				RecalculateShapes();
				return;
			}
			ClothesAnchor clothesAnchor = GetClothesAnchor(type);
			CharacterElementsPreset elementsPreset = GetElementsPreset(type, characterSelectedElements.GetSelectedIndex(type));
			CharacterElementsPreset elementsPreset2 = GetElementsPreset(type, index);
			float y = 0f;
			if (elementsPreset != null && (elementsPreset2 != null || index == -1))
			{
				UnHideParts(elementsPreset.hideParts, type);
			}
			if (elementsPreset2 != null)
			{
				if (elementsPreset2.mesh.Length == 0)
				{
					Debug.LogErrorFormat($"Not found meshes for <{elementsPreset2.name}> element");
					return;
				}
				if (type == CharacterElementType.Shirt)
				{
					GetBlendshapeData(CharacterBlendShapeType.BackpackOffset).value = 100f;
					if (characterSelectedElements.GetSelectedIndex(CharacterElementType.Item1) != -1)
					{
						SetBlendshapeValue(CharacterBlendShapeType.BackpackOffset, 100f);
					}
				}
				y = elementsPreset2.yOffset;
				for (int i = 0; i < MaxLODLevels - MinLODLevels + 1; i++)
				{
					int num = i + MinLODLevels;
					if (!clothesAnchor.skinnedMesh[i].gameObject.activeSelf && !IsBaked())
					{
						clothesAnchor.skinnedMesh[i].gameObject.SetActive(value: true);
					}
					clothesAnchor.skinnedMesh[i].sharedMesh = elementsPreset2.mesh[num];
					if (elementsPreset2.mats != null && elementsPreset2.mats.Length != 0)
					{
						List<Material> list = elementsPreset2.mats.ToList();
						clothesAnchor.skinnedMesh[i].sharedMaterials = list.ToArray();
						for (int j = 0; j < list.Count; j++)
						{
							if (list[j].name == _settings.bodyMaterial.name)
							{
								clothesAnchor.skinnedMesh[i].SetPropertyBlock(bodyPropertyBlock, j);
							}
						}
					}
					for (int k = 0; k < clothesAnchor.skinnedMesh[i].sharedMesh.blendShapeCount; k++)
					{
						if (clothesAnchor.skinnedMesh[i] != null && clothesAnchor.skinnedMesh[i].sharedMesh != null)
						{
							string blendShapeName = clothesAnchor.skinnedMesh[i].sharedMesh.GetBlendShapeName(k);
							CharacterBlendshapeData blendshapeData = GetBlendshapeData(blendShapeName);
							if (blendshapeData != null && !Settings.DisableBlendshapeModifier)
							{
								clothesAnchor.skinnedMesh[i].SetBlendShapeWeight(k, blendshapeData.value);
							}
						}
					}
				}
				HideParts(elementsPreset2.hideParts);
			}
			else
			{
				if (type == CharacterElementType.Shirt)
				{
					GetBlendshapeData(CharacterBlendShapeType.BackpackOffset).value = 0f;
					if (characterSelectedElements.GetSelectedIndex(CharacterElementType.Item1) != -1)
					{
						SetBlendshapeValue(CharacterBlendShapeType.BackpackOffset, 0f);
					}
				}
				if (index != -1)
				{
					Debug.LogError($"Element <{type.ToString()}> with index {index} not found. Please check Character Presets arrays.");
					return;
				}
				if (clothesAnchor != null && clothesAnchor.skinnedMesh != null)
				{
					foreach (SkinnedMeshRenderer item in clothesAnchor.skinnedMesh)
					{
						if (item != null)
						{
							item.sharedMesh = null;
							item.gameObject.SetActive(value: false);
						}
					}
				}
			}
			if (type == CharacterElementType.Shoes)
			{
				SetFeetOffset(new Vector3(0f, y, 0f));
				feetOffset = y;
			}
			characterSelectedElements.SetSelectedIndex(type, index);
		}

		public void ClearElement(CharacterElementType type)
		{
			switch (type)
			{
			case CharacterElementType.Hair:
				SetHairByIndex(-1);
				break;
			case CharacterElementType.Beard:
				SetBeardByIndex(-1);
				break;
			default:
				SetElementByIndex(type, -1);
				break;
			}
		}

		public void SetHeight(float height)
		{
			heightValue = height;
			if (originHip != null)
			{
				originHip.localScale = new Vector3(1f + height / 1.4f, 1f + height, 1f + height);
			}
		}

		public void SetHeadSize(float size)
		{
			headSizeValue = size;
			if (headHip != null)
			{
				headHip.localScale = Vector3.one + Vector3.one * size;
			}
		}

		public void SetFeetOffset(Vector3 offset)
		{
			originHip.localPosition = offset;
		}

		private void SetHairByIndex(int index)
		{
			CharacterPart characterPart = GetCharacterPart("Hair");
			if (characterPart == null || characterPart.skinnedMesh.Count <= 0)
			{
				return;
			}
			if (index != -1)
			{
				CharacterElementsPreset characterElementsPreset = _settings.hairPresets.ElementAtOrDefault(index);
				if (characterElementsPreset == null)
				{
					Debug.LogError($"Hair with index {index} not found");
					return;
				}
				for (int i = 0; i < MaxLODLevels - MinLODLevels + 1; i++)
				{
					int num = i + MinLODLevels;
					if (characterPart.skinnedMesh.Count > 0 && characterPart.skinnedMesh.Count - 1 >= i && characterPart.skinnedMesh[i] != null)
					{
						if (!characterPart.skinnedMesh[i].gameObject.activeSelf)
						{
							characterPart.skinnedMesh[i].gameObject.SetActive(value: true);
						}
						characterPart.skinnedMesh[i].sharedMesh = _settings.hairPresets[index].mesh[num];
					}
					if (characterElementsPreset.mats == null || characterElementsPreset.mats.Length == 0)
					{
						continue;
					}
					List<Material> list = characterElementsPreset.mats.ToList();
					characterPart.skinnedMesh[i].sharedMaterials = list.ToArray();
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].name == _settings.bodyMaterial.name)
						{
							characterPart.skinnedMesh[i].SetPropertyBlock(bodyPropertyBlock, j);
						}
					}
				}
			}
			else
			{
				foreach (SkinnedMeshRenderer item in characterPart.skinnedMesh)
				{
					item.sharedMesh = null;
					item.gameObject.SetActive(value: false);
				}
			}
			characterSelectedElements.SetSelectedIndex(CharacterElementType.Hair, index);
		}

		private void SetBeardByIndex(int index)
		{
			CharacterPart characterPart = GetCharacterPart("Beard");
			if (characterPart == null || characterPart.skinnedMesh.Count <= 0)
			{
				return;
			}
			if (index != -1)
			{
				CharacterElementsPreset characterElementsPreset = _settings.beardPresets.ElementAtOrDefault(index);
				if (characterElementsPreset == null)
				{
					Debug.LogError($"Beard with index {index} not found");
					return;
				}
				for (int i = 0; i < MaxLODLevels - MinLODLevels + 1; i++)
				{
					int num = i + MinLODLevels;
					if (!characterPart.skinnedMesh[i].gameObject.activeSelf)
					{
						characterPart.skinnedMesh[i].gameObject.SetActive(value: true);
					}
					characterPart.skinnedMesh[i].sharedMesh = _settings.beardPresets[index].mesh[num];
					if (characterElementsPreset.mats == null || characterElementsPreset.mats.Length == 0)
					{
						continue;
					}
					List<Material> list = characterElementsPreset.mats.ToList();
					characterPart.skinnedMesh[i].sharedMaterials = list.ToArray();
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].name == _settings.bodyMaterial.name)
						{
							characterPart.skinnedMesh[i].SetPropertyBlock(bodyPropertyBlock, j);
						}
					}
				}
			}
			else
			{
				foreach (SkinnedMeshRenderer item in characterPart.skinnedMesh)
				{
					item.sharedMesh = null;
					item.gameObject.SetActive(value: false);
				}
			}
			characterSelectedElements.SetSelectedIndex(CharacterElementType.Beard, index);
		}

		public ClothesAnchor GetClothesAnchor(CharacterElementType type)
		{
			foreach (ClothesAnchor clothesAnchor in clothesAnchors)
			{
				if (clothesAnchor.partType == type)
				{
					return clothesAnchor;
				}
			}
			return null;
		}

		public CharacterPart GetCharacterPart(string name)
		{
			foreach (CharacterPart characterPart in characterParts)
			{
				if (characterPart.name.ToLowerInvariant() == name.ToLowerInvariant())
				{
					return characterPart;
				}
			}
			return null;
		}

		public List<SkinnedMeshRenderer> GetAllMeshesByLod(int lod)
		{
			List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
			foreach (CharacterPart characterPart in characterParts)
			{
				if (characterPart.skinnedMesh.Count >= lod)
				{
					list.Add(characterPart.skinnedMesh[lod]);
				}
			}
			foreach (ClothesAnchor clothesAnchor in clothesAnchors)
			{
				if (clothesAnchor.skinnedMesh.Count >= lod)
				{
					list.Add(clothesAnchor.skinnedMesh[lod]);
				}
			}
			return list;
		}

		public List<SkinnedMeshRenderer> GetAllMeshes()
		{
			List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
			foreach (CharacterPart characterPart in characterParts)
			{
				list.AddRange(characterPart.skinnedMesh);
			}
			foreach (ClothesAnchor clothesAnchor in clothesAnchors)
			{
				list.AddRange(clothesAnchor.skinnedMesh);
			}
			return list;
		}

		public List<SkinnedMeshRenderer> GetAllMeshes(bool onlyBodyMeshes = false, string[] excludeNames = null)
		{
			List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
			foreach (CharacterPart characterPart in characterParts)
			{
				if (excludeNames == null || !excludeNames.Contains(characterPart.name))
				{
					list.AddRange(characterPart.skinnedMesh);
				}
			}
			if (onlyBodyMeshes)
			{
				foreach (ClothesAnchor clothesAnchor in clothesAnchors)
				{
					list.AddRange(clothesAnchor.skinnedMesh);
				}
			}
			return list;
		}

		public void HideParts(string[] parts)
		{
			foreach (string text in parts)
			{
				foreach (CharacterPart characterPart in characterParts)
				{
					if (!(characterPart.name.ToLowerInvariant() == text.ToLowerInvariant()))
					{
						continue;
					}
					foreach (SkinnedMeshRenderer item in characterPart.skinnedMesh)
					{
						item.enabled = false;
					}
				}
			}
		}

		public void UnHideParts(string[] parts, CharacterElementType hidePartsForElement)
		{
			foreach (string text in parts)
			{
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				int selectedIndex = characterSelectedElements.GetSelectedIndex(CharacterElementType.Shirt);
				int selectedIndex2 = characterSelectedElements.GetSelectedIndex(CharacterElementType.Pants);
				int selectedIndex3 = characterSelectedElements.GetSelectedIndex(CharacterElementType.Shoes);
				if (selectedIndex != -1 && hidePartsForElement != CharacterElementType.Shirt)
				{
					string[] hideParts = GetElementsPreset(CharacterElementType.Shirt, selectedIndex).hideParts;
					for (int j = 0; j < hideParts.Length; j++)
					{
						if (hideParts[j] == text)
						{
							flag = true;
							break;
						}
					}
				}
				if (selectedIndex2 != -1 && hidePartsForElement != CharacterElementType.Pants)
				{
					string[] hideParts = GetElementsPreset(CharacterElementType.Pants, selectedIndex2).hideParts;
					for (int j = 0; j < hideParts.Length; j++)
					{
						if (hideParts[j] == text)
						{
							flag2 = true;
							break;
						}
					}
				}
				if (selectedIndex3 != -1 && hidePartsForElement != CharacterElementType.Shoes)
				{
					string[] hideParts = GetElementsPreset(CharacterElementType.Shoes, selectedIndex3).hideParts;
					for (int j = 0; j < hideParts.Length; j++)
					{
						if (hideParts[j] == text)
						{
							flag3 = true;
							break;
						}
					}
				}
				if (flag || flag2 || flag3)
				{
					continue;
				}
				foreach (CharacterPart characterPart in characterParts)
				{
					if (!(characterPart.name.ToLowerInvariant() == text.ToLowerInvariant()))
					{
						continue;
					}
					foreach (SkinnedMeshRenderer item in characterPart.skinnedMesh)
					{
						item.enabled = true;
					}
				}
			}
		}

		public void SetBodyColor(BodyColorPart bodyColorPart, Color color)
		{
			if (notAPP2Shader)
			{
				return;
			}
			if (bodyPropertyBlock == null)
			{
				bodyPropertyBlock = new MaterialPropertyBlock();
			}
			switch (bodyColorPart)
			{
			case BodyColorPart.Skin:
				bodyPropertyBlock.SetColor("_SkinColor", color);
				break;
			case BodyColorPart.Eye:
				bodyPropertyBlock.SetColor("_EyeColor", color);
				break;
			case BodyColorPart.Hair:
				bodyPropertyBlock.SetColor("_HairColor", color);
				break;
			case BodyColorPart.Underpants:
				bodyPropertyBlock.SetColor("_UnderpantsColor", color);
				break;
			case BodyColorPart.Teeth:
				bodyPropertyBlock.SetColor("_TeethColor", color);
				break;
			case BodyColorPart.OralCavity:
				bodyPropertyBlock.SetColor("_OralCavityColor", color);
				break;
			}
			foreach (SkinnedMeshRenderer item in IsBaked() ? GetCharacterPart("COMBINED").skinnedMesh : GetAllMeshes(onlyBodyMeshes: true, new string[1] { "COMBINED" }))
			{
				for (int i = 0; i < item.sharedMaterials.Length; i++)
				{
					if (item.sharedMaterials[i] == _settings.bodyMaterial)
					{
						item.SetPropertyBlock(bodyPropertyBlock, i);
					}
				}
			}
			MeshRenderer component = base.transform.Find("hips/Root/Spine1/Spine2/Chest/Neck/Head/nose").GetComponent<MeshRenderer>();
			for (int j = 0; j < component.sharedMaterials.Length; j++)
			{
				if (component.sharedMaterials[j] == _settings.bodyMaterial)
				{
					component.SetPropertyBlock(bodyPropertyBlock, j);
				}
			}
			switch (bodyColorPart)
			{
			case BodyColorPart.Skin:
				Skin = color;
				break;
			case BodyColorPart.Eye:
				Eye = color;
				break;
			case BodyColorPart.Hair:
				Hair = color;
				break;
			case BodyColorPart.Underpants:
				Underpants = color;
				break;
			case BodyColorPart.OralCavity:
				OralCavity = color;
				break;
			case BodyColorPart.Teeth:
				Teeth = color;
				break;
			}
		}

		public Color GetBodyColor(BodyColorPart bodyColorPart)
		{
			return bodyColorPart switch
			{
				BodyColorPart.Skin => Skin, 
				BodyColorPart.Eye => Eye, 
				BodyColorPart.Hair => Hair, 
				BodyColorPart.Underpants => Underpants, 
				BodyColorPart.OralCavity => OralCavity, 
				BodyColorPart.Teeth => Teeth, 
				_ => Color.clear, 
			};
		}

		public void SetCharacterSetup(CharacterCustomizationSetup characterCustomizationSetup)
		{
			characterCustomizationSetup.ApplyToCharacter(this);
		}

		public CharacterCustomizationSetup GetSetup()
		{
			CharacterCustomizationSetup characterCustomizationSetup = new CharacterCustomizationSetup();
			foreach (CharacterBlendshapeData characterBlendshapeData in characterBlendshapeDatas)
			{
				characterCustomizationSetup.blendshapes.Add(new CharacterBlendshapeData(characterBlendshapeData.blendshapeName, characterBlendshapeData.type, characterBlendshapeData.group, characterBlendshapeData.value));
			}
			characterCustomizationSetup.MinLod = MinLODLevels;
			characterCustomizationSetup.MaxLod = MaxLODLevels;
			characterCustomizationSetup.selectedElements.Accessory = characterSelectedElements.Accessory;
			characterCustomizationSetup.selectedElements.Beard = characterSelectedElements.Beard;
			characterCustomizationSetup.selectedElements.Hair = characterSelectedElements.Hair;
			characterCustomizationSetup.selectedElements.Hat = characterSelectedElements.Hat;
			characterCustomizationSetup.selectedElements.Item1 = characterSelectedElements.Item1;
			characterCustomizationSetup.selectedElements.Pants = characterSelectedElements.Pants;
			characterCustomizationSetup.selectedElements.Shirt = characterSelectedElements.Shirt;
			characterCustomizationSetup.selectedElements.Shoes = characterSelectedElements.Shoes;
			characterCustomizationSetup.Height = heightValue;
			characterCustomizationSetup.HeadSize = headSizeValue;
			characterCustomizationSetup.SkinColor = new float[4] { Skin.r, Skin.g, Skin.b, Skin.a };
			characterCustomizationSetup.HairColor = new float[4] { Hair.r, Hair.g, Hair.b, Hair.a };
			characterCustomizationSetup.UnderpantsColor = new float[4] { Underpants.r, Underpants.g, Underpants.b, Underpants.a };
			characterCustomizationSetup.TeethColor = new float[4] { Teeth.r, Teeth.g, Teeth.b, Teeth.a };
			characterCustomizationSetup.OralCavityColor = new float[4] { OralCavity.r, OralCavity.g, OralCavity.b, OralCavity.a };
			characterCustomizationSetup.EyeColor = new float[4] { Eye.r, Eye.g, Eye.b, Eye.a };
			characterCustomizationSetup.settingsName = Settings.name;
			return characterCustomizationSetup;
		}

		public void ApplySavedCharacterData(SavedCharacterData data)
		{
			LoadCharacterFromFile(data.path);
		}

		public void LoadCharacterFromFile(string path)
		{
			if (!File.Exists(path))
			{
				return;
			}
			string extension = Path.GetExtension(path);
			string text = File.ReadAllText(path);
			if (text.Length > 0)
			{
				CharacterCustomizationSetup.CharacterFileSaveFormat characterFileSaveFormat = CharacterCustomizationSetup.CharacterFileSaveFormat.Binary;
				switch (extension)
				{
				case ".json":
					characterFileSaveFormat = CharacterCustomizationSetup.CharacterFileSaveFormat.Json;
					break;
				case ".xml":
					characterFileSaveFormat = CharacterCustomizationSetup.CharacterFileSaveFormat.Xml;
					break;
				case ".bin":
					characterFileSaveFormat = CharacterCustomizationSetup.CharacterFileSaveFormat.Binary;
					break;
				default:
					Debug.LogError("File format not supported - " + extension);
					return;
				}
				CharacterCustomizationSetup characterCustomizationSetup = CharacterCustomizationSetup.Deserialize(text, characterFileSaveFormat);
				if (characterCustomizationSetup != null)
				{
					SetCharacterSetup(characterCustomizationSetup);
					Debug.Log($"Loaded {path} save");
				}
			}
		}

		public List<SavedCharacterData> GetSavedCharacterDatas(string path = "")
		{
			List<SavedCharacterData> list = new List<SavedCharacterData>();
			string persistentDataPath = Application.persistentDataPath;
			string arg = string.Format("{0}/{1}", persistentDataPath, "apack_characters_data");
			arg = $"{arg}/{Settings.name}";
			if (!Directory.Exists(arg))
			{
				return list;
			}
			string[] files = Directory.GetFiles(arg, "appack25*");
			foreach (string path2 in files)
			{
				SavedCharacterData savedCharacterData = new SavedCharacterData();
				string[] array = Path.GetFileName(path2).Split('_');
				string text = "";
				for (int j = 1; j < array.Length - 1; j++)
				{
					text = text + array[j] + ((j != array.Length - 2) ? "_" : "");
				}
				savedCharacterData.name = text;
				savedCharacterData.path = path2;
				list.Add(savedCharacterData);
			}
			return list;
		}

		public void ClearSavedData(SavedCharacterData data)
		{
			if (data != null && File.Exists(data.path))
			{
				File.Delete(data.path);
			}
		}

		public void ClearSavedData()
		{
			List<SavedCharacterData> savedCharacterDatas = GetSavedCharacterDatas();
			foreach (SavedCharacterData item in savedCharacterDatas)
			{
				ClearSavedData(item);
			}
			Debug.Log($"Removed {savedCharacterDatas.Count} saves");
		}

		public void SaveCharacterToFile(CharacterCustomizationSetup.CharacterFileSaveFormat format, string path = "", string name = "")
		{
			string text = path;
			string text2 = "json";
			switch (format)
			{
			case CharacterCustomizationSetup.CharacterFileSaveFormat.Json:
				text2 = "json";
				break;
			case CharacterCustomizationSetup.CharacterFileSaveFormat.Xml:
				text2 = "xml";
				break;
			case CharacterCustomizationSetup.CharacterFileSaveFormat.Binary:
				text2 = "bin";
				break;
			}
			if (path.Length == 0)
			{
				_ = new string[3] { "json", "xml", "bin" };
				string persistentDataPath = Application.persistentDataPath;
				string arg = string.Format("{0}/{1}", persistentDataPath, "apack_characters_data");
				arg = $"{arg}/{Settings.name}";
				if (!Directory.Exists(arg))
				{
					Directory.CreateDirectory(arg);
				}
				string text3 = base.gameObject.name;
				text = $"{arg}/appack25_{text3}_{DateTimeOffset.Now.ToUnixTimeSeconds()}.{text2}";
			}
			else
			{
				text = $"{path}/{name}_{DateTimeOffset.Now.ToUnixTimeSeconds()}.{text2}";
			}
			string text4 = GetSetup().Serialize(format);
			if (text4.Length > 0)
			{
				File.WriteAllText(text, text4, Encoding.UTF8);
				Debug.Log($"Character data saved to ({text})");
			}
		}

		public void RecalculateShapes()
		{
			foreach (CharacterBlendshapeData characterBlendshapeData in characterBlendshapeDatas)
			{
				SetBlendshapeValue(characterBlendshapeData.type, characterBlendshapeData.value);
			}
		}

		public void EditorSavePreBuiltPrefab()
		{
			Debug.LogError("Pre Built Character can be created only in editor.");
		}

		public void BakeCharacter(bool usePreBuiltMeshes = false)
		{
			if (usePreBuiltMeshes)
			{
				if (combinedCharacter == null)
				{
					Debug.LogError("CombinedCharacter variable == null");
					return;
				}
				if (combinedCharacter != null && combinedCharacter.settings != Settings)
				{
					Debug.LogError("PreBuilt settings not equal current character settings");
					return;
				}
				if (combinedCharacter.preBuiltDatas == null || combinedCharacter.preBuiltDatas.Count == 0)
				{
					Debug.LogErrorFormat("CombinedCharacter object({0}) is not valid!", combinedCharacter.name);
					return;
				}
				if (combinedCharacter.preBuiltDatas[0].meshes.Count < MaxLODLevels - MinLODLevels + 1)
				{
					Debug.LogErrorFormat("CombinedCharacter number of meshes({0}) is less than the number of LODs({1}) in the character\nTry combine character again or change LODs count", combinedCharacter.preBuiltDatas[0].meshes.Count, MaxLODLevels - MinLODLevels + 1);
					return;
				}
			}
			foreach (CharacterPart characterPart in characterParts)
			{
				if (characterPart.name.ToLowerInvariant() == "combined")
				{
					characterPart.skinnedMesh.ForEach(delegate(SkinnedMeshRenderer m)
					{
						m.sharedMesh = null;
						m.gameObject.SetActive(value: false);
					});
				}
			}
			try
			{
				if (!usePreBuiltMeshes)
				{
					CharacterCustomizationCombiner.MakeCombinedMeshes(this, null, 0f, delegate
					{
						MeshesProcess();
					});
				}
				else
				{
					MeshesProcess(usePreBuilt: true);
					CurrentCombinerState = CombinerState.UsedPreBuitMeshes;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				ClearBake();
			}
			RecalculateShapes();
			RecalculateLOD();
			void MeshesProcess(bool usePreBuilt = false)
			{
				foreach (CharacterPart characterPart2 in characterParts)
				{
					if (characterPart2.name.ToLowerInvariant() != "combined")
					{
						characterPart2.skinnedMesh.ForEach(delegate(SkinnedMeshRenderer m)
						{
							m.gameObject.SetActive(value: false);
						});
					}
					else
					{
						for (int i = 0; i < characterPart2.skinnedMesh.Count; i++)
						{
							if (usePreBuilt)
							{
								characterPart2.skinnedMesh[i].sharedMesh = combinedCharacter.preBuiltDatas[0].meshes[i];
								characterPart2.skinnedMesh[i].sharedMaterials = combinedCharacter.preBuiltDatas[0].materials.ToArray();
							}
							characterPart2.skinnedMesh[i].gameObject.SetActive(value: true);
						}
					}
				}
				foreach (ClothesAnchor clothesAnchor in clothesAnchors)
				{
					clothesAnchor.skinnedMesh.ForEach(delegate(SkinnedMeshRenderer m)
					{
						m.gameObject.SetActive(value: false);
					});
				}
				if (Application.isPlaying)
				{
					InitColors();
				}
			}
		}

		public void ClearBake()
		{
			foreach (CharacterPart characterPart in characterParts)
			{
				if (characterPart.name.ToLowerInvariant() == "combined")
				{
					characterPart.skinnedMesh.ForEach(delegate(SkinnedMeshRenderer m)
					{
						if (m.sharedMesh != null && CurrentCombinerState != CombinerState.UsedPreBuitMeshes)
						{
							m.sharedMesh.Clear();
							m.sharedMesh.ClearBlendShapes();
						}
						if (CurrentCombinerState == CombinerState.UsedPreBuitMeshes)
						{
							m.sharedMesh = null;
						}
						m.sharedMaterials = new Material[0];
						m.gameObject.SetActive(value: false);
					});
					continue;
				}
				characterPart.skinnedMesh.ForEach(delegate(SkinnedMeshRenderer m)
				{
					if (m.sharedMesh != null)
					{
						m.gameObject.SetActive(value: true);
					}
				});
			}
			foreach (ClothesAnchor clothesAnchor in clothesAnchors)
			{
				clothesAnchor.skinnedMesh.ForEach(delegate(SkinnedMeshRenderer m)
				{
					if (m.sharedMesh != null)
					{
						m.gameObject.SetActive(value: true);
					}
				});
			}
			CurrentCombinerState = CombinerState.NotCombined;
			RecalculateShapes();
			RecalculateLOD();
			Resources.UnloadUnusedAssets();
			ApplyPrefab();
		}

		public void RecalculateLOD()
		{
			if (!_lodGroup && MinLODLevels != MaxLODLevels)
			{
				_lodGroup = GetComponent<LODGroup>();
			}
			else if (MinLODLevels == MaxLODLevels)
			{
				return;
			}
			float[][] array = new float[4][]
			{
				new float[4] { 0.5f, 0.2f, 0.05f, 0f },
				new float[4] { 0.4f, 0.1f, 0f, 0f },
				new float[4] { 0.3f, 0f, 0f, 0f },
				new float[4]
			};
			LOD[] array2 = new LOD[MaxLODLevels - MinLODLevels + 1];
			for (int i = 0; i < MaxLODLevels - MinLODLevels + 1; i++)
			{
				if (clothesAnchors.ElementAtOrDefault(i) == null)
				{
					continue;
				}
				List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
				foreach (CharacterPart characterPart in characterParts)
				{
					list.Add(characterPart.skinnedMesh[i]);
				}
				foreach (ClothesAnchor clothesAnchor in clothesAnchors)
				{
					list.Add(clothesAnchor.skinnedMesh[i]);
				}
				int num = i;
				float screenRelativeTransitionHeight = array[3 - (MaxLODLevels - MinLODLevels)][i];
				Renderer[] renderers = list.ToArray();
				array2[num] = new LOD(screenRelativeTransitionHeight, renderers);
			}
			_lodGroup.SetLODs(array2);
			_lodGroup.RecalculateBounds();
		}

		public void SetLODRange(int minLod, int maxLod)
		{
			if (!IsBaked())
			{
				MinLODLevels = minLod;
				MaxLODLevels = maxLod;
				InitializeMeshes();
			}
		}

		public bool IsBaked()
		{
			if (CurrentCombinerState != CombinerState.Combined)
			{
				return CurrentCombinerState == CombinerState.UsedPreBuitMeshes;
			}
			return true;
		}

		public CharacterElementsPreset GetElementsPreset(CharacterElementType type, int index)
		{
			List<CharacterElementsPreset> elementsPresets = GetElementsPresets(type);
			if (elementsPresets.Count <= 0 || elementsPresets.Count - 1 < index || index == -1)
			{
				return null;
			}
			return elementsPresets[index];
		}

		public CharacterElementsPreset GetElementsPreset(CharacterElementType type, string name)
		{
			List<CharacterElementsPreset> elementsPresets = GetElementsPresets(type);
			if (elementsPresets.Count <= 0)
			{
				return null;
			}
			return elementsPresets.Find((CharacterElementsPreset f) => f.name == name);
		}

		public List<CharacterElementsPreset> GetElementsPresets(CharacterElementType type)
		{
			return type switch
			{
				CharacterElementType.Hat => _settings.hatsPresets, 
				CharacterElementType.Shirt => _settings.shirtsPresets, 
				CharacterElementType.Pants => _settings.pantsPresets, 
				CharacterElementType.Shoes => _settings.shoesPresets, 
				CharacterElementType.Accessory => _settings.accessoryPresets, 
				CharacterElementType.Hair => _settings.hairPresets, 
				CharacterElementType.Beard => _settings.beardPresets, 
				CharacterElementType.Item1 => _settings.item1Presets, 
				_ => null, 
			};
		}

		public void PlayBlendshapeAnimation(string animationName, float duration = 1f, float weightPower = 1f)
		{
			if (this.currentBlendshapeAnimation != null)
			{
				StopBlendshapeAnimations();
			}
			CurrentBlendshapeAnimation currentBlendshapeAnimation = new CurrentBlendshapeAnimation();
			foreach (CharacterAnimationPreset characterAnimationPreset in _settings.characterAnimationPresets)
			{
				if (characterAnimationPreset.name == animationName)
				{
					currentBlendshapeAnimation.preset = characterAnimationPreset;
					break;
				}
			}
			foreach (BlendshapeEmotionValue blendshape in currentBlendshapeAnimation.preset.blendshapes)
			{
				CharacterBlendshapeData blendshapeData = GetBlendshapeData(blendshape.BlendType);
				if (blendshapeData != null)
				{
					currentBlendshapeAnimation.blendShapesTemp.Add(new BlendshapeEmotionValue
					{
						BlendType = blendshape.BlendType,
						BlendValue = blendshapeData.value
					});
				}
			}
			currentBlendshapeAnimation.preset.AnimationPlayDuration = 1f / duration;
			currentBlendshapeAnimation.preset.weightPower = weightPower;
			this.currentBlendshapeAnimation = currentBlendshapeAnimation;
		}

		public void StopBlendshapeAnimations()
		{
			if (currentBlendshapeAnimation != null)
			{
				for (int i = 0; i < currentBlendshapeAnimation.preset.blendshapes.Count; i++)
				{
					SetBlendshapeValue(currentBlendshapeAnimation.preset.blendshapes[i].BlendType, currentBlendshapeAnimation.blendShapesTemp[i].BlendValue);
				}
			}
		}

		public void ResetAll(bool ignore_settingsDefaultElements = true)
		{
			ResetBodyColors();
			foreach (CharacterBlendshapeData characterBlendshapeData in characterBlendshapeDatas)
			{
				SetBlendshapeValue(characterBlendshapeData.type, 0f);
			}
			SetHeadSize(0f);
			SetHeight(0f);
			foreach (CharacterElementType item in Enum.GetValues(typeof(CharacterElementType)).Cast<CharacterElementType>().ToList())
			{
				SetElementByIndex(item, -1);
			}
			characterSelectedElements = (ignore_settingsDefaultElements ? new CharacterSelectedElements() : ((CharacterSelectedElements)_settings.DefaultSelectedElements.Clone()));
			if (ignore_settingsDefaultElements)
			{
				return;
			}
			foreach (CharacterElementType item2 in Enum.GetValues(typeof(CharacterElementType)).Cast<CharacterElementType>().ToList())
			{
				int selectedIndex = characterSelectedElements.GetSelectedIndex(item2);
				if (selectedIndex != -1)
				{
					SetElementByIndex(item2, selectedIndex);
				}
			}
		}

		public void Randomize()
		{
			CharacterGenerator.Generate(this);
		}

		public Animator GetAnimator()
		{
			return animator;
		}

		public void UnlockPrefab()
		{
			_ = Application.isPlaying;
		}

		public void LockPrefab(string custompath = "")
		{
			_ = Application.isPlaying;
		}

		public void ApplyPrefab()
		{
			if (!applyFeetOffset)
			{
				SetFeetOffset(Vector3.zero);
			}
			else
			{
				SetFeetOffset(new Vector3(0f, feetOffset, 0f));
			}
			ResetBodyMaterial();
		}

		public void RevertBonesChanges()
		{
			SkinnedMeshRenderer obj = Settings.OriginalMesh.GetComponentsInChildren<SkinnedMeshRenderer>()[0];
			Transform[] bones = obj.bones;
			_ = obj.rootBone;
			Transform[] bones2 = GetCharacterPart("Head").skinnedMesh[0].bones;
			originHip.localPosition = new Vector3(0f, feetOffset, 0f);
			for (int i = 0; i < bones2.Length; i++)
			{
				bones2[i].localPosition = bones[i].localPosition;
				bones2[i].localRotation = bones[i].localRotation;
			}
		}

		public void ApplyPrefabInPlaymode()
		{
		}

		public void UpdateActualCharacterInstanceStatus(bool igroneUserNonPrefab = false)
		{
			if (!Application.isPlaying && instanceStatus != CharacterInstanceStatus.NotAPrefabByUser)
			{
			}
		}

		public CharacterInstanceStatus GetCharacterInstanceStatus()
		{
			return instanceStatus;
		}

		public void SetNewCharacterInstanceStatus(CharacterInstanceStatus characterInstanceStatus)
		{
			instanceStatus = characterInstanceStatus;
		}

		public CharacterBlendshapeData GetBlendshapeData(CharacterBlendShapeType type)
		{
			foreach (CharacterBlendshapeData characterBlendshapeData in characterBlendshapeDatas)
			{
				if (characterBlendshapeData.type == type)
				{
					return characterBlendshapeData;
				}
			}
			return null;
		}

		public CharacterBlendshapeData GetBlendshapeData(string name)
		{
			foreach (CharacterBlendshapeData characterBlendshapeData in characterBlendshapeDatas)
			{
				if (characterBlendshapeData.blendshapeName == name)
				{
					return characterBlendshapeData;
				}
			}
			return null;
		}

		public List<CharacterBlendshapeData> GetBlendshapeDatasByGroup(CharacterBlendShapeGroup group)
		{
			List<CharacterBlendshapeData> list = new List<CharacterBlendshapeData>();
			foreach (CharacterBlendshapeData characterBlendshapeData in characterBlendshapeDatas)
			{
				if (characterBlendshapeData.group == group)
				{
					list.Add(characterBlendshapeData);
				}
			}
			return list;
		}

		private void DestroyObjects(UnityEngine.Object[] objects)
		{
			foreach (UnityEngine.Object @object in objects)
			{
				if (@object != null)
				{
					UnityEngine.Object.Destroy(@object);
				}
			}
		}
	}
}
