using UnityEngine;

namespace ScheduleOne.Management
{
	public class TransitLineVisuals : MonoBehaviour
	{
		public LineRenderer Renderer;

		public void SetSourcePosition(Vector3 position)
		{
			Renderer.SetPosition(0, position);
		}

		public void SetDestinationPosition(Vector3 position)
		{
			Renderer.SetPosition(1, position);
		}
	}
}
