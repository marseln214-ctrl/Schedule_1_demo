using FishNet.Serializing;
using ScheduleOne.Clothing;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.Product;
using ScheduleOne.Storage;

namespace ScheduleOne.ItemFramework
{
	public static class ItemSerializers
	{
		public const bool DEBUG = false;

		private static ItemInstance Read(this Reader reader)
		{
			if (reader.Remaining == 0)
			{
				return null;
			}
			string text = reader.ReadString();
			if (text == typeof(ItemInstance).Name)
			{
				return reader.DirectReadItemInstance();
			}
			if (text == typeof(StorableItemInstance).Name)
			{
				return reader.DirectReadStorableItemInstance();
			}
			if (text == typeof(CashInstance).Name)
			{
				return reader.DirectReadCashInstance();
			}
			if (text == typeof(ClothingInstance).Name)
			{
				return reader.DirectReadClothingInstance();
			}
			if (text == typeof(QualityItemInstance).Name)
			{
				return reader.DirectReadQualityItemInstance();
			}
			if (text == typeof(ProductItemInstance).Name)
			{
				return reader.DirectReadProductItemInstance();
			}
			if (text == typeof(WeedInstance).Name)
			{
				return reader.DirectReadWeedInstance();
			}
			if (text == typeof(IntegerItemInstance).Name)
			{
				return reader.DirectReadIntegerItemInstance();
			}
			if (text == typeof(WateringCanInstance).Name)
			{
				return reader.DirectReadWateringCanInstance();
			}
			if (text == typeof(TrashGrabberInstance).Name)
			{
				return reader.DirectReadTrashGrabberInstance();
			}
			if (reader.ReadString() == string.Empty)
			{
				return null;
			}
			Console.LogError("ItemSerializers: reader not found for '" + text + "'!");
			return null;
		}

		public static void WriteItemInstance(this Writer writer, ItemInstance value)
		{
			if (value is StorableItemInstance)
			{
				writer.WriteStorableItemInstance((StorableItemInstance)value);
			}
			else if (value == null)
			{
				writer.WriteString(typeof(ItemInstance).Name);
				writer.WriteString(string.Empty);
			}
			else
			{
				writer.WriteString(typeof(ItemInstance).Name);
				writer.WriteString(value.ID);
				writer.WriteUInt16((ushort)value.Quantity);
			}
		}

		public static ItemInstance ReadItemInstance(this Reader reader)
		{
			return reader.Read();
		}

		private static ItemInstance DirectReadItemInstance(this Reader reader)
		{
			_ = reader.ReadString() == string.Empty;
			return null;
		}

		public static void WriteStorableItemInstance(this Writer writer, StorableItemInstance value)
		{
			if (value is QualityItemInstance)
			{
				writer.WriteQualityItemInstance((QualityItemInstance)value);
			}
			else if (value is CashInstance)
			{
				writer.WriteCashInstance(value as CashInstance);
			}
			else if (value is ClothingInstance)
			{
				writer.WriteClothingInstance(value as ClothingInstance);
			}
			else if (value is IntegerItemInstance)
			{
				writer.WriteIntegerItemInstance(value as IntegerItemInstance);
			}
			else if (value is WateringCanInstance)
			{
				writer.WriteWateringCanInstance(value as WateringCanInstance);
			}
			else if (value is TrashGrabberInstance)
			{
				writer.WriteTrashGrabberInstance(value as TrashGrabberInstance);
			}
			else if (value != null)
			{
				writer.WriteString(typeof(StorableItemInstance).Name);
				writer.WriteString(value.ID);
				writer.WriteUInt16((ushort)value.Quantity);
			}
		}

		public static StorableItemInstance ReadStorableItemInstance(this Reader reader)
		{
			return reader.Read() as StorableItemInstance;
		}

		private static StorableItemInstance DirectReadStorableItemInstance(this Reader reader)
		{
			string text = reader.ReadString();
			if (text == string.Empty)
			{
				return null;
			}
			return new StorableItemInstance
			{
				ID = text,
				Quantity = reader.ReadUInt16()
			};
		}

		public static void WriteCashInstance(this Writer writer, CashInstance value)
		{
			if (value != null)
			{
				writer.WriteString(typeof(CashInstance).Name);
				writer.WriteString(value.ID);
				writer.WriteUInt16((ushort)value.Quantity);
				writer.WriteSingle(value.Balance);
			}
		}

		public static CashInstance ReadCashInstance(this Reader reader)
		{
			return reader.Read() as CashInstance;
		}

		private static CashInstance DirectReadCashInstance(this Reader reader)
		{
			string text = reader.ReadString();
			if (text == string.Empty)
			{
				return null;
			}
			CashInstance cashInstance = new CashInstance();
			cashInstance.ID = text;
			cashInstance.Quantity = reader.ReadUInt16();
			cashInstance.SetBalance(reader.ReadSingle());
			return cashInstance;
		}

		public static void WriteQualityItemInstance(this Writer writer, QualityItemInstance value)
		{
			if (value is ProductItemInstance)
			{
				writer.WriteProductItemInstance(value as ProductItemInstance);
			}
			else if (value != null)
			{
				writer.WriteString(typeof(QualityItemInstance).Name);
				writer.WriteString(value.ID);
				writer.WriteUInt16((ushort)value.Quantity);
				writer.WriteUInt16((ushort)value.Quality);
			}
		}

		public static QualityItemInstance ReadQualityItemInstance(this Reader reader)
		{
			return reader.Read() as QualityItemInstance;
		}

		private static QualityItemInstance DirectReadQualityItemInstance(this Reader reader)
		{
			string text = reader.ReadString();
			if (text == string.Empty)
			{
				return null;
			}
			return new QualityItemInstance
			{
				ID = text,
				Quantity = reader.ReadUInt16(),
				Quality = (EQuality)reader.ReadUInt16()
			};
		}

		public static void WriteClothingInstance(this Writer writer, ClothingInstance value)
		{
			if (value != null)
			{
				writer.WriteString(typeof(ClothingInstance).Name);
				writer.WriteString(value.ID);
				writer.WriteUInt16((ushort)value.Quantity);
				writer.WriteUInt16((ushort)value.Color);
			}
		}

		public static ClothingInstance ReadClothingInstance(this Reader reader)
		{
			return reader.Read() as ClothingInstance;
		}

		private static ClothingInstance DirectReadClothingInstance(this Reader reader)
		{
			string text = reader.ReadString();
			if (text == string.Empty)
			{
				return null;
			}
			return new ClothingInstance
			{
				ID = text,
				Quantity = reader.ReadUInt16(),
				Color = (EClothingColor)reader.ReadUInt16()
			};
		}

		public static void WriteProductItemInstance(this Writer writer, ProductItemInstance value)
		{
			if (value is WeedInstance)
			{
				writer.WriteWeedInstance(value as WeedInstance);
			}
			else if (value != null)
			{
				writer.WriteString(typeof(ProductItemInstance).Name);
				writer.WriteString(value.ID);
				writer.WriteUInt16((ushort)value.Quantity);
				writer.WriteUInt16((ushort)value.Quality);
				writer.WriteString(value.PackagingID);
			}
		}

		public static ProductItemInstance ReadProductItemInstance(this Reader reader)
		{
			return reader.Read() as ProductItemInstance;
		}

		private static ProductItemInstance DirectReadProductItemInstance(this Reader reader)
		{
			string text = reader.ReadString();
			if (text == string.Empty)
			{
				return null;
			}
			return new ProductItemInstance
			{
				ID = text,
				Quantity = reader.ReadUInt16(),
				Quality = (EQuality)reader.ReadUInt16(),
				PackagingID = reader.ReadString()
			};
		}

		public static void WriteWeedInstance(this Writer writer, WeedInstance value)
		{
			if (value != null)
			{
				writer.WriteString(typeof(WeedInstance).Name);
				writer.WriteString(value.ID);
				writer.WriteUInt16((ushort)value.Quantity);
				writer.WriteUInt16((ushort)value.Quality);
				writer.WriteString(value.PackagingID);
			}
		}

		public static WeedInstance ReadWeedInstance(this Reader reader)
		{
			return reader.Read() as WeedInstance;
		}

		private static WeedInstance DirectReadWeedInstance(this Reader reader)
		{
			string text = reader.ReadString();
			if (text == string.Empty)
			{
				return null;
			}
			return new WeedInstance
			{
				ID = text,
				Quantity = reader.ReadUInt16(),
				Quality = (EQuality)reader.ReadUInt16(),
				PackagingID = reader.ReadString()
			};
		}

		public static void WriteIntegerItemInstance(this Writer writer, IntegerItemInstance value)
		{
			if (value != null)
			{
				writer.WriteString(typeof(IntegerItemInstance).Name);
				writer.WriteString(value.ID);
				writer.WriteUInt16((ushort)value.Quantity);
				writer.WriteUInt16((ushort)value.Value);
			}
		}

		public static IntegerItemInstance ReadIntegerItemInstance(this Reader reader)
		{
			return reader.Read() as IntegerItemInstance;
		}

		private static IntegerItemInstance DirectReadIntegerItemInstance(this Reader reader)
		{
			string text = reader.ReadString();
			if (text == string.Empty)
			{
				return null;
			}
			return new IntegerItemInstance
			{
				ID = text,
				Quantity = reader.ReadUInt16(),
				Value = reader.ReadUInt16()
			};
		}

		public static void WriteWateringCanInstance(this Writer writer, WateringCanInstance value)
		{
			if (value != null)
			{
				writer.WriteString(typeof(WateringCanInstance).Name);
				writer.WriteString(value.ID);
				writer.WriteUInt16((ushort)value.Quantity);
				writer.WriteSingle(value.CurrentFillAmount);
			}
		}

		public static WateringCanInstance ReadWateringCanInstance(this Reader reader)
		{
			return reader.Read() as WateringCanInstance;
		}

		private static WateringCanInstance DirectReadWateringCanInstance(this Reader reader)
		{
			string text = reader.ReadString();
			if (text == string.Empty)
			{
				return null;
			}
			return new WateringCanInstance
			{
				ID = text,
				Quantity = reader.ReadUInt16(),
				CurrentFillAmount = reader.ReadSingle()
			};
		}

		public static void WriteTrashGrabberInstance(this Writer writer, TrashGrabberInstance value)
		{
			if (value != null)
			{
				writer.WriteString(typeof(TrashGrabberInstance).Name);
				writer.WriteString(value.ID);
				writer.WriteUInt16((ushort)value.Quantity);
				string[] array = value.GetTrashIDs().ToArray();
				writer.WriteArray(array, 0, array.Length);
				ushort[] array2 = value.GetTrashUshortQuantities().ToArray();
				writer.WriteArray(array2, 0, array2.Length);
			}
		}

		public static TrashGrabberInstance ReadTrashGrabberInstance(this Reader reader)
		{
			return reader.Read() as TrashGrabberInstance;
		}

		private static TrashGrabberInstance DirectReadTrashGrabberInstance(this Reader reader)
		{
			string text = reader.ReadString();
			if (text == string.Empty)
			{
				return null;
			}
			TrashGrabberInstance trashGrabberInstance = new TrashGrabberInstance();
			trashGrabberInstance.ID = text;
			trashGrabberInstance.Quantity = reader.ReadUInt16();
			string[] collection = new string[20];
			ushort[] collection2 = new ushort[20];
			int num = reader.ReadArray(ref collection);
			reader.ReadArray(ref collection2);
			for (int i = 0; i < num; i++)
			{
				trashGrabberInstance.AddTrash(collection[i], collection2[i]);
			}
			return trashGrabberInstance;
		}
	}
}
