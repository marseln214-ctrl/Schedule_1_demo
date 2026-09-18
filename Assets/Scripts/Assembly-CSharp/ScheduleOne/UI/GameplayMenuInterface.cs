using System.Collections;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class GameplayMenuInterface : Singleton<GameplayMenuInterface>
	{
		public Canvas Canvas;

		public Button PhoneButton;

		public Button CharacterButton;

		public RectTransform SelectionIndicator;

		public CharacterInterface CharacterInterface;

		private Coroutine selectionLerp;

		protected override void Awake()
		{
			base.Awake();
			PhoneButton.onClick.AddListener(PhoneClicked);
			CharacterButton.onClick.AddListener(CharacterClicked);
			Close();
		}

		public void Open()
		{
			Canvas.enabled = true;
		}

		public void Close()
		{
			Canvas.enabled = false;
		}

		public void PhoneClicked()
		{
			Singleton<GameplayMenu>.Instance.SetScreen(GameplayMenu.EGameplayScreen.Phone);
		}

		public void CharacterClicked()
		{
			Singleton<GameplayMenu>.Instance.SetScreen(GameplayMenu.EGameplayScreen.Character);
		}

		public void SetSelected(GameplayMenu.EGameplayScreen screen)
		{
			Vector2 pos = Vector2.zero;
			PhoneButton.interactable = true;
			CharacterButton.interactable = true;
			if (screen == GameplayMenu.EGameplayScreen.Character)
			{
				CharacterInterface.Open();
			}
			else
			{
				CharacterInterface.Close();
			}
			switch (screen)
			{
			case GameplayMenu.EGameplayScreen.Phone:
				pos = PhoneButton.transform.position;
				PhoneButton.interactable = false;
				break;
			case GameplayMenu.EGameplayScreen.Character:
				pos = CharacterButton.transform.position;
				CharacterButton.interactable = false;
				break;
			}
			if (selectionLerp != null)
			{
				StopCoroutine(selectionLerp);
			}
			selectionLerp = StartCoroutine(Lerp());
			IEnumerator Lerp()
			{
				float startX = SelectionIndicator.position.x;
				for (float t = 0f; t < 0.12f; t += Time.deltaTime)
				{
					SelectionIndicator.position = new Vector2(Mathf.Lerp(startX, pos.x, t / 0.12f), SelectionIndicator.position.y);
					yield return new WaitForEndOfFrame();
				}
				SelectionIndicator.position = new Vector2(pos.x, SelectionIndicator.position.y);
				selectionLerp = null;
			}
		}
	}
}
