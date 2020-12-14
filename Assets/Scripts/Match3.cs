using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEditorInternal;
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

	private List<NodePiece> update;
	private List<NodePiece> dead;
	private List<FlippedPieces> flipped;
	private List<KilledPiece> killed;
	

	void Start()
	{
		StartGame();
	}




	void Update()
	{
		List<NodePiece> finishedUpdating = new List<NodePiece>();
		for (int i = 0; i < update.Count; i++)
		{
			NodePiece piece = update[i];
			if(!piece.UpdatePiece())
				finishedUpdating.Add(piece);
		}
		for (int i = 0; i < finishedUpdating.Count; i++)
		{
			NodePiece piece = finishedUpdating[i];
			
			FlippedPieces flip = GetFlipped(piece);
			NodePiece flippedPiece = null;

			int x = piece.Index.x;
			fills[x] = Mathf.Clamp(fills[x] - 1, 0, width);
			
			List<Point> connected = IsConnected(piece.Index, true);
			bool wasFlipped = (flip != null);


			if (wasFlipped)
			{
				flippedPiece = flip.GetOtherNodePiece(piece);
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
					NodePiece nodePiece = node.GetPiece();
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
				if(GetValueAtPoint(p) != 0) continue; //if it isn't a hole, do nothing
				for (int ny = (y - 1); ny >= -1; ny--)
				{
					Point next = new Point(x, ny);
					int nextVal = GetValueAtPoint(next);
					if(nextVal == 0) 
						continue;
					if (nextVal != -1) //if we didn't hit an end, but its not 0 then use this to fill the current hole
					{
						Node got = GetNodeAtPoint(next);
						NodePiece piece = got.GetPiece();
						
						//Set hole
						node.SetPiece(piece);
						update.Add(piece);
						
						//Replase the hole
						got.SetPiece(null);
					}
					else //Hit and end
					{
						//Fill in the hole
						int newVal = FillPiece();
						NodePiece piece;
						Point fallPoint = new Point(x, (-1 - fills[x]));
						if (dead.Count > 0)
						{
							NodePiece revived = dead[0];
							revived.gameObject.SetActive(true);
							piece = revived;
							dead.RemoveAt(0);
						}
						else
						{
							GameObject obj = Instantiate(NodePrefab, GameBoard);
							piece = obj.GetComponent<NodePiece>();
						}
						
						piece.Initialize(newVal, p, SpritePieces[newVal - 1]);
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

	
	FlippedPieces  GetFlipped(NodePiece p)
	{
		FlippedPieces flip = null;
		for (int i = 0; i < flipped.Count; i++)
		{
			if (flipped[i].GetOtherNodePiece(p) != null)
			{
				flip = flipped[i];
				break;
			}
		}

		return flip;
	}
	
	private void RemoveFlipped(NodePiece p)
	{
		FlippedPieces flip = null;
		for (int i = 0; i < flipped.Count; i++)
		{
			if (flipped[i].GetOtherNodePiece(p) != null)
			{
				flip = flipped[i];
				break;
			}
		}
	}

	private void StartGame()
	{
		fills = new int[width];
		
		random = new System.Random(GetRandomSeed().GetHashCode());

		update = new List<NodePiece>();
		dead = new List<NodePiece>();
		flipped = new List<FlippedPieces>();
		killed = new List<KilledPiece>();
		
		InitializeBoard();
		VerifyBoard();
		InstantiateBoard();
	}

	void InitializeBoard()
	{
		board = new Node[width, height];

		for (int y = 0; y < height; y++)
		for (int x = 0; x < width; x++)
			board[x, y] = new Node(BoardLayout.rows[y].row[x] ? -1 : FillPiece(), new Point(x, y));

	}

	void VerifyBoard()
	{
		List<int> remove;

		for (int x = 0; x < width; x++)
		for (int y = 0; y < height; y++)
		{
			var p = new Point(x, y);
			int value = GetValueAtPoint(p);

			if(value <= 0) continue;

			remove = new List<int>();
			while (IsConnected(p, true).Count > 0)
			{
				if(!remove.Contains(value))
					remove.Add(value);

				SetValueAtPoint(p, NewValue(ref remove));
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


				int value = board[x, y].Value;
				if(value <= 0) continue;

				GameObject piece = Instantiate(NodePrefab, GameBoard);
				NodePiece nodePiece = piece.GetComponent<NodePiece>();
				RectTransform rect = piece.GetComponent<RectTransform>();
				rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));

				nodePiece.Initialize(value, new Point(x, y), SpritePieces[value - 1]);
				node.SetPiece(nodePiece);
			}
		}
	}

	public void ResetPiece(NodePiece piece)
	{
		piece.ResetPosition();
		piece.Flipped = null;
		update.Add(piece);
	}

	public void FlipPieces(Point one, Point two, bool main = false)
	{
		if(GetValueAtPoint(one) < 0) return;

		Node nodeOne = GetNodeAtPoint(one);
		NodePiece pieceOne = nodeOne.GetPiece();
		if (GetValueAtPoint(two) > 0)
		{
			Node nodeTwo = GetNodeAtPoint(two);
			NodePiece pieceTwo = nodeTwo.GetPiece();
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

		int val = GetValueAtPoint(point) - 1;
		if(set != null && val >= 0 && val < SpritePieces.Length)
			set.Initialize(SpritePieces[val], GetPositionFromPoint(point));
	}
	
	private Node GetNodeAtPoint(Point point)
	{
		return board[point.x, point.y];
	}

	private int NewValue(ref List<int> remove)
	{
		List<int> available = new List<int>();
		for (int i = 0; i < SpritePieces.Length; i++)
		{
			available.Add(i + 1);
		}

		foreach (int i in remove)
		{
			available.Remove(i);
		}

		if (available.Count <= 0)
			return 0;

		return available[random.Next(0, available.Count)];
	}


	List<Point> IsConnected(Point point, bool main)
	{
		List<Point> connected = new List<Point>();
		int value = GetValueAtPoint(point);
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
				if (GetValueAtPoint(check) == value)
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
				if (GetValueAtPoint(next) == value)
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
				if (GetValueAtPoint(p) == value)
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


	int GetValueAtPoint(Point point)
	{
		if (point.x < 0
		    || point.x >= width
		    || point.y < 0
		    || point.y >= height)
			return -1;

		return board[point.x, point.y].Value;
	}

	void SetValueAtPoint(Point point, int value)
	{
		board[point.x, point.y].Value = value;
	}


	private int FillPiece()
	{
		int value = 1;

		value = random.Next(0, 100) / (100 / SpritePieces.Length) + 1;
		value = Mathf.Clamp(value, 1, SpritePieces.Length);

		return value;
	}

	private string GetRandomSeed()
	{
		string seed = string.Empty;
		string acceptableChars = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopzxcvbnmasdfghjkl1234567890!@#$%^&*()";

		for (int i = 0; i < 20; i++)
			seed += acceptableChars[Random.Range(0, acceptableChars.Length)];

		return seed;
	}

	public Vector2 GetPositionFromPoint(Point point)
	{
		return new Vector2(32 + (64 * point.x), -32 - (64 * point.y));
	}
}