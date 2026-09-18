using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	[RequireComponent(typeof(EventTrigger))]
	public class PropagateDrag : MonoBehaviour
	{
		public ScrollRect ScrollView;

		private void Start()
		{
			if (ScrollView == null)
			{
				ScrollView = GetComponentInParent<ScrollRect>();
			}
			if (!(ScrollView == null))
			{
				EventTrigger component = GetComponent<EventTrigger>();
				EventTrigger.Entry entry = new EventTrigger.Entry();
				EventTrigger.Entry entry2 = new EventTrigger.Entry();
				EventTrigger.Entry entry3 = new EventTrigger.Entry();
				EventTrigger.Entry entry4 = new EventTrigger.Entry();
				EventTrigger.Entry entry5 = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.BeginDrag;
				entry.callback.AddListener(delegate(BaseEventData data)
				{
					ScrollView.OnBeginDrag((PointerEventData)data);
				});
				component.triggers.Add(entry);
				entry2.eventID = EventTriggerType.Drag;
				entry2.callback.AddListener(delegate(BaseEventData data)
				{
					ScrollView.OnDrag((PointerEventData)data);
				});
				component.triggers.Add(entry2);
				entry3.eventID = EventTriggerType.EndDrag;
				entry3.callback.AddListener(delegate(BaseEventData data)
				{
					ScrollView.OnEndDrag((PointerEventData)data);
				});
				component.triggers.Add(entry3);
				entry4.eventID = EventTriggerType.InitializePotentialDrag;
				entry4.callback.AddListener(delegate(BaseEventData data)
				{
					ScrollView.OnInitializePotentialDrag((PointerEventData)data);
				});
				component.triggers.Add(entry4);
				entry5.eventID = EventTriggerType.Scroll;
				entry5.callback.AddListener(delegate(BaseEventData data)
				{
					ScrollView.OnScroll((PointerEventData)data);
				});
				component.triggers.Add(entry5);
			}
		}
	}
}
