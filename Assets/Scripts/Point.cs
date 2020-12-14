using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Point
{
	//---[PUBLIC  SECTION]--------------------------------------------//
	public int x;
	public int y;

	public Point(int newX, int newY)
	{
		x = newX;
		y = newY;
	}




	//---[PUBLIC  METHODS]--------------------------------------------//
	public void Multiply(int m)
	{
		x *= m;
		y *= m;
	}
	
	public void Add(Point point)
	{
		x += point.x;
		y += point.y;
	}
	
	public Vector2 ToVector()
	{
		return new Vector2(x, y);
	}
	

	public override bool Equals(object other)
	{
		if (other is Point point)
			return (point.x == x && point.y == y);

		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
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
		return new Point(point.x * m, point.y * m);
	}

	public static Point Add(Point point1, Point point2)
	{
		return new Point(point1.x + point2.x, point1.y + point2.y);
	}

	public static Point Clone(Point point)
	{
		return new Point(point.x, point.y);
	}




	//---[  PROPERTIES   ]--------------------------------------------//
	public static Point Zero => new Point(0, 0);
	public static Point One => new Point(1, 1);
	public static Point Up => new Point(0, 1);
	public static Point Down => new Point(0, -1);
	public static Point Right => new Point(1, 0);
	public static Point Left => new Point(-1, 0);
}  
