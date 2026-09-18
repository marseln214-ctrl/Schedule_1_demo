using System;
using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.ExtendedComponents;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.MainMenu
{
	public class SetupScreen : MainMenuScreen
	{
		public const string DEFAULT_SAVE_PATH = "DefaultDemoSave";

		[Header("References")]
		public GameInputField InputField;

		public Button StartButton;

		public RectTransform SkipIntroContainer;

		public Toggle SkipIntroToggle;

		public RectTransform NotHostWarning;

		private int slotIndex;

		protected virtual void Start()
		{
			InputField.onSubmit.AddListener(delegate
			{
				StartGame();
			});
			SkipIntroContainer.gameObject.SetActive(value: false);
		}

		public void Initialize(int index)
		{
			slotIndex = index;
		}

		private void Update()
		{
			if (base.IsOpen)
			{
				StartButton.interactable = IsInputValid() && Singleton<Lobby>.Instance.IsHost;
				NotHostWarning.gameObject.SetActive(!Singleton<Lobby>.Instance.IsHost);
			}
		}

		public void StartGame()
		{
			if (!IsInputValid())
			{
				return;
			}
			if (!Singleton<Lobby>.Instance.IsHost)
			{
				Console.LogWarning("Only the host can start the game.");
				return;
			}
			string text = Path.Combine(Singleton<SaveManager>.Instance.SaveContainerFolderPath, "SaveGame_" + (slotIndex + 1));
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			ClearFolderContents(text);
			CopyDefaultSaveToFolder(text);
			string path = Path.Combine(text, "Game.json");
			string json = new GameData(seed: UnityEngine.Random.Range(0, int.MaxValue), organisationName: InputField.text).GetJson();
			File.WriteAllText(path, json);
			bool isOn = SkipIntroToggle.isOn;
			isOn = true;
			string path2 = Path.Combine(text, "Metadata.json");
			string json2 = new MetaData(new DateTimeData(DateTime.Now), new DateTimeData(DateTime.Now), Application.version, Application.version, !isOn).GetJson();
			File.WriteAllText(path2, json2);
			Singleton<LoadManager>.Instance.RefreshSaveInfo();
			Singleton<LoadManager>.Instance.StartGame(LoadManager.SaveGames[slotIndex]);
		}

		private bool IsInputValid()
		{
			return !string.IsNullOrEmpty(InputField.text);
		}

		private void ClearFolderContents(string folderPath)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
			FileInfo[] files = directoryInfo.GetFiles();
			for (int i = 0; i < files.Length; i++)
			{
				files[i].Delete();
			}
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			for (int i = 0; i < directories.Length; i++)
			{
				directories[i].Delete(recursive: true);
			}
		}

		private void CopyDefaultSaveToFolder(string folderPath)
		{
			CopyFilesRecursively(Path.Combine(Application.streamingAssetsPath, "DefaultDemoSave"), folderPath);
		}

		private static void CopyFilesRecursively(string sourcePath, string targetPath)
		{
			string[] directories = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
			for (int i = 0; i < directories.Length; i++)
			{
				Directory.CreateDirectory(directories[i].Replace(sourcePath, targetPath));
			}
			directories = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
			foreach (string text in directories)
			{
				if (!text.EndsWith(".meta"))
				{
					File.Copy(text, text.Replace(sourcePath, targetPath), overwrite: true);
				}
			}
		}
	}
}
