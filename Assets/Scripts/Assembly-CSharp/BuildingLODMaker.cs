using System;
using EasyButtons;
using UnityEngine;

public class BuildingLODMaker : MonoBehaviour
{
	[Serializable]
	public class LODGroupData
	{
		public string ObjectName;

		public GameObject LODObject;
	}

	public LODGroupData[] LODGroups;

	public LODGroup LodGroup;

	[Button]
	public void CreateLODs()
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			LODGroupData[] lODGroups = LODGroups;
			foreach (LODGroupData lODGroupData in lODGroups)
			{
				string text = transform.gameObject.name;
				if (text.Contains(" "))
				{
					text = text.Substring(0, text.IndexOf(" "));
				}
				if (text == lODGroupData.ObjectName)
				{
					GameObject obj = UnityEngine.Object.Instantiate(lODGroupData.LODObject, transform);
					obj.transform.localPosition = Vector3.zero;
					obj.transform.localRotation = Quaternion.identity;
					obj.transform.localScale = Vector3.one;
					Debug.Log("Created LOD object " + lODGroupData.ObjectName + " under " + transform.gameObject.name);
				}
			}
		}
	}
}
