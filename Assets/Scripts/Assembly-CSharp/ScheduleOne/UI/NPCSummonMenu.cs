using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class NPCSummonMenu : Singleton<NPCSummonMenu>
	{
		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public RectTransform EntryContainer;

		public RectTransform[] Entries;

		private Action<NPC> callback;

		public bool IsOpen { get; private set; }

		protected override void Start()
		{
			base.Start();
			GameInput.RegisterExitListener(Exit, 5);
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
		}

		private void Exit(ExitAction exit)
		{
			if (IsOpen && !exit.used && exit.exitType == ExitType.Escape)
			{
				exit.used = true;
				Close();
			}
		}

		public void Open(List<NPC> npcs, Action<NPC> _callback)
		{
			IsOpen = true;
			Canvas.enabled = true;
			Container.gameObject.SetActive(value: true);
			callback = _callback;
			for (int i = 0; i < Entries.Length; i++)
			{
				if (npcs.Count > i)
				{
					Entries[i].Find("Icon").GetComponent<Image>().sprite = npcs[i].MugshotSprite;
					Entries[i].Find("Name").GetComponent<TextMeshProUGUI>().text = npcs[i].fullName;
					Entries[i].gameObject.SetActive(value: true);
					NPC npc = npcs[i];
					Entries[i].GetComponent<Button>().onClick.RemoveAllListeners();
					Entries[i].GetComponent<Button>().onClick.AddListener(delegate
					{
						NPCSelected(npc);
					});
				}
				else
				{
					Entries[i].gameObject.SetActive(value: false);
				}
			}
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		}

		public void Close()
		{
			IsOpen = false;
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
			callback = null;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		}

		public void NPCSelected(NPC npc)
		{
			callback(npc);
			Close();
		}
	}
}
