using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.StationFramework;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class StartChemistryStationBehaviour : Behaviour
	{
		public const float PLACE_INGREDIENTS_TIME = 8f;

		public const float STIR_TIME = 6f;

		public const float BURNER_TIME = 6f;

		private Chemist chemist;

		private Coroutine cookRoutine;

		private Beaker beaker;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public ChemistryStation targetStation { get; private set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviour_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public void SetTargetStation(ChemistryStation station)
		{
			targetStation = station;
		}

		protected override void End()
		{
			base.End();
			if (beaker != null)
			{
				beaker.Destroy();
				beaker = null;
			}
			if (targetStation != null)
			{
				targetStation.StaticBeaker.gameObject.SetActive(value: true);
			}
			if (cookRoutine != null)
			{
				StopCook();
			}
			Disable();
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (cookRoutine != null)
			{
				base.Npc.Avatar.LookController.OverrideLookTarget(targetStation.UIPoint.position, 5);
			}
			else if (InstanceFinder.IsServer && !base.Npc.Movement.IsMoving)
			{
				if (IsAtStation())
				{
					StartCook();
				}
				else
				{
					base.Npc.Movement.SetDestination(GetStationAccessPoint());
				}
			}
		}

		[ObserversRpc(RunLocally = true)]
		private void StartCook()
		{
			RpcWriter___Observers_StartCook_2166136261();
			RpcLogic___StartCook_2166136261();
		}

		private void SetupBeaker()
		{
			if (beaker != null)
			{
				Console.LogWarning("Beaker already exists!");
				return;
			}
			beaker = targetStation.CreateBeaker();
			targetStation.StaticBeaker.gameObject.SetActive(value: false);
		}

		private void FillBeaker(StationRecipe recipe, Beaker beaker)
		{
			for (int i = 0; i < recipe.Ingredients.Count; i++)
			{
				StorableItemDefinition storableItemDefinition = null;
				foreach (ItemDefinition item in recipe.Ingredients[i].Items)
				{
					StorableItemDefinition storableItemDefinition2 = item as StorableItemDefinition;
					for (int j = 0; j < targetStation.IngredientSlots.Length; j++)
					{
						if (targetStation.IngredientSlots[j].ItemInstance != null && targetStation.IngredientSlots[j].ItemInstance.Definition.ID == storableItemDefinition2.ID)
						{
							storableItemDefinition = storableItemDefinition2;
							break;
						}
					}
				}
				if (storableItemDefinition.StationItem == null)
				{
					Console.LogError("Ingredient '" + storableItemDefinition.Name + "' does not have a station item");
					continue;
				}
				StationItem stationItem = storableItemDefinition.StationItem;
				if (!stationItem.HasModule<IngredientModule>())
				{
					if (stationItem.HasModule<PourableModule>())
					{
						PourableModule module = stationItem.GetModule<PourableModule>();
						beaker.Fillable.AddLiquid(module.LiquidType, module.LiquidCapacity_L, module.LiquidColor);
					}
					else
					{
						Console.LogError("Ingredient '" + storableItemDefinition.Name + "' does not have an ingredient or pourable module");
					}
				}
			}
		}

		private bool CanCookStart()
		{
			if (targetStation == null)
			{
				return false;
			}
			if (((IUsable)targetStation).IsInUse && ((IUsable)targetStation).NPCUserObject != base.Npc.NetworkObject)
			{
				return false;
			}
			ChemistryStationConfiguration chemistryStationConfiguration = targetStation.Configuration as ChemistryStationConfiguration;
			if (chemistryStationConfiguration.Recipe.SelectedRecipe == null)
			{
				return false;
			}
			if (!targetStation.HasIngredientsForRecipe(chemistryStationConfiguration.Recipe.SelectedRecipe))
			{
				return false;
			}
			return true;
		}

		private void StopCook()
		{
			targetStation.SetNPCUser(null);
			base.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", value: false);
			if (cookRoutine != null)
			{
				StopCoroutine(cookRoutine);
				cookRoutine = null;
			}
			Debug.Log("Stopping cook");
		}

		private Vector3 GetStationAccessPoint()
		{
			if (targetStation == null)
			{
				return base.Npc.transform.position;
			}
			return ((ITransitEntity)targetStation).AccessPoints[0].position;
		}

		private bool IsAtStation()
		{
			if (targetStation == null)
			{
				return false;
			}
			return Vector3.Distance(base.Npc.transform.position, GetStationAccessPoint()) < 1f;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterObserversRpc(15u, RpcReader___Observers_StartCook_2166136261);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_StartCook_2166136261()
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
				SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___StartCook_2166136261()
		{
			if (cookRoutine == null)
			{
				cookRoutine = StartCoroutine(CookRoutine());
			}
			IEnumerator CookRoutine()
			{
				Console.Log("Starting cook...");
				base.Npc.Movement.FacePoint(targetStation.transform.position);
				yield return new WaitForSeconds(0.5f);
				base.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", value: true);
				if (!CanCookStart())
				{
					StopCook();
					End_Networked(null);
				}
				else
				{
					targetStation.SetNPCUser(base.Npc.NetworkObject);
					StationRecipe recipe = (targetStation.Configuration as ChemistryStationConfiguration).Recipe.SelectedRecipe;
					SetupBeaker();
					yield return new WaitForSeconds(1f);
					FillBeaker(recipe, beaker);
					yield return new WaitForSeconds(8f);
					yield return new WaitForSeconds(6f);
					yield return new WaitForSeconds(6f);
					List<ItemInstance> list = new List<ItemInstance>();
					for (int i = 0; i < recipe.Ingredients.Count; i++)
					{
						foreach (ItemDefinition item in recipe.Ingredients[i].Items)
						{
							StorableItemDefinition storableItemDefinition = item as StorableItemDefinition;
							for (int j = 0; j < targetStation.IngredientSlots.Length; j++)
							{
								if (targetStation.IngredientSlots[j].ItemInstance != null && targetStation.IngredientSlots[j].ItemInstance.Definition.ID == storableItemDefinition.ID)
								{
									list.Add(targetStation.IngredientSlots[j].ItemInstance.GetCopy(recipe.Ingredients[i].Quantity));
									targetStation.IngredientSlots[j].ChangeQuantity(-recipe.Ingredients[i].Quantity);
									break;
								}
							}
						}
					}
					EQuality productQuality = recipe.CalculateQuality(list);
					targetStation.SendCookOperation(new ChemistryCookOperation(recipe, productQuality, beaker.Container.LiquidColor, beaker.Fillable.LiquidContainer.CurrentLiquidLevel));
					beaker.Destroy();
					beaker = null;
					StopCook();
					End_Networked(null);
				}
			}
		}

		private void RpcReader___Observers_StartCook_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___StartCook_2166136261();
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviour_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			chemist = base.Npc as Chemist;
		}
	}
}
