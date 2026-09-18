using System;
using UnityEngine;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	public struct Matrix2D
	{
		public float m00;

		public float m10;

		public float m01;

		public float m11;

		public float m02;

		public float m12;

		private static readonly Matrix2D zeroMatrix = new Matrix2D(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));

		private static readonly Matrix2D identityMatrix = new Matrix2D(new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f));

		public float this[int row, int column]
		{
			get
			{
				return this[row + column * 2];
			}
			set
			{
				this[row + column * 2] = value;
			}
		}

		public float this[int index]
		{
			get
			{
				return index switch
				{
					0 => m00, 
					1 => m10, 
					2 => m01, 
					3 => m11, 
					4 => m02, 
					5 => m12, 
					_ => throw new IndexOutOfRangeException("Invalid matrix index!"), 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					m00 = value;
					break;
				case 1:
					m10 = value;
					break;
				case 2:
					m01 = value;
					break;
				case 3:
					m11 = value;
					break;
				case 4:
					m02 = value;
					break;
				case 5:
					m12 = value;
					break;
				default:
					throw new IndexOutOfRangeException("Invalid matrix index!");
				}
			}
		}

		public static Matrix2D zero => zeroMatrix;

		public static Matrix2D identity => identityMatrix;

		public Matrix2D(Vector2 column0, Vector2 column1, Vector2 column2)
		{
			m00 = column0.x;
			m01 = column1.x;
			m02 = column2.x;
			m10 = column0.y;
			m11 = column1.y;
			m12 = column2.y;
		}

		public override int GetHashCode()
		{
			return GetColumn(0).GetHashCode() ^ (GetColumn(1).GetHashCode() << 2) ^ (GetColumn(2).GetHashCode() >> 2);
		}

		public override bool Equals(object other)
		{
			if (!(other is Matrix2D matrix2D))
			{
				return false;
			}
			if (GetColumn(0).Equals(matrix2D.GetColumn(0)) && GetColumn(1).Equals(matrix2D.GetColumn(1)))
			{
				return GetColumn(2).Equals(matrix2D.GetColumn(2));
			}
			return false;
		}

		public static Matrix2D operator *(Matrix2D lhs, Matrix2D rhs)
		{
			Matrix2D result = default(Matrix2D);
			result.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10;
			result.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11;
			result.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02;
			result.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10;
			result.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11;
			result.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12;
			return result;
		}

		public static Vector2 operator *(Matrix2D lhs, Vector2 vector)
		{
			Vector2 result = default(Vector2);
			result.x = lhs.m00 * vector.x + lhs.m01 * vector.y + lhs.m02;
			result.y = lhs.m10 * vector.x + lhs.m11 * vector.y + lhs.m12;
			return result;
		}

		public static bool operator ==(Matrix2D lhs, Matrix2D rhs)
		{
			if (lhs.GetColumn(0) == rhs.GetColumn(0) && lhs.GetColumn(1) == rhs.GetColumn(1))
			{
				return lhs.GetColumn(2) == rhs.GetColumn(2);
			}
			return false;
		}

		public static bool operator !=(Matrix2D lhs, Matrix2D rhs)
		{
			return !(lhs == rhs);
		}

		public Vector2 GetColumn(int index)
		{
			return index switch
			{
				0 => new Vector2(m00, m10), 
				1 => new Vector2(m01, m11), 
				2 => new Vector2(m02, m12), 
				_ => throw new IndexOutOfRangeException("Invalid column index!"), 
			};
		}

		public Vector3 GetRow(int index)
		{
			return index switch
			{
				0 => new Vector3(m00, m01, m02), 
				1 => new Vector3(m10, m11, m12), 
				_ => throw new IndexOutOfRangeException("Invalid row index!"), 
			};
		}

		public void SetColumn(int index, Vector2 column)
		{
			this[0, index] = column.x;
			this[1, index] = column.y;
		}

		public void SetRow(int index, Vector3 row)
		{
			this[index, 0] = row.x;
			this[index, 1] = row.y;
			this[index, 2] = row.z;
		}

		public Vector2 MultiplyPoint(Vector2 point)
		{
			Vector2 result = default(Vector2);
			result.x = m00 * point.x + m01 * point.y + m02;
			result.y = m10 * point.x + m11 * point.y + m12;
			return result;
		}

		public Vector2 MultiplyVector(Vector2 vector)
		{
			Vector2 result = default(Vector2);
			result.x = m00 * vector.x + m01 * vector.y;
			result.y = m10 * vector.x + m11 * vector.y;
			return result;
		}

		public Matrix2D Inverse()
		{
			Matrix2D result = default(Matrix2D);
			float num = this[0, 0] * this[1, 1] - this[0, 1] * this[1, 0];
			if (Mathf.Approximately(0f, num))
			{
				return zero;
			}
			float num2 = 1f / num;
			result[0, 0] = this[1, 1] * num2;
			result[0, 1] = (0f - this[0, 1]) * num2;
			result[1, 0] = (0f - this[1, 0]) * num2;
			result[1, 1] = this[0, 0] * num2;
			result[0, 2] = 0f - (this[0, 2] * result[0, 0] + this[1, 2] * result[0, 1]);
			result[1, 2] = 0f - (this[0, 2] * result[1, 0] + this[1, 2] * result[1, 1]);
			return result;
		}

		public static Matrix2D Scale(Vector2 vector)
		{
			Matrix2D result = default(Matrix2D);
			result.m00 = vector.x;
			result.m01 = 0f;
			result.m02 = 0f;
			result.m10 = 0f;
			result.m11 = vector.y;
			result.m12 = 0f;
			return result;
		}

		public static Matrix2D Translate(Vector2 vector)
		{
			Matrix2D result = default(Matrix2D);
			result.m00 = 1f;
			result.m01 = 0f;
			result.m02 = vector.x;
			result.m10 = 0f;
			result.m11 = 1f;
			result.m12 = vector.y;
			return result;
		}

		public static Matrix2D RotateRH(float angleRadians)
		{
			return RotateLH(0f - angleRadians);
		}

		public static Matrix2D RotateLH(float angleRadians)
		{
			float num = Mathf.Sin(angleRadians);
			Matrix2D result = default(Matrix2D);
			float num2 = (result.m00 = Mathf.Cos(angleRadians));
			result.m10 = 0f - num;
			result.m01 = num;
			result.m11 = num2;
			result.m02 = 0f;
			result.m12 = 0f;
			return result;
		}

		public static Matrix2D SkewX(float angleRadians)
		{
			Matrix2D result = default(Matrix2D);
			result.m00 = 1f;
			result.m01 = Mathf.Tan(angleRadians);
			result.m02 = 0f;
			result.m10 = 0f;
			result.m11 = 1f;
			result.m12 = 0f;
			return result;
		}

		public static Matrix2D SkewY(float angleRadians)
		{
			Matrix2D result = default(Matrix2D);
			result.m00 = 1f;
			result.m01 = 0f;
			result.m02 = 0f;
			result.m10 = Mathf.Tan(angleRadians);
			result.m11 = 1f;
			result.m12 = 0f;
			return result;
		}

		public override string ToString()
		{
			return $"{m00:F5}\t{m01:F5}\t{m02:F5}\n{m10:F5}\t{m11:F5}\t{m12:F5}\n";
		}

		public string ToString(string format)
		{
			return $"{m00.ToString(format)}\t{m01.ToString(format)}\t{m02.ToString(format)}\n{m10.ToString(format)}\t{m11.ToString(format)}\t{m12.ToString(format)}\n";
		}
	}
}
