using System.Collections.Generic;
using AdvancedPeopleSystem;
using UnityEngine;
using UnityEngine.UI;

public class UIControllerDEMO : MonoBehaviour
{
	[Space(5f)]
	[Header("I do not recommend using it in your projects")]
	[Header("This script was created to demonstrate api")]
	public CharacterCustomization CharacterCustomization;

	[Space(15f)]
	public Text playbutton_text;

	public Text bake_text;

	public Text lod_text;

	public Text panelNameText;

	public Slider fatSlider;

	public Slider musclesSlider;

	public Slider thinSlider;

	public Slider slimnessSlider;

	public Slider breastSlider;

	public Slider heightSlider;

	public Slider legSlider;

	public Slider headSizeSlider;

	public Slider headOffsetSlider;

	public Slider[] faceShapeSliders;

	public RectTransform HairPanel;

	public RectTransform BeardPanel;

	public RectTransform ShirtPanel;

	public RectTransform PantsPanel;

	public RectTransform ShoesPanel;

	public RectTransform HatPanel;

	public RectTransform AccessoryPanel;

	public RectTransform BackpackPanel;

	public RectTransform FaceEditPanel;

	public RectTransform BaseEditPanel;

	public RectTransform SkinColorPanel;

	public RectTransform EyeColorPanel;

	public RectTransform HairColorPanel;

	public RectTransform UnderpantsColorPanel;

	public RectTransform EmotionsPanel;

	public RectTransform SavesPanel;

	public RectTransform SavesPanelList;

	public RectTransform SavesPrefab;

	public List<RectTransform> SavesList = new List<RectTransform>();

	public Image SkinColorButtonColor;

	public Image EyeColorButtonColor;

	public Image HairColorButtonColor;

	public Image UnderpantsColorButtonColor;

	public Vector3[] CameraPositionForPanels;

	public Vector3[] CameraEulerForPanels;

	private int currentPanelIndex;

	public Camera Camera;

	public RectTransform femaleUI;

	public RectTransform maleUI;

	private int lodIndex;

	private bool walk_active;

	private bool canvasVisible = true;

	public void SwitchCharacterSettings(string name)
	{
		CharacterCustomization.SwitchCharacterSettings(name);
		if (name == "Male")
		{
			maleUI.gameObject.SetActive(value: true);
			femaleUI.gameObject.SetActive(value: false);
		}
		if (name == "Female")
		{
			femaleUI.gameObject.SetActive(value: true);
			maleUI.gameObject.SetActive(value: false);
		}
	}

	public void ShowFaceEdit()
	{
		FaceEditPanel.gameObject.SetActive(value: true);
		BaseEditPanel.gameObject.SetActive(value: false);
		currentPanelIndex = 1;
		panelNameText.text = "FACE CUSTOMIZER";
	}

	public void ShowBaseEdit()
	{
		FaceEditPanel.gameObject.SetActive(value: false);
		BaseEditPanel.gameObject.SetActive(value: true);
		currentPanelIndex = 0;
		panelNameText.text = "BASE CUSTOMIZER";
	}

	public void SetFaceShape(int index)
	{
		List<CharacterBlendshapeData> blendshapeDatasByGroup = CharacterCustomization.GetBlendshapeDatasByGroup(CharacterBlendShapeGroup.Face);
		CharacterCustomization.SetBlendshapeValue(blendshapeDatasByGroup[index].type, faceShapeSliders[index].value);
	}

	public void SetHeadOffset()
	{
		CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Head_Offset, headOffsetSlider.value);
	}

	public void BodyFat()
	{
		CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, fatSlider.value);
	}

	public void BodyMuscles()
	{
		CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Muscles, musclesSlider.value);
	}

	public void BodyThin()
	{
		CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Thin, thinSlider.value);
	}

	public void BodySlimness()
	{
		CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Slimness, slimnessSlider.value);
	}

	public void BodyBreast()
	{
		CharacterCustomization.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, breastSlider.value, new string[3] { "Chest", "Stomach", "Head" }, new CharacterElementType[1] { CharacterElementType.Shirt });
	}

	public void SetHeight()
	{
		CharacterCustomization.SetHeight(heightSlider.value);
	}

	public void SetHeadSize()
	{
		CharacterCustomization.SetHeadSize(headSizeSlider.value);
	}

	public void Lod_Event(int next)
	{
		lodIndex += next;
		if (lodIndex < 0)
		{
			lodIndex = 3;
		}
		if (lodIndex > 3)
		{
			lodIndex = 0;
		}
		lod_text.text = lodIndex.ToString();
		CharacterCustomization.ForceLOD(lodIndex);
	}

	public void SetNewSkinColor(Color color)
	{
		SkinColorButtonColor.color = color;
		CharacterCustomization.SetBodyColor(BodyColorPart.Skin, color);
	}

	public void SetNewEyeColor(Color color)
	{
		EyeColorButtonColor.color = color;
		CharacterCustomization.SetBodyColor(BodyColorPart.Eye, color);
	}

	public void SetNewHairColor(Color color)
	{
		HairColorButtonColor.color = color;
		CharacterCustomization.SetBodyColor(BodyColorPart.Hair, color);
	}

	public void SetNewUnderpantsColor(Color color)
	{
		UnderpantsColorButtonColor.color = color;
		CharacterCustomization.SetBodyColor(BodyColorPart.Underpants, color);
	}

	public void VisibleSkinColorPanel(bool v)
	{
		HideAllPanels();
		SkinColorPanel.gameObject.SetActive(v);
	}

	public void VisibleEyeColorPanel(bool v)
	{
		HideAllPanels();
		EyeColorPanel.gameObject.SetActive(v);
	}

	public void VisibleHairColorPanel(bool v)
	{
		HideAllPanels();
		HairColorPanel.gameObject.SetActive(v);
	}

	public void VisibleUnderpantsColorPanel(bool v)
	{
		HideAllPanels();
		UnderpantsColorPanel.gameObject.SetActive(v);
	}

	public void ShirtPanel_Select(bool v)
	{
		HideAllPanels();
		if (!v)
		{
			ShirtPanel.gameObject.SetActive(value: false);
		}
		else
		{
			ShirtPanel.gameObject.SetActive(value: true);
		}
	}

	public void PantsPanel_Select(bool v)
	{
		HideAllPanels();
		if (!v)
		{
			PantsPanel.gameObject.SetActive(value: false);
		}
		else
		{
			PantsPanel.gameObject.SetActive(value: true);
		}
	}

	public void ShoesPanel_Select(bool v)
	{
		HideAllPanels();
		if (!v)
		{
			ShoesPanel.gameObject.SetActive(value: false);
		}
		else
		{
			ShoesPanel.gameObject.SetActive(value: true);
		}
	}

	public void BackpackPanel_Select(bool v)
	{
		HideAllPanels();
		if (!v)
		{
			BackpackPanel.gameObject.SetActive(value: false);
		}
		else
		{
			BackpackPanel.gameObject.SetActive(value: true);
		}
	}

	public void HairPanel_Select(bool v)
	{
		HideAllPanels();
		if (!v)
		{
			HairPanel.gameObject.SetActive(value: false);
		}
		else
		{
			HairPanel.gameObject.SetActive(value: true);
		}
		currentPanelIndex = (v ? 1 : 0);
	}

	public void BeardPanel_Select(bool v)
	{
		HideAllPanels();
		if (!v)
		{
			BeardPanel.gameObject.SetActive(value: false);
		}
		else
		{
			BeardPanel.gameObject.SetActive(value: true);
		}
		currentPanelIndex = (v ? 1 : 0);
	}

	public void HatPanel_Select(bool v)
	{
		HideAllPanels();
		if (!v)
		{
			HatPanel.gameObject.SetActive(value: false);
		}
		else
		{
			HatPanel.gameObject.SetActive(value: true);
		}
		currentPanelIndex = (v ? 1 : 0);
	}

	public void EmotionsPanel_Select(bool v)
	{
		HideAllPanels();
		if (!v)
		{
			EmotionsPanel.gameObject.SetActive(value: false);
		}
		else
		{
			EmotionsPanel.gameObject.SetActive(value: true);
		}
		currentPanelIndex = (v ? 1 : 0);
	}

	public void AccessoryPanel_Select(bool v)
	{
		HideAllPanels();
		if (!v)
		{
			AccessoryPanel.gameObject.SetActive(value: false);
		}
		else
		{
			AccessoryPanel.gameObject.SetActive(value: true);
		}
		currentPanelIndex = (v ? 1 : 0);
	}

	public void SavesPanel_Select(bool v)
	{
		HideAllPanels();
		if (!v)
		{
			SavesPanel.gameObject.SetActive(value: false);
			foreach (RectTransform saves in SavesList)
			{
				Object.Destroy(saves.gameObject);
			}
			SavesList.Clear();
			return;
		}
		List<SavedCharacterData> savedCharacterDatas = CharacterCustomization.GetSavedCharacterDatas();
		for (int i = 0; i < savedCharacterDatas.Count; i++)
		{
			RectTransform rectTransform = Object.Instantiate(SavesPrefab, SavesPanelList);
			int index = i;
			rectTransform.GetComponent<Button>().onClick.AddListener(delegate
			{
				SaveSelect(index);
			});
			rectTransform.GetComponentInChildren<Text>().text = $"({index}) {savedCharacterDatas[i].name}";
			SavesList.Add(rectTransform);
		}
		SavesPanel.gameObject.SetActive(value: true);
	}

	public void SaveSelect(int index)
	{
		List<SavedCharacterData> savedCharacterDatas = CharacterCustomization.GetSavedCharacterDatas();
		CharacterCustomization.ApplySavedCharacterData(savedCharacterDatas[index]);
	}

	public void EmotionsChange_Event(int index)
	{
		CharacterAnimationPreset characterAnimationPreset = CharacterCustomization.Settings.characterAnimationPresets[index];
		if (characterAnimationPreset != null)
		{
			CharacterCustomization.PlayBlendshapeAnimation(characterAnimationPreset.name, 2f);
		}
	}

	public void HairChange_Event(int index)
	{
		CharacterCustomization.SetElementByIndex(CharacterElementType.Hair, index);
	}

	public void BeardChange_Event(int index)
	{
		CharacterCustomization.SetElementByIndex(CharacterElementType.Beard, index);
	}

	public void ShirtChange_Event(int index)
	{
		CharacterCustomization.SetElementByIndex(CharacterElementType.Shirt, index);
	}

	public void PantsChange_Event(int index)
	{
		CharacterCustomization.SetElementByIndex(CharacterElementType.Pants, index);
	}

	public void ShoesChange_Event(int index)
	{
		CharacterCustomization.SetElementByIndex(CharacterElementType.Shoes, index);
	}

	public void BackpackChange_Event(int index)
	{
		CharacterCustomization.SetElementByIndex(CharacterElementType.Item1, index);
	}

	public void HatChange_Event(int index)
	{
		CharacterCustomization.SetElementByIndex(CharacterElementType.Hat, index);
	}

	public void AccessoryChange_Event(int index)
	{
		CharacterCustomization.SetElementByIndex(CharacterElementType.Accessory, index);
	}

	public void HideAllPanels()
	{
		SkinColorPanel.gameObject.SetActive(value: false);
		EyeColorPanel.gameObject.SetActive(value: false);
		HairColorPanel.gameObject.SetActive(value: false);
		UnderpantsColorPanel.gameObject.SetActive(value: false);
		if (EmotionsPanel != null)
		{
			EmotionsPanel.gameObject.SetActive(value: false);
		}
		if (BeardPanel != null)
		{
			BeardPanel.gameObject.SetActive(value: false);
		}
		HairPanel.gameObject.SetActive(value: false);
		ShirtPanel.gameObject.SetActive(value: false);
		PantsPanel.gameObject.SetActive(value: false);
		ShoesPanel.gameObject.SetActive(value: false);
		BackpackPanel.gameObject.SetActive(value: false);
		HatPanel.gameObject.SetActive(value: false);
		AccessoryPanel.gameObject.SetActive(value: false);
		SavesPanel.gameObject.SetActive(value: false);
		currentPanelIndex = 0;
	}

	public void SaveToFile()
	{
		CharacterCustomization.SaveCharacterToFile(CharacterCustomizationSetup.CharacterFileSaveFormat.Json);
	}

	public void ClearFromFile()
	{
		SavesPanel.gameObject.SetActive(value: false);
		CharacterCustomization.ClearSavedData();
	}

	public void Randimize()
	{
		CharacterCustomization.Randomize();
	}

	public void PlayAnim()
	{
		walk_active = !walk_active;
		CharacterCustomization.GetAnimator().SetBool("walk", walk_active);
		playbutton_text.text = (walk_active ? "STOP" : "PLAY");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.H))
		{
			canvasVisible = !canvasVisible;
			GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>().enabled = canvasVisible;
		}
		Camera.transform.position = Vector3.Lerp(Camera.transform.position, CameraPositionForPanels[currentPanelIndex], Time.deltaTime * 5f);
		Camera.transform.eulerAngles = Vector3.Lerp(Camera.transform.eulerAngles, CameraEulerForPanels[currentPanelIndex], Time.deltaTime * 5f);
	}
}
