using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.ItemFramework;

namespace ScheduleOne.DevUtilities
{
	public class PackingAlgorithm : Singleton<PackingAlgorithm>
	{
		[Serializable]
		public class Rectangle
		{
			public string name;

			public int sizeX;

			public int sizeY;

			public bool flipped;

			public int actualSizeX
			{
				get
				{
					if (flipped)
					{
						return sizeY;
					}
					return sizeX;
				}
			}

			public int actualSizeY
			{
				get
				{
					if (flipped)
					{
						return sizeX;
					}
					return sizeY;
				}
			}

			public Rectangle(string _name, int x, int y)
			{
				name = _name;
				sizeX = x;
				sizeY = y;
			}
		}

		public class StoredItemData : Rectangle
		{
			public ItemInstance item;

			public int xPos;

			public int yPos;

			public float rotation
			{
				get
				{
					if (!flipped)
					{
						return 0f;
					}
					return 90f;
				}
			}

			public StoredItemData(string _name, int x, int y, ItemInstance _item)
				: base(_name, x, y)
			{
				item = _item;
			}
		}

		public class Coordinate
		{
			public int x;

			public int y;

			public Rectangle occupant;

			public Coordinate(int _x, int _y)
			{
				x = _x;
				y = _y;
			}
		}

		public List<Rectangle> rectsToPack = new List<Rectangle>();

		public List<StoredItemData> PackItems(List<ItemInstance> datas, int gridX, int gridY)
		{
			List<StoredItemData> list = new List<StoredItemData>();
			for (int i = 0; i < datas.Count; i++)
			{
				StorableItemDefinition storableItemDefinition = datas[i].Definition as StorableItemDefinition;
				if (!(storableItemDefinition == null))
				{
					StoredItemData item = new StoredItemData(storableItemDefinition.Name, storableItemDefinition.StoredItem.xSize, storableItemDefinition.StoredItem.ySize, datas[i]);
					list.Add(item);
				}
			}
			AttemptPack(list, gridX, gridY);
			return list;
		}

		public List<StoredItemData> AttemptPack(List<StoredItemData> rects, int gridX, int gridY)
		{
			List<StoredItemData> list = rects.OrderBy((StoredItemData o) => o.sizeX * o.sizeY).ToList();
			list.Reverse();
			Coordinate[,] array = new Coordinate[gridX, gridY];
			for (int i = 0; i < gridX; i++)
			{
				for (int j = 0; j < gridY; j++)
				{
					array[i, j] = new Coordinate(i, j);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				List<Coordinate> list2 = new List<Coordinate>();
				if (k == 0)
				{
					list2.Add(new Coordinate(0, 0));
				}
				for (int l = 0; l < gridX; l++)
				{
					for (int m = 0; m < gridY; m++)
					{
						if (array[l, m].occupant == null && DoesCoordinateHaveOccupiedAdjacent(array, new Coordinate(l, m), gridX, gridY))
						{
							list2.Add(new Coordinate(l, m));
						}
					}
				}
				int regionSize = GetRegionSize(array, gridX, gridY);
				int num = int.MaxValue;
				Coordinate coordinate = null;
				bool flipped = false;
				for (int n = 0; n < list2.Count; n++)
				{
					bool flag = true;
					for (int num2 = 0; num2 < list[k].actualSizeX; num2++)
					{
						for (int num3 = 0; num3 < list[k].actualSizeY; num3++)
						{
							Coordinate coordinate2 = TransformCoordinatePoint(array, list2[n], new Coordinate(num2, num3), gridX, gridY);
							if (coordinate2 == null)
							{
								flag = false;
							}
							else if (coordinate2.occupant != null)
							{
								flag = false;
							}
							if (!flag)
							{
								break;
							}
						}
						if (!flag)
						{
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
					for (int num4 = 0; num4 < list[k].actualSizeX; num4++)
					{
						for (int num5 = 0; num5 < list[k].actualSizeY; num5++)
						{
							TransformCoordinatePoint(array, list2[n], new Coordinate(num4, num5), gridX, gridY).occupant = list[k];
						}
					}
					int num6 = GetRegionSize(array, gridX, gridY) - regionSize;
					if (num6 < num)
					{
						num = num6;
						coordinate = list2[n];
						flipped = false;
					}
					for (int num7 = 0; num7 < list[k].actualSizeX; num7++)
					{
						for (int num8 = 0; num8 < list[k].actualSizeY; num8++)
						{
							TransformCoordinatePoint(array, list2[n], new Coordinate(num7, num8), gridX, gridY).occupant = null;
						}
					}
				}
				for (int num9 = 0; num9 < list2.Count; num9++)
				{
					bool flag2 = true;
					list[k].flipped = true;
					for (int num10 = 0; num10 < list[k].actualSizeX; num10++)
					{
						for (int num11 = 0; num11 < list[k].actualSizeY; num11++)
						{
							Coordinate coordinate3 = TransformCoordinatePoint(array, list2[num9], new Coordinate(num10, num11), gridX, gridY);
							if (coordinate3 == null)
							{
								flag2 = false;
							}
							else if (coordinate3.occupant != null)
							{
								flag2 = false;
							}
							if (!flag2)
							{
								break;
							}
						}
						if (!flag2)
						{
							break;
						}
					}
					if (!flag2)
					{
						continue;
					}
					for (int num12 = 0; num12 < list[k].actualSizeX; num12++)
					{
						for (int num13 = 0; num13 < list[k].actualSizeY; num13++)
						{
							TransformCoordinatePoint(array, list2[num9], new Coordinate(num12, num13), gridX, gridY).occupant = list[k];
						}
					}
					int num14 = GetRegionSize(array, gridX, gridY) - regionSize;
					if (num14 < num)
					{
						num = num14;
						coordinate = list2[num9];
						flipped = true;
					}
					for (int num15 = 0; num15 < list[k].actualSizeX; num15++)
					{
						for (int num16 = 0; num16 < list[k].actualSizeY; num16++)
						{
							TransformCoordinatePoint(array, list2[num9], new Coordinate(num15, num16), gridX, gridY).occupant = null;
						}
					}
				}
				if (coordinate == null)
				{
					Console.LogWarning("Unable to resolve rectangle position.");
					continue;
				}
				list[k].flipped = flipped;
				for (int num17 = 0; num17 < list[k].actualSizeX; num17++)
				{
					for (int num18 = 0; num18 < list[k].actualSizeY; num18++)
					{
						TransformCoordinatePoint(array, coordinate, new Coordinate(num17, num18), gridX, gridY).occupant = list[k];
					}
				}
				list[k].xPos = coordinate.x;
				list[k].yPos = coordinate.y;
			}
			return rects;
		}

		private bool DoesCoordinateHaveOccupiedAdjacent(Coordinate[,] grid, Coordinate coord, int gridX, int gridY)
		{
			Coordinate coordinate = new Coordinate(coord.x - 1, coord.y);
			if (IsCoordinateInBounds(coordinate, gridX, gridY) && grid[coordinate.x, coordinate.y].occupant != null)
			{
				return true;
			}
			Coordinate coordinate2 = new Coordinate(coord.x + 1, coord.y);
			if (IsCoordinateInBounds(coordinate2, gridX, gridY) && grid[coordinate2.x, coordinate2.y].occupant != null)
			{
				return true;
			}
			Coordinate coordinate3 = new Coordinate(coord.x, coord.y - 1);
			if (IsCoordinateInBounds(coordinate3, gridX, gridY) && grid[coordinate3.x, coordinate3.y].occupant != null)
			{
				return true;
			}
			Coordinate coordinate4 = new Coordinate(coord.x, coord.y + 1);
			if (IsCoordinateInBounds(coordinate4, gridX, gridY) && grid[coordinate4.x, coordinate4.y].occupant != null)
			{
				return true;
			}
			return false;
		}

		private bool IsCoordinateInBounds(Coordinate coord, int gridX, int gridY)
		{
			if (coord.x < 0 || coord.x >= gridX)
			{
				return false;
			}
			if (coord.y < 0 || coord.y >= gridY)
			{
				return false;
			}
			return true;
		}

		private void PrintGrid(Coordinate[,] grid, int gridX, int gridY)
		{
			string text = string.Empty;
			for (int i = 0; i < gridY; i++)
			{
				for (int j = 0; j < gridX; j++)
				{
					text = ((grid[j, gridY - i - 1].occupant != null) ? (text + grid[j, gridY - i - 1].occupant.name + ", ") : (text + "*, "));
				}
				text += "\n";
			}
			Console.Log(text);
		}

		private int GetRegionSize(Coordinate[,] grid, int gridX, int gridY)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < gridX; i++)
			{
				for (int j = 0; j < gridY; j++)
				{
					if (grid[i, j].occupant != null)
					{
						if (i > num3)
						{
							num3 = i;
						}
						if (j > num4)
						{
							num4 = j;
						}
					}
				}
			}
			return (num3 - num) * (num4 - num2);
		}

		private Coordinate TransformCoordinatePoint(Coordinate[,] grid, Coordinate baseCoordinate, Coordinate offset, int gridX, int gridY)
		{
			if (IsCoordinateInBounds(new Coordinate(baseCoordinate.x + offset.x, baseCoordinate.y + offset.y), gridX, gridY))
			{
				return grid[baseCoordinate.x + offset.x, baseCoordinate.y + offset.y];
			}
			return null;
		}
	}
}
