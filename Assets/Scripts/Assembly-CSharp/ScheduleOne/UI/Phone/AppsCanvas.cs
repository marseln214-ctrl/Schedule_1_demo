using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Tooltips;
using UnityEngine;

namespace ScheduleOne.UI.Phone
{
	public class AppsCanvas : PlayerSingleton<AppsCanvas>
	{
		[Header("References")]
		public Canvas canvas;

		protected override void Awake()
		{
			base.Awake();
			SetIsActive(active: false);
		}

		public void SetIsActive(bool active)
		{
			canvas.enabled = active;
		}

		protected override void Start()
		{
			base.Start();
			Singleton<TooltipManager>.Instance.AddCanvas(canvas);
		}
	}
}
