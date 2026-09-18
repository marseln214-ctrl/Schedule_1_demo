using System;
using System.Collections.Generic;
using EasyButtons;
using ScheduleOne.AvatarFramework.Animation;
using ScheduleOne.AvatarFramework.Emotions;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework
{
	public class Avatar : MonoBehaviour
	{
		public const int MAX_ACCESSORIES = 9;

		public const bool USE_COMBINED_LAYERS = true;

		public const float DEFAULT_SMOOTHNESS = 0.25f;

		private static float maleShoulderScale = 0.93f;

		private static float femaleShoulderScale = 0.875f;

		[Header("References")]
		public AvatarAnimation Anim;

		public AvatarLookController LookController;

		public SkinnedMeshRenderer[] BodyMeshes;

		public SkinnedMeshRenderer[] ShapeKeyMeshes;

		public SkinnedMeshRenderer FaceMesh;

		public EyeController Eyes;

		public EyebrowController EyeBrows;

		public Transform BodyContainer;

		public Transform Armature;

		public Transform LeftShoulder;

		public Transform RightShoulder;

		public Transform HeadBone;

		public Transform HipBone;

		public Rigidbody[] RagdollRBs;

		public Collider[] RagdollColliders;

		public Rigidbody MiddleSpineRB;

		public AvatarEmotionManager EmotionManager;

		public AvatarEffects Effects;

		public Transform MiddleSpine;

		public Transform LowerSpine;

		public Transform LowestSpine;

		[Header("Settings")]
		public AvatarSettings InitialAvatarSettings;

		public Material DefaultAvatarMaterial;

		public UnityEvent<bool, bool, bool> onRagdollChange;

		[Header("Data - readonly")]
		[SerializeField]
		protected float appliedGender;

		[SerializeField]
		protected float appliedWeight;

		[SerializeField]
		protected Hair appliedHair;

		[SerializeField]
		protected Color appliedHairColor;

		[SerializeField]
		protected Accessory[] appliedAccessories = new Accessory[9];

		[SerializeField]
		protected bool wearingHairBlockingAccessory;

		private float additionalWeight;

		private float additionalGender;

		[Header("Runtime loading")]
		public AvatarSettings SettingsToLoad;

		public UnityEvent onSettingsLoaded;

		private Vector3 originalHipPos = Vector3.zero;

		private bool usingCombinedLayer;

		public bool Ragdolled { get; protected set; }

		public AvatarEquippable CurrentEquippable { get; protected set; }

		public AvatarSettings CurrentSettings { get; protected set; }

		public Vector3 CenterPoint => MiddleSpine.transform.position;

		[Button]
		public void Load()
		{
			LoadAvatarSettings(SettingsToLoad);
		}

		[Button]
		public void LoadNaked()
		{
			LoadNakedSettings(SettingsToLoad);
		}

		protected virtual void Awake()
		{
			if (InitialAvatarSettings != null)
			{
				LoadAvatarSettings(InitialAvatarSettings);
			}
			SetRagdollPhysicsEnabled(ragdollEnabled: false, playStandUpAnim: false);
			originalHipPos = HipBone.localPosition;
		}

		protected virtual void Update()
		{
			if (!Ragdolled && Anim != null && !Anim.StandUpAnimationPlaying)
			{
				HipBone.localPosition = originalHipPos;
			}
		}

		protected virtual void LateUpdate()
		{
			if (BodyContainer.gameObject.activeInHierarchy && CurrentSettings != null && PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				_ = CenterPoint;
				if (Vector3.SqrMagnitude(PlayerSingleton<PlayerCamera>.Instance.transform.position - CenterPoint) < 2500f * QualitySettings.lodBias)
				{
					ApplyShapeKeys(Mathf.Clamp01(appliedGender + additionalGender) * 100f, Mathf.Clamp01(appliedWeight + additionalWeight) * 100f);
				}
			}
		}

		public void SetVisible(bool vis)
		{
			Eyes.SetEyesOpen(open: true);
			BodyContainer.gameObject.SetActive(vis);
		}

		public void GetMugshot(Action<Texture2D> callback)
		{
			Singleton<MugshotGenerator>.Instance.GenerateMugshot(CurrentSettings, fileToFile: false, callback);
		}

		public bool IsMale()
		{
			if (CurrentSettings == null)
			{
				return true;
			}
			return CurrentSettings.Gender < 0.5f;
		}

		public bool IsWhite()
		{
			if (CurrentSettings == null)
			{
				return true;
			}
			return CurrentSettings.SkinColor.r + CurrentSettings.SkinColor.g + CurrentSettings.SkinColor.b > 1.5f;
		}

		private void ApplyShapeKeys(float gender, float weight, bool bodyOnly = false)
		{
			bool flag = true;
			if (Anim.animator != null)
			{
				flag = Anim.animator.enabled;
				Anim.animator.enabled = false;
			}
			for (int i = 0; i < ShapeKeyMeshes.Length; i++)
			{
				if (ShapeKeyMeshes[i].sharedMesh.blendShapeCount >= 2)
				{
					ShapeKeyMeshes[i].SetBlendShapeWeight(0, gender);
					ShapeKeyMeshes[i].SetBlendShapeWeight(1, weight);
				}
			}
			float num = Mathf.Lerp(maleShoulderScale, femaleShoulderScale, gender / 100f);
			LeftShoulder.localScale = new Vector3(num, num, num);
			RightShoulder.localScale = new Vector3(num, num, num);
			if (Anim.animator != null)
			{
				Anim.animator.enabled = flag;
			}
			if (bodyOnly)
			{
				return;
			}
			for (int j = 0; j < appliedAccessories.Length; j++)
			{
				if (appliedAccessories[j] != null)
				{
					appliedAccessories[j].ApplyShapeKeys(gender, weight);
				}
			}
		}

		private void SetFeetShrunk(bool shrink, float reduction)
		{
			if (shrink)
			{
				for (int i = 0; i < BodyMeshes.Length; i++)
				{
					BodyMeshes[i].SetBlendShapeWeight(2, reduction * 100f);
				}
			}
			else
			{
				for (int j = 0; j < BodyMeshes.Length; j++)
				{
					BodyMeshes[j].SetBlendShapeWeight(2, 0f);
				}
			}
		}

		private void SetWearingHairBlockingAccessory(bool blocked)
		{
			wearingHairBlockingAccessory = blocked;
			if (appliedHair != null)
			{
				appliedHair.SetBlockedByHat(blocked);
			}
		}

		public void LoadAvatarSettings(AvatarSettings settings)
		{
			if (settings == null)
			{
				Console.LogWarning("LoadAvatarSettings: given settings are null");
				return;
			}
			CurrentSettings = settings;
			ApplyBodySettings(CurrentSettings);
			ApplyHairSettings(CurrentSettings);
			ApplyHairColorSettings(CurrentSettings);
			ApplyEyeLidSettings(CurrentSettings);
			ApplyEyeLidColorSettings(CurrentSettings);
			ApplyEyebrowSettings(CurrentSettings);
			ApplyEyeBallSettings(CurrentSettings);
			ApplyFaceLayerSettings(CurrentSettings);
			ApplyBodyLayerSettings(CurrentSettings);
			ApplyAccessorySettings(CurrentSettings);
			FaceLayer faceLayer = Resources.Load(CurrentSettings.FaceLayer1Path) as FaceLayer;
			Texture2D faceTex = ((faceLayer != null) ? faceLayer.Texture : null);
			EmotionManager.ConfigureNeutralFace(faceTex, CurrentSettings.EyebrowRestingHeight, CurrentSettings.EyebrowRestingAngle, CurrentSettings.LeftEyeRestingState, CurrentSettings.RightEyeRestingState);
			if (onSettingsLoaded != null)
			{
				onSettingsLoaded.Invoke();
			}
		}

		public void LoadNakedSettings(AvatarSettings settings, int maxLayerOrder = 19)
		{
			if (settings == null)
			{
				Console.LogWarning("LoadAvatarSettings: given settings are null");
				return;
			}
			AvatarSettings currentSettings = CurrentSettings;
			CurrentSettings = settings;
			if (CurrentSettings == null)
			{
				CurrentSettings = new AvatarSettings();
			}
			if (currentSettings != null)
			{
				CurrentSettings.BodyLayerSettings.AddRange(currentSettings.BodyLayerSettings);
			}
			ApplyBodySettings(CurrentSettings);
			ApplyHairSettings(CurrentSettings);
			ApplyHairColorSettings(CurrentSettings);
			ApplyEyeLidSettings(CurrentSettings);
			ApplyEyeLidColorSettings(CurrentSettings);
			ApplyEyebrowSettings(CurrentSettings);
			ApplyEyeBallSettings(CurrentSettings);
			ApplyFaceLayerSettings(CurrentSettings);
			ApplyBodyLayerSettings(CurrentSettings, maxLayerOrder);
			FaceLayer faceLayer = Resources.Load(CurrentSettings.FaceLayer1Path) as FaceLayer;
			Texture2D faceTex = ((faceLayer != null) ? faceLayer.Texture : null);
			EmotionManager.ConfigureNeutralFace(faceTex, CurrentSettings.EyebrowRestingHeight, CurrentSettings.EyebrowRestingAngle, CurrentSettings.LeftEyeRestingState, CurrentSettings.RightEyeRestingState);
			if (onSettingsLoaded != null)
			{
				onSettingsLoaded.Invoke();
			}
		}

		public void ApplyBodySettings(AvatarSettings settings)
		{
			appliedGender = settings.Gender;
			appliedWeight = settings.Weight;
			CurrentSettings.SkinColor = settings.SkinColor;
			ApplyShapeKeys(settings.Gender * 100f, settings.Weight * 100f);
			base.transform.localScale = new Vector3(settings.Height, settings.Height, settings.Height);
			if (onSettingsLoaded != null)
			{
				onSettingsLoaded.Invoke();
			}
		}

		public void SetAdditionalWeight(float weight)
		{
			additionalWeight = weight;
		}

		public void SetAdditionalGender(float gender)
		{
			additionalGender = gender;
		}

		public void SetSkinColor(Color color)
		{
			if (!usingCombinedLayer)
			{
				if (BodyMeshes[0].material.GetColor("_SkinColor") == color)
				{
					return;
				}
				SkinnedMeshRenderer[] bodyMeshes = BodyMeshes;
				for (int i = 0; i < bodyMeshes.Length; i++)
				{
					bodyMeshes[i].material.SetColor("_SkinColor", color);
				}
			}
			Eyes.leftEye.SetLidColor(color);
			Eyes.rightEye.SetLidColor(color);
		}

		public void ApplyHairSettings(AvatarSettings settings)
		{
			if (appliedHair != null)
			{
				UnityEngine.Object.Destroy(appliedHair.gameObject);
			}
			UnityEngine.Object @object = ((settings.HairPath != null) ? Resources.Load(settings.HairPath) : null);
			if (@object != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(@object, HeadBone) as GameObject;
				appliedHair = gameObject.GetComponent<Hair>();
			}
			ApplyHairColorSettings(settings);
			if (appliedHair != null)
			{
				appliedHair.SetBlockedByHat(wearingHairBlockingAccessory);
			}
		}

		public void SetHairVisible(bool visible)
		{
			if (appliedHair != null)
			{
				appliedHair.gameObject.SetActive(visible);
			}
		}

		public void ApplyHairColorSettings(AvatarSettings settings)
		{
			appliedHairColor = settings.HairColor;
			if (appliedHair != null)
			{
				appliedHair.ApplyColor(appliedHairColor);
			}
			EyeBrows.ApplySettings(settings);
			SetFaceLayer(2, settings.FaceLayer2Path, settings.HairColor);
		}

		public void OverrideHairColor(Color color)
		{
			if (appliedHair != null)
			{
				appliedHair.ApplyColor(color);
			}
			EyeBrows.leftBrow.SetColor(color);
			EyeBrows.rightBrow.SetColor(color);
			if (CurrentSettings != null)
			{
				SetFaceLayer(2, CurrentSettings.FaceLayer2Path, color);
			}
		}

		public void ResetHairColor()
		{
			if (!(CurrentSettings == null))
			{
				if (appliedHair != null)
				{
					appliedHair.ApplyColor(CurrentSettings.HairColor);
				}
				EyeBrows.leftBrow.SetColor(CurrentSettings.HairColor);
				EyeBrows.rightBrow.SetColor(CurrentSettings.HairColor);
				SetFaceLayer(2, CurrentSettings.FaceLayer2Path, CurrentSettings.HairColor);
			}
		}

		public void ApplyEyeBallSettings(AvatarSettings settings)
		{
			Eyes.SetEyeballTint(settings.EyeBallTint);
			Eyes.SetPupilDilation(settings.PupilDilation);
		}

		public void ApplyEyeLidSettings(AvatarSettings settings)
		{
			Eyes.SetLeftEyeRestingLidState(settings.LeftEyeRestingState);
			Eyes.SetRightEyeRestingLidState(settings.RightEyeRestingState);
		}

		public void ApplyEyeLidColorSettings(AvatarSettings settings)
		{
			Eyes.leftEye.SetLidColor(settings.LeftEyeLidColor);
			Eyes.rightEye.SetLidColor(settings.RightEyeLidColor);
		}

		public void ApplyEyebrowSettings(AvatarSettings settings)
		{
			EyeBrows.ApplySettings(settings);
		}

		public void ApplyFaceLayerSettings(AvatarSettings settings)
		{
			for (int i = 1; i <= 6; i++)
			{
				SetFaceLayer(i, string.Empty, Color.white);
			}
			SetFaceLayer(1, settings.FaceLayer1Path, settings.FaceLayer1Color);
			SetFaceLayer(6, settings.FaceLayer2Path, settings.HairColor);
			List<Tuple<FaceLayer, Color>> list = new List<Tuple<FaceLayer, Color>>();
			for (int j = 2; j < settings.FaceLayerSettings.Count; j++)
			{
				if (!string.IsNullOrEmpty(settings.FaceLayerSettings[j].layerPath))
				{
					FaceLayer faceLayer = Resources.Load(settings.FaceLayerSettings[j].layerPath) as FaceLayer;
					if (faceLayer != null)
					{
						list.Add(new Tuple<FaceLayer, Color>(faceLayer, settings.FaceLayerSettings[j].layerTint));
					}
					else
					{
						Console.LogWarning("Face layer not found at path " + settings.FaceLayerSettings[j].layerPath);
					}
				}
			}
			list.Sort((Tuple<FaceLayer, Color> x, Tuple<FaceLayer, Color> y) => x.Item1.Order.CompareTo(y.Item1.Order));
			for (int k = 0; k < list.Count; k++)
			{
				SetFaceLayer(3 + k, list[k].Item1.AssetPath, list[k].Item2);
			}
		}

		private void SetFaceLayer(int index, string assetPath, Color color)
		{
			FaceLayer faceLayer = Resources.Load(assetPath) as FaceLayer;
			Texture2D texture2D = ((faceLayer != null) ? faceLayer.Texture : null);
			if (texture2D == null)
			{
				color.a = 0f;
			}
			FaceMesh.material.SetTexture("_Layer_" + index + "_Texture", texture2D);
			FaceMesh.material.SetColor("_Layer_" + index + "_Color", color);
		}

		public void SetFaceTexture(Texture2D tex, Color color)
		{
			FaceMesh.material.SetTexture("_Layer_" + 1 + "_Texture", tex);
			FaceMesh.material.SetColor("_Layer_" + 1 + "_Color", color);
		}

		public void ApplyBodyLayerSettings(AvatarSettings settings, int maxOrder = -1)
		{
			for (int i = 1; i <= 6; i++)
			{
				SetBodyLayer(i, string.Empty, Color.white);
			}
			AvatarLayer avatarLayer = null;
			if (settings.UseCombinedLayer && settings.CombinedLayerPath != string.Empty)
			{
				avatarLayer = Resources.Load(settings.CombinedLayerPath) as AvatarLayer;
			}
			if (avatarLayer != null)
			{
				usingCombinedLayer = true;
				SkinnedMeshRenderer[] bodyMeshes = BodyMeshes;
				for (int j = 0; j < bodyMeshes.Length; j++)
				{
					bodyMeshes[j].material = avatarLayer.CombinedMaterial;
				}
				return;
			}
			usingCombinedLayer = false;
			List<Tuple<AvatarLayer, Color>> list = new List<Tuple<AvatarLayer, Color>>();
			for (int k = 0; k < settings.BodyLayerSettings.Count; k++)
			{
				if (string.IsNullOrEmpty(settings.BodyLayerSettings[k].layerPath))
				{
					continue;
				}
				AvatarLayer avatarLayer2 = Resources.Load(settings.BodyLayerSettings[k].layerPath) as AvatarLayer;
				if (maxOrder <= -1 || avatarLayer2.Order <= maxOrder)
				{
					if (avatarLayer2 != null)
					{
						list.Add(new Tuple<AvatarLayer, Color>(avatarLayer2, settings.BodyLayerSettings[k].layerTint));
					}
					else
					{
						Console.LogWarning("Body layer not found at path " + settings.BodyLayerSettings[k].layerPath);
					}
				}
			}
			list.Sort((Tuple<AvatarLayer, Color> x, Tuple<AvatarLayer, Color> y) => x.Item1.Order.CompareTo(y.Item1.Order));
			for (int l = 0; l < list.Count; l++)
			{
				SetBodyLayer(l + 1, list[l].Item1.AssetPath, list[l].Item2);
			}
		}

		private void SetBodyLayer(int index, string assetPath, Color color)
		{
			AvatarLayer avatarLayer = Resources.Load(assetPath) as AvatarLayer;
			Texture2D texture2D = ((avatarLayer != null) ? avatarLayer.Texture : null);
			if (texture2D == null)
			{
				color.a = 0f;
			}
			SkinnedMeshRenderer[] bodyMeshes = BodyMeshes;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in bodyMeshes)
			{
				if (skinnedMeshRenderer.material.shader != DefaultAvatarMaterial.shader)
				{
					skinnedMeshRenderer.material = new Material(DefaultAvatarMaterial);
				}
				skinnedMeshRenderer.material.SetTexture("_Layer_" + index + "_Texture", texture2D);
				skinnedMeshRenderer.material.SetColor("_Layer_" + index + "_Color", color);
				if (avatarLayer != null)
				{
					skinnedMeshRenderer.material.SetTexture("_Layer_" + index + "_Normal", avatarLayer.Normal);
				}
			}
		}

		public void ApplyAccessorySettings(AvatarSettings settings)
		{
			if (appliedAccessories.Length != 9)
			{
				DestroyAccessories();
				appliedAccessories = new Accessory[9];
			}
			bool shrink = false;
			float num = 0f;
			bool flag = false;
			for (int i = 0; i < 9; i++)
			{
				if (settings.AccessorySettings.Count > i && settings.AccessorySettings[i].path != string.Empty)
				{
					if (appliedAccessories[i] != null && appliedAccessories[i].AssetPath != settings.AccessorySettings[i].path)
					{
						UnityEngine.Object.Destroy(appliedAccessories[i].gameObject);
						appliedAccessories[i] = null;
					}
					if (appliedAccessories[i] == null)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load(settings.AccessorySettings[i].path), BodyContainer) as GameObject;
						appliedAccessories[i] = gameObject.GetComponent<Accessory>();
						appliedAccessories[i].BindBones(BodyMeshes[0].bones);
						appliedAccessories[i].ApplyShapeKeys(appliedGender * 100f, appliedWeight * 100f);
					}
					if (appliedAccessories[i].ReduceFootSize)
					{
						shrink = true;
						num = Mathf.Max(num, appliedAccessories[i].FootSizeReduction);
					}
					if (appliedAccessories[i].ShouldBlockHair)
					{
						flag = true;
					}
				}
				else if (appliedAccessories[i] != null)
				{
					UnityEngine.Object.Destroy(appliedAccessories[i].gameObject);
					appliedAccessories[i] = null;
				}
			}
			SetFeetShrunk(shrink, num);
			SetWearingHairBlockingAccessory(flag);
			for (int j = 0; j < appliedAccessories.Length; j++)
			{
				if (appliedAccessories[j] != null)
				{
					appliedAccessories[j].ApplyColor(settings.AccessorySettings[j].color);
				}
			}
		}

		private void DestroyAccessories()
		{
			for (int i = 0; i < appliedAccessories.Length; i++)
			{
				if (appliedAccessories[i] != null)
				{
					UnityEngine.Object.Destroy(appliedAccessories[i].gameObject);
				}
			}
		}

		public virtual void SetRagdollPhysicsEnabled(bool ragdollEnabled, bool playStandUpAnim = true)
		{
			bool ragdolled = Ragdolled;
			Ragdolled = ragdollEnabled;
			if (onRagdollChange != null)
			{
				onRagdollChange.Invoke(ragdolled, ragdollEnabled, playStandUpAnim);
			}
			Rigidbody[] ragdollRBs = RagdollRBs;
			foreach (Rigidbody rigidbody in ragdollRBs)
			{
				if (!(rigidbody == null))
				{
					rigidbody.isKinematic = !ragdollEnabled;
					if (!rigidbody.isKinematic)
					{
						rigidbody.velocity = Vector3.zero;
						rigidbody.angularVelocity = Vector3.zero;
					}
				}
			}
			Collider[] ragdollColliders = RagdollColliders;
			foreach (Collider collider in ragdollColliders)
			{
				if (!(collider == null))
				{
					collider.enabled = ragdollEnabled;
				}
			}
		}

		public virtual AvatarEquippable SetEquippable(string assetPath)
		{
			if (CurrentEquippable != null)
			{
				CurrentEquippable.Unequip();
			}
			if (assetPath != string.Empty)
			{
				GameObject gameObject = Resources.Load(assetPath) as GameObject;
				if (gameObject == null)
				{
					Console.LogError("Couldn't find equippable at path " + assetPath);
					return null;
				}
				CurrentEquippable = UnityEngine.Object.Instantiate(gameObject, null).GetComponent<AvatarEquippable>();
				CurrentEquippable.Equip(this);
				return CurrentEquippable;
			}
			return null;
		}

		public virtual void ReceiveEquippableMessage(string message, object data)
		{
			if (CurrentEquippable != null)
			{
				CurrentEquippable.ReceiveMessage(message, data);
			}
			else
			{
				Console.LogWarning("Received equippable message but no equippable is equipped!");
			}
		}
	}
}
