using System.Collections.Generic;
using EPOOutline;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Construction.Features;
using ScheduleOne.EntityFramework;
using UnityEngine;

namespace ScheduleOne.ConstructableScripts
{
	public class Constructable : NetworkBehaviour
	{
		[Header("Basic settings")]
		[SerializeField]
		protected bool isStatic;

		[SerializeField]
		protected string constructableName = "Constructable";

		[SerializeField]
		protected string constructableDescription = "Description";

		[SerializeField]
		protected string constructableAssetPath = string.Empty;

		[SerializeField]
		protected string ID = string.Empty;

		[SerializeField]
		protected Sprite constructableIcon;

		[Header("Bounds settings")]
		public BoxCollider boundingBox;

		[Header("Construction Handler")]
		[SerializeField]
		protected GameObject constructionHandler_Asset;

		[Header("Outline settings")]
		[SerializeField]
		protected List<GameObject> outlineRenderers = new List<GameObject>();

		protected Outlinable outlineEffect;

		[Header("Features")]
		public List<Feature> features = new List<Feature>();

		private bool isDestroyed;

		private Dictionary<Transform, LayerMask> originalLayers = new Dictionary<Transform, LayerMask>();

		private bool NetworkInitialize___EarlyScheduleOne_002EConstructableScripts_002EConstructableAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EConstructableScripts_002EConstructableAssembly_002DCSharp_002Edll_Excuted;

		public bool IsStatic => isStatic;

		public string ConstructableName => constructableName;

		public string ConstructableDescription => constructableDescription;

		public string ConstructableAssetPath => constructableAssetPath;

		public string PrefabID => ID;

		public Sprite ConstructableIcon => constructableIcon;

		public GameObject _constructionHandler_Asset => constructionHandler_Asset;

		public bool isVisible { get; protected set; } = true;


		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EConstructableScripts_002EConstructable_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void OnStartClient()
		{
			base.OnStartClient();
		}

		public virtual bool CanBeDestroyed(out string reason)
		{
			reason = string.Empty;
			return !isStatic;
		}

		public virtual bool CanBeDestroyed()
		{
			string reason;
			return CanBeDestroyed(out reason);
		}

		public virtual void DestroyConstructable(bool callOnServer = true)
		{
			if (!isDestroyed)
			{
				isDestroyed = true;
				Console.Log("Destroying constructable");
				if (callOnServer)
				{
					Destroy_Networked();
				}
				base.gameObject.SetActive(value: false);
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void Destroy_Networked()
		{
			RpcWriter___Server_Destroy_Networked_2166136261();
		}

		[ObserversRpc]
		private void DestroyConstructableWrapper()
		{
			RpcWriter___Observers_DestroyConstructableWrapper_2166136261();
		}

		public virtual bool CanBeModified()
		{
			return true;
		}

		public virtual bool CanBePickedUpByHand()
		{
			return false;
		}

		public virtual bool CanBeSelected()
		{
			return !isStatic;
		}

		public virtual string GetBuildableVersionAssetPath()
		{
			return string.Empty;
		}

		public void ShowOutline(BuildableItem.EOutlineColor color)
		{
			if (outlineEffect == null)
			{
				outlineEffect = base.gameObject.AddComponent<Outlinable>();
				outlineEffect.OutlineParameters.BlurShift = 0f;
				outlineEffect.OutlineParameters.DilateShift = 0.5f;
				outlineEffect.OutlineParameters.FillPass.Shader = Resources.Load<Shader>("Easy performant outline/Shaders/Fills/ColorFill");
				foreach (GameObject outlineRenderer in outlineRenderers)
				{
					MeshRenderer[] componentsInChildren = outlineRenderer.GetComponentsInChildren<MeshRenderer>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						OutlineTarget target = new OutlineTarget(componentsInChildren[i]);
						outlineEffect.TryAddTarget(target);
					}
				}
			}
			outlineEffect.OutlineParameters.Color = BuildableItem.GetColorFromOutlineColorEnum(color);
			Color32 colorFromOutlineColorEnum = BuildableItem.GetColorFromOutlineColorEnum(color);
			colorFromOutlineColorEnum.a = 9;
			outlineEffect.OutlineParameters.FillPass.SetColor("_PublicColor", colorFromOutlineColorEnum);
			outlineEffect.enabled = true;
		}

		public void HideOutline()
		{
			if (outlineEffect != null)
			{
				outlineEffect.enabled = false;
			}
		}

		public virtual Vector3 GetCosmeticCenter()
		{
			return base.transform.position;
		}

		public float GetBoundingBoxLongestSide()
		{
			return Mathf.Max(Mathf.Max(boundingBox.size.x, boundingBox.size.y), boundingBox.size.z);
		}

		public virtual void SetInvisible()
		{
			isVisible = false;
			SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Invisible"));
		}

		public virtual void RestoreVisibility()
		{
			isVisible = true;
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.gameObject.layer != LayerMask.NameToLayer("Grid"))
				{
					if (originalLayers.ContainsKey(transform))
					{
						transform.gameObject.layer = originalLayers[transform];
					}
					else
					{
						transform.gameObject.layer = LayerMask.NameToLayer("Default");
					}
				}
			}
		}

		public void SetLayerRecursively(GameObject go, int layerNumber)
		{
			Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.gameObject.layer == LayerMask.NameToLayer("Grid"))
				{
					continue;
				}
				if (transform.gameObject.layer != LayerMask.NameToLayer("Default"))
				{
					if (originalLayers.ContainsKey(transform))
					{
						originalLayers[transform] = transform.gameObject.layer;
					}
					else
					{
						originalLayers.Add(transform, transform.gameObject.layer);
					}
				}
				transform.gameObject.layer = layerNumber;
			}
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EConstructableScripts_002EConstructableAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EConstructableScripts_002EConstructableAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_Destroy_Networked_2166136261);
				RegisterObserversRpc(1u, RpcReader___Observers_DestroyConstructableWrapper_2166136261);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EConstructableScripts_002EConstructableAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EConstructableScripts_002EConstructableAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_Destroy_Networked_2166136261()
		{
			if (!base.IsClientInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___Destroy_Networked_2166136261()
		{
			Console.Log("Networked");
			DestroyConstructableWrapper();
			Despawn(DespawnType.Destroy);
		}

		private void RpcReader___Server_Destroy_Networked_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized)
			{
				RpcLogic___Destroy_Networked_2166136261();
			}
		}

		private void RpcWriter___Observers_DestroyConstructableWrapper_2166136261()
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___DestroyConstructableWrapper_2166136261()
		{
			Console.Log("Wrapper");
			DestroyConstructable(callOnServer: false);
		}

		private void RpcReader___Observers_DestroyConstructableWrapper_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___DestroyConstructableWrapper_2166136261();
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EConstructableScripts_002EConstructable_Assembly_002DCSharp_002Edll()
		{
			boundingBox.isTrigger = true;
			boundingBox.gameObject.layer = LayerMask.NameToLayer("Invisible");
			foreach (Feature feature in features)
			{
				_ = feature;
			}
		}
	}
}
