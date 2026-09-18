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
	public static class GeneratedReaders___Internal
	{
		[RuntimeInitializeOnLoadMethod]
		private static void InitializeOnce()
		{
			GenericReader<ItemInstance>.Read = ItemSerializers.ReadItemInstance;
			GenericReader<StorableItemInstance>.Read = ItemSerializers.ReadStorableItemInstance;
			GenericReader<CashInstance>.Read = ItemSerializers.ReadCashInstance;
			GenericReader<QualityItemInstance>.Read = ItemSerializers.ReadQualityItemInstance;
			GenericReader<ClothingInstance>.Read = ItemSerializers.ReadClothingInstance;
			GenericReader<ProductItemInstance>.Read = ItemSerializers.ReadProductItemInstance;
			GenericReader<WeedInstance>.Read = ItemSerializers.ReadWeedInstance;
			GenericReader<IntegerItemInstance>.Read = ItemSerializers.ReadIntegerItemInstance;
			GenericReader<WateringCanInstance>.Read = ItemSerializers.ReadWateringCanInstance;
			GenericReader<TrashGrabberInstance>.Read = ItemSerializers.ReadTrashGrabberInstance;
			GenericReader<VisionEventReceipt>.Read = Read___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerateds;
			GenericReader<PlayerVisualState.EVisualState>.Read = Read___ScheduleOne_002EPlayerScripts_002EPlayerVisualState_002FEVisualStateFishNet_002ESerializing_002EGenerateds;
			GenericReader<VisionCone.EEventLevel>.Read = Read___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerateds;
			GenericReader<ContractInfo>.Read = Read___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerateds;
			GenericReader<ProductList>.Read = Read___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerateds;
			GenericReader<ProductList.Entry>.Read = Read___ScheduleOne_002EProduct_002EProductList_002FEntryFishNet_002ESerializing_002EGenerateds;
			GenericReader<EQuality>.Read = Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds;
			GenericReader<List<ProductList.Entry>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerateds;
			GenericReader<QuestWindowConfig>.Read = Read___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerateds;
			GenericReader<GameDateTime>.Read = Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds;
			GenericReader<QuestManager.EQuestAction>.Read = Read___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerateds;
			GenericReader<EQuestState>.Read = Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds;
			GenericReader<Impact>.Read = Read___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerateds;
			GenericReader<EImpactType>.Read = Read___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerateds;
			GenericReader<LandVehicle>.Read = Read___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerateds;
			GenericReader<CheckpointManager.ECheckpointLocation>.Read = Read___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerateds;
			GenericReader<Player>.Read = Read___ScheduleOne_002EPlayerScripts_002EPlayerFishNet_002ESerializing_002EGenerateds;
			GenericReader<List<string>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds;
			GenericReader<StringIntPair>.Read = Read___ScheduleOne_002EDevUtilities_002EStringIntPairFishNet_002ESerializing_002EGenerateds;
			GenericReader<StringIntPair[]>.Read = Read___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerateds;
			GenericReader<Message>.Read = Read___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerateds;
			GenericReader<Message.ESenderType>.Read = Read___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerateds;
			GenericReader<MessageChain>.Read = Read___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerateds;
			GenericReader<MSGConversationData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerateds;
			GenericReader<TextMessageData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002ETextMessageDataFishNet_002ESerializing_002EGenerateds;
			GenericReader<TextMessageData[]>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerateds;
			GenericReader<TextResponseData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002ETextResponseDataFishNet_002ESerializing_002EGenerateds;
			GenericReader<TextResponseData[]>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerateds;
			GenericReader<Response>.Read = Read___ScheduleOne_002EMessaging_002EResponseFishNet_002ESerializing_002EGenerateds;
			GenericReader<List<Response>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerateds;
			GenericReader<List<NetworkObject>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerateds;
			GenericReader<ERank>.Read = Read___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerateds;
			GenericReader<FullRank>.Read = Read___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerateds;
			GenericReader<PlayerData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerateds;
			GenericReader<VariableData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EVariableDataFishNet_002ESerializing_002EGenerateds;
			GenericReader<VariableData[]>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerateds;
			GenericReader<AvatarSettings>.Read = Read___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerateds;
			GenericReader<Eye.EyeLidConfiguration>.Read = Read___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerateds;
			GenericReader<AvatarSettings.LayerSetting>.Read = Read___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSettingFishNet_002ESerializing_002EGenerateds;
			GenericReader<List<AvatarSettings.LayerSetting>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerateds;
			GenericReader<AvatarSettings.AccessorySetting>.Read = Read___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySettingFishNet_002ESerializing_002EGenerateds;
			GenericReader<List<AvatarSettings.AccessorySetting>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerateds;
			GenericReader<BasicAvatarSettings>.Read = Read___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerateds;
			GenericReader<object>.Read = Read___System_002EObjectFishNet_002ESerializing_002EGenerateds;
			GenericReader<PlayerCrimeData.EPursuitLevel>.Read = Read___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_002FEPursuitLevelFishNet_002ESerializing_002EGenerateds;
			GenericReader<Property>.Read = Read___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerateds;
			GenericReader<EEmployeeType>.Read = Read___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerateds;
			GenericReader<EDealWindow>.Read = Read___ScheduleOne_002EEconomy_002EEDealWindowFishNet_002ESerializing_002EGenerateds;
			GenericReader<HandoverScreen.EHandoverOutcome>.Read = Read___ScheduleOne_002EUI_002EHandover_002EHandoverScreen_002FEHandoverOutcomeFishNet_002ESerializing_002EGenerateds;
			GenericReader<List<ItemInstance>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EItemInstance_003EFishNet_002ESerializing_002EGenerateds;
			GenericReader<ScheduleOne.Persistence.Datas.CustomerData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002ECustomerDataFishNet_002ESerializing_002EGenerateds;
			GenericReader<string[]>.Read = Read___System_002EString_005B_005DFishNet_002ESerializing_002EGenerateds;
			GenericReader<float[]>.Read = Read___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds;
			GenericReader<EDrugType>.Read = Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds;
			GenericReader<GameData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerateds;
			GenericReader<EDoorSide>.Read = Read___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerateds;
			GenericReader<ParkData>.Read = Read___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerateds;
			GenericReader<EParkingAlignment>.Read = Read___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerateds;
			GenericReader<TrashContentData>.Read = Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds;
			GenericReader<int[]>.Read = Read___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerateds;
			GenericReader<Coordinate>.Read = Read___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerateds;
			GenericReader<WeedAppearanceSettings>.Read = Read___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerateds;
			GenericReader<CocaineAppearanceSettings>.Read = Read___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerateds;
			GenericReader<MethAppearanceSettings>.Read = Read___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerateds;
			GenericReader<NewMixOperation>.Read = Read___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerateds;
			GenericReader<Recycler.EState>.Read = Read___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerateds;
			GenericReader<CoordinateProceduralTilePair>.Read = Read___ScheduleOne_002ETiles_002ECoordinateProceduralTilePairFishNet_002ESerializing_002EGenerateds;
			GenericReader<List<CoordinateProceduralTilePair>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds;
			GenericReader<ChemistryCookOperation>.Read = Read___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerateds;
			GenericReader<OvenCookOperation>.Read = Read___ScheduleOne_002EObjectScripts_002EOvenCookOperationFishNet_002ESerializing_002EGenerateds;
			GenericReader<MixOperation>.Read = Read___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerateds;
		}

		public static VisionEventReceipt Read___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			VisionEventReceipt visionEventReceipt = new VisionEventReceipt();
			visionEventReceipt.TargetPlayer = reader.ReadNetworkObject();
			visionEventReceipt.State = Read___ScheduleOne_002EPlayerScripts_002EPlayerVisualState_002FEVisualStateFishNet_002ESerializing_002EGenerateds(reader);
			return visionEventReceipt;
		}

		public static PlayerVisualState.EVisualState Read___ScheduleOne_002EPlayerScripts_002EPlayerVisualState_002FEVisualStateFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (PlayerVisualState.EVisualState)reader.ReadInt32();
		}

		public static VisionCone.EEventLevel Read___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (VisionCone.EEventLevel)reader.ReadInt32();
		}

		public static ContractInfo Read___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			ContractInfo contractInfo = new ContractInfo();
			contractInfo.Payment = reader.ReadSingle();
			contractInfo.Products = Read___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerateds(reader);
			contractInfo.DeliveryLocationGUID = reader.ReadString();
			contractInfo.DeliveryWindow = Read___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerateds(reader);
			contractInfo.Expires = reader.ReadBoolean();
			contractInfo.ExpiresAfter = reader.ReadInt32();
			contractInfo.PickupScheduleIndex = reader.ReadInt32();
			contractInfo.IsCounterOffer = reader.ReadBoolean();
			return contractInfo;
		}

		public static ProductList Read___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			ProductList productList = new ProductList();
			productList.entries = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerateds(reader);
			return productList;
		}

		public static ProductList.Entry Read___ScheduleOne_002EProduct_002EProductList_002FEntryFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			ProductList.Entry entry = new ProductList.Entry();
			entry.ProductID = reader.ReadString();
			entry.Quality = Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds(reader);
			entry.Quantity = reader.ReadInt32();
			return entry;
		}

		public static EQuality Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (EQuality)reader.ReadInt32();
		}

		public static List<ProductList.Entry> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadListAllocated<ProductList.Entry>();
		}

		public static QuestWindowConfig Read___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			QuestWindowConfig questWindowConfig = new QuestWindowConfig();
			questWindowConfig.IsEnabled = reader.ReadBoolean();
			questWindowConfig.WindowStartTime = reader.ReadInt32();
			questWindowConfig.WindowEndTime = reader.ReadInt32();
			return questWindowConfig;
		}

		public static GameDateTime Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			GameDateTime result = default(GameDateTime);
			result.elapsedDays = reader.ReadInt32();
			result.time = reader.ReadInt32();
			return result;
		}

		public static QuestManager.EQuestAction Read___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (QuestManager.EQuestAction)reader.ReadInt32();
		}

		public static EQuestState Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (EQuestState)reader.ReadInt32();
		}

		public static Impact Read___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			Impact impact = new Impact();
			impact.HitPoint = reader.ReadVector3();
			impact.ImpactForceDirection = reader.ReadVector3();
			impact.ImpactForce = reader.ReadSingle();
			impact.ImpactDamage = reader.ReadSingle();
			impact.ImpactType = Read___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerateds(reader);
			impact.ImpactSource = reader.ReadNetworkObject();
			impact.ImpactID = reader.ReadInt32();
			return impact;
		}

		public static EImpactType Read___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (EImpactType)reader.ReadInt32();
		}

		public static LandVehicle Read___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (LandVehicle)reader.ReadNetworkBehaviour();
		}

		public static CheckpointManager.ECheckpointLocation Read___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (CheckpointManager.ECheckpointLocation)reader.ReadInt32();
		}

		public static Player Read___ScheduleOne_002EPlayerScripts_002EPlayerFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Player)reader.ReadNetworkBehaviour();
		}

		public static List<string> Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadListAllocated<string>();
		}

		public static StringIntPair Read___ScheduleOne_002EDevUtilities_002EStringIntPairFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			StringIntPair stringIntPair = new StringIntPair();
			stringIntPair.String = reader.ReadString();
			stringIntPair.Int = reader.ReadInt32();
			return stringIntPair;
		}

		public static StringIntPair[] Read___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadArrayAllocated<StringIntPair>();
		}

		public static Message Read___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			Message message = new Message();
			message.messageId = reader.ReadInt32();
			message.text = reader.ReadString();
			message.sender = Read___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerateds(reader);
			message.endOfGroup = reader.ReadBoolean();
			return message;
		}

		public static Message.ESenderType Read___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Message.ESenderType)reader.ReadInt32();
		}

		public static MessageChain Read___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			MessageChain messageChain = new MessageChain();
			messageChain.Messages = Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds(reader);
			messageChain.id = reader.ReadInt32();
			return messageChain;
		}

		public static MSGConversationData Read___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			MSGConversationData mSGConversationData = new MSGConversationData();
			mSGConversationData.ConversationIndex = reader.ReadInt32();
			mSGConversationData.Read = reader.ReadBoolean();
			mSGConversationData.MessageHistory = Read___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
			mSGConversationData.ActiveResponses = Read___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
			mSGConversationData.IsHidden = reader.ReadBoolean();
			mSGConversationData.DataType = reader.ReadString();
			mSGConversationData.DataVersion = reader.ReadInt32();
			mSGConversationData.GameVersion = reader.ReadString();
			return mSGConversationData;
		}

		public static TextMessageData Read___ScheduleOne_002EPersistence_002EDatas_002ETextMessageDataFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			TextMessageData textMessageData = new TextMessageData();
			textMessageData.Sender = reader.ReadInt32();
			textMessageData.MessageID = reader.ReadInt32();
			textMessageData.Text = reader.ReadString();
			textMessageData.EndOfChain = reader.ReadBoolean();
			return textMessageData;
		}

		public static TextMessageData[] Read___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadArrayAllocated<TextMessageData>();
		}

		public static TextResponseData Read___ScheduleOne_002EPersistence_002EDatas_002ETextResponseDataFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			TextResponseData textResponseData = new TextResponseData();
			textResponseData.Text = reader.ReadString();
			textResponseData.Label = reader.ReadString();
			return textResponseData;
		}

		public static TextResponseData[] Read___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadArrayAllocated<TextResponseData>();
		}

		public static Response Read___ScheduleOne_002EMessaging_002EResponseFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			Response response = new Response();
			response.text = reader.ReadString();
			response.label = reader.ReadString();
			response.disableDefaultResponseBehaviour = reader.ReadBoolean();
			return response;
		}

		public static List<Response> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadListAllocated<Response>();
		}

		public static List<NetworkObject> Read___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadListAllocated<NetworkObject>();
		}

		public static ERank Read___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (ERank)reader.ReadInt32();
		}

		public static FullRank Read___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			FullRank result = default(FullRank);
			result.Rank = Read___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerateds(reader);
			result.Tier = reader.ReadInt32();
			return result;
		}

		public static PlayerData Read___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			PlayerData playerData = new PlayerData();
			playerData.PlayerCode = reader.ReadString();
			playerData.Position = reader.ReadVector3();
			playerData.Rotation = reader.ReadSingle();
			playerData.IntroCompleted = reader.ReadBoolean();
			playerData.DataType = reader.ReadString();
			playerData.DataVersion = reader.ReadInt32();
			playerData.GameVersion = reader.ReadString();
			return playerData;
		}

		public static VariableData Read___ScheduleOne_002EPersistence_002EDatas_002EVariableDataFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			VariableData variableData = new VariableData();
			variableData.Name = reader.ReadString();
			variableData.Value = reader.ReadString();
			variableData.DataType = reader.ReadString();
			variableData.DataVersion = reader.ReadInt32();
			variableData.GameVersion = reader.ReadString();
			return variableData;
		}

		public static VariableData[] Read___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadArrayAllocated<VariableData>();
		}

		public static AvatarSettings Read___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			AvatarSettings avatarSettings = ScriptableObject.CreateInstance<AvatarSettings>();
			avatarSettings.SkinColor = reader.ReadColor();
			avatarSettings.Height = reader.ReadSingle();
			avatarSettings.Gender = reader.ReadSingle();
			avatarSettings.Weight = reader.ReadSingle();
			avatarSettings.HairPath = reader.ReadString();
			avatarSettings.HairColor = reader.ReadColor();
			avatarSettings.EyebrowScale = reader.ReadSingle();
			avatarSettings.EyebrowThickness = reader.ReadSingle();
			avatarSettings.EyebrowRestingHeight = reader.ReadSingle();
			avatarSettings.EyebrowRestingAngle = reader.ReadSingle();
			avatarSettings.LeftEyeLidColor = reader.ReadColor();
			avatarSettings.RightEyeLidColor = reader.ReadColor();
			avatarSettings.LeftEyeRestingState = Read___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerateds(reader);
			avatarSettings.RightEyeRestingState = Read___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerateds(reader);
			avatarSettings.EyeballMaterialIdentifier = reader.ReadString();
			avatarSettings.EyeBallTint = reader.ReadColor();
			avatarSettings.PupilDilation = reader.ReadSingle();
			avatarSettings.FaceLayerSettings = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerateds(reader);
			avatarSettings.BodyLayerSettings = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerateds(reader);
			avatarSettings.AccessorySettings = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerateds(reader);
			avatarSettings.UseCombinedLayer = reader.ReadBoolean();
			avatarSettings.CombinedLayerPath = reader.ReadString();
			return avatarSettings;
		}

		public static Eye.EyeLidConfiguration Read___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			Eye.EyeLidConfiguration result = default(Eye.EyeLidConfiguration);
			result.topLidOpen = reader.ReadSingle();
			result.bottomLidOpen = reader.ReadSingle();
			return result;
		}

		public static AvatarSettings.LayerSetting Read___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSettingFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			AvatarSettings.LayerSetting result = default(AvatarSettings.LayerSetting);
			result.layerPath = reader.ReadString();
			result.layerTint = reader.ReadColor();
			return result;
		}

		public static List<AvatarSettings.LayerSetting> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadListAllocated<AvatarSettings.LayerSetting>();
		}

		public static AvatarSettings.AccessorySetting Read___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySettingFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			AvatarSettings.AccessorySetting accessorySetting = new AvatarSettings.AccessorySetting();
			accessorySetting.path = reader.ReadString();
			accessorySetting.color = reader.ReadColor();
			return accessorySetting;
		}

		public static List<AvatarSettings.AccessorySetting> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadListAllocated<AvatarSettings.AccessorySetting>();
		}

		public static BasicAvatarSettings Read___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			BasicAvatarSettings basicAvatarSettings = ScriptableObject.CreateInstance<BasicAvatarSettings>();
			basicAvatarSettings.Gender = reader.ReadInt32();
			basicAvatarSettings.Weight = reader.ReadSingle();
			basicAvatarSettings.SkinColor = reader.ReadColor();
			basicAvatarSettings.HairStyle = reader.ReadString();
			basicAvatarSettings.HairColor = reader.ReadColor();
			basicAvatarSettings.Mouth = reader.ReadString();
			basicAvatarSettings.FacialHair = reader.ReadString();
			basicAvatarSettings.FacialDetails = reader.ReadString();
			basicAvatarSettings.FacialDetailsIntensity = reader.ReadSingle();
			basicAvatarSettings.EyeballColor = reader.ReadColor();
			basicAvatarSettings.UpperEyeLidRestingPosition = reader.ReadSingle();
			basicAvatarSettings.LowerEyeLidRestingPosition = reader.ReadSingle();
			basicAvatarSettings.PupilDilation = reader.ReadSingle();
			basicAvatarSettings.EyebrowScale = reader.ReadSingle();
			basicAvatarSettings.EyebrowThickness = reader.ReadSingle();
			basicAvatarSettings.EyebrowRestingHeight = reader.ReadSingle();
			basicAvatarSettings.EyebrowRestingAngle = reader.ReadSingle();
			basicAvatarSettings.Top = reader.ReadString();
			basicAvatarSettings.TopColor = reader.ReadColor();
			basicAvatarSettings.Bottom = reader.ReadString();
			basicAvatarSettings.BottomColor = reader.ReadColor();
			basicAvatarSettings.Shoes = reader.ReadString();
			basicAvatarSettings.ShoesColor = reader.ReadColor();
			basicAvatarSettings.Headwear = reader.ReadString();
			basicAvatarSettings.HeadwearColor = reader.ReadColor();
			basicAvatarSettings.Eyewear = reader.ReadString();
			basicAvatarSettings.EyewearColor = reader.ReadColor();
			basicAvatarSettings.Tattoos = Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds(reader);
			return basicAvatarSettings;
		}

		public static object Read___System_002EObjectFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			return new object();
		}

		public static PlayerCrimeData.EPursuitLevel Read___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_002FEPursuitLevelFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (PlayerCrimeData.EPursuitLevel)reader.ReadInt32();
		}

		public static Property Read___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Property)reader.ReadNetworkBehaviour();
		}

		public static EEmployeeType Read___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (EEmployeeType)reader.ReadInt32();
		}

		public static EDealWindow Read___ScheduleOne_002EEconomy_002EEDealWindowFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (EDealWindow)reader.ReadInt32();
		}

		public static HandoverScreen.EHandoverOutcome Read___ScheduleOne_002EUI_002EHandover_002EHandoverScreen_002FEHandoverOutcomeFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (HandoverScreen.EHandoverOutcome)reader.ReadInt32();
		}

		public static List<ItemInstance> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EItemInstance_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadListAllocated<ItemInstance>();
		}

		public static ScheduleOne.Persistence.Datas.CustomerData Read___ScheduleOne_002EPersistence_002EDatas_002ECustomerDataFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			ScheduleOne.Persistence.Datas.CustomerData customerData = new ScheduleOne.Persistence.Datas.CustomerData();
			customerData.Dependence = reader.ReadSingle();
			customerData.PurchaseableProducts = Read___System_002EString_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
			customerData.ProductAffinities = Read___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
			customerData.TimeSinceLastDealCompleted = reader.ReadInt32();
			customerData.TimeSinceLastDealOffered = reader.ReadInt32();
			customerData.OfferedDeals = reader.ReadInt32();
			customerData.CompletedDeals = reader.ReadInt32();
			customerData.IsContractOffered = reader.ReadBoolean();
			customerData.OfferedContract = Read___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerateds(reader);
			customerData.OfferedContractTime = Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds(reader);
			customerData.TimeSincePlayerApproached = reader.ReadInt32();
			customerData.TimeSinceInstantDealOffered = reader.ReadInt32();
			customerData.HasBeenRecommended = reader.ReadBoolean();
			customerData.DataType = reader.ReadString();
			customerData.DataVersion = reader.ReadInt32();
			customerData.GameVersion = reader.ReadString();
			return customerData;
		}

		public static string[] Read___System_002EString_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadArrayAllocated<string>();
		}

		public static float[] Read___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadArrayAllocated<float>();
		}

		public static EDrugType Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (EDrugType)reader.ReadInt32();
		}

		public static GameData Read___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			GameData gameData = new GameData();
			gameData.OrganisationName = reader.ReadString();
			gameData.Seed = reader.ReadInt32();
			gameData.DataType = reader.ReadString();
			gameData.DataVersion = reader.ReadInt32();
			gameData.GameVersion = reader.ReadString();
			return gameData;
		}

		public static EDoorSide Read___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (EDoorSide)reader.ReadInt32();
		}

		public static ParkData Read___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			ParkData parkData = new ParkData();
			parkData.lotGUID = reader.ReadGuid();
			parkData.spotIndex = reader.ReadInt32();
			parkData.alignment = Read___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerateds(reader);
			return parkData;
		}

		public static EParkingAlignment Read___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (EParkingAlignment)reader.ReadInt32();
		}

		public static TrashContentData Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			TrashContentData trashContentData = new TrashContentData();
			trashContentData.TrashIDs = Read___System_002EString_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
			trashContentData.TrashQuantities = Read___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
			return trashContentData;
		}

		public static int[] Read___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadArrayAllocated<int>();
		}

		public static Coordinate Read___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			Coordinate coordinate = new Coordinate();
			coordinate.x = reader.ReadInt32();
			coordinate.y = reader.ReadInt32();
			return coordinate;
		}

		public static WeedAppearanceSettings Read___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			WeedAppearanceSettings weedAppearanceSettings = new WeedAppearanceSettings();
			weedAppearanceSettings.MainColor = reader.ReadColor32();
			weedAppearanceSettings.SecondaryColor = reader.ReadColor32();
			weedAppearanceSettings.LeafColor = reader.ReadColor32();
			weedAppearanceSettings.StemColor = reader.ReadColor32();
			return weedAppearanceSettings;
		}

		public static CocaineAppearanceSettings Read___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			CocaineAppearanceSettings cocaineAppearanceSettings = new CocaineAppearanceSettings();
			cocaineAppearanceSettings.MainColor = reader.ReadColor32();
			cocaineAppearanceSettings.SecondaryColor = reader.ReadColor32();
			return cocaineAppearanceSettings;
		}

		public static MethAppearanceSettings Read___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			MethAppearanceSettings methAppearanceSettings = new MethAppearanceSettings();
			methAppearanceSettings.MainColor = reader.ReadColor32();
			methAppearanceSettings.SecondaryColor = reader.ReadColor32();
			return methAppearanceSettings;
		}

		public static NewMixOperation Read___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			NewMixOperation newMixOperation = new NewMixOperation();
			newMixOperation.ProductID = reader.ReadString();
			newMixOperation.IngredientID = reader.ReadString();
			return newMixOperation;
		}

		public static Recycler.EState Read___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Recycler.EState)reader.ReadInt32();
		}

		public static CoordinateProceduralTilePair Read___ScheduleOne_002ETiles_002ECoordinateProceduralTilePairFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			CoordinateProceduralTilePair result = default(CoordinateProceduralTilePair);
			result.coord = Read___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerateds(reader);
			result.tileParent = reader.ReadNetworkObject();
			result.tileIndex = reader.ReadInt32();
			return result;
		}

		public static List<CoordinateProceduralTilePair> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadListAllocated<CoordinateProceduralTilePair>();
		}

		public static ChemistryCookOperation Read___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			ChemistryCookOperation chemistryCookOperation = new ChemistryCookOperation();
			chemistryCookOperation.RecipeID = reader.ReadString();
			chemistryCookOperation.ProductQuality = Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds(reader);
			chemistryCookOperation.StartLiquidColor = reader.ReadColor();
			chemistryCookOperation.LiquidLevel = reader.ReadSingle();
			return chemistryCookOperation;
		}

		public static OvenCookOperation Read___ScheduleOne_002EObjectScripts_002EOvenCookOperationFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			OvenCookOperation ovenCookOperation = new OvenCookOperation();
			ovenCookOperation.IngredientID = reader.ReadString();
			ovenCookOperation.IngredientQuality = Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds(reader);
			ovenCookOperation.IngredientQuantity = reader.ReadInt32();
			ovenCookOperation.ProductID = reader.ReadString();
			ovenCookOperation.CookProgress = reader.ReadInt32();
			return ovenCookOperation;
		}

		public static MixOperation Read___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			MixOperation mixOperation = new MixOperation();
			mixOperation.ProductID = reader.ReadString();
			mixOperation.ProductQuality = Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds(reader);
			mixOperation.IngredientID = reader.ReadString();
			mixOperation.Quantity = reader.ReadInt32();
			return mixOperation;
		}
	}
}
