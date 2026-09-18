using System.Collections.Generic;
using UnityEngine;

public class NavMeshCleaner : MonoBehaviour
{
	public List<Vector3> m_WalkablePoint = new List<Vector3>();

	public float m_Height = 1f;

	public float m_Offset;

	public int m_MidLayerCount = 3;
}
