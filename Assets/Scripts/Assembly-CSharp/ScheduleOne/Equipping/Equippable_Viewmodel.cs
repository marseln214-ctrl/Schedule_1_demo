using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScheduleOne.Equipping
{
	public class Equippable_Viewmodel : Equippable_StorableItem
	{
		[Header("Viewmodel settings")]
		public Vector3 localPosition;

		public Vector3 localEulerAngles;

		public Vector3 localScale = Vector3.one;

		[Header("Third person animation settings")]
		public AvatarEquippable AvatarEquippable;

		public override void Equip(ItemInstance item)
		{
			base.Equip(item);
			base.transform.localPosition = localPosition;
			base.transform.localEulerAngles = localEulerAngles;
			base.transform.localScale = localScale;
			LayerUtility.SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Viewmodel"));
			MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
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
			PlayEquipAnimation();
		}

		public override void Unequip()
		{
			base.Unequip();
			PlayUnequipAnimation();
		}

		protected virtual void PlayEquipAnimation()
		{
			if (AvatarEquippable != null)
			{
				Player.Local.SendEquippable_Networked(AvatarEquippable.AssetPath);
			}
		}

		protected virtual void PlayUnequipAnimation()
		{
			if (AvatarEquippable != null)
			{
				Player.Local.SendEquippable_Networked(string.Empty);
			}
		}
	}
}
