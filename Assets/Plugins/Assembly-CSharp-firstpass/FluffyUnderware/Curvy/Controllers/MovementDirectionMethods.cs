using System;

namespace FluffyUnderware.Curvy.Controllers
{
	public static class MovementDirectionMethods
	{
		public static MovementDirection FromInt(int value)
		{
			if (value < 0)
			{
				return MovementDirection.Backward;
			}
			return MovementDirection.Forward;
		}

		public static MovementDirection GetOpposite(this MovementDirection value)
		{
			return value switch
			{
				MovementDirection.Forward => MovementDirection.Backward, 
				MovementDirection.Backward => MovementDirection.Forward, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		public static int ToInt(this MovementDirection direction)
		{
			return direction switch
			{
				MovementDirection.Forward => 1, 
				MovementDirection.Backward => -1, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}
}
