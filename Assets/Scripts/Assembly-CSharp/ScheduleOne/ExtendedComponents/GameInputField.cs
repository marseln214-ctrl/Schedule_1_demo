using TMPro;

namespace ScheduleOne.ExtendedComponents
{
	public class GameInputField : TMP_InputField
	{
		protected override void Awake()
		{
			base.Awake();
			base.onSelect.AddListener(EditStart);
			base.onEndEdit.AddListener(EndEdit);
		}

		private void EditStart(string newVal)
		{
			GameInput.IsTyping = true;
		}

		private void EndEdit(string newVal)
		{
			GameInput.IsTyping = false;
		}
	}
}
