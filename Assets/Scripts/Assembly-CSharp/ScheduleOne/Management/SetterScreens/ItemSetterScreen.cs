using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management.Presets.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.Management.SetterScreens
{
	public class ItemSetterScreen : Singleton<ItemSetterScreen>
	{
		private class Pair
		{
			public string prefabID;

			public RectTransform entry;
		}

		[Header("Prefabs")]
		public GameObject ListEntryPrefab;

		[Header("References")]
		public RectTransform EntryContainer;

		public TextMeshProUGUI TitleLabel;

		private RectTransform allEntry;

		private RectTransform noneEntry;

		private List<Pair> pairs = new List<Pair>();

		public ItemList Option { get; private set; }

		public bool IsOpen => Option != null;

		protected override void Awake()
		{
			base.Awake();
			allEntry = CreateEntry(null, "All", AllClicked);
			noneEntry = CreateEntry(null, "None", NoneClicked);
			GameInput.RegisterExitListener(Exit, 5);
			Close();
		}

		public virtual void Open(ItemList option)
		{
			Option = option;
			TitleLabel.text = Option.Name;
			base.gameObject.SetActive(value: true);
			allEntry.gameObject.SetActive(option.CanBeAll);
			noneEntry.gameObject.SetActive(option.CanBeNone);
			CreateEntries();
			RefreshTicks();
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && IsOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				Close();
			}
		}

		public virtual void Close()
		{
			Option = null;
			DestroyEntries();
			base.gameObject.SetActive(value: false);
		}

		private RectTransform CreateEntry(Sprite icon, string label, Action onClick, string prefabID = "", bool createPair = false)
		{
			RectTransform component = UnityEngine.Object.Instantiate(ListEntryPrefab, EntryContainer).GetComponent<RectTransform>();
			if (icon == null)
			{
				component.Find("Icon").gameObject.SetActive(value: false);
				component.Find("Title").GetComponent<RectTransform>().offsetMin = new Vector2(0.5f, 0f);
			}
			else
			{
				component.Find("Icon").GetComponent<Image>().sprite = icon;
			}
			component.Find("Title").GetComponent<TextMeshProUGUI>().text = label;
			component.GetComponent<Button>().onClick.AddListener(delegate
			{
				onClick();
			});
			if (createPair)
			{
				pairs.Add(new Pair
				{
					prefabID = prefabID,
					entry = component
				});
			}
			return component;
		}

		private void AllClicked()
		{
			Option.All = true;
			Option.None = false;
			RefreshTicks();
		}

		private void NoneClicked()
		{
			Option.All = false;
			Option.None = true;
			Option.Selection.Clear();
			RefreshTicks();
		}

		private void EntryClicked(string prefabID)
		{
			if (Option.All)
			{
				Option.Selection.Clear();
				Option.Selection.AddRange(Option.OptionList);
				Option.Selection.Remove(prefabID);
			}
			else if (Option.Selection.Contains(prefabID))
			{
				Option.Selection.Remove(prefabID);
			}
			else
			{
				Option.Selection.Add(prefabID);
			}
			Option.All = false;
			Option.None = false;
			RefreshTicks();
		}

		private void RefreshTicks()
		{
			SetEntryTicked(allEntry, ticked: false);
			SetEntryTicked(noneEntry, ticked: false);
			for (int j = 0; j < pairs.Count; j++)
			{
				SetEntryTicked(pairs[j].entry, ticked: false);
			}
			if (Option.All)
			{
				SetEntryTicked(allEntry, ticked: true);
				for (int k = 0; k < pairs.Count; k++)
				{
					SetEntryTicked(pairs[k].entry, ticked: true);
				}
				return;
			}
			if (Option.None || Option.Selection.Count == 0)
			{
				SetEntryTicked(noneEntry, ticked: true);
				return;
			}
			int i;
			for (i = 0; i < Option.Selection.Count; i++)
			{
				SetEntryTicked(pairs.Find((Pair x) => x.prefabID == Option.Selection[i]).entry, ticked: true);
			}
		}

		private void SetEntryTicked(RectTransform entry, bool ticked)
		{
			entry.Find("Tick").gameObject.SetActive(ticked);
		}

		private void CreateEntries()
		{
			for (int i = 0; i < Option.OptionList.Count; i++)
			{
				Console.Log(Option.OptionList[i]);
			}
		}

		private void DestroyEntries()
		{
			foreach (Pair pair in pairs)
			{
				UnityEngine.Object.Destroy(pair.entry.gameObject);
			}
			pairs.Clear();
		}
	}
}
