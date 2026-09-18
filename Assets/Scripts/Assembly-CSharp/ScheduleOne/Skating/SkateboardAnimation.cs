using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Skating
{
	public class SkateboardAnimation : MonoBehaviour
	{
		[Serializable]
		public class AlignmentSet
		{
			public Transform Transform;

			public Transform Default;

			public Transform Animated;
		}

		[Header("Settings")]
		public float JumpCrouchAmount = 0.4f;

		public float CrouchSpeed = 4f;

		public float ArmLiftRate = 5f;

		public float PelvisMaxRotation = 10f;

		public float HandsMaxRotation = 10f;

		public float PelvisOffsetBlend;

		public float VerticalMomentumMultiplier = 0.5f;

		public float VerticalMomentumOffsetClamp = 0.3f;

		public float MomentumMoveSpeed = 5f;

		public float IKBlendChangeRate = 3f;

		public float PushAnimationDuration = 1.1f;

		public float PushAnimationSpeed = 1.3f;

		[Header("References")]
		public AlignmentSet PelvisContainerAlignment;

		public AlignmentSet PelvisAlignment;

		public AlignmentSet SpineContainerAlignment;

		public AlignmentSet SpineAlignment;

		public Transform SpineAlignment_Hunched;

		public AlignmentSet LeftFootAlignment;

		public AlignmentSet RightFootAlignment;

		public AlignmentSet LeftLegBendTarget;

		public AlignmentSet RightLegBendTarget;

		public AlignmentSet LeftHandAlignment;

		public AlignmentSet RightHandAlignment;

		public Transform AvatarFaceTarget;

		public Transform HandContainer;

		public Animation IKAnimation;

		[Header("Arm Lift")]
		public AlignmentSet LeftHandLoweredAlignment;

		public AlignmentSet LeftHandRaisedAlignment;

		public AlignmentSet RightHandLoweredAlignment;

		public AlignmentSet RightHandRaisedAlignment;

		private Skateboard board;

		private float currentCrouchShift;

		private float targetArmLift;

		private float currentArmLift;

		private Quaternion pelvisDefaultRotation;

		private Vector3 pelvisDefaultPosition;

		private Vector3 spineDefaultPosition;

		private float currentMomentumOffset;

		private float ikBlend;

		private List<AlignmentSet> alignmentSets = new List<AlignmentSet>();

		public float CurrentCrouchShift => currentCrouchShift;

		private void Awake()
		{
			board = GetComponent<Skateboard>();
			board.OnPushStart.AddListener(OnPushStart);
			pelvisDefaultPosition = PelvisAlignment.Transform.localPosition;
			pelvisDefaultRotation = PelvisAlignment.Transform.localRotation;
			spineDefaultPosition = SpineAlignment.Transform.localPosition;
			alignmentSets.Add(PelvisContainerAlignment);
			alignmentSets.Add(PelvisAlignment);
			alignmentSets.Add(SpineContainerAlignment);
			alignmentSets.Add(SpineAlignment);
			alignmentSets.Add(LeftFootAlignment);
			alignmentSets.Add(RightFootAlignment);
			alignmentSets.Add(LeftLegBendTarget);
			alignmentSets.Add(RightLegBendTarget);
			alignmentSets.Add(LeftHandAlignment);
			alignmentSets.Add(RightHandAlignment);
			alignmentSets.Add(LeftHandLoweredAlignment);
			alignmentSets.Add(LeftHandRaisedAlignment);
			alignmentSets.Add(RightHandLoweredAlignment);
			alignmentSets.Add(RightHandRaisedAlignment);
		}

		private void Update()
		{
			UpdateIKBlend();
		}

		private void LateUpdate()
		{
			UpdateBodyAlignment();
			UpdateArmLift();
			UpdatePelvisRotation();
		}

		private void FixedUpdate()
		{
		}

		private void UpdateIKBlend()
		{
			if (board.IsPushing || (board.TimeSincePushStart < PushAnimationDuration && board.JumpBuildAmount < 0.1f))
			{
				ikBlend = Mathf.Lerp(ikBlend, 1f, Time.deltaTime * IKBlendChangeRate);
			}
			else
			{
				ikBlend = Mathf.Lerp(ikBlend, 0f, Time.deltaTime * IKBlendChangeRate);
			}
			foreach (AlignmentSet alignmentSet in alignmentSets)
			{
				alignmentSet.Transform.localPosition = Vector3.Lerp(alignmentSet.Default.localPosition, alignmentSet.Animated.localPosition, ikBlend);
				alignmentSet.Transform.localRotation = Quaternion.Lerp(alignmentSet.Default.localRotation, alignmentSet.Animated.localRotation, ikBlend);
			}
		}

		private void UpdateBodyAlignment()
		{
			Vector3 position = PelvisAlignment.Transform.parent.TransformPoint(new Vector3(pelvisDefaultPosition.x, 0f, pelvisDefaultPosition.z));
			Vector3 a = new Vector3(0f, pelvisDefaultPosition.y, 0f);
			Vector3 b = base.transform.up * pelvisDefaultPosition.y;
			position += Vector3.Lerp(a, b, PelvisOffsetBlend);
			float jumpBuildAmount = board.JumpBuildAmount;
			float b2 = Mathf.Clamp01(board.CurrentSpeed_Kmh / board.TopSpeed_Kmh) * 0.1f;
			float b3 = Mathf.Max(jumpBuildAmount, b2);
			currentCrouchShift = Mathf.Lerp(currentCrouchShift, b3, Time.deltaTime * CrouchSpeed);
			position.y -= currentCrouchShift * JumpCrouchAmount;
			float b4 = Mathf.Clamp((0f - board.Accelerometer.Acceleration.y) * VerticalMomentumMultiplier, 0f - VerticalMomentumOffsetClamp, 0f);
			currentMomentumOffset = Mathf.Lerp(currentMomentumOffset, b4, Time.deltaTime * MomentumMoveSpeed);
			position.y += currentMomentumOffset;
			PelvisAlignment.Transform.position = position;
			SpineAlignment.Transform.localPosition = Vector3.Lerp(spineDefaultPosition, SpineAlignment_Hunched.localPosition, currentCrouchShift);
		}

		private void UpdateArmLift()
		{
			float jumpBuildAmount = board.JumpBuildAmount;
			float num = Mathf.Clamp01(board.CurrentSpeed_Kmh / board.TopSpeed_Kmh) * 0f;
			float num2 = Mathf.Abs(board.CurrentSteerInput) * 0f;
			SetArmLift(Mathf.Max(jumpBuildAmount, num, num2));
			currentArmLift = Mathf.Lerp(currentArmLift, targetArmLift, Time.deltaTime * ArmLiftRate);
			RightHandAlignment.Transform.localPosition = Vector3.Lerp(RightHandLoweredAlignment.Transform.localPosition, RightHandRaisedAlignment.Transform.localPosition, currentArmLift);
			RightHandAlignment.Transform.localRotation = Quaternion.Lerp(RightHandLoweredAlignment.Transform.localRotation, RightHandRaisedAlignment.Transform.localRotation, currentArmLift);
			LeftHandAlignment.Transform.localPosition = Vector3.Lerp(LeftHandLoweredAlignment.Transform.localPosition, LeftHandRaisedAlignment.Transform.localPosition, currentArmLift);
			LeftHandAlignment.Transform.localRotation = Quaternion.Lerp(LeftHandLoweredAlignment.Transform.localRotation, LeftHandRaisedAlignment.Transform.localRotation, currentArmLift);
		}

		private void UpdatePelvisRotation()
		{
			float num = board.CurrentSteerInput * PelvisMaxRotation;
			Quaternion b = pelvisDefaultRotation * Quaternion.AngleAxis(num, Vector3.up);
			PelvisAlignment.Transform.localRotation = Quaternion.Lerp(PelvisAlignment.Transform.localRotation, b, Time.deltaTime * 5f);
			HandContainer.localRotation = Quaternion.Lerp(HandContainer.localRotation, Quaternion.Euler(num, 0f, 0f), Time.deltaTime * 5f);
		}

		public void SetArmLift(float lift)
		{
			targetArmLift = lift;
		}

		private void OnPushStart()
		{
			IKAnimation.Stop();
			IKAnimation["Skateboard push"].speed = PushAnimationSpeed;
			IKAnimation.Play("Skateboard push");
		}
	}
}
