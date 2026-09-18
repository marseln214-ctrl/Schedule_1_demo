using System;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vision
{
	public class VisionEvent
	{
		private const float NOTICE_DROP_THRESHOLD = 1f;

		private float timeSinceSighted;

		private float currentNoticeTime;

		public Player Target { get; protected set; }

		public PlayerVisualState.VisualState State { get; protected set; }

		public VisionCone Owner { get; protected set; }

		public float FullNoticeTime { get; protected set; }

		public float NormalizedNoticeLevel => currentNoticeTime / FullNoticeTime;

		public VisionEvent(VisionCone _owner, Player _target, PlayerVisualState.VisualState _state, float _noticeTime)
		{
			Owner = _owner;
			Target = _target;
			State = _state;
			FullNoticeTime = _noticeTime;
			PlayerVisualState.VisualState state = State;
			state.stateDestroyed = (Action)Delegate.Combine(state.stateDestroyed, new Action(EndEvent));
		}

		public void UpdateEvent(float visionDeltaThisFrame, float tickTime)
		{
			float normalizedNoticeLevel = NormalizedNoticeLevel;
			if (visionDeltaThisFrame > 0f)
			{
				timeSinceSighted = 0f;
			}
			else
			{
				timeSinceSighted += tickTime;
			}
			if (visionDeltaThisFrame > 0f)
			{
				currentNoticeTime += visionDeltaThisFrame * (Owner.Attentiveness * VisionCone.UniversalAttentivenessScale) * tickTime;
			}
			else if (timeSinceSighted > 1f * (Owner.Memory * VisionCone.UniversalMemoryScale))
			{
				currentNoticeTime -= tickTime / (Owner.Memory * VisionCone.UniversalMemoryScale);
			}
			currentNoticeTime = Mathf.Clamp(currentNoticeTime, 0f, FullNoticeTime);
			if (Target.Visibility.HighestVisionEvent == null || NormalizedNoticeLevel > Target.Visibility.HighestVisionEvent.NormalizedNoticeLevel)
			{
				Target.Visibility.HighestVisionEvent = this;
			}
			if (NormalizedNoticeLevel <= 0f && normalizedNoticeLevel > 0f)
			{
				EndEvent();
			}
			if (NormalizedNoticeLevel >= 0.5f && normalizedNoticeLevel < 0.5f)
			{
				if (Target.Visibility.HighestVisionEvent == this)
				{
					Target.Visibility.HighestVisionEvent = null;
				}
				Owner.EventHalfNoticed(this);
			}
			if (NormalizedNoticeLevel >= 1f && normalizedNoticeLevel < 1f)
			{
				if (Target.Visibility.HighestVisionEvent == this)
				{
					Target.Visibility.HighestVisionEvent = null;
				}
				Owner.EventFullyNoticed(this);
			}
		}

		public void EndEvent()
		{
			if (Target.Visibility.HighestVisionEvent == this)
			{
				Target.Visibility.HighestVisionEvent = null;
			}
			Owner.EventReachedZero(this);
		}
	}
}
