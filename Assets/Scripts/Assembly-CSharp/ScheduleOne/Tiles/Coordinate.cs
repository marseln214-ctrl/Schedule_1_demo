using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Tiles
{
	[Serializable]
	public class Coordinate
	{
		public int x;

		public int y;

		public static implicit operator Vector2(Coordinate c)
		{
			return new Vector2(c.x, c.y);
		}

		public Coordinate()
		{
			x = 0;
			y = 0;
		}

		public Coordinate(int _x, int _y)
		{
			x = _x;
			y = _y;
		}

		public Coordinate(Vector2 vector)
		{
			x = (int)vector.x;
			y = (int)vector.y;
		}

		public override int GetHashCode()
		{
			return SignedCantorPair(x, y);
		}

		public override bool Equals(object obj)
		{
			if (obj is Coordinate coordinate && coordinate.x == x)
			{
				return coordinate.y == y;
			}
			return false;
		}

		public static Coordinate operator +(Coordinate a, Coordinate b)
		{
			return new Coordinate(a.x + b.x, a.y + b.y);
		}

		public static Coordinate operator -(Coordinate a, Coordinate b)
		{
			return new Coordinate(a.x - b.x, a.y - b.y);
		}

		private int CantorPair(int x, int y)
		{
			return (int)(0.5f * (float)(x + y) * ((float)(x + y) + 1f) + (float)y);
		}

		private int SignedCantorPair(int x, int y)
		{
			int num = (int)(((float)x >= 0f) ? (2f * (float)x) : (-2f * (float)x - 1f));
			int num2 = (int)(((float)y >= 0f) ? (2f * (float)y) : (-2f * (float)y - 1f));
			return CantorPair(num, num2);
		}

		public override string ToString()
		{
			return "[" + x + "," + y + "]";
		}

		public static List<CoordinatePair> BuildCoordinateMatches(Coordinate originCoord, int sizeX, int sizeY, float rot)
		{
			List<CoordinatePair> list = new List<CoordinatePair>();
			rot = MathMod(Mathf.RoundToInt(rot), 360);
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					Coordinate coordinate = new Coordinate(originCoord.x, originCoord.y);
					if ((double)rot == 0.0)
					{
						coordinate.x += i;
						coordinate.y += j;
					}
					else if (rot == 90f)
					{
						coordinate.x += j;
						coordinate.y -= i;
					}
					else if (rot == 180f)
					{
						coordinate.x -= i;
						coordinate.y -= j;
					}
					else if (rot == 270f)
					{
						coordinate.x -= j;
						coordinate.y += i;
					}
					else
					{
						Console.LogWarning("Cock!!!!!! " + rot);
					}
					list.Add(new CoordinatePair(new Coordinate(i, j), coordinate));
				}
			}
			return list;
		}

		public static Coordinate RotateCoordinates(Coordinate coord, float angle)
		{
			angle = MathMod(Mathf.RoundToInt(angle), 360);
			if (Mathf.Abs(angle - 90f) < 0.01f)
			{
				return new Coordinate(coord.y, -coord.x);
			}
			if (Mathf.Abs(angle - 180f) < 0.01f)
			{
				return new Coordinate(-coord.x, -coord.y);
			}
			if (Mathf.Abs(angle - 270f) < 0.01f)
			{
				return new Coordinate(-coord.y, coord.x);
			}
			return coord;
		}

		private static int MathMod(int a, int b)
		{
			return (Mathf.Abs(a * b) + a) % b;
		}
	}
}
