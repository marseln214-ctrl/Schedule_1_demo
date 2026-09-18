using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ItemIconCreator
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[DisallowMultipleComponent]
	public class IconCreator : MonoBehaviour
	{
		public enum SaveLocation
		{
			persistentDataPath = 0,
			dataPath = 1,
			projectFolder = 2,
			custom = 3
		}

		public enum Mode
		{
			Automatic = 0,
			Manual = 1
		}

		protected bool isCreatingIcons;

		public bool useDafaultName;

		public bool includeResolutionInFileName;

		public string iconFileName;

		public SaveLocation pathLocation;

		public Mode mode;

		public string folderName = "Screenshots";

		public bool useTransparency = true;

		public bool lookAtObjectCenter;

		public bool dynamicFov;

		public float fovOffset;

		protected string finalPath;

		private Vector3 mousePostion;

		public KeyCode nextIconKey = KeyCode.Space;

		protected bool CanMove;

		public bool preview = true;

		protected Camera whiteCam;

		protected Camera blackCam;

		public Camera mainCam;

		protected Texture2D texBlack;

		protected Texture2D texWhite;

		protected Texture2D finalTexture;

		private CameraClearFlags originalClearFlags;

		protected Transform currentObject;

		private void Awake()
		{
			mainCam = base.gameObject.GetComponent<Camera>();
			originalClearFlags = mainCam.clearFlags;
			if (IconCreatorCanvas.instance != null)
			{
				IconCreatorCanvas.instance.SetInfo(0, 0, "", isRecording: false, nextIconKey);
			}
		}

		protected void Initialize()
		{
			mainCam.clearFlags = originalClearFlags;
			isCreatingIcons = true;
			Camera[] array = Object.FindObjectsOfType<Camera>();
			foreach (Camera camera in array)
			{
				if (!(camera == mainCam))
				{
					camera.gameObject.SetActive(value: false);
				}
			}
			if (useTransparency)
			{
				CreateBlackAndWhiteCameras();
			}
			CacheAndInitialiseFields();
		}

		protected void DeleteCameras()
		{
			if (whiteCam != null)
			{
				Object.Destroy(whiteCam.gameObject);
			}
			if (blackCam != null)
			{
				Object.Destroy(blackCam.gameObject);
			}
			isCreatingIcons = false;
		}

		public virtual void BuildIcons()
		{
			Debug.LogError("Not implemented");
		}

		protected IEnumerator CaptureFrame(string objectName, int i)
		{
			if (whiteCam != null)
			{
				whiteCam.enabled = true;
			}
			if (blackCam != null)
			{
				blackCam.enabled = true;
			}
			yield return new WaitForEndOfFrame();
			if (useTransparency)
			{
				RenderCamToTexture(blackCam, texBlack);
				RenderCamToTexture(whiteCam, texWhite);
				CalculateOutputTexture();
			}
			else
			{
				RenderCamToTexture(mainCam, finalTexture);
			}
			SavePng(objectName, i);
			mainCam.enabled = true;
		}

		protected virtual void Update()
		{
			if (mode == Mode.Automatic || !CanMove)
			{
				return;
			}
			if (Input.GetMouseButtonDown(0))
			{
				mousePostion = Input.mousePosition;
			}
			if (Input.GetMouseButton(0))
			{
				Vector3 vector = mousePostion - Input.mousePosition;
				currentObject.Rotate(new Vector3(0f - vector.y, vector.x, vector.z) * Time.deltaTime * 40f, Space.World);
				mousePostion = Input.mousePosition;
				if (dynamicFov)
				{
					UpdateFOV(currentObject.gameObject);
				}
				if (lookAtObjectCenter)
				{
					LookAtTargetCenter(currentObject.gameObject);
				}
			}
			UpdateFOV(Input.mouseScrollDelta.y * -2.5f);
		}

		private void RenderCamToTexture(Camera cam, Texture2D tex)
		{
			cam.enabled = true;
			cam.Render();
			WriteScreenImageToTexture(tex);
			cam.enabled = false;
		}

		private void CreateBlackAndWhiteCameras()
		{
			mainCam.clearFlags = CameraClearFlags.Color;
			GameObject gameObject = new GameObject();
			gameObject.name = "White Background Camera";
			whiteCam = gameObject.AddComponent<Camera>();
			whiteCam.CopyFrom(mainCam);
			whiteCam.backgroundColor = Color.white;
			gameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: true);
			gameObject = new GameObject();
			gameObject.name = "Black Background Camera";
			blackCam = gameObject.AddComponent<Camera>();
			blackCam.CopyFrom(mainCam);
			blackCam.backgroundColor = Color.black;
			gameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: true);
		}

		protected void CreateNewFolderForIcons()
		{
			finalPath = GetFinalFolder();
			if (Directory.Exists(finalPath))
			{
				int num = 1;
				while (Directory.Exists(finalPath + " " + num))
				{
					num++;
				}
				finalPath = finalPath + " " + num;
			}
			Directory.CreateDirectory(finalPath);
		}

		public string GetFinalFolder()
		{
			if (!string.IsNullOrWhiteSpace(GetBaseLocation()))
			{
				return Path.Combine(GetBaseLocation(), folderName);
			}
			return folderName;
		}

		private void WriteScreenImageToTexture(Texture2D tex)
		{
			tex.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.width), 0, 0);
			tex.Apply();
		}

		private void CalculateOutputTexture()
		{
			for (int i = 0; i < finalTexture.height; i++)
			{
				for (int j = 0; j < finalTexture.width; j++)
				{
					float num = texWhite.GetPixel(j, i).r - texBlack.GetPixel(j, i).r;
					num = 1f - num;
					Color color = ((num != 0f) ? (texBlack.GetPixel(j, i) / num) : Color.clear);
					color.a = num;
					finalTexture.SetPixel(j, i, color);
				}
			}
		}

		private void SavePng(string name, int i)
		{
			string fileName = GetFileName(name, i);
			string text = finalPath + "/" + fileName;
			Debug.Log("Writing to: " + text);
			byte[] bytes = finalTexture.EncodeToPNG();
			File.WriteAllBytes(text, bytes);
		}

		public string GetFileName(string name, int i)
		{
			string text = ((!useDafaultName) ? iconFileName : name);
			text += " Icon";
			if (includeResolutionInFileName)
			{
				text = text + " " + mainCam.scaledPixelHeight + "x";
			}
			return text + ".png";
		}

		private void CacheAndInitialiseFields()
		{
			texBlack = new Texture2D(mainCam.pixelWidth, mainCam.pixelHeight, TextureFormat.RGB24, mipChain: false);
			texWhite = new Texture2D(mainCam.pixelWidth, mainCam.pixelHeight, TextureFormat.RGB24, mipChain: false);
			finalTexture = new Texture2D(mainCam.pixelWidth, mainCam.pixelHeight, TextureFormat.ARGB32, mipChain: false);
		}

		protected void UpdateFOV(GameObject targetItem)
		{
			float targetFov = GetTargetFov(targetItem);
			if (useTransparency && isCreatingIcons)
			{
				whiteCam.fieldOfView = targetFov;
				blackCam.fieldOfView = targetFov;
			}
			mainCam.fieldOfView = targetFov;
		}

		protected void UpdateFOV(float value)
		{
			if (value != 0f)
			{
				value = mainCam.fieldOfView * value / 100f;
				dynamicFov = false;
				if (useTransparency)
				{
					whiteCam.fieldOfView += value;
					blackCam.fieldOfView += value;
				}
				mainCam.fieldOfView += value;
			}
		}

		protected void LookAtTargetCenter(GameObject targetItem)
		{
			Vector3 meshCenter = GetMeshCenter(targetItem);
			mainCam.transform.LookAt(meshCenter);
			if (whiteCam != null)
			{
				whiteCam.transform.LookAt(meshCenter);
			}
			if (blackCam != null)
			{
				blackCam.transform.LookAt(meshCenter);
			}
		}

		private float GetTargetFov(GameObject a)
		{
			Vector3 vector = Vector3.one * 30000f;
			Vector3 vector2 = Vector3.zero;
			List<Renderer> renderers = GetRenderers(a);
			for (int i = 0; i < renderers.Count; i++)
			{
				if (Vector3.Distance(Vector3.zero, renderers[i].bounds.min) < Vector3.Distance(Vector3.zero, vector))
				{
					vector = renderers[i].bounds.min;
				}
				if (Vector3.Distance(Vector3.zero, renderers[i].bounds.max) > Vector3.Distance(Vector3.zero, vector2))
				{
					vector2 = renderers[i].bounds.max;
				}
			}
			Vector3 a2 = (vector + vector2) / 2f;
			float num = (vector2 - vector).magnitude / 2f;
			float num2 = Vector3.Distance(a2, base.transform.position);
			float num3 = Mathf.Sqrt(num * num + num2 * num2);
			return Mathf.Asin(num / num3) * 57.29578f * 2f + fovOffset;
		}

		private List<Renderer> GetRenderers(GameObject obj)
		{
			List<Renderer> list = new List<Renderer>();
			if (obj.GetComponents<Renderer>() != null)
			{
				list.AddRange(obj.GetComponents<Renderer>());
			}
			if (obj.GetComponentsInChildren<Renderer>() != null)
			{
				list.AddRange(obj.GetComponentsInChildren<Renderer>());
			}
			return list;
		}

		private Vector3 GetMeshCenter(GameObject a)
		{
			Vector3 zero = Vector3.zero;
			List<Renderer> renderers = GetRenderers(a);
			if (renderers == null)
			{
				Debug.LogError("No mesh was founded in object " + a.name);
				return a.transform.position;
			}
			for (int i = 0; i < renderers.Count; i++)
			{
				zero += renderers[i].bounds.center;
			}
			return zero / renderers.Count;
		}

		protected void RevealInFinder()
		{
		}

		public virtual bool CheckConditions()
		{
			if (pathLocation == SaveLocation.custom && !Directory.Exists(folderName))
			{
				Debug.LogError("Folder " + folderName + " does not exists");
				return false;
			}
			if (!useDafaultName && string.IsNullOrWhiteSpace(iconFileName))
			{
				Debug.LogError("Invalid icon file name");
				return false;
			}
			return true;
		}

		private string GetBaseLocation()
		{
			if (pathLocation == SaveLocation.dataPath)
			{
				return Application.dataPath;
			}
			if (pathLocation == SaveLocation.persistentDataPath)
			{
				return Application.persistentDataPath;
			}
			if (pathLocation == SaveLocation.projectFolder)
			{
				return Path.GetDirectoryName(Application.dataPath);
			}
			return "";
		}

		private void OnValidate()
		{
			if (mainCam == null)
			{
				mainCam = GetComponent<Camera>();
			}
		}
	}
}
