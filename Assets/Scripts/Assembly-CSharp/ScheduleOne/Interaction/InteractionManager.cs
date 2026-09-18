using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.ConstructableScripts;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dragging;
using ScheduleOne.EntityFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Storage;
using ScheduleOne.UI;
using ScheduleOne.UI.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ScheduleOne.Interaction
{
	public class InteractionManager : Singleton<InteractionManager>
	{
		public const float RayRadius = 0.075f;

		public const float MaxInteractionRange = 5f;

		[SerializeField]
		protected LayerMask interaction_SearchMask;

		[SerializeField]
		protected float rightClickRange = 5f;

		public EInteractionSearchType interactionSearchType;

		public bool DEBUG;

		[Header("Visuals Settings")]
		public Color messageColor_Default;

		public Color iconColor_Default;

		public Color iconColor_Default_Key;

		public Color messageColor_Invalid;

		public Color iconColor_Invalid;

		public Sprite icon_Key;

		public Sprite icon_LeftMouse;

		public Sprite icon_Cross;

		public float displaySizeMultiplier = 1f;

		[Header("References")]
		[SerializeField]
		protected Canvas interaction_Canvas;

		[SerializeField]
		protected RectTransform interactionDisplay_Container;

		[SerializeField]
		protected Image interactionDisplay_Icon;

		[SerializeField]
		protected Text interactionDisplay_IconText;

		[SerializeField]
		protected Text interactionDisplay_MessageText;

		public RectTransform wsLabelContainer;

		[SerializeField]
		protected InputActionReference InteractInput;

		[HideInInspector]
		public string InteractKey = string.Empty;

		[SerializeField]
		protected RectTransform backgroundImage;

		[Header("Prefabs")]
		public GameObject WSLabelPrefab;

		private bool interactionDisplayEnabledThisFrame;

		private BuildableItem itemBeingDestroyed;

		private Pallet palletBeingDestroyed;

		private Constructable constructableBeingDestroyed;

		private float destroyTime;

		private float tempDisplayScale = 0.75f;

		public static float interactCooldown = 0.1f;

		private float timeSinceLastInteractStart;

		public List<WorldSpaceLabel> activeWSlabels = new List<WorldSpaceLabel>();

		private static float timeToDestroy = 0.5f;

		private Coroutine ILerpDisplayScale_Coroutine;

		public LayerMask Interaction_SearchMask => interaction_SearchMask;

		public bool CanDestroy { get; set; } = true;


		public InteractableObject hoveredInteractableObject { get; protected set; }

		public InteractableObject hoveredValidInteractableObject { get; protected set; }

		public InteractableObject interactedObject { get; protected set; }

		protected override void Start()
		{
			base.Start();
			LoadInteractKey();
			Settings settings = Singleton<Settings>.Instance;
			settings.onInputsApplied = (Action)Delegate.Remove(settings.onInputsApplied, new Action(LoadInteractKey));
			Settings settings2 = Singleton<Settings>.Instance;
			settings2.onInputsApplied = (Action)Delegate.Combine(settings2.onInputsApplied, new Action(LoadInteractKey));
		}

		protected override void OnDestroy()
		{
			if (Singleton<Settings>.InstanceExists)
			{
				Settings settings = Singleton<Settings>.Instance;
				settings.onInputsApplied = (Action)Delegate.Remove(settings.onInputsApplied, new Action(LoadInteractKey));
			}
			base.OnDestroy();
		}

		private void LoadInteractKey()
		{
			InteractInput.action.GetBindingDisplayString(0, out var _, out var controlPath);
			InteractKey = Singleton<InputPromptsManager>.Instance.GetDisplayNameForControlPath(controlPath);
		}

		protected virtual void Update()
		{
			timeSinceLastInteractStart += Time.deltaTime;
			if (Singleton<GameInput>.InstanceExists)
			{
				CheckRightClick();
			}
		}

		protected virtual void LateUpdate()
		{
			if (Singleton<GameInput>.InstanceExists)
			{
				CheckHover();
				CheckInteraction();
				interactionDisplayEnabledThisFrame = false;
				CheckHover();
				interaction_Canvas.enabled = interactionDisplayEnabledThisFrame || activeWSlabels.Count > 0;
				interactionDisplay_Container.gameObject.SetActive(interactionDisplayEnabledThisFrame);
				if (!interactionDisplayEnabledThisFrame)
				{
					tempDisplayScale = 0.75f;
				}
				for (int i = 0; i < activeWSlabels.Count; i++)
				{
					activeWSlabels[i].RefreshDisplay();
				}
				interactionDisplayEnabledThisFrame = false;
			}
		}

		protected virtual void CheckHover()
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				hoveredInteractableObject = null;
				return;
			}
			if (Singleton<TaskManager>.InstanceExists && Singleton<TaskManager>.Instance.currentTask != null)
			{
				hoveredInteractableObject = null;
				return;
			}
			if (Singleton<ObjectSelector>.InstanceExists && Singleton<ObjectSelector>.Instance.isSelecting)
			{
				hoveredInteractableObject = null;
				return;
			}
			if (Singleton<GameplayMenu>.InstanceExists && Singleton<GameplayMenu>.Instance.IsOpen)
			{
				hoveredInteractableObject = null;
				return;
			}
			if (PlayerSingleton<PlayerMovement>.Instance.currentVehicle != null)
			{
				hoveredInteractableObject = null;
				return;
			}
			if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
			{
				hoveredInteractableObject = null;
				return;
			}
			if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && PlayerSingleton<PlayerInventory>.Instance.equippable != null && !PlayerSingleton<PlayerInventory>.Instance.equippable.CanInteractWhenEquipped)
			{
				hoveredInteractableObject = null;
				return;
			}
			if (Player.Local.IsSkating)
			{
				hoveredInteractableObject = null;
				return;
			}
			if (Singleton<PauseMenu>.Instance.IsPaused)
			{
				hoveredInteractableObject = null;
				return;
			}
			if (NetworkSingleton<DragManager>.Instance.IsDragging)
			{
				hoveredInteractableObject = null;
				return;
			}
			Ray ray = default(Ray);
			switch (interactionSearchType)
			{
			case EInteractionSearchType.CameraForward:
				ray.origin = PlayerSingleton<PlayerCamera>.Instance.transform.position;
				ray.direction = PlayerSingleton<PlayerCamera>.Instance.transform.forward;
				break;
			case EInteractionSearchType.Mouse:
				ray = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(Input.mousePosition);
				break;
			default:
				Console.LogWarning("EInteractionSearchType type not accounted for");
				return;
			}
			InteractableObject interactableObject = hoveredInteractableObject;
			hoveredInteractableObject = null;
			RaycastHit[] array = Physics.SphereCastAll(ray, 0.075f, 5f, interaction_SearchMask, QueryTriggerInteraction.Collide);
			RaycastHit[] array2 = Physics.RaycastAll(ray, 5f, interaction_SearchMask, QueryTriggerInteraction.Collide);
			if (array.Length != 0)
			{
				Array.Sort(array, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
				List<InteractableObject> list = new List<InteractableObject>();
				Dictionary<InteractableObject, RaycastHit> objectHits = new Dictionary<InteractableObject, RaycastHit>();
				for (int i = 0; i < array.Length; i++)
				{
					RaycastHit value = array[i];
					InteractableObject componentInParent = value.collider.GetComponentInParent<InteractableObject>();
					if (componentInParent == null)
					{
						bool flag = false;
						RaycastHit[] array3 = array2;
						foreach (RaycastHit raycastHit in array3)
						{
							if (raycastHit.collider == value.collider)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
					else if (!list.Contains(componentInParent) && componentInParent != null && Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, value.point) <= componentInParent.MaxInteractionRange)
					{
						list.Add(componentInParent);
						objectHits.Add(componentInParent, value);
					}
				}
				list.Sort(delegate(InteractableObject x, InteractableObject y)
				{
					int num = y.Priority.CompareTo(x.Priority);
					return (num == 0) ? objectHits[x].distance.CompareTo(objectHits[y].distance) : num;
				});
				for (int k = 0; k < list.Count; k++)
				{
					RaycastHit raycastHit2 = objectHits[list[k]];
					InteractableObject interactableObject2 = list[k];
					if (interactableObject2 == null)
					{
						bool flag2 = false;
						RaycastHit[] array3 = array2;
						foreach (RaycastHit raycastHit3 in array3)
						{
							if (raycastHit3.collider == raycastHit2.collider)
							{
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							break;
						}
						continue;
					}
					if (!interactableObject2.CheckAngleLimit(ray.origin))
					{
						interactableObject2 = null;
					}
					if (interactableObject2 != null && !interactableObject2.enabled)
					{
						interactableObject2 = null;
					}
					if (interactableObject2 != null && Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, raycastHit2.point) <= interactableObject2.MaxInteractionRange)
					{
						hoveredInteractableObject = interactableObject2;
						hoveredInteractableObject.Hovered();
						if (interactableObject2 != interactableObject)
						{
							tempDisplayScale = 1f;
						}
						break;
					}
				}
			}
			if (DEBUG)
			{
				Debug.Log("Hovered interactable object: " + hoveredInteractableObject?.name);
			}
		}

		protected virtual void CheckInteraction()
		{
			hoveredValidInteractableObject = null;
			if (interactedObject != null && ((interactedObject._interactionType == InteractableObject.EInteractionType.Key_Press && !GameInput.GetButton(GameInput.ButtonCode.Interact)) || (interactedObject._interactionType == InteractableObject.EInteractionType.LeftMouse_Click && !GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))))
			{
				interactedObject.EndInteract();
				interactedObject = null;
			}
			if (!(hoveredInteractableObject == null) && hoveredInteractableObject._interactionState != InteractableObject.EInteractableState.Disabled)
			{
				hoveredValidInteractableObject = hoveredInteractableObject;
				if (GameInput.GetButton(GameInput.ButtonCode.Interact) && timeSinceLastInteractStart >= interactCooldown && hoveredInteractableObject._interactionType == InteractableObject.EInteractionType.Key_Press && (!hoveredInteractableObject.RequiresUniqueClick || GameInput.GetButtonDown(GameInput.ButtonCode.Interact)))
				{
					timeSinceLastInteractStart = 0f;
					hoveredInteractableObject.StartInteract();
					interactedObject = hoveredInteractableObject;
				}
				if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && timeSinceLastInteractStart >= interactCooldown && hoveredInteractableObject._interactionType == InteractableObject.EInteractionType.LeftMouse_Click && (!hoveredInteractableObject.RequiresUniqueClick || GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick)))
				{
					timeSinceLastInteractStart = 0f;
					hoveredInteractableObject.StartInteract();
					interactedObject = hoveredInteractableObject;
				}
			}
		}

		protected virtual void CheckRightClick()
		{
			bool flag = false;
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				return;
			}
			if (Singleton<TaskManager>.Instance.currentTask == null && !PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount == 0 && CanDestroy && GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
			{
				BuildableItem hoveredBuildableItem = GetHoveredBuildableItem();
				GetHoveredPallet();
				GetHoveredConstructable();
				if (hoveredBuildableItem != null)
				{
					if (hoveredBuildableItem.CanBePickedUp(out var reason))
					{
						if (itemBeingDestroyed == hoveredBuildableItem)
						{
							destroyTime += Time.deltaTime;
						}
						itemBeingDestroyed = hoveredBuildableItem;
						if (destroyTime >= timeToDestroy)
						{
							itemBeingDestroyed.PickupItem();
							destroyTime = 0f;
						}
						flag = true;
						Singleton<HUD>.Instance.ShowRadialIndicator(destroyTime / timeToDestroy);
					}
					else
					{
						Singleton<HUD>.Instance.CrosshairText.Show(reason, new Color32(byte.MaxValue, 100, 100, byte.MaxValue));
					}
				}
			}
			if (!flag)
			{
				destroyTime = 0f;
			}
		}

		protected virtual BuildableItem GetHoveredBuildableItem()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(rightClickRange, out var hit, 1 << LayerMask.NameToLayer("Default")))
			{
				return hit.collider.GetComponentInParent<BuildableItem>();
			}
			return null;
		}

		protected virtual Pallet GetHoveredPallet()
		{
			LayerMask layerMask = (int)default(LayerMask) | (1 << LayerMask.NameToLayer("Default"));
			layerMask = (int)layerMask | (1 << LayerMask.NameToLayer("Pallet"));
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(rightClickRange, out var hit, layerMask))
			{
				return hit.collider.GetComponentInParent<Pallet>();
			}
			return null;
		}

		protected virtual Constructable GetHoveredConstructable()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(rightClickRange, out var hit, 1 << LayerMask.NameToLayer("Default")))
			{
				return hit.collider.GetComponentInParent<Constructable>();
			}
			return null;
		}

		public void SetCanDestroy(bool canDestroy)
		{
			CanDestroy = canDestroy;
		}

		public void EnableInteractionDisplay(Vector3 pos, Sprite icon, string spriteText, string message, Color messageColor, Color iconColor)
		{
			interactionDisplayEnabledThisFrame = true;
			interactionDisplay_Container.position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(pos);
			interactionDisplay_Icon.gameObject.SetActive(icon != null);
			interactionDisplay_Icon.sprite = icon;
			interactionDisplay_Icon.color = iconColor;
			interactionDisplay_IconText.enabled = spriteText != string.Empty;
			interactionDisplay_IconText.text = spriteText.ToUpper();
			interactionDisplay_MessageText.text = message;
			interactionDisplay_MessageText.color = messageColor;
			interactionDisplay_Container.sizeDelta = new Vector2(60f + interactionDisplay_MessageText.preferredWidth, interactionDisplay_Container.sizeDelta.y);
			backgroundImage.sizeDelta = new Vector2(interactionDisplay_MessageText.preferredWidth + 180f, 140f);
			float num = Mathf.Clamp(1f / Vector3.Distance(pos, PlayerSingleton<PlayerCamera>.Instance.transform.position), 0f, 1f) * tempDisplayScale * displaySizeMultiplier;
			interactionDisplay_Container.localScale = new Vector3(num, num, 1f);
		}

		public void LerpDisplayScale(float endScale)
		{
			if (ILerpDisplayScale_Coroutine != null)
			{
				StopCoroutine(ILerpDisplayScale_Coroutine);
			}
			ILerpDisplayScale_Coroutine = StartCoroutine(ILerpDisplayScale(tempDisplayScale, endScale));
		}

		protected IEnumerator ILerpDisplayScale(float startScale, float endScale)
		{
			float lerpTime = Mathf.Abs(startScale - endScale) * 0.75f;
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				tempDisplayScale = Mathf.Lerp(startScale, endScale, i / lerpTime);
				yield return new WaitForEndOfFrame();
			}
			tempDisplayScale = endScale;
			ILerpDisplayScale_Coroutine = null;
		}
	}
}
