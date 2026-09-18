using UnityEngine;

namespace ScheduleOne.Map.Infrastructure
{
	public class TrafficLight : MonoBehaviour
	{
		public enum State
		{
			Red = 0,
			Orange = 1,
			Green = 2
		}

		public static float amberTime = 3f;

		[Header("References")]
		[SerializeField]
		protected MeshRenderer redMesh;

		[SerializeField]
		protected MeshRenderer orangeMesh;

		[SerializeField]
		protected MeshRenderer greenMesh;

		[Header("Materials")]
		[SerializeField]
		protected Material redOn_Mat;

		[SerializeField]
		protected Material redOff_Mat;

		[SerializeField]
		protected Material orangeOn_Mat;

		[SerializeField]
		protected Material orangeOff_Mat;

		[SerializeField]
		protected Material greenOn_Mat;

		[SerializeField]
		protected Material greenOff_Mat;

		[Header("Settings")]
		public State state;

		private State appliedState;

		protected virtual void Start()
		{
			ApplyState();
		}

		protected virtual void Update()
		{
			if (appliedState != state)
			{
				ApplyState();
			}
		}

		protected virtual void ApplyState()
		{
			appliedState = state;
			redMesh.material = redOff_Mat;
			orangeMesh.material = orangeOff_Mat;
			greenMesh.material = greenOff_Mat;
			switch (state)
			{
			case State.Red:
				redMesh.material = redOn_Mat;
				break;
			case State.Orange:
				orangeMesh.material = orangeOn_Mat;
				break;
			case State.Green:
				greenMesh.material = greenOn_Mat;
				break;
			}
		}
	}
}
