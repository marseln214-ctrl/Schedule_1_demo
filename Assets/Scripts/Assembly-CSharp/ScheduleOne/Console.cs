using System;
using System.Collections.Generic;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.GameTime;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Law;
using ScheduleOne.Levelling;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Property;
using ScheduleOne.Quests;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne
{
	public class Console : Singleton<Console>
	{
		public abstract class ConsoleCommand
		{
			public abstract string CommandWord { get; }

			public abstract string CommandDescription { get; }

			public abstract string ExampleUsage { get; }

			public abstract void Execute(List<string> args);
		}

		public class SetTimeCommand : ConsoleCommand
		{
			public override string CommandWord => "settime";

			public override string CommandDescription => "Sets the time of day to the specified 24-hour time";

			public override string ExampleUsage => "settime 1530";

			public override void Execute(List<string> args)
			{
				if (args.Count > 0 && TimeManager.IsValid24HourTime(args[0]))
				{
					if (Player.Local.IsSleeping)
					{
						LogWarning("Can't set time whilst sleeping");
						return;
					}
					Log("Time set to " + args[0]);
					NetworkSingleton<TimeManager>.Instance.SetTime(int.Parse(args[0]));
				}
				else
				{
					LogWarning("Unrecognized command format. Correct format example(s): 'settime 1530'");
				}
			}
		}

		public class SpawnVehicleCommand : ConsoleCommand
		{
			public override string CommandWord => "spawnvehicle";

			public override string CommandDescription => "Spawns a vehicle at the player's location";

			public override string ExampleUsage => "spawnvehicle shitbox";

			public override void Execute(List<string> args)
			{
				bool flag = false;
				if (args.Count > 0 && NetworkSingleton<VehicleManager>.Instance.GetVehiclePrefab(args[0]) != null)
				{
					flag = true;
					Log("Spawning '" + args[0] + "'...");
					Vector3 position = player.transform.position + player.transform.forward * 4f + player.transform.up * 1f;
					Quaternion rotation = player.transform.rotation;
					NetworkSingleton<VehicleManager>.Instance.SpawnVehicle(args[0], position, rotation, playerOwned: true);
				}
				if (!flag)
				{
					LogWarning("Unrecognized command format. Correct format example(s): 'spawnvehicle shitbox'");
				}
			}
		}

		public class AddItemToInventoryCommand : ConsoleCommand
		{
			public override string CommandWord => "give";

			public override string CommandDescription => "Gives the player the specified item. Optionally specify a quantity.";

			public override string ExampleUsage => "give watering_can, give watering_can 5";

			public override void Execute(List<string> args)
			{
				if (args.Count > 0)
				{
					ItemDefinition item = Registry.GetItem(args[0]);
					if (item != null)
					{
						ItemInstance defaultInstance = item.GetDefaultInstance();
						if (args[0] == "cash")
						{
							LogWarning("Unrecognized item code '" + args[0] + "'");
						}
						else if (PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(defaultInstance))
						{
							int result = 1;
							if (args.Count > 1)
							{
								bool flag = false;
								if (int.TryParse(args[1], out result) && result > 0)
								{
									flag = true;
								}
								if (!flag)
								{
									LogWarning("Unrecognized quantity '" + args[1] + "'. Please provide a positive integer");
								}
							}
							int num = 0;
							while (result > 0 && PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(defaultInstance))
							{
								PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(defaultInstance);
								result--;
								num++;
							}
							Log("Added " + num + " " + item.Name + " to inventory");
						}
						else
						{
							LogWarning("Insufficient inventory space");
						}
					}
					else
					{
						LogWarning("Unrecognized item code '" + args[0] + "'");
					}
				}
				else
				{
					LogWarning("Unrecognized command format. Correct format example(s): 'give watering_can', 'give watering_can 5'");
				}
			}
		}

		public class ClearInventoryCommand : ConsoleCommand
		{
			public override string CommandWord => "clearinventory";

			public override string CommandDescription => "Clears the player's inventory";

			public override string ExampleUsage => "clearinventory";

			public override void Execute(List<string> args)
			{
				Log("Clearing player inventory...");
				PlayerSingleton<PlayerInventory>.Instance.ClearInventory();
			}
		}

		public class ChangeCashCommand : ConsoleCommand
		{
			public override string CommandWord => "changecash";

			public override string CommandDescription => "Changes the player's cash balance by the specified amount";

			public override string ExampleUsage => "changecash 5000";

			public override void Execute(List<string> args)
			{
				float result = 0f;
				if (args.Count == 0 || !float.TryParse(args[0], out result))
				{
					LogWarning("Unrecognized command format. Correct format example(s): 'changecash 5000', 'changecash -5000'");
				}
				else if (result > 0f)
				{
					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(result);
					Log("Gave player " + MoneyManager.FormatAmount(result) + " cash");
				}
				else if (result < 0f)
				{
					result = Mathf.Clamp(result, 0f - NetworkSingleton<MoneyManager>.Instance.cashBalance, 0f);
					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(result);
					Log("Removed " + MoneyManager.FormatAmount(result) + " cash from player");
				}
			}
		}

		public class ChangeOnlineBalanceCommand : ConsoleCommand
		{
			public override string CommandWord => "changebalance";

			public override string CommandDescription => "Changes the player's online balance by the specified amount";

			public override string ExampleUsage => "changebalance 5000";

			public override void Execute(List<string> args)
			{
				float result = 0f;
				if (args.Count == 0 || !float.TryParse(args[0], out result))
				{
					LogWarning("Unrecognized command format. Correct format example(s): 'changebalance 5000', 'changebalance -5000'");
				}
				else if (result > 0f)
				{
					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Added online balance", result, 1f, "Added by developer console");
					Log("Increased online balance by " + MoneyManager.FormatAmount(result));
				}
				else if (result < 0f)
				{
					result = Mathf.Clamp(result, 0f - NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance, 0f);
					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Removed online balance", result, 1f, "Removed by developer console");
					Log("Decreased online balance by " + MoneyManager.FormatAmount(result));
				}
			}
		}

		public class SetMoveSpeedCommand : ConsoleCommand
		{
			public override string CommandWord => "setmovespeed";

			public override string CommandDescription => "Sets the player's move speed multiplier";

			public override string ExampleUsage => "setmovespeed 1";

			public override void Execute(List<string> args)
			{
				float result = 0f;
				if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
				{
					LogWarning("Unrecognized command format. Correct format example(s): 'setmovespeed 1'");
					return;
				}
				Log("Setting player move speed multiplier to " + result);
				PlayerMovement.StaticMoveSpeedMultiplier = result;
			}
		}

		public class SetJumpMultiplier : ConsoleCommand
		{
			public override string CommandWord => "setjumpforce";

			public override string CommandDescription => "Sets the player's jump force multiplier";

			public override string ExampleUsage => "setjumpforce 1";

			public override void Execute(List<string> args)
			{
				float result = 0f;
				if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
				{
					LogWarning("Unrecognized command format. Correct format example(s): 'setjumpforce 1'");
					return;
				}
				Log("Setting player jump force multiplier to " + result);
				PlayerMovement.JumpMultiplier = result;
			}
		}

		public class SetPropertyOwned : ConsoleCommand
		{
			public override string CommandWord => "setowned";

			public override string CommandDescription => "Sets the specified property or business as owned";

			public override string ExampleUsage => "setowned barn, setowned laundromat";

			public override void Execute(List<string> args)
			{
				if (args.Count > 0)
				{
					string code = args[0].ToLower();
					ScheduleOne.Property.Property property = ScheduleOne.Property.Property.UnownedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == code);
					Business business = Business.UnownedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == code);
					if (property == null && business == null)
					{
						LogCommandError("Could not find unowned property with code '" + code + "'");
						return;
					}
					if (property != null)
					{
						property.SetOwned();
					}
					if (business != null)
					{
						business.SetOwned();
					}
					Log("Property with code '" + code + "' is now owned");
				}
				else
				{
					LogUnrecognizedFormat(new string[2] { "setowned barn", "setowned manor" });
				}
			}
		}

		public class Teleport : ConsoleCommand
		{
			public override string CommandWord => "teleport";

			public override string CommandDescription => "Teleports the player to the specified location";

			public override string ExampleUsage => "teleport townhall, teleport barn";

			public override void Execute(List<string> args)
			{
				if (args.Count > 0)
				{
					string text = args[0].ToLower();
					Transform transform = null;
					Vector3 vector = Vector3.zero;
					for (int i = 0; i < Singleton<Console>.Instance.TeleportPointsContainer.childCount; i++)
					{
						if (Singleton<Console>.Instance.TeleportPointsContainer.GetChild(i).name.ToLower() == text)
						{
							transform = Singleton<Console>.Instance.TeleportPointsContainer.GetChild(i);
							break;
						}
					}
					if (transform == null)
					{
						for (int j = 0; j < ScheduleOne.Property.Property.Properties.Count; j++)
						{
							if (ScheduleOne.Property.Property.Properties[j].PropertyCode.ToLower() == text)
							{
								transform = ScheduleOne.Property.Property.Properties[j].SpawnPoint;
								vector = Vector3.up * 1f;
								break;
							}
						}
					}
					if (transform == null)
					{
						for (int k = 0; k < Business.Businesses.Count; k++)
						{
							if (Business.Businesses[k].PropertyCode.ToLower() == text)
							{
								transform = Business.Businesses[k].SpawnPoint;
								vector = Vector3.up * 1f;
								break;
							}
						}
					}
					if (transform == null)
					{
						LogCommandError("Unrecognized destination");
						return;
					}
					PlayerSingleton<PlayerMovement>.Instance.Teleport(transform.transform.position + vector);
					Player.Local.transform.forward = transform.transform.forward;
					Log("Teleported to '" + text + "'");
				}
				else
				{
					LogUnrecognizedFormat(new string[2] { "teleport docks", "teleport barn" });
				}
			}
		}

		public class PackageProduct : ConsoleCommand
		{
			public override string CommandWord => "packageprodcut";

			public override string CommandDescription => "Packages the equipped product with the specified packaging";

			public override string ExampleUsage => "packageproduct jar, packageproduct baggie";

			public override void Execute(List<string> args)
			{
				if (args.Count > 0)
				{
					PackagingDefinition packagingDefinition = Registry.GetItem(args[0].ToLower()) as PackagingDefinition;
					if (packagingDefinition == null)
					{
						LogCommandError("Unrecognized packaging ID");
					}
					else if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance is ProductItemInstance)
					{
						(PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance as ProductItemInstance).SetPackaging(packagingDefinition);
						PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
						PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
						Log("Applied packaging '" + packagingDefinition.Name + "' to equipped product");
					}
					else
					{
						LogCommandError("No product equipped");
					}
				}
				else
				{
					LogUnrecognizedFormat(new string[2] { "packageproduct jar", "packageproduct baggie" });
				}
			}
		}

		public class SetStaminaReserve : ConsoleCommand
		{
			public override string CommandWord => "setstaminareserve";

			public override string CommandDescription => "Sets the player's stamina reserve (default 100) to the specified amount.";

			public override string ExampleUsage => "setstaminareserve 200";

			public override void Execute(List<string> args)
			{
				float result = 0f;
				if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
				{
					LogWarning("Unrecognized command format. Correct format example(s): 'setstaminareserve 200'");
					return;
				}
				Log("Setting player stamina reserve to " + result);
				PlayerMovement.StaminaReserveMax = result;
				PlayerSingleton<PlayerMovement>.Instance.SetStamina(result);
			}
		}

		public class RaisedWanted : ConsoleCommand
		{
			public override string CommandWord => "raisewanted";

			public override string CommandDescription => "Raises the player's wanted level";

			public override string ExampleUsage => "raisewanted";

			public override void Execute(List<string> args)
			{
				Log("Raising wanted level...");
				if (player.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
				{
					Singleton<LawManager>.Instance.PoliceCalled(player, new Crime());
				}
				player.CrimeData.Escalate();
			}
		}

		public class LowerWanted : ConsoleCommand
		{
			public override string CommandWord => "lowerwanted";

			public override string CommandDescription => "Lowers the player's wanted level";

			public override string ExampleUsage => "lowerwanted";

			public override void Execute(List<string> args)
			{
				Log("Lowering wanted level...");
				player.CrimeData.Deescalate();
			}
		}

		public class ClearWanted : ConsoleCommand
		{
			public override string CommandWord => "clearwanted";

			public override string CommandDescription => "Clears the player's wanted level";

			public override string ExampleUsage => "clearwanted";

			public override void Execute(List<string> args)
			{
				Log("Clearing wanted level...");
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.None);
				player.CrimeData.ClearCrimes();
			}
		}

		public class SetHealth : ConsoleCommand
		{
			public override string CommandWord => "sethealth";

			public override string CommandDescription => "Sets the player's health to the specified amount";

			public override string ExampleUsage => "sethealth 100";

			public override void Execute(List<string> args)
			{
				if (!player.Health.IsAlive)
				{
					LogWarning("Can't set health whilst dead");
					return;
				}
				float result = 0f;
				if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
				{
					LogWarning("Unrecognized command format. Correct format example(s): 'sethealth 100'");
					return;
				}
				Log("Setting player health to " + result);
				player.Health.SetHealth(result);
				if (result < 0f)
				{
					PlayerSingleton<PlayerCamera>.Instance.JoltCamera();
				}
			}
		}

		public class SetEnergy : ConsoleCommand
		{
			public override string CommandWord => "setenergy";

			public override string CommandDescription => "Sets the player's energy to the specified amount";

			public override string ExampleUsage => "setenergy 100";

			public override void Execute(List<string> args)
			{
				float result = 0f;
				if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
				{
					LogWarning("Unrecognized command format. Correct format example(s): 'setenergy 100'");
					return;
				}
				result = Mathf.Clamp(result, 0f, 100f);
				Log("Setting player energy to " + result);
				Player.Local.Energy.SetEnergy(result);
			}
		}

		public class FreeCamCommand : ConsoleCommand
		{
			public override string CommandWord => "freecam";

			public override string CommandDescription => "Toggles free cam mode";

			public override string ExampleUsage => "freecam";

			public override void Execute(List<string> args)
			{
				if (PlayerSingleton<PlayerCamera>.Instance.FreeCamEnabled)
				{
					PlayerSingleton<PlayerCamera>.Instance.SetFreeCam(enable: false);
				}
				else
				{
					PlayerSingleton<PlayerCamera>.Instance.SetFreeCam(enable: true);
				}
			}
		}

		public class Save : ConsoleCommand
		{
			public override string CommandWord => "save";

			public override string CommandDescription => "Forces a save";

			public override string ExampleUsage => "save";

			public override void Execute(List<string> args)
			{
				Log("Forcing save...");
				Singleton<SaveManager>.Instance.Save();
			}
		}

		public class SetTimeScale : ConsoleCommand
		{
			public override string CommandWord => "settimescale";

			public override string CommandDescription => "Sets the time scale. Default 1";

			public override string ExampleUsage => "settimescale 1";

			public override void Execute(List<string> args)
			{
				if (!Singleton<Settings>.Instance.PausingFreezesTime)
				{
					LogWarning("Can't set time scale right now.");
					return;
				}
				float result = 0f;
				if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
				{
					LogWarning("Unrecognized command format. Correct format example(s): 'settimescale 1'");
					return;
				}
				result = Mathf.Clamp(result, 0f, 20f);
				Log("Setting time scale to " + result);
				Time.timeScale = result;
			}
		}

		public class SetVariableValue : ConsoleCommand
		{
			public override string CommandWord => "setvar";

			public override string CommandDescription => "Sets the value of the specified variable";

			public override string ExampleUsage => "setvar <variable> <value>";

			public override void Execute(List<string> args)
			{
				if (args.Count >= 2)
				{
					string variableName = args[0].ToLower();
					string value = args[1];
					NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(variableName, value);
				}
				else
				{
					LogWarning("Unrecognized command format. Example usage: " + ExampleUsage);
				}
			}
		}

		public class SetQuestState : ConsoleCommand
		{
			public override string CommandWord => "setqueststate";

			public override string CommandDescription => "Sets the state of the specified quest";

			public override string ExampleUsage => "setqueststate <quest name> <state>";

			public override void Execute(List<string> args)
			{
				if (args.Count >= 2)
				{
					string text = args[0].ToLower();
					string text2 = args[1];
					text = text.Replace("_", " ");
					Quest quest = Quest.GetQuest(text);
					if (quest == null)
					{
						LogWarning("Failed to find quest with name '" + text + "'");
						return;
					}
					EQuestState result = EQuestState.Inactive;
					if (!Enum.TryParse<EQuestState>(text2, ignoreCase: true, out result))
					{
						LogWarning("Failed to parse quest state '" + text2 + "'");
					}
					else
					{
						quest.SetQuestState(result);
					}
				}
				else
				{
					LogWarning("Unrecognized command format. Example usage: " + ExampleUsage);
				}
			}
		}

		public class SetQuestEntryState : ConsoleCommand
		{
			public override string CommandWord => "setquestentrystate";

			public override string CommandDescription => "Sets the state of the specified quest entry";

			public override string ExampleUsage => "setquestentrystate <quest name> <entry index> <state>";

			public override void Execute(List<string> args)
			{
				if (args.Count >= 3)
				{
					string text = args[0].ToLower();
					int result = (int.TryParse(args[1], out result) ? result : (-1));
					string text2 = args[2];
					text = text.Replace("_", " ");
					Quest quest = Quest.GetQuest(text);
					if (quest == null)
					{
						LogWarning("Failed to find quest with name '" + text + "'");
						return;
					}
					if (result < 0 || result >= quest.Entries.Count)
					{
						LogWarning("Invalid entry index");
						return;
					}
					EQuestState result2 = EQuestState.Inactive;
					if (!Enum.TryParse<EQuestState>(text2, ignoreCase: true, out result2))
					{
						LogWarning("Failed to parse quest state '" + text2 + "'");
					}
					else
					{
						quest.SetQuestEntryState(result, result2);
					}
				}
				else
				{
					LogWarning("Unrecognized command format. Example usage: " + ExampleUsage);
				}
			}
		}

		public class SetEmotion : ConsoleCommand
		{
			public override string CommandWord => "setemotion";

			public override string CommandDescription => "Sets the facial expression of the player's avatar.";

			public override string ExampleUsage => "setemotion cheery";

			public override void Execute(List<string> args)
			{
				if (!Singleton<Settings>.Instance.PausingFreezesTime)
				{
					LogWarning("Can't set time scale right now.");
					return;
				}
				if (args.Count == 0)
				{
					LogWarning("Unrecognized command format. Correct format example(s): " + ExampleUsage);
					return;
				}
				string text = args[0].ToLower();
				if (!Player.Local.Avatar.EmotionManager.HasEmotion(text))
				{
					LogWarning("Unrecognized emotion '" + text + "'");
					return;
				}
				Log("Setting emotion to " + text);
				Player.Local.Avatar.EmotionManager.AddEmotionOverride(text, "console");
			}
		}

		public class SetUnlocked : ConsoleCommand
		{
			public override string CommandWord => "setunlocked";

			public override string CommandDescription => "Unlocks the given NPC";

			public override string ExampleUsage => "setunlocked <npc_id>";

			public override void Execute(List<string> args)
			{
				if (args.Count >= 1)
				{
					string text = args[0].ToLower();
					NPC nPC = NPCManager.GetNPC(text);
					if (nPC == null)
					{
						LogWarning("Failed to find NPC with ID '" + text + "'");
					}
					else
					{
						nPC.RelationData.Unlock(NPCRelationData.EUnlockType.DirectApproach);
					}
				}
				else
				{
					LogWarning("Unrecognized command format. Example usage: " + ExampleUsage);
				}
			}
		}

		public class SetRelationship : ConsoleCommand
		{
			public override string CommandWord => "setrelationship";

			public override string CommandDescription => "Sets the relationship scalar of the given NPC. Range is 0-5.";

			public override string ExampleUsage => "setrelationship <npc_id> 5";

			public override void Execute(List<string> args)
			{
				if (args.Count >= 2)
				{
					string text = args[0].ToLower();
					NPC nPC = NPCManager.GetNPC(text);
					if (nPC == null)
					{
						LogWarning("Failed to find NPC with ID '" + text + "'");
						return;
					}
					float result = 0f;
					if (!float.TryParse(args[1], out result) || result < 0f || result > 5f)
					{
						LogWarning("Invalid scalar value. Must be between 0 and 5.");
					}
					else
					{
						nPC.RelationData.SetRelationship(result);
					}
				}
				else
				{
					LogWarning("Unrecognized command format. Example usage: " + ExampleUsage);
				}
			}
		}

		public class AddEmployeeCommand : ConsoleCommand
		{
			public override string CommandWord => "addemployee";

			public override string CommandDescription => "Adds an employee of the specified type to the given property.";

			public override string ExampleUsage => "addemployee botanist barn";

			public override void Execute(List<string> args)
			{
				if (args.Count >= 2)
				{
					args[0].ToLower();
					EEmployeeType result = EEmployeeType.Botanist;
					if (!Enum.TryParse<EEmployeeType>(args[0], ignoreCase: true, out result))
					{
						LogCommandError("Unrecognized employee type '" + args[0] + "'");
						return;
					}
					string code = args[1].ToLower();
					ScheduleOne.Property.Property property = ScheduleOne.Property.Property.OwnedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == code);
					if (property == null)
					{
						LogCommandError("Could not find property with code '" + code + "'");
						return;
					}
					NetworkSingleton<EmployeeManager>.Instance.CreateNewEmployee(property, result);
					Log("Adding employee of type '" + result.ToString() + "' to property '" + property.PropertyCode + "'");
				}
				else
				{
					LogUnrecognizedFormat(new string[2] { "setowned barn", "setowned manor" });
				}
			}
		}

		public class SetDiscovered : ConsoleCommand
		{
			public override string CommandWord => "setdiscovered";

			public override string CommandDescription => "Sets the specified product as discovered";

			public override string ExampleUsage => "setdiscovered ogkush";

			public override void Execute(List<string> args)
			{
				if (args.Count > 0)
				{
					string text = args[0].ToLower();
					ProductDefinition productDefinition = Registry.GetItem(text) as ProductDefinition;
					if (productDefinition == null)
					{
						LogCommandError("Unrecognized product code '" + text + "'");
						return;
					}
					NetworkSingleton<ProductManager>.Instance.DiscoverProduct(productDefinition.ID);
					Log(productDefinition.Name + " now discovered");
				}
				else
				{
					LogUnrecognizedFormat(new string[1] { ExampleUsage });
				}
			}
		}

		public class GrowPlants : ConsoleCommand
		{
			public override string CommandWord => "growplants";

			public override string CommandDescription => "Sets ALL plants in the world fully grown";

			public override string ExampleUsage => "growplants";

			public override void Execute(List<string> args)
			{
				Plant[] array = UnityEngine.Object.FindObjectsOfType<Plant>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Pot.FullyGrowPlant();
				}
			}
		}

		public class SetLawIntensity : ConsoleCommand
		{
			public override string CommandWord => "setlawintensity";

			public override string CommandDescription => "Sets the intensity of law enforcement activity on a scale of 0-10.";

			public override string ExampleUsage => "setlawintensity 6";

			public override void Execute(List<string> args)
			{
				float result = 0f;
				if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
				{
					LogWarning("Unrecognized command format. Correct format example(s): " + ExampleUsage);
					return;
				}
				float num = Mathf.Clamp(result, 0f, 10f);
				Log("Setting law enforcement intensity to " + num);
				Singleton<LawController>.Instance.SetInternalIntensity(num / 10f);
			}
		}

		public class SetQuality : ConsoleCommand
		{
			public override string CommandWord => "setquality";

			public override string CommandDescription => "Sets the quality of the currently equipped item.";

			public override string ExampleUsage => "setquality standard, setquality heavenly";

			public override void Execute(List<string> args)
			{
				if (args.Count > 0)
				{
					string text = args[0].ToLower();
					if (!Enum.TryParse<EQuality>(text, ignoreCase: true, out var result))
					{
						LogCommandError("Unrecognized quality '" + text + "'");
					}
					if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance is QualityItemInstance)
					{
						(PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance as QualityItemInstance).SetQuality(result);
						PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
						PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
						Log("Set quality to " + result);
					}
					else
					{
						LogCommandError("No quality item equipped");
					}
				}
				else
				{
					LogUnrecognizedFormat(new string[1] { ExampleUsage });
				}
			}
		}

		public class Bind : ConsoleCommand
		{
			public override string CommandWord => "bind";

			public override string CommandDescription => "Binds the given key to the given command.";

			public override string ExampleUsage => "bind t 'settime 1200'";

			public override void Execute(List<string> args)
			{
				if (args.Count > 1)
				{
					string text = args[0].ToLower();
					if (!Enum.TryParse<KeyCode>(text, ignoreCase: true, out var result))
					{
						LogCommandError("Unrecognized keycode '" + text + "'");
					}
					string command = string.Join(" ", args.ToArray()).Substring(text.Length + 1);
					Singleton<Console>.Instance.AddBinding(result, command);
				}
				else
				{
					LogUnrecognizedFormat(new string[1] { ExampleUsage });
				}
			}
		}

		public class Unbind : ConsoleCommand
		{
			public override string CommandWord => "unbind";

			public override string CommandDescription => "Removes the given bind.";

			public override string ExampleUsage => "unbind t";

			public override void Execute(List<string> args)
			{
				if (args.Count > 0)
				{
					string text = args[0].ToLower();
					if (!Enum.TryParse<KeyCode>(text, ignoreCase: true, out var result))
					{
						LogCommandError("Unrecognized keycode '" + text + "'");
					}
					Singleton<Console>.Instance.RemoveBinding(result);
				}
				else
				{
					LogUnrecognizedFormat(new string[1] { ExampleUsage });
				}
			}
		}

		public class ClearBinds : ConsoleCommand
		{
			public override string CommandWord => "clearbinds";

			public override string CommandDescription => "Clears ALL binds.";

			public override string ExampleUsage => "clearbinds";

			public override void Execute(List<string> args)
			{
				Singleton<Console>.Instance.ClearBindings();
			}
		}

		public class HideUI : ConsoleCommand
		{
			public override string CommandWord => "hideui";

			public override string CommandDescription => "Hides all on-screen UI.";

			public override string ExampleUsage => "hideui";

			public override void Execute(List<string> args)
			{
				Singleton<HUD>.Instance.canvas.enabled = false;
			}
		}

		public class GiveXP : ConsoleCommand
		{
			public override string CommandWord => "addxp";

			public override string CommandDescription => "Adds the specified amount of experience points.";

			public override string ExampleUsage => "addxp 100";

			public override void Execute(List<string> args)
			{
				int result = 0;
				if (args.Count == 0 || !int.TryParse(args[0], out result) || result < 0)
				{
					LogWarning("Unrecognized command format. Correct format example(s): " + ExampleUsage);
					return;
				}
				Log("Giving " + result + " experience points");
				NetworkSingleton<LevelManager>.Instance.AddXP(result);
			}
		}

		public class Disable : ConsoleCommand
		{
			public override string CommandWord => "disable";

			public override string CommandDescription => "Disables the specified GameObject";

			public override string ExampleUsage => "disable pp";

			public override void Execute(List<string> args)
			{
				if (args.Count > 0)
				{
					string code = args[0].ToLower();
					LabelledGameObject labelledGameObject = Singleton<Console>.Instance.LabelledGameObjectList.Find((LabelledGameObject x) => x.Label.ToLower() == code);
					if (labelledGameObject == null)
					{
						LogCommandError("Could not find GameObject with label '" + code + "'");
					}
					else
					{
						labelledGameObject.GameObject.SetActive(value: false);
					}
				}
				else
				{
					LogUnrecognizedFormat(new string[1] { ExampleUsage });
				}
			}
		}

		public class Enable : ConsoleCommand
		{
			public override string CommandWord => "enable";

			public override string CommandDescription => "Enables the specified GameObject";

			public override string ExampleUsage => "enable pp";

			public override void Execute(List<string> args)
			{
				if (args.Count > 0)
				{
					string code = args[0].ToLower();
					LabelledGameObject labelledGameObject = Singleton<Console>.Instance.LabelledGameObjectList.Find((LabelledGameObject x) => x.Label.ToLower() == code);
					if (labelledGameObject == null)
					{
						LogCommandError("Could not find GameObject with label '" + code + "'");
					}
					else
					{
						labelledGameObject.GameObject.SetActive(value: true);
					}
				}
				else
				{
					LogUnrecognizedFormat(new string[1] { ExampleUsage });
				}
			}
		}

		[Serializable]
		public class LabelledGameObject
		{
			public string Label;

			public GameObject GameObject;
		}

		public Transform TeleportPointsContainer;

		public List<LabelledGameObject> LabelledGameObjectList;

		[Tooltip("Commands that run on startup (Editor only)")]
		public List<string> startupCommands = new List<string>();

		private static Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>();

		private Dictionary<KeyCode, string> keyBindings = new Dictionary<KeyCode, string>();

		private static Player player => Player.Local;

		private static void LogCommandError(string error)
		{
			LogWarning(error);
		}

		private static void LogUnrecognizedFormat(string[] correctExamples)
		{
			string text = string.Empty;
			for (int i = 0; i < correctExamples.Length; i++)
			{
				if (i > 0)
				{
					text += ",";
				}
				text = text + "'" + correctExamples[i] + "'";
			}
			LogWarning("Unrecognized command format. Correct format example(s): " + text);
		}

		protected override void Awake()
		{
			base.Awake();
			if (!(Singleton<Console>.Instance != this))
			{
				Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(RunStartupCommands));
				Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(RunStartupCommands));
			}
		}

		private void RunStartupCommands()
		{
			if (!Application.isEditor && !Debug.isDebugBuild)
			{
				return;
			}
			foreach (string startupCommand in startupCommands)
			{
				SubmitCommand(startupCommand);
			}
		}

		[HideInCallstack]
		public static void Log(object message, UnityEngine.Object context = null)
		{
			Debug.Log(message, context);
		}

		[HideInCallstack]
		public static void LogWarning(object message, UnityEngine.Object context = null)
		{
			Debug.LogWarning(message, context);
		}

		[HideInCallstack]
		public static void LogError(object message, UnityEngine.Object context = null)
		{
			Debug.LogError(message, context);
		}

		public static void SubmitCommand(List<string> args)
		{
			if (args.Count != 0 && (InstanceFinder.IsHost || Application.isEditor || Debug.isDebugBuild))
			{
				for (int i = 0; i < args.Count; i++)
				{
					args[i] = args[i].ToLower();
				}
				string text = args[0];
				if (commands.TryGetValue(text, out var value))
				{
					args.RemoveAt(0);
					value.Execute(args);
				}
				else
				{
					LogWarning("Command '" + text + "' not found.");
				}
			}
		}

		public static void SubmitCommand(string args)
		{
			SubmitCommand(new List<string>(args.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
		}

		public void AddBinding(KeyCode key, string command)
		{
			Log("Binding " + key.ToString() + " to " + command);
			if (keyBindings.ContainsKey(key))
			{
				keyBindings[key] = command;
			}
			else
			{
				keyBindings.Add(key, command);
			}
		}

		public void RemoveBinding(KeyCode key)
		{
			Log("Unbinding " + key);
			keyBindings.Remove(key);
		}

		public void ClearBindings()
		{
			Log("Clearing all key bindings");
			keyBindings.Clear();
		}

		private void Update()
		{
			if (GameInput.IsTyping || Singleton<PauseMenu>.Instance.IsPaused)
			{
				return;
			}
			foreach (KeyValuePair<KeyCode, string> keyBinding in keyBindings)
			{
				if (Input.GetKeyDown(keyBinding.Key))
				{
					SubmitCommand(keyBinding.Value);
				}
			}
		}
	}
}
