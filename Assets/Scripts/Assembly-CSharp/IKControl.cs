using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKControl : MonoBehaviour
{
	protected Animator animator;

	public bool ikActive;

	public Transform rightHandObj;

	public Transform lookObj;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	private void OnAnimatorIK()
	{
		if (!animator)
		{
			return;
		}
		if (ikActive)
		{
			if (lookObj != null)
			{
				animator.SetLookAtWeight(1f);
				animator.SetLookAtPosition(lookObj.position);
			}
			if (rightHandObj != null)
			{
				animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
				animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
				animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
				animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
			}
		}
		else
		{
			animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
			animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
			animator.SetLookAtWeight(0f);
		}
	}
}
