using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product;
using TMPro;
using UnityEngine;

namespace ScheduleOne.Packaging
{
	public class PackagingTool : MonoBehaviour
	{
		public class PackagingInstance
		{
			public Transform Container;

			public Rigidbody ContainerRb;

			public FunctionalPackaging Packaging;

			public float AnglePosition;

			public void ChangePosition(float angleDelta)
			{
				AnglePosition += angleDelta;
				AnglePosition = Mathf.Repeat(AnglePosition, 360f);
				Quaternion quaternion = Quaternion.Euler(0f, 0f - AnglePosition, 0f);
				Quaternion rot = Container.parent.rotation * quaternion;
				ContainerRb.MoveRotation(rot);
			}
		}

		private const float FinalizeRange_Min = 255f;

		private const float FinalizeRange_Max = 270f;

		[Header("Settings")]
		public float ConveyorSpeed = 1f;

		public float ConveyorAcceleration = 1f;

		public float BaggieRadius = 0.3f;

		public float JarRadius = 0.35f;

		public float DeployAngle = 60f;

		public float ProductInitialForce = 0.2f;

		public float ProductRandomTorque = 0.5f;

		public float KickForce = 1f;

		public float DropCooldown = 0.25f;

		[Header("References")]
		public PackagingStation Station;

		public Transform ConveyorModel;

		public Animation DoorAnim;

		public Animation CapAnim;

		public Animation SealAnim;

		public Animation KickAnim;

		public Clickable LeftButton;

		public Clickable RightButton;

		public Clickable DropButton;

		public Transform PackagingContainer;

		public TextMeshPro ProductCountText;

		public Transform HopperDropPoint;

		public Transform BaggieStartPoint;

		public Transform JarStartPoint;

		public Transform ProductContainer;

		public Transform KickOrigin;

		public SphereCollider HopperInputCollider;

		public AudioSourceController KickSound;

		public AudioSourceController MotorSound;

		public AudioSourceController DropSound;

		private FunctionalPackaging PackagingPrefab;

		private int ConcealedPackaging;

		private ProductItemInstance ProductItem;

		private FunctionalProduct ProductPrefab;

		private int ProductInHopper;

		private List<PackagingInstance> PackagingInstances = new List<PackagingInstance>();

		private List<FunctionalProduct> ProductInstances = new List<FunctionalProduct>();

		private List<FunctionalPackaging> FinalizedPackaging = new List<FunctionalPackaging>();

		private float conveyorVelocity;

		private int directionInput;

		private Task task;

		private PackagingInstance finalizeInstance;

		private Coroutine finalizeCoroutine;

		private bool leftDown;

		private bool rightDown;

		private bool dropDown;

		private float timeSinceLastDrop = 10f;

		public bool ReceiveInput { get; private set; }

		public void Initialize(Task _task, FunctionalPackaging packaging, int packagingQuantity, ProductItemInstance product, int productQuantity)
		{
			task = _task;
			ReceiveInput = true;
			LeftButton.ClickableEnabled = true;
			RightButton.ClickableEnabled = true;
			DropButton.ClickableEnabled = true;
			LoadPackaging(packaging, packagingQuantity);
			LoadProduct(product, productQuantity);
			int num = Mathf.RoundToInt(180f / DeployAngle);
			for (int i = 0; i < num; i++)
			{
				CheckDeployPackaging();
				Rotate(DeployAngle);
			}
		}

		public void Deinitialize()
		{
			ReceiveInput = false;
			if (LeftButton.IsHeld)
			{
				task.ForceEndClick(LeftButton);
			}
			if (RightButton.IsHeld)
			{
				task.ForceEndClick(RightButton);
			}
			if (DropButton.IsHeld)
			{
				task.ForceEndClick(DropButton);
			}
			LeftButton.ClickableEnabled = false;
			RightButton.ClickableEnabled = false;
			DropButton.ClickableEnabled = false;
			for (int i = 0; i < ProductInstances.Count; i++)
			{
				Object.Destroy(ProductInstances[i].gameObject);
			}
			ProductInstances.Clear();
			for (int j = 0; j < PackagingInstances.Count; j++)
			{
				Object.Destroy(PackagingInstances[j].Container.gameObject);
			}
			PackagingInstances.Clear();
			for (int k = 0; k < FinalizedPackaging.Count; k++)
			{
				Object.Destroy(FinalizedPackaging[k].gameObject);
			}
			FinalizedPackaging.Clear();
			if (finalizeCoroutine != null)
			{
				StopCoroutine(finalizeCoroutine);
				finalizeCoroutine = null;
			}
			UnloadPackaging();
			UnloadProduct();
			task = null;
		}

		private void LoadPackaging(FunctionalPackaging prefab, int quantity)
		{
			PackagingPrefab = prefab;
			ConcealedPackaging = quantity;
		}

		private void UnloadPackaging()
		{
			PackagingPrefab = null;
			ConcealedPackaging = 0;
		}

		private void LoadProduct(ProductItemInstance product, int quantity)
		{
			ProductItem = product;
			ProductPrefab = (product.Definition as ProductDefinition).FunctionalProduct;
			ProductInHopper = quantity;
			UpdateScreen();
		}

		private void UnloadProduct()
		{
			ProductPrefab = null;
			ProductInHopper = 0;
			UpdateScreen();
		}

		public void Update()
		{
			timeSinceLastDrop += Time.deltaTime;
			UpdateInput();
			UpdateConveyor();
			if (ConcealedPackaging > 0)
			{
				CheckDeployPackaging();
			}
			if (DropButton.IsHeld && ProductInHopper > 0 && timeSinceLastDrop > DropCooldown)
			{
				DropProduct();
			}
			if (Mathf.Abs(conveyorVelocity) > 0f && !MotorSound.isPlaying)
			{
				MotorSound.Play();
			}
			MotorSound.VolumeMultiplier = Mathf.Abs(conveyorVelocity);
			MotorSound.PitchMultiplier = Mathf.Lerp(0.7f, 1f, Mathf.Abs(conveyorVelocity));
			if (MotorSound.VolumeMultiplier <= 0f)
			{
				MotorSound.Stop();
			}
			else if (MotorSound.VolumeMultiplier > 0f && !MotorSound.isPlaying)
			{
				MotorSound.Play();
			}
			CheckFinalize();
			CheckInsertions();
		}

		private void UpdateInput()
		{
			directionInput = 0;
			if (!ReceiveInput)
			{
				return;
			}
			if (GameInput.GetButton(GameInput.ButtonCode.Left))
			{
				if (!LeftButton.IsHeld)
				{
					leftDown = true;
					task.ForceStartClick(LeftButton);
				}
			}
			else if (leftDown)
			{
				leftDown = false;
				task.ForceEndClick(LeftButton);
			}
			if (GameInput.GetButton(GameInput.ButtonCode.Right))
			{
				if (!RightButton.IsHeld)
				{
					rightDown = true;
					task.ForceStartClick(RightButton);
				}
			}
			else if (rightDown)
			{
				rightDown = false;
				task.ForceEndClick(RightButton);
			}
			if (GameInput.GetButton(GameInput.ButtonCode.Jump))
			{
				if (!DropButton.IsHeld)
				{
					dropDown = true;
					task.ForceStartClick(DropButton);
				}
			}
			else if (dropDown)
			{
				dropDown = false;
				task.ForceEndClick(DropButton);
			}
			if (LeftButton.IsHeld)
			{
				directionInput--;
			}
			if (RightButton.IsHeld)
			{
				directionInput++;
			}
		}

		private void UpdateScreen()
		{
			ProductCountText.text = ProductInHopper.ToString();
			ProductCountText.gameObject.SetActive(ProductInHopper > 0);
		}

		private void UpdateConveyor()
		{
			float num = Mathf.MoveTowards(conveyorVelocity, directionInput, ConveyorAcceleration * Time.deltaTime);
			conveyorVelocity = num;
			Rotate(conveyorVelocity * ConveyorSpeed * Time.deltaTime);
		}

		private void Rotate(float angle)
		{
			ConveyorModel.Rotate(Vector3.forward, 0f - angle);
			for (int i = 0; i < PackagingInstances.Count; i++)
			{
				PackagingInstances[i].ChangePosition(angle);
			}
			PackagingInstances.Sort((PackagingInstance a, PackagingInstance b) => a.AnglePosition.CompareTo(b.AnglePosition));
		}

		private void CheckDeployPackaging()
		{
			if (PackagingInstances.Count <= 0 || (!(PackagingInstances[0].AnglePosition < DeployAngle) && !(PackagingInstances[PackagingInstances.Count - 1].AnglePosition > 360f - DeployAngle)))
			{
				DeployPackaging();
			}
		}

		private void CheckFinalize()
		{
			if (finalizeCoroutine != null)
			{
				return;
			}
			for (int i = 0; i < PackagingInstances.Count; i++)
			{
				if (PackagingInstances[i].Packaging.IsFull && PackagingInstances[i].AnglePosition > 255f && PackagingInstances[i].AnglePosition < 270f)
				{
					Finalize(PackagingInstances[i]);
					break;
				}
			}
		}

		private void Finalize(PackagingInstance instance)
		{
			finalizeInstance = instance;
			finalizeCoroutine = StartCoroutine(FinalizeRoutine());
			IEnumerator FinalizeRoutine()
			{
				if (instance.Packaging is FunctionalBaggie)
				{
					SealAnim.Play();
				}
				else
				{
					CapAnim.Play();
				}
				Station.SetHatchOpen(open: true);
				yield return new WaitForSeconds(0.15f);
				instance.Packaging.Seal();
				yield return new WaitForSeconds(0.05f);
				KickAnim.Play();
				KickSound.Play();
				yield return new WaitForSeconds(0.05f);
				instance.Packaging.transform.SetParent(ProductContainer);
				instance.Packaging.Rb.isKinematic = false;
				instance.Packaging.NormalRBDrag = 0f;
				instance.Packaging.Rb.AddForceAtPosition(KickOrigin.forward * KickForce, KickOrigin.position, ForceMode.VelocityChange);
				instance.Packaging.Rb.AddTorque(Random.insideUnitSphere * KickForce, ForceMode.VelocityChange);
				Station.PackSingleInstance();
				Object.Destroy(instance.Container.gameObject);
				PackagingInstances.Remove(instance);
				FinalizedPackaging.Add(instance.Packaging);
				finalizeCoroutine = null;
				if (ConcealedPackaging + PackagingInstances.Count == 0)
				{
					task.Success();
				}
			}
		}

		private void DropProduct()
		{
			if (ProductInHopper > 0)
			{
				timeSinceLastDrop = 0f;
				ProductInHopper--;
				UpdateScreen();
				DropSound.Play();
				FunctionalProduct functionalProduct = Object.Instantiate(ProductPrefab, HopperDropPoint.position, HopperDropPoint.rotation);
				functionalProduct.Initialize(ProductItem);
				functionalProduct.transform.SetParent(ProductContainer);
				functionalProduct.ClampZ = true;
				functionalProduct.DragProjectionMode = Draggable.EDragProjectionMode.FlatCameraForward;
				functionalProduct.Rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				functionalProduct.Rb.AddForce(Vector3.down * ProductInitialForce, ForceMode.VelocityChange);
				functionalProduct.Rb.AddTorque(Random.insideUnitSphere * ProductRandomTorque, ForceMode.VelocityChange);
				ProductInstances.Add(functionalProduct);
			}
		}

		private void CheckInsertions()
		{
			for (int i = 0; i < ProductInstances.Count; i++)
			{
				if (!(ProductInstances[i].Rb == null) && !ProductInstances[i].Rb.isKinematic && HopperInputCollider.bounds.Contains(ProductInstances[i].transform.position))
				{
					InsertIntoHopper(ProductInstances[i]);
					i--;
				}
			}
		}

		private void InsertIntoHopper(FunctionalProduct product)
		{
			ProductInHopper++;
			UpdateScreen();
			if (product.IsHeld)
			{
				task.ForceEndClick(product);
			}
			Object.Destroy(product.gameObject);
			ProductInstances.Remove(product);
		}

		private void DeployPackaging()
		{
			if (ConcealedPackaging > 0)
			{
				ConcealedPackaging--;
				GameObject gameObject = new GameObject("Packaging Container");
				gameObject.transform.SetParent(PackagingContainer);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				FunctionalPackaging functionalPackaging = Object.Instantiate(PackagingPrefab, gameObject.transform);
				functionalPackaging.AutoEnableSealing = false;
				functionalPackaging.Initialize(Station, null, align: false);
				functionalPackaging.Rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				if (functionalPackaging is FunctionalBaggie)
				{
					functionalPackaging.transform.position = BaggieStartPoint.position;
					functionalPackaging.transform.rotation = BaggieStartPoint.rotation;
				}
				else if (functionalPackaging is FunctionalJar)
				{
					functionalPackaging.transform.position = JarStartPoint.position;
					functionalPackaging.transform.rotation = JarStartPoint.rotation;
				}
				else
				{
					Console.LogError("Unknown packaging type!");
				}
				PackagingInstance packagingInstance = new PackagingInstance();
				packagingInstance.Container = gameObject.transform;
				packagingInstance.ContainerRb = gameObject.AddComponent<Rigidbody>();
				packagingInstance.ContainerRb.isKinematic = true;
				packagingInstance.ContainerRb.useGravity = false;
				packagingInstance.ContainerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				packagingInstance.Packaging = functionalPackaging;
				PackagingInstances.Insert(0, packagingInstance);
			}
		}
	}
}
