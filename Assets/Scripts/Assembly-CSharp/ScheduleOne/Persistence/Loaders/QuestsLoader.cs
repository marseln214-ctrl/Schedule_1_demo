using System;
using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Quests;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class QuestsLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (!Directory.Exists(mainPath))
			{
				return;
			}
			string[] files = Directory.GetFiles(mainPath);
			for (int i = 0; i < files.Length; i++)
			{
				if (!TryLoadFile(files[i], out var contents, autoAddExtension: false))
				{
					continue;
				}
				QuestData questData = null;
				try
				{
					questData = JsonUtility.FromJson<QuestData>(contents);
				}
				catch (Exception ex)
				{
					Debug.LogError("Error loading quest data: " + ex.Message);
				}
				if (questData == null)
				{
					continue;
				}
				Quest quest = null;
				if (questData.DataType == "DeaddropQuestData")
				{
					DeaddropQuestData deaddropQuestData = null;
					try
					{
						deaddropQuestData = JsonUtility.FromJson<DeaddropQuestData>(contents);
					}
					catch (Exception ex2)
					{
						Debug.LogError("Error loading quest data: " + ex2.Message);
					}
					if (deaddropQuestData == null)
					{
						continue;
					}
					DeadDrop @object = GUIDManager.GetObject<DeadDrop>(new Guid(deaddropQuestData.DeaddropGUID));
					if (@object == null)
					{
						Console.LogWarning("Failed to find deaddrop with GUID: " + deaddropQuestData.DeaddropGUID);
						continue;
					}
					quest = NetworkSingleton<QuestManager>.Instance.CreateDeaddropCollectionQuest(@object.GUID.ToString(), questData.GUID);
				}
				else
				{
					quest = GUIDManager.GetObject<Quest>(new Guid(questData.GUID));
				}
				if (quest == null)
				{
					Console.LogWarning("Failed to find quest with GUID: " + questData.GUID);
				}
				else
				{
					quest.Load(questData);
				}
			}
			string path = Path.Combine(mainPath, "Contracts");
			if (!Directory.Exists(path))
			{
				return;
			}
			string[] files2 = Directory.GetFiles(path);
			for (int j = 0; j < files2.Length; j++)
			{
				if (!TryLoadFile(files2[j], out var contents2, autoAddExtension: false))
				{
					continue;
				}
				ContractData contractData = null;
				try
				{
					contractData = JsonUtility.FromJson<ContractData>(contents2);
				}
				catch (Exception ex3)
				{
					Debug.LogError("Error loading contract data: " + ex3.Message);
				}
				if (contractData != null)
				{
					NPC object2 = GUIDManager.GetObject<NPC>(new Guid(contractData.CustomerGUID));
					if (object2 == null)
					{
						Console.LogWarning("Failed to find customer with GUID: " + contractData.CustomerGUID);
					}
					else
					{
						NetworkSingleton<QuestManager>.Instance.CreateContract_Local(contractData.Title, contractData.Description, contractData.Entries, contractData.GUID, contractData.IsTracked, object2.NetworkObject, contractData.Payment, contractData.ProductList, contractData.DeliveryLocationGUID, contractData.DeliveryWindow, contractData.Expires, new GameDateTime(contractData.ExpiryDate), contractData.PickupScheduleIndex, new GameDateTime(contractData.AcceptTime));
					}
				}
			}
		}
	}
}
