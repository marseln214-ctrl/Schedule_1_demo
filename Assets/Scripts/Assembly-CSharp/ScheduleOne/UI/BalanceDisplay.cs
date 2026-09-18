using ScheduleOne.Money;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class BalanceDisplay : MonoBehaviour
	{
		public const float RESIDUAL_TIME = 3f;

		public const float FADE_TIME = 0.25f;

		[Header("References")]
		public CanvasGroup Group;

		public TextMeshProUGUI BalanceLabel;

		public bool active { get; protected set; }

		public float timeSinceActiveSet { get; protected set; }

		protected virtual void Update()
		{
			timeSinceActiveSet += Time.deltaTime;
			if (timeSinceActiveSet > 3f)
			{
				active = false;
			}
			if (Group != null)
			{
				Group.alpha = Mathf.MoveTowards(Group.alpha, active ? 1f : 0f, Time.deltaTime / 0.25f);
			}
		}

		public void SetBalance(float balance)
		{
			BalanceLabel.text = MoneyManager.FormatAmount(balance);
		}

		public void Show()
		{
			active = true;
			timeSinceActiveSet = 0f;
		}
	}
}
