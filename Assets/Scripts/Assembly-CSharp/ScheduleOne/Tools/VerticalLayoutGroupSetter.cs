using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.Tools
{
	public class VerticalLayoutGroupSetter : MonoBehaviour
	{
		public float LeftSpacing;

		private VerticalLayoutGroup layoutGroup;

		private void Awake()
		{
			layoutGroup = GetComponent<VerticalLayoutGroup>();
		}

		public void Update()
		{
			if (layoutGroup.padding.left != (int)LeftSpacing)
			{
				layoutGroup.padding.left = (int)LeftSpacing;
				LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
			}
		}
	}
}
