using UnityEngine;

namespace ScheduleOne.ObjectScripts.Cash
{
	public class CashStackVisuals : MonoBehaviour
	{
		public const float MAX_AMOUNT = 1000f;

		[Header("References")]
		public GameObject Visuals_Under100;

		public GameObject[] Notes;

		public GameObject Visuals_Over100;

		public GameObject[] Bills;

		public void ShowAmount(float amount)
		{
			Visuals_Over100.SetActive(amount >= 100f);
			Visuals_Under100.SetActive(amount < 100f);
			if (amount >= 100f)
			{
				int num = Mathf.RoundToInt(amount / 100f);
				for (int i = 0; i < Bills.Length; i++)
				{
					Bills[i].SetActive(num > i);
				}
			}
			else
			{
				int num2 = Mathf.Clamp(Mathf.RoundToInt(amount / 10f), 0, 10);
				for (int j = 0; j < Notes.Length; j++)
				{
					Notes[j].SetActive(num2 > j);
				}
			}
		}
	}
}
