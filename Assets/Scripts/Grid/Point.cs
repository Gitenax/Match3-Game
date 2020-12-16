using UnityEngine;

namespace Grid
{
	[System.Serializable]
	public class Point
	{
		//---[PUBLIC  SECTION]--------------------------------------------//
		public int X { get; set; }
		public int Y { get; set; }

		public Point(int newX, int newY)
		{
			X = newX;
			Y = newY;
		}




		//---[PUBLIC  METHODS]--------------------------------------------//
		public void Multiply(int m)
		{
			X *= m;
			Y *= m;
		}
	
		public void Add(Point point)
		{
			X += point.X;
			Y += point.Y;
		}
	
		public Vector2 ToVector()
		{
			return new Vector2(X, Y);
		}
	

		public override bool Equals(object other)
		{
			if (other is Point point)
				return (point.X == X && point.Y == Y);

			return false;
		}



		//---[STATIC  METHODS]--------------------------------------------//
		public static Point FromVector(Vector2 vector)
		{
			return new Point((int)vector.x, (int)vector.y);
		}

		public static Point FromVector(Vector3 vector)
		{
			return new Point((int)vector.x, (int)vector.y);
		}

		public static Point Multiply(Point point, int m)
		{
			return new Point(point.X * m, point.Y * m);
		}

		public static Point Add(Point point1, Point point2)
		{
			return new Point(point1.X + point2.X, point1.Y + point2.Y);
		}

		public static Point Clone(Point point)
		{
			return new Point(point.X, point.Y);
		}




		//---[  PROPERTIES   ]--------------------------------------------//
		public static Point Zero => new Point(0, 0);
		public static Point One => new Point(1, 1);
		public static Point Up => new Point(0, 1);
		public static Point Down => new Point(0, -1);
		public static Point Right => new Point(1, 0);
		public static Point Left => new Point(-1, 0);
	}
}  
