using UnityEngine;

namespace EasyButtons.Example
{
	public class ButtonsExample : MonoBehaviour
	{
		[Button]
		public void SayMyName()
		{
			Debug.Log(base.name);
		}

		[Button(Mode = ButtonMode.DisabledInPlayMode)]
		protected void SayHelloEditor()
		{
			Debug.Log("Hello from edit mode");
		}

		[Button(Mode = ButtonMode.EnabledInPlayMode)]
		private void SayHelloInRuntime()
		{
			Debug.Log("Hello from play mode");
		}

		[Button("Special Name", Spacing = ButtonSpacing.Before)]
		private void TestButtonName()
		{
			Debug.Log("Hello from special name button");
		}

		[Button("Special Name Editor Only", Mode = ButtonMode.DisabledInPlayMode)]
		private void TestButtonNameEditorOnly()
		{
			Debug.Log("Hello from special name button for editor only");
		}

		[Button]
		private static void TestStaticMethod()
		{
			Debug.Log("Hello from static method");
		}

		[Button("Space Before and After", Spacing = (ButtonSpacing.Before | ButtonSpacing.After))]
		private void TestButtonSpaceBoth()
		{
			Debug.Log("Hello from a button surround by spaces");
		}

		[Button("Button With Parameters")]
		private void TestButtonWithParams(string message, int number)
		{
			Debug.Log($"Your message #{number}: \"{message}\"");
		}

		[Button("Expanded Button Example", Expanded = true)]
		private void TestExpandedButton(string message)
		{
			Debug.Log(message);
		}
	}
}
