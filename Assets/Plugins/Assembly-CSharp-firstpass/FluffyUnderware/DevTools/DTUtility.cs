using System;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public static class DTUtility
	{
		public const string HelpUrlBase = "https://curvyeditor.com/doclink/";

		public static bool IsEditorStateChange => false;

		public static bool IsInEditMode => false;

		[UsedImplicitly]
		[Obsolete("Will get removed since it is not used by Curvy, and needs maintenance to be compatible with Unity's Enter Play Mode Settings")]
		public static Material GetDefaultMaterial()
		{
			return null;
		}

		public static float GetHandleSize(Vector3 position)
		{
			Camera current = Camera.current;
			if ((bool)current)
			{
				Transform transform = current.transform;
				Vector3 direction = default(Vector3);
				direction.x = 0f;
				direction.y = 0f;
				direction.z = 1f;
				Vector3 cameraZDirection = transform.TransformDirection(direction);
				direction.x = 1f;
				direction.y = 0f;
				direction.z = 0f;
				Vector3 cameraXDirection = transform.TransformDirection(direction);
				return GetHandleSize(Gizmos.matrix.MultiplyPoint3x4(position), current, (float)current.pixelWidth * 0.5f, (float)current.pixelHeight * 0.5f, transform.position, cameraZDirection, cameraXDirection);
			}
			return 20f;
		}

		public static float GetHandleSize(Vector3 position, Camera camera, float cameraCenterWidth, float cameraCenterHeight, Vector3 cameraPosition, Vector3 cameraZDirection, Vector3 cameraXDirection)
		{
			float num = (position.x - cameraPosition.x) * cameraZDirection.x + (position.y - cameraPosition.y) * cameraZDirection.y + (position.z - cameraPosition.z) * cameraZDirection.z;
			Vector3 position2 = default(Vector3);
			position2.x = cameraPosition.x + cameraZDirection.x * num + cameraXDirection.x;
			position2.y = cameraPosition.y + cameraZDirection.y * num + cameraXDirection.y;
			position2.z = cameraPosition.z + cameraZDirection.z * num + cameraXDirection.z;
			Vector3 vector = camera.WorldToScreenPoint(position2, Camera.MonoOrStereoscopicEye.Mono);
			float num2 = cameraCenterWidth - vector.x;
			float num3 = cameraCenterHeight - vector.y;
			return 80f / (float)Math.Sqrt(num2 * num2 + num3 * num3);
		}

		public static void SetPlayerPrefs<T>(string key, T value)
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsEnum)
			{
				PlayerPrefs.SetInt(key, Convert.ToInt32(Enum.Parse(typeof(T), value.ToString()) as Enum));
				return;
			}
			if (typeFromHandle.IsArray)
			{
				throw new NotImplementedException();
			}
			if (typeFromHandle.Matches(typeof(int), typeof(int)))
			{
				PlayerPrefs.SetInt(key, (value as int?).Value);
			}
			else if (typeFromHandle == typeof(string))
			{
				PlayerPrefs.SetString(key, value as string);
			}
			else if (typeFromHandle == typeof(float))
			{
				PlayerPrefs.SetFloat(key, (value as float?).Value);
			}
			else if (typeFromHandle == typeof(bool))
			{
				PlayerPrefs.SetInt(key, (value as bool?).Value ? 1 : 0);
			}
			else if (typeFromHandle == typeof(Color))
			{
				PlayerPrefs.SetString(key, (value as Color?).Value.ToHtml());
			}
			else
			{
				Debug.LogError("[DevTools.SetEditorPrefs] Unsupported datatype: " + typeFromHandle.Name);
			}
		}

		public static T GetPlayerPrefs<T>(string key, T defaultValue)
		{
			if (PlayerPrefs.HasKey(key))
			{
				Type typeFromHandle = typeof(T);
				try
				{
					if (typeFromHandle.IsEnum || typeFromHandle.Matches(typeof(int), typeof(int)))
					{
						return (T)(object)PlayerPrefs.GetInt(key, (int)(object)defaultValue);
					}
					if (typeFromHandle.IsArray)
					{
						throw new NotImplementedException();
					}
					if (typeFromHandle == typeof(string))
					{
						return (T)(object)PlayerPrefs.GetString(key, defaultValue.ToString());
					}
					if (typeFromHandle == typeof(float))
					{
						return (T)(object)PlayerPrefs.GetFloat(key, (float)(object)defaultValue);
					}
					if (typeFromHandle == typeof(bool))
					{
						return (T)(object)(PlayerPrefs.GetInt(key, ((bool)(object)defaultValue) ? 1 : 0) == 1);
					}
					if (typeFromHandle == typeof(Color))
					{
						return (T)(object)PlayerPrefs.GetString(key, ((Color)(object)defaultValue).ToHtml()).ColorFromHtml();
					}
					Debug.LogError("[DevTools.SetEditorPrefs] Unsupported datatype: " + typeFromHandle.Name);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					return defaultValue;
				}
			}
			return defaultValue;
		}

		public static float RandomSign()
		{
			return UnityEngine.Random.Range(0, 2) * 2 - 1;
		}

		public static string GetHelpUrl(object forClass)
		{
			if (forClass != null)
			{
				return GetHelpUrl(forClass.GetType());
			}
			return string.Empty;
		}

		public static string GetHelpUrl(Type classType)
		{
			if (classType != null)
			{
				object[] customAttributes = classType.GetCustomAttributes(typeof(HelpURLAttribute), inherit: true);
				if (customAttributes.Length != 0)
				{
					return ((HelpURLAttribute)customAttributes[0]).URL;
				}
			}
			return string.Empty;
		}

		public static Vector3 GetCenterPosition(Vector3 fallback, params Vector3[] vectors)
		{
			if (vectors.Length == 0)
			{
				return fallback;
			}
			Vector3 vector = vectors[0];
			for (int i = 1; i < vectors.Length; i++)
			{
				vector += vectors[i];
			}
			return vector / vectors.Length;
		}

		public static T CreateGameObject<T>(Transform parent, string name) where T : MonoBehaviour
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			return gameObject.AddComponent<T>();
		}
	}
}
