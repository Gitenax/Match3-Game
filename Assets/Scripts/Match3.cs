﻿using System;
using System.Collections.Generic;
using Grid;
using Pieces;
using UnityEngine;
using Random = UnityEngine.Random;

public class Match3 : MonoBehaviour
{
	public ArrayLayout BoardLayout;

	[Header("UI Elements")]
	public Sprite[] SpritePieces;
	public RectTransform GameBoard;
	public RectTransform KilledBoard;

	[Header("Prefabs")] 
	public GameObject NodePrefab;
	public GameObject KilledPiece;


	private int width = 9;
	private int height = 14;
	private int[] fills;

	private Node[,] board;

	private System.Random random;

	private List<GamePiece> update;
	private List<GamePiece> dead;
	private List<FlippedPieces> flipped;
	private List<KilledPiece> killed;


	//Количество элементов исключая (hole & blank)
	private readonly int _piecesCount = Enum.GetValues(typeof(PieceType)).Length - 2;
	
	void Start()
	{
		fills = new int[width];
		
		random = new System.Random(GetRandomSeed());

		update = new List<GamePiece>();
		dead = new List<GamePiece>();
		flipped = new List<FlippedPieces>();
		killed = new List<KilledPiece>();
		
		InitializeBoard();
		VerifyBoard();
		InstantiateBoard();
	}
	
	void Update()
	{
		List<GamePiece> finishedUpdating = new List<GamePiece>();
		for (int i = 0; i < update.Count; i++)
		{
			GamePiece piece = update[i];
			if(!piece.UpdatePiece())
				finishedUpdating.Add(piece);
		}
		for (int i = 0; i < finishedUpdating.Count; i++)
		{
			GamePiece piece = finishedUpdating[i];
			
			FlippedPieces flip = GetFlipped(piece);
			GamePiece flippedPiece = null;

			int x = piece.Index.X;
			fills[x] = Mathf.Clamp(fills[x] - 1, 0, width);
			
			List<Point> connected = IsConnected(piece.Index, true);
			bool wasFlipped = (flip != null);


			if (wasFlipped)
			{
				flippedPiece = flip.GetOtherGamePiece(piece);
				AddPoints(ref connected, IsConnected(flippedPiece.Index, true));
			}

			if (connected.Count == 0) //If we didtn't make match
			{
				if(wasFlipped)
					FlipPieces(piece.Index, flippedPiece.Index, false);
			}
			else //If made a match
			{
				//Remove pieces connected
				foreach (var point in connected)
				{
					KillPieceAtPoint(point);
					Node node = GetNodeAtPoint(point);
					GamePiece nodePiece = node.GetPiece();
					if (nodePiece != null)
					{
						nodePiece.gameObject.SetActive(false);
						dead.Add(nodePiece);
					}
					node.SetPiece(null);
				}
				ApplyGravityToBoard();
			}

			flipped.Remove(flip);
			update.Remove(piece);
		}

	}

	void ApplyGravityToBoard()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = (height - 1); y >= 0; y--)
			{
				Point p = new Point(x, y);
				Node node = GetNodeAtPoint(p);
				if(GetTypeAtPoint(p) != 0) continue; //if it isn't a hole, do nothing
				for (int ny = (y - 1); ny >= -1; ny--)
				{
					Point next = new Point(x, ny);
					PieceType nextVal = GetTypeAtPoint(next);
					if(nextVal == 0) 
						continue;
					if (nextVal != PieceType.Hole) //if we didn't hit an end, but its not 0 then use this to fill the current hole
					{
						Node got = GetNodeAtPoint(next);
						GamePiece piece = got.GetPiece();
						
						//Set hole
						node.SetPiece(piece);
						update.Add(piece);
						
						//Replase the hole
						got.SetPiece(null);
					}
					else //Hit and end
					{
						//Fill in the hole
						PieceType newVal = FillPiece();
						GamePiece piece;
						Point fallPoint = new Point(x, (-1 - fills[x]));
						if (dead.Count > 0)
						{
							GamePiece revived = dead[0];
							revived.gameObject.SetActive(true);
							piece = revived;
							dead.RemoveAt(0);
						}
						else
						{
							GameObject obj = Instantiate(NodePrefab, GameBoard);
							piece = obj.GetComponent<GamePiece>();
						}
						
						piece.Initialize(newVal, p);
						piece.Rect.anchoredPosition = GetPositionFromPoint(fallPoint);
						Node hole = GetNodeAtPoint(p);
						hole.SetPiece(piece);
						ResetPiece(piece);
						fills[x]++;
					}
					break;
				}
			}
		}
	}

	
	FlippedPieces  GetFlipped(GamePiece p)
	{
		FlippedPieces flip = null;
		for (int i = 0; i < flipped.Count; i++)
		{
			if (flipped[i].GetOtherGamePiece(p) != null)
			{
				flip = flipped[i];
				break;
			}
		}

		return flip;
	}
	
	private void RemoveFlipped(GamePiece p)
	{
		FlippedPieces flip = null;
		for (int i = 0; i < flipped.Count; i++)
		{
			if (flipped[i].GetOtherGamePiece(p) != null)
			{
				flip = flipped[i];
				break;
			}
		}
	}
	

	void InitializeBoard()
	{
		board = new Node[width, height];

		for (int y = 0; y < height; y++)
		for (int x = 0; x < width; x++)
			board[x, y] = new Node((BoardLayout.Rows[y].Column[x] ? PieceType.Hole : FillPiece()), new Point(x, y));

	}

	void VerifyBoard()
	{
		List<PieceType> remove;

		for (int x = 0; x < width; x++)
		for (int y = 0; y < height; y++)
		{
			var p = new Point(x, y);
			PieceType value = GetTypeAtPoint(p);

			if(value <= 0) continue;

			remove = new List<PieceType>();
			while (IsConnected(p, true).Count > 0)
			{
				if(!remove.Contains(value))
					remove.Add(value);

				SetTypeAtPoint(p, NewValue(ref remove));
			}
		}
	}

	void InstantiateBoard()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Node node = GetNodeAtPoint(new Point(x, y));


				PieceType value = board[x, y].Type;
				if(value <= 0) continue;

				GameObject piece = Instantiate(NodePrefab, GameBoard);
				GamePiece nodePiece = piece.GetComponent<GamePiece>();
				RectTransform rect = piece.GetComponent<RectTransform>();
				rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));

				//nodePiece.Initialize(value, new Point(x, y), SpritePieces[(int)value - 1]);
				nodePiece.Initialize(value, new Point(x, y));
				node.SetPiece(nodePiece);
			}
		}
	}

	public void ResetPiece(GamePiece piece)
	{
		piece.ResetPosition();
		piece.Flipped = null;
		update.Add(piece);
	}

	public void FlipPieces(Point one, Point two, bool main = false)
	{
		if(GetTypeAtPoint(one) < 0) return;

		Node nodeOne = GetNodeAtPoint(one);
		GamePiece pieceOne = nodeOne.GetPiece();
		if (GetTypeAtPoint(two) > 0)
		{
			Node nodeTwo = GetNodeAtPoint(two);
			GamePiece pieceTwo = nodeTwo.GetPiece();
			nodeOne.SetPiece(pieceTwo);
			nodeTwo.SetPiece(pieceOne);

			pieceOne.Flipped = pieceTwo;
			pieceTwo.Flipped = pieceOne;

			update.Add(pieceOne);
			update.Add(pieceTwo);
		}
		else
		{
			ResetPiece(pieceOne);
		}
	}

	private void KillPieceAtPoint(Point point)
	{
		List<KilledPiece> available = new List<KilledPiece>();
		for (int i = 0; i < killed.Count; i++)
			if(!killed[i].Falling) available.Add(killed[i]);

		KilledPiece set = null;
		if (available.Count > 0)
			set = available[0];
		else
		{
			GameObject kill = Instantiate(KilledPiece, KilledBoard);
			var kPiece = kill.GetComponent<KilledPiece>();
			set = kPiece;
			killed.Add(set);
		}

		PieceType val = GetTypeAtPoint(point) - 1;
		if(set != null && (int)val >= 0 && (int)val < SpritePieces.Length)
			set.Initialize(SpritePieces[(int)val], GetPositionFromPoint(point));
	}
	
	private Node GetNodeAtPoint(Point point)
	{
		return board[point.X, point.Y];
	}

	private PieceType NewValue(ref List<PieceType> remove)
	{
		List<PieceType> available = new List<PieceType>();
		for (int i = 0; i < _piecesCount; i++)
		{
			available.Add((PieceType)(i + 1));
		}

		foreach (var i in remove)
		{
			available.Remove(i);
		}

		if (available.Count <= 0)
			return PieceType.Blank;

		return available[random.Next(0, available.Count)];
	}


	List<Point> IsConnected(Point point, bool main)
	{
		List<Point> connected = new List<Point>();
		PieceType value = GetTypeAtPoint(point);
		Point[] directions =
		{
			Point.Up,
			Point.Right, 
			Point.Down, 
			Point.Left
		};

		// Проверка наличия 2-х или более одинаковых фигур по направлениям
		foreach (Point dir in directions)
		{
			List<Point> line = new List<Point>();
			int same = 0;

			for (int i = 1; i < 3; i++)
			{
				Point check = Point.Add(point, Point.Multiply(dir, i));
				if (GetTypeAtPoint(check) == value)
				{
					line.Add(check);
					same++;
				}
			}

			if(same > 1) // Если есть более 1-й одинаковой фигуры в направлении
				AddPoints(ref connected, line); // Добавление точки в connected
		}


		// Проверка если фигура находится в середине
		for (int i = 0; i < 2; i++)
		{
			List<Point> line = new List<Point>();
			int same = 0;
			Point[] check =
			{
				Point.Add(point, directions[i]), 
				Point.Add(point, directions[i + 2]) 
			};

			foreach (Point next in check)
			{
				if (GetTypeAtPoint(next) == value)
				{
					line.Add(next);
					same++;
				}
			}

			if (same > 1) // Если есть более 1-й одинаковой фигуры в направлении
				AddPoints(ref connected, line); // Добавление точки в connected
		}


		// Проверка на 2x2
		for (int i = 0; i < 4; i++)
		{
			List<Point> square = new List<Point>();

			int same = 0;
			int next = i + 1;
			if (next >= 4)
				next -= 4;

			Point[] check =
			{
				Point.Add(point, directions[i]), 
				Point.Add(point, directions[next]), 
				Point.Add(point, Point.Add(directions[i], directions[next])) 
			};

			foreach (Point p in check)
			{
				if (GetTypeAtPoint(p) == value)
				{
					square.Add(p);
					same++;
				}
			}

			if (same > 2) // Если есть более 1-й одинаковой фигуры в направлении
				AddPoints(ref connected, square); // Добавление точки в connected
		}

		// Проверка других совпадений
		if (main)
		{
			for(int i = 0; i < connected.Count; i++)
				AddPoints(ref connected, IsConnected(connected[i], false));
		}

		
		// if(connected.Count > 0)
		// 	connected.Add(point);


		return connected;
	}

	void AddPoints(ref List<Point> points, List<Point> add)
	{
		foreach (Point p in add)
		{
			bool addt = true;
			for (int i = 0; i < points.Count; i++)
			{
				if (points[i].Equals(p))
				{
					addt = false;
					break;
				}
			}

			if(addt)
				points.Add(p);
		}
	}


	private PieceType GetTypeAtPoint(Point point)
	{
		if (point.X < 0
		    || point.X >= width
		    || point.Y < 0
		    || point.Y >= height)
			return PieceType.Hole;

		return board[point.X, point.Y].Type;
	}

	private void SetTypeAtPoint(Point point, PieceType value)
	{
		board[point.X, point.Y].Type = value;
	}
	
	//Refactored
	private PieceType FillPiece()
	{
		int value = random.Next(0, 100) / (100 / _piecesCount) + 1;
		
		return (PieceType) Mathf.Clamp(value, 1, _piecesCount);
	}

	private int GetRandomSeed()
	{
		string seed = string.Empty;
		string acceptableChars = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopzxcvbnmasdfghjkl1234567890!@#$%^&*()";

		for (int i = 0; i < 20; i++)
			seed += acceptableChars[Random.Range(0, acceptableChars.Length)];

		return seed.GetHashCode();
	}

	public Vector2 GetPositionFromPoint(Point point)
	{
		return new Vector2(32 + (64 * point.X), -32 - (64 * point.Y));
	}
}