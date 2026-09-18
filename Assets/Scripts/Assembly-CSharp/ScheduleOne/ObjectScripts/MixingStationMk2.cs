using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.ObjectScripts
{
	public class MixingStationMk2 : MixingStation
	{
		public Animation Animation;

		[Header("Screen")]
		public Canvas ScreenCanvas;

		public Image OutputIcon;

		public RectTransform QuestionMark;

		public TextMeshProUGUI QuantityLabel;

		public TextMeshProUGUI ProgressLabel;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted;

		protected override void MinPass()
		{
			base.MinPass();
			UpdateScreen();
		}

		public override void MixingStart()
		{
			base.MixingStart();
			Animation.Play("Mixing station start");
			EnableScreen();
		}

		public override void MixingDone()
		{
			base.MixingDone();
			Animation.Play("Mixing station end");
			DisableScreen();
		}

		private void EnableScreen()
		{
			if (base.CurrentMixOperation != null)
			{
				QuantityLabel.text = base.CurrentMixOperation.Quantity + "x";
				if (base.CurrentMixOperation.IsOutputKnown(out var knownProduct))
				{
					OutputIcon.sprite = knownProduct.Icon;
					OutputIcon.color = Color.white;
					QuestionMark.gameObject.SetActive(value: false);
				}
				else
				{
					OutputIcon.sprite = Registry.GetItem(base.CurrentMixOperation.ProductID).Icon;
					OutputIcon.color = Color.black;
					QuestionMark.gameObject.SetActive(value: true);
				}
				UpdateScreen();
				ScreenCanvas.enabled = true;
			}
		}

		private void UpdateScreen()
		{
			if (base.CurrentMixOperation != null)
			{
				ProgressLabel.text = GetMixTimeForCurrentOperation() - base.CurrentMixTime + " mins remaining";
			}
		}

		private void DisableScreen()
		{
			ScreenCanvas.enabled = false;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
