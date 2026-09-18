using System;
using ScheduleOne.AvatarFramework;
using ScheduleOne.Clothing;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Items;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScheduleOne.UI
{
	public class CharacterDisplay : Singleton<CharacterDisplay>
	{
		[Serializable]
		public class SlotAlignmentPoint
		{
			public EClothingSlot SlotType;

			public Transform Point;
		}

		public SlotAlignmentPoint[] AlignmentPoints;

		[Header("References")]
		public Transform Container;

		public ScheduleOne.AvatarFramework.Avatar ParentAvatar;

		public ScheduleOne.AvatarFramework.Avatar Avatar;

		public Transform AvatarContainer;

		private float targetRotation;

		public bool IsOpen { get; private set; }

		protected override void Awake()
		{
			base.Awake();
			SetOpen(open: false);
			if (ParentAvatar.CurrentSettings != null)
			{
				SetAppearance(ParentAvatar.CurrentSettings);
			}
			ParentAvatar.onSettingsLoaded.AddListener(delegate
			{
				SetAppearance(ParentAvatar.CurrentSettings);
			});
		}

		public void SetOpen(bool open)
		{
			IsOpen = open;
			Container.gameObject.SetActive(open);
			if (IsOpen)
			{
				LayerUtility.SetLayerRecursively(Container.gameObject, LayerMask.NameToLayer("Overlay"));
				Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), Player.Local.Clothing.ItemSlots);
			}
		}

		private void Update()
		{
			if (IsOpen)
			{
				targetRotation = Mathf.Lerp(targetRotation, Mathf.Lerp(0f, 359f, Singleton<GameplayMenuInterface>.Instance.CharacterInterface.RotationSlider.value), Time.deltaTime * 5f);
				AvatarContainer.localEulerAngles = new Vector3(0f, targetRotation, 0f);
			}
		}

		public void SetAppearance(AvatarSettings settings)
		{
			AvatarSettings settings2 = UnityEngine.Object.Instantiate(settings);
			Avatar.LoadAvatarSettings(settings2);
			LayerUtility.SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Overlay"));
			MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				if (meshRenderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
				{
					meshRenderer.enabled = false;
				}
				else
				{
					meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
				}
			}
			SkinnedMeshRenderer[] componentsInChildren2 = GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].shadowCastingMode = ShadowCastingMode.Off;
			}
		}
	}
}
