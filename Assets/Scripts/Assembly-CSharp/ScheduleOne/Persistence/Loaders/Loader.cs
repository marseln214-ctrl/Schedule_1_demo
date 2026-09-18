using System;
using System.Collections.Generic;
using System.IO;

namespace ScheduleOne.Persistence.Loaders
{
	public class Loader
	{
		public virtual void Load(string mainPath)
		{
		}

		public bool TryLoadFile(string parentPath, string fileName, out string contents)
		{
			return TryLoadFile(Path.Combine(parentPath, fileName), out contents);
		}

		public bool TryLoadFile(string path, out string contents, bool autoAddExtension = true)
		{
			contents = string.Empty;
			string text = path;
			if (autoAddExtension)
			{
				text += ".json";
			}
			if (!File.Exists(text))
			{
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

		protected List<DirectoryInfo> GetDirectories(string parentPath)
		{
			if (!Directory.Exists(parentPath))
			{
				return new List<DirectoryInfo>();
			}
			List<DirectoryInfo> list = new List<DirectoryInfo>();
			string[] directories = Directory.GetDirectories(parentPath);
			for (int i = 0; i < directories.Length; i++)
			{
				list.Add(new DirectoryInfo(directories[i]));
			}
			return list;
		}
	}
}
