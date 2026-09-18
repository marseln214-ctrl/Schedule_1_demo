using System.Collections.Generic;
using System.Runtime.InteropServices;
using FishNet.Object;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.Clothing;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.Economy;
using ScheduleOne.Employees;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Law;
using ScheduleOne.Levelling;
using ScheduleOne.Messaging;
using ScheduleOne.ObjectScripts;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Property;
using ScheduleOne.Quests;
using ScheduleOne.Storage;
using ScheduleOne.Tiles;
using ScheduleOne.UI.Handover;
using ScheduleOne.UI.Phone.Messages;
using ScheduleOne.Vehicles;
using ScheduleOne.Vision;
using UnityEngine;

namespace FishNet.Serializing.Generated
{
	[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
	public static class GeneratedWriters___Internal
	{
		[RuntimeInitializeOnLoadMethod]
		private static void InitializeOnce()
		{
			GenericWriter<ItemInstance>.Write = ItemSerializers.WriteItemInstance;
			GenericWriter<StorableItemInstance>.Write = ItemSerializers.WriteStorableItemInstance;
			GenericWriter<CashInstance>.Write = ItemSerializers.WriteCashInstance;
			GenericWriter<QualityItemInstance>.Write = ItemSerializers.WriteQualityItemInstance;
			GenericWriter<ClothingInstance>.Write = ItemSerializers.WriteClothingInstance;
			GenericWriter<ProductItemInstance>.Write = ItemSerializers.WriteProductItemInstance;
			GenericWriter<WeedInstance>.Write = ItemSerializers.WriteWeedInstance;
			GenericWriter<IntegerItemInstance>.Write = ItemSerializers.WriteIntegerItemInstance;
			GenericWriter<WateringCanInstance>.Write = ItemSerializers.WriteWateringCanInstance;
			GenericWriter<TrashGrabberInstance>.Write = ItemSerializers.WriteTrashGrabberInstance;
			GenericWriter<VisionEventReceipt>.Write = Write___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerated;
			GenericWriter<PlayerVisualState.EVisualState>.Write = Write___ScheduleOne_002EPlayerScripts_002EPlayerVisualState_002FEVisualStateFishNet_002ESerializing_002EGenerated;
			GenericWriter<VisionCone.EEventLevel>.Write = Write___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerated;
			GenericWriter<ContractInfo>.Write = Write___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerated;
			GenericWriter<ProductList>.Write = Write___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerated;
			GenericWriter<ProductList.Entry>.Write = Write___ScheduleOne_002EProduct_002EProductList_002FEntryFishNet_002ESerializing_002EGenerated;
			GenericWriter<EQuality>.Write = Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated;
			GenericWriter<List<ProductList.Entry>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerated;
			GenericWriter<QuestWindowConfig>.Write = Write___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerated;
			GenericWriter<GameDateTime>.Write = Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated;
			GenericWriter<QuestManager.EQuestAction>.Write = Write___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerated;
			GenericWriter<EQuestState>.Write = Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated;
			GenericWriter<Impact>.Write = Write___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerated;
			GenericWriter<EImpactType>.Write = Write___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerated;
			GenericWriter<LandVehicle>.Write = Write___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerated;
			GenericWriter<CheckpointManager.ECheckpointLocation>.Write = Write___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerated;
			GenericWriter<Player>.Write = Write___ScheduleOne_002EPlayerScripts_002EPlayerFishNet_002ESerializing_002EGenerated;
			GenericWriter<List<string>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated;
			GenericWriter<StringIntPair>.Write = Write___ScheduleOne_002EDevUtilities_002EStringIntPairFishNet_002ESerializing_002EGenerated;
			GenericWriter<StringIntPair[]>.Write = Write___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerated;
			GenericWriter<Message>.Write = Write___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerated;
			GenericWriter<Message.ESenderType>.Write = Write___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerated;
			GenericWriter<MessageChain>.Write = Write___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerated;
			GenericWriter<MSGConversationData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerated;
			GenericWriter<TextMessageData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002ETextMessageDataFishNet_002ESerializing_002EGenerated;
			GenericWriter<TextMessageData[]>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerated;
			GenericWriter<TextResponseData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002ETextResponseDataFishNet_002ESerializing_002EGenerated;
			GenericWriter<TextResponseData[]>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerated;
			GenericWriter<Response>.Write = Write___ScheduleOne_002EMessaging_002EResponseFishNet_002ESerializing_002EGenerated;
			GenericWriter<List<Response>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerated;
			GenericWriter<List<NetworkObject>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerated;
			GenericWriter<ERank>.Write = Write___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerated;
			GenericWriter<FullRank>.Write = Write___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerated;
			GenericWriter<PlayerData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerated;
			GenericWriter<VariableData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EVariableDataFishNet_002ESerializing_002EGenerated;
			GenericWriter<VariableData[]>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerated;
			GenericWriter<AvatarSettings>.Write = Write___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerated;
			GenericWriter<Eye.EyeLidConfiguration>.Write = Write___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerated;
			GenericWriter<AvatarSettings.LayerSetting>.Write = Write___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSettingFishNet_002ESerializing_002EGenerated;
			GenericWriter<List<AvatarSettings.LayerSetting>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerated;
			GenericWriter<AvatarSettings.AccessorySetting>.Write = Write___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySettingFishNet_002ESerializing_002EGenerated;
			GenericWriter<List<AvatarSettings.AccessorySetting>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerated;
			GenericWriter<BasicAvatarSettings>.Write = Write___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerated;
			GenericWriter<object>.Write = Write___System_002EObjectFishNet_002ESerializing_002EGenerated;
			GenericWriter<PlayerCrimeData.EPursuitLevel>.Write = Write___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_002FEPursuitLevelFishNet_002ESerializing_002EGenerated;
			GenericWriter<Property>.Write = Write___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerated;
			GenericWriter<EEmployeeType>.Write = Write___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerated;
			GenericWriter<EDealWindow>.Write = Write___ScheduleOne_002EEconomy_002EEDealWindowFishNet_002ESerializing_002EGenerated;
			GenericWriter<HandoverScreen.EHandoverOutcome>.Write = Write___ScheduleOne_002EUI_002EHandover_002EHandoverScreen_002FEHandoverOutcomeFishNet_002ESerializing_002EGenerated;
			GenericWriter<List<ItemInstance>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EItemInstance_003EFishNet_002ESerializing_002EGenerated;
			GenericWriter<ScheduleOne.Persistence.Datas.CustomerData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002ECustomerDataFishNet_002ESerializing_002EGenerated;
			GenericWriter<string[]>.Write = Write___System_002EString_005B_005DFishNet_002ESerializing_002EGenerated;
			GenericWriter<float[]>.Write = Write___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated;
			GenericWriter<EDrugType>.Write = Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated;
			GenericWriter<GameData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerated;
			GenericWriter<EDoorSide>.Write = Write___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerated;
			GenericWriter<ParkData>.Write = Write___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerated;
			GenericWriter<EParkingAlignment>.Write = Write___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerated;
			GenericWriter<TrashContentData>.Write = Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated;
			GenericWriter<int[]>.Write = Write___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerated;
			GenericWriter<Coordinate>.Write = Write___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerated;
			GenericWriter<WeedAppearanceSettings>.Write = Write___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerated;
			GenericWriter<CocaineAppearanceSettings>.Write = Write___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerated;
			GenericWriter<MethAppearanceSettings>.Write = Write___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerated;
			GenericWriter<NewMixOperation>.Write = Write___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerated;
			GenericWriter<Recycler.EState>.Write = Write___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerated;
			GenericWriter<CoordinateProceduralTilePair>.Write = Write___ScheduleOne_002ETiles_002ECoordinateProceduralTilePairFishNet_002ESerializing_002EGenerated;
			GenericWriter<List<CoordinateProceduralTilePair>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerated;
			GenericWriter<ChemistryCookOperation>.Write = Write___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerated;
			GenericWriter<OvenCookOperation>.Write = Write___ScheduleOne_002EObjectScripts_002EOvenCookOperationFishNet_002ESerializing_002EGenerated;
			GenericWriter<MixOperation>.Write = Write___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerated;
		}

		public static void Write___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerated(this Writer writer, VisionEventReceipt value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteNetworkObject(value.TargetPlayer);
			Write___ScheduleOne_002EPlayerScripts_002EPlayerVisualState_002FEVisualStateFishNet_002ESerializing_002EGenerated(writer, value.State);
		}

		public static void Write___ScheduleOne_002EPlayerScripts_002EPlayerVisualState_002FEVisualStateFishNet_002ESerializing_002EGenerated(this Writer writer, PlayerVisualState.EVisualState value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerated(this Writer writer, VisionCone.EEventLevel value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerated(this Writer writer, ContractInfo value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteSingle(value.Payment);
			Write___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerated(writer, value.Products);
			writer.WriteString(value.DeliveryLocationGUID);
			Write___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerated(writer, value.DeliveryWindow);
			writer.WriteBoolean(value.Expires);
			writer.WriteInt32(value.ExpiresAfter);
			writer.WriteInt32(value.PickupScheduleIndex);
			writer.WriteBoolean(value.IsCounterOffer);
		}

		public static void Write___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerated(this Writer writer, ProductList value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerated(writer, value.entries);
		}

		public static void Write___ScheduleOne_002EProduct_002EProductList_002FEntryFishNet_002ESerializing_002EGenerated(this Writer writer, ProductList.Entry value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.ProductID);
			Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated(writer, value.Quality);
			writer.WriteInt32(value.Quantity);
		}

		public static void Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated(this Writer writer, EQuality value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<ProductList.Entry> value)
		{
			writer.WriteList(value);
		}

		public static void Write___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerated(this Writer writer, QuestWindowConfig value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteBoolean(value.IsEnabled);
			writer.WriteInt32(value.WindowStartTime);
			writer.WriteInt32(value.WindowEndTime);
		}

		public static void Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated(this Writer writer, GameDateTime value)
		{
			writer.WriteInt32(value.elapsedDays);
			writer.WriteInt32(value.time);
		}

		public static void Write___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerated(this Writer writer, QuestManager.EQuestAction value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated(this Writer writer, EQuestState value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerated(this Writer writer, Impact value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteVector3(value.HitPoint);
			writer.WriteVector3(value.ImpactForceDirection);
			writer.WriteSingle(value.ImpactForce);
			writer.WriteSingle(value.ImpactDamage);
			Write___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerated(writer, value.ImpactType);
			writer.WriteNetworkObject(value.ImpactSource);
			writer.WriteInt32(value.ImpactID);
		}

		public static void Write___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerated(this Writer writer, EImpactType value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerated(this Writer writer, LandVehicle value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void Write___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerated(this Writer writer, CheckpointManager.ECheckpointLocation value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EPlayerScripts_002EPlayerFishNet_002ESerializing_002EGenerated(this Writer writer, Player value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<string> value)
		{
			writer.WriteList(value);
		}

		public static void Write___ScheduleOne_002EDevUtilities_002EStringIntPairFishNet_002ESerializing_002EGenerated(this Writer writer, StringIntPair value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.String);
			writer.WriteInt32(value.Int);
		}

		public static void Write___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, StringIntPair[] value)
		{
			writer.WriteArray(value);
		}

		public static void Write___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerated(this Writer writer, Message value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteInt32(value.messageId);
			writer.WriteString(value.text);
			Write___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerated(writer, value.sender);
			writer.WriteBoolean(value.endOfGroup);
		}

		public static void Write___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerated(this Writer writer, Message.ESenderType value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerated(this Writer writer, MessageChain value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated(writer, value.Messages);
			writer.WriteInt32(value.id);
		}

		public static void Write___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerated(this Writer writer, MSGConversationData value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteInt32(value.ConversationIndex);
			writer.WriteBoolean(value.Read);
			Write___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.MessageHistory);
			Write___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.ActiveResponses);
			writer.WriteBoolean(value.IsHidden);
			writer.WriteString(value.DataType);
			writer.WriteInt32(value.DataVersion);
			writer.WriteString(value.GameVersion);
		}

		public static void Write___ScheduleOne_002EPersistence_002EDatas_002ETextMessageDataFishNet_002ESerializing_002EGenerated(this Writer writer, TextMessageData value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteInt32(value.Sender);
			writer.WriteInt32(value.MessageID);
			writer.WriteString(value.Text);
			writer.WriteBoolean(value.EndOfChain);
		}

		public static void Write___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, TextMessageData[] value)
		{
			writer.WriteArray(value);
		}

		public static void Write___ScheduleOne_002EPersistence_002EDatas_002ETextResponseDataFishNet_002ESerializing_002EGenerated(this Writer writer, TextResponseData value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.Text);
			writer.WriteString(value.Label);
		}

		public static void Write___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, TextResponseData[] value)
		{
			writer.WriteArray(value);
		}

		public static void Write___ScheduleOne_002EMessaging_002EResponseFishNet_002ESerializing_002EGenerated(this Writer writer, Response value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.text);
			writer.WriteString(value.label);
			writer.WriteBoolean(value.disableDefaultResponseBehaviour);
		}

		public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<Response> value)
		{
			writer.WriteList(value);
		}

		public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<NetworkObject> value)
		{
			writer.WriteList(value);
		}

		public static void Write___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerated(this Writer writer, ERank value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerated(this Writer writer, FullRank value)
		{
			Write___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerated(writer, value.Rank);
			writer.WriteInt32(value.Tier);
		}

		public static void Write___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerated(this Writer writer, PlayerData value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.PlayerCode);
			writer.WriteVector3(value.Position);
			writer.WriteSingle(value.Rotation);
			writer.WriteBoolean(value.IntroCompleted);
			writer.WriteString(value.DataType);
			writer.WriteInt32(value.DataVersion);
			writer.WriteString(value.GameVersion);
		}

		public static void Write___ScheduleOne_002EPersistence_002EDatas_002EVariableDataFishNet_002ESerializing_002EGenerated(this Writer writer, VariableData value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.Name);
			writer.WriteString(value.Value);
			writer.WriteString(value.DataType);
			writer.WriteInt32(value.DataVersion);
			writer.WriteString(value.GameVersion);
		}

		public static void Write___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, VariableData[] value)
		{
			writer.WriteArray(value);
		}

		public static void Write___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, AvatarSettings value)
		{
			if ((object)value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteColor(value.SkinColor);
			writer.WriteSingle(value.Height);
			writer.WriteSingle(value.Gender);
			writer.WriteSingle(value.Weight);
			writer.WriteString(value.HairPath);
			writer.WriteColor(value.HairColor);
			writer.WriteSingle(value.EyebrowScale);
			writer.WriteSingle(value.EyebrowThickness);
			writer.WriteSingle(value.EyebrowRestingHeight);
			writer.WriteSingle(value.EyebrowRestingAngle);
			writer.WriteColor(value.LeftEyeLidColor);
			writer.WriteColor(value.RightEyeLidColor);
			Write___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerated(writer, value.LeftEyeRestingState);
			Write___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerated(writer, value.RightEyeRestingState);
			writer.WriteString(value.EyeballMaterialIdentifier);
			writer.WriteColor(value.EyeBallTint);
			writer.WriteSingle(value.PupilDilation);
			Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerated(writer, value.FaceLayerSettings);
			Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerated(writer, value.BodyLayerSettings);
			Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerated(writer, value.AccessorySettings);
			writer.WriteBoolean(value.UseCombinedLayer);
			writer.WriteString(value.CombinedLayerPath);
		}

		public static void Write___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerated(this Writer writer, Eye.EyeLidConfiguration value)
		{
			writer.WriteSingle(value.topLidOpen);
			writer.WriteSingle(value.bottomLidOpen);
		}

		public static void Write___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSettingFishNet_002ESerializing_002EGenerated(this Writer writer, AvatarSettings.LayerSetting value)
		{
			writer.WriteString(value.layerPath);
			writer.WriteColor(value.layerTint);
		}

		public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<AvatarSettings.LayerSetting> value)
		{
			writer.WriteList(value);
		}

		public static void Write___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySettingFishNet_002ESerializing_002EGenerated(this Writer writer, AvatarSettings.AccessorySetting value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.path);
			writer.WriteColor(value.color);
		}

		public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<AvatarSettings.AccessorySetting> value)
		{
			writer.WriteList(value);
		}

		public static void Write___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, BasicAvatarSettings value)
		{
			if ((object)value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteInt32(value.Gender);
			writer.WriteSingle(value.Weight);
			writer.WriteColor(value.SkinColor);
			writer.WriteString(value.HairStyle);
			writer.WriteColor(value.HairColor);
			writer.WriteString(value.Mouth);
			writer.WriteString(value.FacialHair);
			writer.WriteString(value.FacialDetails);
			writer.WriteSingle(value.FacialDetailsIntensity);
			writer.WriteColor(value.EyeballColor);
			writer.WriteSingle(value.UpperEyeLidRestingPosition);
			writer.WriteSingle(value.LowerEyeLidRestingPosition);
			writer.WriteSingle(value.PupilDilation);
			writer.WriteSingle(value.EyebrowScale);
			writer.WriteSingle(value.EyebrowThickness);
			writer.WriteSingle(value.EyebrowRestingHeight);
			writer.WriteSingle(value.EyebrowRestingAngle);
			writer.WriteString(value.Top);
			writer.WriteColor(value.TopColor);
			writer.WriteString(value.Bottom);
			writer.WriteColor(value.BottomColor);
			writer.WriteString(value.Shoes);
			writer.WriteColor(value.ShoesColor);
			writer.WriteString(value.Headwear);
			writer.WriteColor(value.HeadwearColor);
			writer.WriteString(value.Eyewear);
			writer.WriteColor(value.EyewearColor);
			Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated(writer, value.Tattoos);
		}

		public static void Write___System_002EObjectFishNet_002ESerializing_002EGenerated(this Writer writer, object value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
			}
			else
			{
				writer.WriteBoolean(value: false);
			}
		}

		public static void Write___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_002FEPursuitLevelFishNet_002ESerializing_002EGenerated(this Writer writer, PlayerCrimeData.EPursuitLevel value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerated(this Writer writer, Property value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void Write___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerated(this Writer writer, EEmployeeType value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EEconomy_002EEDealWindowFishNet_002ESerializing_002EGenerated(this Writer writer, EDealWindow value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EUI_002EHandover_002EHandoverScreen_002FEHandoverOutcomeFishNet_002ESerializing_002EGenerated(this Writer writer, HandoverScreen.EHandoverOutcome value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EItemInstance_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<ItemInstance> value)
		{
			writer.WriteList(value);
		}

		public static void Write___ScheduleOne_002EPersistence_002EDatas_002ECustomerDataFishNet_002ESerializing_002EGenerated(this Writer writer, ScheduleOne.Persistence.Datas.CustomerData value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteSingle(value.Dependence);
			Write___System_002EString_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.PurchaseableProducts);
			Write___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.ProductAffinities);
			writer.WriteInt32(value.TimeSinceLastDealCompleted);
			writer.WriteInt32(value.TimeSinceLastDealOffered);
			writer.WriteInt32(value.OfferedDeals);
			writer.WriteInt32(value.CompletedDeals);
			writer.WriteBoolean(value.IsContractOffered);
			Write___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerated(writer, value.OfferedContract);
			Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated(writer, value.OfferedContractTime);
			writer.WriteInt32(value.TimeSincePlayerApproached);
			writer.WriteInt32(value.TimeSinceInstantDealOffered);
			writer.WriteBoolean(value.HasBeenRecommended);
			writer.WriteString(value.DataType);
			writer.WriteInt32(value.DataVersion);
			writer.WriteString(value.GameVersion);
		}

		public static void Write___System_002EString_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, string[] value)
		{
			writer.WriteArray(value);
		}

		public static void Write___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, float[] value)
		{
			writer.WriteArray(value);
		}

		public static void Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated(this Writer writer, EDrugType value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerated(this Writer writer, GameData value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.OrganisationName);
			writer.WriteInt32(value.Seed);
			writer.WriteString(value.DataType);
			writer.WriteInt32(value.DataVersion);
			writer.WriteString(value.GameVersion);
		}

		public static void Write___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerated(this Writer writer, EDoorSide value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerated(this Writer writer, ParkData value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteGuidAllocated(value.lotGUID);
			writer.WriteInt32(value.spotIndex);
			Write___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerated(writer, value.alignment);
		}

		public static void Write___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerated(this Writer writer, EParkingAlignment value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated(this Writer writer, TrashContentData value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			Write___System_002EString_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.TrashIDs);
			Write___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.TrashQuantities);
		}

		public static void Write___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, int[] value)
		{
			writer.WriteArray(value);
		}

		public static void Write___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerated(this Writer writer, Coordinate value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteInt32(value.x);
			writer.WriteInt32(value.y);
		}

		public static void Write___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, WeedAppearanceSettings value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteColor32(value.MainColor);
			writer.WriteColor32(value.SecondaryColor);
			writer.WriteColor32(value.LeafColor);
			writer.WriteColor32(value.StemColor);
		}

		public static void Write___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, CocaineAppearanceSettings value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteColor32(value.MainColor);
			writer.WriteColor32(value.SecondaryColor);
		}

		public static void Write___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, MethAppearanceSettings value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteColor32(value.MainColor);
			writer.WriteColor32(value.SecondaryColor);
		}

		public static void Write___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerated(this Writer writer, NewMixOperation value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.ProductID);
			writer.WriteString(value.IngredientID);
		}

		public static void Write___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerated(this Writer writer, Recycler.EState value)
		{
			writer.WriteInt32((int)value);
		}

		public static void Write___ScheduleOne_002ETiles_002ECoordinateProceduralTilePairFishNet_002ESerializing_002EGenerated(this Writer writer, CoordinateProceduralTilePair value)
		{
			Write___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerated(writer, value.coord);
			writer.WriteNetworkObject(value.tileParent);
			writer.WriteInt32(value.tileIndex);
		}

		public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<CoordinateProceduralTilePair> value)
		{
			writer.WriteList(value);
		}

		public static void Write___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerated(this Writer writer, ChemistryCookOperation value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.RecipeID);
			Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated(writer, value.ProductQuality);
			writer.WriteColor(value.StartLiquidColor);
			writer.WriteSingle(value.LiquidLevel);
		}

		public static void Write___ScheduleOne_002EObjectScripts_002EOvenCookOperationFishNet_002ESerializing_002EGenerated(this Writer writer, OvenCookOperation value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.IngredientID);
			Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated(writer, value.IngredientQuality);
			writer.WriteInt32(value.IngredientQuantity);
			writer.WriteString(value.ProductID);
			writer.WriteInt32(value.CookProgress);
		}

		public static void Write___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerated(this Writer writer, MixOperation value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			writer.WriteString(value.ProductID);
			Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated(writer, value.ProductQuality);
			writer.WriteString(value.IngredientID);
			writer.WriteInt32(value.Quantity);
		}
	}
}
