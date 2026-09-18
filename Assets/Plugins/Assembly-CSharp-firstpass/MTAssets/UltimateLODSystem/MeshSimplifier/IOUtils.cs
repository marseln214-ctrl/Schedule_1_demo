using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MTAssets.UltimateLODSystem.MeshSimplifier
{
	internal static class IOUtils
	{
		internal static string MakeSafeRelativePath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}
			path = path.Replace('\\', '/').Trim('/');
			if (Path.IsPathRooted(path))
			{
				throw new ArgumentException("The path cannot be rooted.", "path");
			}
			string[] array = path.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = MakeSafeFileName(array[i]);
			}
			return string.Join("/", array);
		}

		internal static string MakeSafeFileName(string name)
		{
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			StringBuilder stringBuilder = new StringBuilder(name.Length);
			bool flag = false;
			foreach (char value in name)
			{
				if (!invalidFileNameChars.Contains(value))
				{
					stringBuilder.Append(value);
				}
				else if (!flag)
				{
					flag = true;
					stringBuilder.Append('_');
				}
			}
			return stringBuilder.ToString();
		}
	}
}
