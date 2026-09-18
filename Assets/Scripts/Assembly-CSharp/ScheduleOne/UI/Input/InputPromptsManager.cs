using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI.Input
{
	public class InputPromptsManager : Singleton<InputPromptsManager>
	{
		[Header("Input Prompt Prefabs")]
		public GameObject KeyPromptPrefab;

		public GameObject WideKeyPromptPrefab;

		public GameObject ExtraWideKeyPromptPrefab;

		public GameObject LeftClickPromptPrefab;

		public GameObject MiddleClickPromptPrefab;

		public GameObject RightClickPromptPrefab;

		public PromptImage GetPromptImage(string controlPath, RectTransform parent)
		{
			if (GetDisplayNameForControlPath(controlPath) == string.Empty)
			{
				Console.LogError("GetPromptImage: controlPath " + controlPath + " not found");
				return null;
			}
			if (IsControlPathMouseRelated(controlPath))
			{
				return controlPath switch
				{
					"leftButton" => Object.Instantiate(LeftClickPromptPrefab, parent).GetComponent<PromptImage>(), 
					"middleButton" => Object.Instantiate(MiddleClickPromptPrefab, parent).GetComponent<PromptImage>(), 
					"rightButton" => Object.Instantiate(RightClickPromptPrefab, parent).GetComponent<PromptImage>(), 
					_ => null, 
				};
			}
			if (IsControlPathExtraWideKey(controlPath))
			{
				PromptImageWithText component = Object.Instantiate(ExtraWideKeyPromptPrefab, parent).GetComponent<PromptImageWithText>();
				component.Label.text = GetDisplayNameForControlPath(controlPath);
				return component;
			}
			if (IsControlPathWideKey(controlPath))
			{
				PromptImageWithText component2 = Object.Instantiate(WideKeyPromptPrefab, parent).GetComponent<PromptImageWithText>();
				component2.Label.text = GetDisplayNameForControlPath(controlPath);
				return component2;
			}
			PromptImageWithText component3 = Object.Instantiate(KeyPromptPrefab, parent).GetComponent<PromptImageWithText>();
			component3.Label.text = GetDisplayNameForControlPath(controlPath);
			return component3;
		}

		private bool IsControlPathMouseRelated(string controlPath)
		{
			return controlPath switch
			{
				"leftButton" => true, 
				"middleButton" => true, 
				"rightButton" => true, 
				_ => false, 
			};
		}

		private bool IsControlPathWideKey(string controlPath)
		{
			return controlPath switch
			{
				"escape" => true, 
				"tab" => true, 
				"capsLock" => true, 
				"enter" => true, 
				"backspace" => true, 
				"leftShift" => true, 
				"rightShift" => true, 
				"shift" => true, 
				"ctrl" => true, 
				"leftCtrl" => true, 
				"rightCtrl" => true, 
				"leftAlt" => true, 
				"rightAlt" => true, 
				_ => false, 
			};
		}

		private bool IsControlPathExtraWideKey(string controlPath)
		{
			if (controlPath == "space")
			{
				return true;
			}
			return false;
		}

		public string GetDisplayNameForControlPath(string controlPath)
		{
			return controlPath switch
			{
				"backquote" => "`", 
				"1" => "1", 
				"2" => "2", 
				"3" => "3", 
				"4" => "4", 
				"5" => "5", 
				"6" => "6", 
				"7" => "7", 
				"8" => "8", 
				"9" => "9", 
				"0" => "0", 
				"minus" => "-", 
				"equals" => "=", 
				"backspace" => "Back", 
				"tab" => "Tab", 
				"q" => "Q", 
				"w" => "W", 
				"e" => "E", 
				"r" => "R", 
				"t" => "T", 
				"y" => "Y", 
				"u" => "U", 
				"i" => "I", 
				"o" => "O", 
				"p" => "P", 
				"leftBracket" => "[", 
				"rightBracket" => "]", 
				"backslash" => "\\", 
				"capsLock" => "Caps", 
				"a" => "A", 
				"s" => "S", 
				"d" => "D", 
				"f" => "F", 
				"g" => "G", 
				"h" => "H", 
				"j" => "J", 
				"k" => "K", 
				"l" => "L", 
				"semicolon" => ";", 
				"quote" => "'", 
				"enter" => "Enter", 
				"leftShift" => "Shift", 
				"shift" => "Shift", 
				"z" => "Z", 
				"x" => "X", 
				"c" => "C", 
				"v" => "V", 
				"b" => "B", 
				"n" => "N", 
				"m" => "M", 
				"comma" => ",", 
				"period" => ".", 
				"slash" => "/", 
				"rightShift" => "Shift", 
				"leftCtrl" => "Ctrl", 
				"ctrl" => "Ctrl", 
				"leftAlt" => "Alt", 
				"space" => "Space", 
				"rightAlt" => "Alt", 
				"rightCtrl" => "Ctrl", 
				"leftButton" => "LMB", 
				"middleButton" => "MMB", 
				"rightButton" => "RMB", 
				"escape" => "Esc", 
				"f1" => "F1", 
				"f2" => "F2", 
				"f3" => "F3", 
				"f4" => "F4", 
				"f5" => "F5", 
				"f6" => "F6", 
				"f7" => "F7", 
				"f8" => "F8", 
				"f9" => "F9", 
				"f10" => "F10", 
				"f11" => "F11", 
				"f12" => "F12", 
				"leftArrow" => "Left", 
				"upArrow" => "Up", 
				"downArrow" => "Down", 
				"rightArrow" => "Right", 
				_ => string.Empty, 
			};
		}
	}
}
