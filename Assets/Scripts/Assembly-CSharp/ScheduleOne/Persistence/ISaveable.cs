using System;
using System.Collections.Generic;
using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Loaders;

namespace ScheduleOne.Persistence
{
	public interface ISaveable
	{
		string SaveFolderName { get; }

		string SaveFileName { get; }

		Loader Loader { get; }

		bool ShouldSaveUnderFolder { get; }

		List<string> LocalExtraFiles { get; set; }

		List<string> LocalExtraFolders { get; set; }

		bool HasChanged { get; set; }

		void InitializeSaveable();

		string GetSaveString();

		string Save(string parentFolderPath)
		{
			bool isFolder;
			string localPath = GetLocalPath(out isFolder);
			bool flag = (isFolder ? Directory.Exists(Path.Combine(parentFolderPath, localPath)) : File.Exists(Path.Combine(parentFolderPath, localPath)));
			if (!HasChanged && flag)
			{
				CompleteSave(parentFolderPath, writeDataFile: true);
				return localPath;
			}
			new SaveRequest(this, parentFolderPath);
			return localPath;
		}

		void WriteBaseData(string parentFolderPath, string saveString)
		{
			string text = Path.Combine(parentFolderPath, SaveFileName + ".json");
			if (ShouldSaveUnderFolder)
			{
				text = Path.Combine(GetContainerFolder(parentFolderPath), SaveFileName + ".json");
			}
			if (!string.IsNullOrEmpty(saveString))
			{
				try
				{
					File.WriteAllText(text, saveString);
				}
				catch (Exception ex)
				{
					Console.LogError("Failed to write save data file. Exception: " + ex?.ToString() + "\nData path: " + text + "\nSave string: " + saveString);
				}
			}
			else
			{
				Console.LogError("Failed to write save data file because the save string is empty. Data path: " + text);
			}
			CompleteSave(parentFolderPath, !string.IsNullOrEmpty(saveString));
		}

		string GetLocalPath(out bool isFolder)
		{
			string result = SaveFileName + ".json";
			if (ShouldSaveUnderFolder)
			{
				isFolder = true;
				result = SaveFolderName;
			}
			else
			{
				isFolder = false;
			}
			return result;
		}

		void CompleteSave(string parentFolderPath, bool writeDataFile)
		{
			List<string> list = new List<string>();
			if (LocalExtraFiles != null)
			{
				for (int i = 0; i < LocalExtraFiles.Count; i++)
				{
					list.Add(LocalExtraFiles[i] + ".json");
				}
			}
			if (LocalExtraFolders != null)
			{
				list.AddRange(LocalExtraFolders);
			}
			if (writeDataFile)
			{
				GetLocalPath(out var isFolder);
				if (isFolder)
				{
					string item = Path.Combine(SaveFileName + ".json");
					list.Add(item);
				}
			}
			List<string> collection = WriteData(parentFolderPath);
			list.AddRange(collection);
			if (ShouldSaveUnderFolder)
			{
				string containerFolder = GetContainerFolder(parentFolderPath);
				string[] files = Directory.GetFiles(containerFolder);
				string[] directories = Directory.GetDirectories(containerFolder);
				string[] array = files;
				foreach (string text in array)
				{
					FileInfo fileInfo = new FileInfo(text);
					if (!list.Contains(fileInfo.Name))
					{
						try
						{
							File.Delete(text);
						}
						catch (Exception ex)
						{
							Console.LogError("Failed to delete file: " + text + "\nException: " + ex);
						}
					}
				}
				array = directories;
				foreach (string text2 in array)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(text2);
					if (!list.Contains(directoryInfo.Name))
					{
						try
						{
							Directory.Delete(text2, recursive: true);
						}
						catch (Exception ex2)
						{
							Console.LogError("Failed to delete folder: " + text2 + "\nException: " + ex2);
						}
					}
				}
				DeleteUnapprovedFiles(parentFolderPath);
			}
			Singleton<SaveManager>.Instance.CompleteSaveable(this);
		}

		List<string> WriteData(string parentFolderPath)
		{
			return new List<string>();
		}

		void DeleteUnapprovedFiles(string parentFolderPath)
		{
		}

		string GetContainerFolder(string parentFolderPath)
		{
			string text = Path.Combine(parentFolderPath, SaveFolderName);
			if (!Directory.Exists(text))
			{
				try
				{
					Directory.CreateDirectory(text);
				}
				catch (Exception ex)
				{
					Console.LogError("Failed to write save folder. Exception: " + ex?.ToString() + "\nFolder path: " + text);
				}
			}
			return text;
		}

		string WriteSubfile(string parentPath, string localPath_NoExtensions, string contents)
		{
			bool isFolder;
			string text = Path.Combine(parentPath, GetLocalPath(out isFolder));
			if (!isFolder)
			{
				Console.LogError("Failed to write subfile: " + localPath_NoExtensions + " because the saveable is not saved under a folder.");
				return string.Empty;
			}
			if (!Directory.Exists(text))
			{
				Console.LogError("Failed to write subfile: " + localPath_NoExtensions + " because the main folder does not exist.");
				return string.Empty;
			}
			if (!LocalExtraFiles.Contains(localPath_NoExtensions))
			{
				Console.LogWarning("Writing subfile called '" + localPath_NoExtensions + "' that is not in the list of extra saveables. Be sure to include it in the returned files list.");
			}
			if (localPath_NoExtensions.Contains(".json"))
			{
				Console.LogError("Failed to write subfile: " + localPath_NoExtensions + " because it contains a data extension.");
				return string.Empty;
			}
			string text2 = localPath_NoExtensions + ".json";
			string text3 = Path.Combine(parentPath, text, text2);
			try
			{
				File.WriteAllText(text3, contents);
			}
			catch (Exception ex)
			{
				Console.LogError("Failed to write sub file. Exception: " + ex?.ToString() + "\nData path: " + text3 + "\nSave string: " + contents);
			}
			return text2;
		}

		string WriteFolder(string parentPath, string localPath_NoExtensions)
		{
			bool isFolder;
			string text = Path.Combine(parentPath, GetLocalPath(out isFolder));
			if (!isFolder)
			{
				Console.LogError("Failed to write subfile: " + localPath_NoExtensions + " because the saveable is not saved under a folder.");
				return string.Empty;
			}
			if (!Directory.Exists(text))
			{
				Console.LogError("Failed to write subfile: " + localPath_NoExtensions + " because the main folder (" + text + ") does not exist.");
				return string.Empty;
			}
			if (!LocalExtraFolders.Contains(localPath_NoExtensions))
			{
				Console.LogWarning("Writing subfile called '" + localPath_NoExtensions + "' that is not in the list of extra saveables. Be sure to include it in the returned files list.");
			}
			if (localPath_NoExtensions.Contains(".json"))
			{
				Console.LogError("Failed to write subfile: " + localPath_NoExtensions + " because it contains a data extension.");
				return string.Empty;
			}
			string text2 = Path.Combine(parentPath, text, localPath_NoExtensions);
			try
			{
				Directory.CreateDirectory(text2);
			}
			catch (Exception ex)
			{
				Console.LogError("Failed to write sub folder. Exception: " + ex?.ToString() + "\nData path: " + text2);
			}
			return text2;
		}

		bool TryLoadFile(string parentPath, string fileName, out string contents)
		{
			return TryLoadFile(Path.Combine(parentPath, fileName), out contents);
		}

		bool TryLoadFile(string path, out string contents, bool autoAddExtension = true)
		{
			contents = string.Empty;
			string text = path;
			if (autoAddExtension)
			{
				text += ".json";
			}
			if (!File.Exists(text))
			{
				Console.LogWarning("File not found at: " + text);
				return false;
			}
			try
			{
				contents = File.ReadAllText(text);
			}
			catch (Exception ex)
			{
				Console.LogError("Error reading file: " + text + "\n" + ex);
				return false;
			}
			return true;
		}
	}
}
