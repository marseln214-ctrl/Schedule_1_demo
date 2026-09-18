using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.NPCs;
using ScheduleOne.UI.Relations;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.ContactsApp
{
	public class ContactsApp : App<ContactsApp>
	{
		[Serializable]
		public class RegionUI
		{
			public EMapRegion Region;

			public Button Button;

			public RectTransform Container;

			public List<NPC> npcs { get; set; } = new List<NPC>();

		}

		public EMapRegion SelectedRegion;

		private Dictionary<EMapRegion, RegionUI> RegionDict = new Dictionary<EMapRegion, RegionUI>();

		[Header("References")]
		public RectTransform CirclesContainer;

		public RectTransform DemoCirclesContainer;

		public RectTransform TutorialCirclesContainer;

		public RectTransform ConnectionsContainer;

		public RectTransform ContentRect;

		public RectTransform SelectionIndicator;

		public ContactsDetailPanel DetailPanel;

		public RegionUI[] RegionUIs;

		public RectTransform RegionSelectionContainer;

		public RectTransform RegionSelectionIndicator;

		public RectTransform LockedRegionContainer;

		public Text RegionRankRequirementLabel;

		[Header("Prefabs")]
		public GameObject ConnectionPrefab;

		private List<RelationCircle> RelationCircles = new List<RelationCircle>();

		private Coroutine contentMoveRoutine;

		private List<Tuple<NPC, NPC>> connections = new List<Tuple<NPC, NPC>>();

		protected override void Start()
		{
			base.Start();
			if (NetworkSingleton<GameManager>.Instance.IsTutorial)
			{
				CirclesContainer.gameObject.SetActive(value: false);
				DemoCirclesContainer.gameObject.SetActive(value: false);
				TutorialCirclesContainer.gameObject.SetActive(value: true);
				CirclesContainer = TutorialCirclesContainer;
				RegionSelectionContainer.gameObject.SetActive(value: false);
			}
			else
			{
				CirclesContainer.gameObject.SetActive(value: false);
				TutorialCirclesContainer.gameObject.SetActive(value: false);
				DemoCirclesContainer.gameObject.SetActive(value: true);
				CirclesContainer = DemoCirclesContainer;
				RegionSelectionContainer.gameObject.SetActive(value: false);
			}
			RelationCircles = CirclesContainer.GetComponentsInChildren<RelationCircle>().ToList();
			foreach (RelationCircle rel in RelationCircles)
			{
				rel.LoadNPCData();
				if (rel.AssignedNPC == null)
				{
					continue;
				}
				RegionUIs.First((RegionUI x) => x.Region == rel.AssignedNPC.Region).npcs.Add(rel.AssignedNPC);
				foreach (NPC other in rel.AssignedNPC.RelationData.Connections)
				{
					if (other == null || connections.Exists((Tuple<NPC, NPC> x) => x.Item1 == rel.AssignedNPC && x.Item2 == other) || connections.Exists((Tuple<NPC, NPC> x) => x.Item1 == other && x.Item2 == rel.AssignedNPC))
					{
						continue;
					}
					connections.Add(new Tuple<NPC, NPC>(rel.AssignedNPC, other));
					RelationCircle otherCirc = GetRelationCircle(other.ID);
					if (otherCirc == null)
					{
						Console.LogWarning("Failed to find relation circle for NPC with ID '" + other.ID + "' for " + rel.AssignedNPC.fullName);
						continue;
					}
					RectTransform component = UnityEngine.Object.Instantiate(ConnectionPrefab, ConnectionsContainer).GetComponent<RectTransform>();
					component.anchoredPosition = (otherCirc.Rect.anchoredPosition + rel.Rect.anchoredPosition) / 2f;
					Vector3 vector = otherCirc.Rect.anchoredPosition - rel.Rect.anchoredPosition;
					float z = (0f - Mathf.Atan2(vector.x, vector.y)) * 57.29578f;
					component.localRotation = Quaternion.Euler(0f, 0f, z);
					component.sizeDelta = new Vector2(component.sizeDelta.x, Vector3.Distance(otherCirc.Rect.anchoredPosition, rel.Rect.anchoredPosition));
					RelationCircle cacheRel = rel;
					component.Find("StartButton").GetComponent<Button>().onClick.AddListener(delegate
					{
						ZoomToRect(otherCirc.Rect);
					});
					component.Find("EndButton").GetComponent<Button>().onClick.AddListener(delegate
					{
						ZoomToRect(cacheRel.Rect);
					});
				}
			}
			foreach (RelationCircle relationCircle in RelationCircles)
			{
				RelationCircle circ = relationCircle;
				relationCircle.onClicked = (Action)Delegate.Combine(relationCircle.onClicked, (Action)delegate
				{
					CircleClicked(circ);
				});
			}
			if (RelationCircles.Count > 0)
			{
				Select(RelationCircles[0]);
			}
		}

		protected override void Update()
		{
			base.Update();
			if (base.isOpen && GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && contentMoveRoutine != null)
			{
				StopContentMove();
			}
		}

		private RelationCircle GetRelationCircle(string npcID)
		{
			return RelationCircles.Find((RelationCircle x) => x.AssignedNPC_ID.ToLower() == npcID.ToLower());
		}

		private void CircleClicked(RelationCircle circ)
		{
			Select(circ);
		}

		private void Select(RelationCircle circ)
		{
			DetailPanel.Open(circ.AssignedNPC);
			ZoomToRect(circ.Rect);
			SelectionIndicator.position = circ.Rect.position;
		}

		public void SetSelectedRegion(EMapRegion region)
		{
		}

		private void ZoomToRect(RectTransform rect)
		{
			ContentRect.pivot = new Vector2(0f, 1f);
			float startScale = ContentRect.localScale.x;
			float endScale = 1f;
			Vector2 endPos = new Vector2((0f - ContentRect.sizeDelta.x) / 2f, ContentRect.sizeDelta.y / 2f);
			endPos.x -= rect.anchoredPosition.x;
			endPos.y -= rect.anchoredPosition.y;
			StopContentMove();
			ContentRect.localScale = new Vector3(endScale, endScale, endScale);
			ContentRect.anchoredPosition = endPos;
		}

		private void StopContentMove()
		{
			if (contentMoveRoutine != null)
			{
				StopCoroutine(contentMoveRoutine);
			}
		}

		public override void SetOpen(bool open)
		{
			base.SetOpen(open);
			if (NetworkSingleton<VariableDatabase>.InstanceExists)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Contacts_App_Open", open.ToString(), network: false);
			}
			if (open)
			{
				DetailPanel.Open(DetailPanel.SelectedNPC);
			}
		}
	}
}
