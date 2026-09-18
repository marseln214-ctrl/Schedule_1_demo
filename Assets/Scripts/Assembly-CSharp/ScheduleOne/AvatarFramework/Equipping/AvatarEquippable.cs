using EasyButtons;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping
{
	public class AvatarEquippable : MonoBehaviour
	{
		public enum ETriggerType
		{
			Trigger = 0,
			Bool = 1
		}

		public enum EHand
		{
			Left = 0,
			Right = 1
		}

		[Header("Settings")]
		public Transform AlignmentPoint;

		[Range(0f, 1f)]
		public float Suspiciousness;

		public EHand Hand = EHand.Right;

		public ETriggerType TriggerType;

		public string AnimationTrigger = "RightArm_Hold_ClosedHand";

		public string AssetPath = string.Empty;

		protected Avatar avatar;

		[Button]
		public void RecalculateAssetPath()
		{
			AssetPath = AssetPathUtility.GetResourcesPath(base.gameObject);
			string[] array = AssetPath.Split('/');
			array[^1] = base.gameObject.name;
			AssetPath = string.Join("/", array);
		}

		protected virtual void Awake()
		{
			if (AssetPath == string.Empty)
			{
				Console.LogWarning(base.gameObject.name + " does not have an assetpath!");
			}
		}

		public virtual void Equip(Avatar _avatar)
		{
			avatar = _avatar;
			if (Hand == EHand.Right)
			{
				base.transform.SetParent(avatar.Anim.RightHandContainer);
			}
			else
			{
				base.transform.SetParent(avatar.Anim.LeftHandContainer);
			}
			PositionAnimationModel();
			if (TriggerType == ETriggerType.Trigger)
			{
				SetTrigger(AnimationTrigger);
			}
			else
			{
				SetBool(AnimationTrigger, val: true);
			}
			Player componentInParent = avatar.GetComponentInParent<Player>();
			if (componentInParent != null && componentInParent.IsOwner && !componentInParent.avatarVisibleToLocalPlayer)
			{
				LayerUtility.SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Invisible"));
			}
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].isTrigger = true;
			}
		}

		public virtual void Unequip()
		{
			if (TriggerType == ETriggerType.Trigger)
			{
				SetTrigger("EndAction");
			}
			else
			{
				SetBool(AnimationTrigger, val: false);
			}
			Object.Destroy(base.gameObject);
		}

		private void PositionAnimationModel()
		{
			Transform transform = ((Hand == EHand.Right) ? avatar.Anim.RightHandAlignmentPoint : avatar.Anim.LeftHandAlignmentPoint);
			base.transform.rotation = transform.rotation * (Quaternion.Inverse(AlignmentPoint.rotation) * base.transform.rotation);
			base.transform.position = transform.position + (base.transform.position - AlignmentPoint.position);
		}

		protected void SetTrigger(string anim)
		{
			if (avatar.GetComponentInParent<Player>() != null)
			{
				avatar.GetComponentInParent<Player>().SetAnimationTrigger(anim);
			}
			else if (avatar.GetComponentInParent<NPC>() != null)
			{
				avatar.GetComponentInParent<NPC>().SetAnimationTrigger(anim);
			}
		}

		protected void SetBool(string anim, bool val)
		{
			if (avatar.GetComponentInParent<Player>() != null)
			{
				avatar.GetComponentInParent<Player>().SetAnimationBool(anim, val);
			}
			else if (avatar.GetComponentInParent<NPC>() != null)
			{
				avatar.GetComponentInParent<NPC>().SetAnimationBool(anim, val);
			}
		}

		protected void ResetTrigger(string anim)
		{
			if (avatar.GetComponentInParent<Player>() != null)
			{
				avatar.GetComponentInParent<Player>().ResetAnimationTrigger(anim);
			}
			else if (avatar.GetComponentInParent<NPC>() != null)
			{
				avatar.GetComponentInParent<NPC>().ResetAnimationTrigger(anim);
			}
		}

		public virtual void ReceiveMessage(string message, object parameter)
		{
		}
	}
}
