using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCreator
{
	public class CharacterCreatorToggle : CharacterCreatorField<int>
	{
		[Header("References")]
		public Button Button1;

		public Button Button2;

		protected override void Awake()
		{
			base.Awake();
			Button1.onClick.AddListener(OnButton1);
			Button2.onClick.AddListener(OnButton2);
		}

		public override void ApplyValue()
		{
			base.ApplyValue();
			Button1.interactable = base.value != 0;
			Button2.interactable = base.value == 0;
		}

		public void OnButton1()
		{
			base.value = 0;
			WriteValue();
		}

		public void OnButton2()
		{
			base.value = 1;
			WriteValue();
		}
	}
}
