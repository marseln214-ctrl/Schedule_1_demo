using System.Collections.Generic;
using UnityEngine;

public class FrameTimingsHUDDisplay : MonoBehaviour
{
	public struct FrameTimingPoint
	{
		public double cpuFrameTime;

		public double cpuMainThreadFrameTime;

		public double cpuRenderThreadFrameTime;

		public double gpuFrameTime;
	}

	private GUIStyle m_Style;

	private readonly FrameTiming[] m_FrameTimings = new FrameTiming[1];

	public const int SAMPLE_SIZE = 200;

	public List<FrameTimingPoint> frameTimingsHistory = new List<FrameTimingPoint>();

	private void Awake()
	{
		m_Style = new GUIStyle();
		m_Style.fontSize = 15;
		m_Style.normal.textColor = Color.white;
	}

	private void OnGUI()
	{
		CaptureTimings();
		frameTimingsHistory.Add(new FrameTimingPoint
		{
			cpuFrameTime = m_FrameTimings[0].cpuFrameTime,
			cpuMainThreadFrameTime = m_FrameTimings[0].cpuMainThreadFrameTime,
			cpuRenderThreadFrameTime = m_FrameTimings[0].cpuRenderThreadFrameTime,
			gpuFrameTime = m_FrameTimings[0].gpuFrameTime
		});
		if (frameTimingsHistory.Count > 200)
		{
			frameTimingsHistory.RemoveAt(0);
		}
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		for (int i = 0; i < frameTimingsHistory.Count; i++)
		{
			num += frameTimingsHistory[i].cpuFrameTime;
			num2 += frameTimingsHistory[i].cpuMainThreadFrameTime;
			num3 += frameTimingsHistory[i].cpuRenderThreadFrameTime;
			num4 += frameTimingsHistory[i].gpuFrameTime;
		}
		num /= (double)frameTimingsHistory.Count;
		num2 /= (double)frameTimingsHistory.Count;
		num3 /= (double)frameTimingsHistory.Count;
		num4 /= (double)frameTimingsHistory.Count;
		string text = $"\nCPU: {num:00.00}" + $"\nMain Thread: {num2:00.00}" + $"\nRender Thread: {num3:00.00}" + $"\nGPU: {num4:00.00}";
		Color color = GUI.color;
		GUI.color = new Color(1f, 1f, 1f, 1f);
		float width = 300f;
		float height = 210f;
		GUILayout.BeginArea(new Rect(32f, 50f, width, height), "Frame Stats", GUI.skin.window);
		GUILayout.Label(text, m_Style);
		GUILayout.EndArea();
		GUI.color = color;
	}

	private void CaptureTimings()
	{
		FrameTimingManager.CaptureFrameTimings();
		FrameTimingManager.GetLatestTimings((uint)m_FrameTimings.Length, m_FrameTimings);
	}
}
