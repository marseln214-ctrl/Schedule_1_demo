using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class Moveable : Clickable
	{
		protected Vector3 clickOffset = Vector3.zero;

		protected float clickDist;

		[Header("Bounds")]
		[SerializeField]
		protected float yMax = 10f;

		[SerializeField]
		protected float yMin = -10f;

		public override void StartClick(RaycastHit hit)
		{
			base.StartClick(hit);
			clickDist = Vector3.Distance(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position);
			clickOffset = base.transform.position - PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, clickDist));
		}

		protected virtual void Update()
		{
			if (base.IsHeld)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, clickDist)) + clickOffset, Time.deltaTime * 10f);
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, Mathf.Clamp(base.transform.localPosition.y, yMin, yMax), base.transform.localPosition.z);
			}
		}

		public override void EndClick()
		{
			base.EndClick();
		}
	}
}
