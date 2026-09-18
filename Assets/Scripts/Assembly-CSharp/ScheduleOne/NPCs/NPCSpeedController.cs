using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.NPCs
{
	public class NPCSpeedController : MonoBehaviour
	{
		[Serializable]
		public class SpeedControl
		{
			public string id;

			public int priority;

			public float speed;

			public SpeedControl(string id, int priority, float speed)
			{
				this.id = id;
				this.priority = priority;
				this.speed = speed;
			}
		}

		[Header("Settings")]
		[Range(0f, 1f)]
		public float DefaultWalkSpeed = 0.08f;

		public float SpeedMultiplier = 1f;

		[Header("References")]
		public NPCMovement Movement;

		protected List<SpeedControl> speedControlStack = new List<SpeedControl>();

		[Header("Debug")]
		public SpeedControl ActiveSpeedControl;

		private void Awake()
		{
			AddSpeedControl(new SpeedControl("default", 0, DefaultWalkSpeed));
		}

		private void FixedUpdate()
		{
			SpeedControl speedControl = (ActiveSpeedControl = GetHighestPriorityControl());
			if (Movement.DEBUG)
			{
				Debug.Log("Active speed control: " + speedControl.id + ", speed : " + speedControl.speed);
			}
			Movement.MovementSpeedScale = speedControl.speed * SpeedMultiplier;
		}

		private SpeedControl GetHighestPriorityControl()
		{
			return speedControlStack[0];
		}

		public void AddSpeedControl(SpeedControl control)
		{
			SpeedControl speedControl = speedControlStack.Find((SpeedControl x) => x.id == control.id);
			if (speedControl != null)
			{
				speedControl.priority = control.priority;
				speedControl.speed = control.speed;
				return;
			}
			for (int i = 0; i < speedControlStack.Count; i++)
			{
				if (control.priority >= speedControlStack[i].priority)
				{
					speedControlStack.Insert(i, control);
					return;
				}
			}
			speedControlStack.Add(control);
		}

		public SpeedControl GetSpeedControl(string id)
		{
			return speedControlStack.Find((SpeedControl x) => x.id == id);
		}

		public bool DoesSpeedControlExist(string id)
		{
			return GetSpeedControl(id) != null;
		}

		public void RemoveSpeedControl(string id)
		{
			SpeedControl speedControl = speedControlStack.Find((SpeedControl x) => x.id == id);
			if (speedControl != null)
			{
				speedControlStack.Remove(speedControl);
			}
		}
	}
}
