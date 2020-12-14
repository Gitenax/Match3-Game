using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePieces : MonoBehaviour
{
	public static MovePieces Instance;

	private Match3 _game;

	private NodePiece _moving;
	private Point _newIndex;
	private Vector2 _mouseStart;


	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		_game = GetComponent<Match3>();
	}

	private void Update()
	{
		if (_moving != null)
		{
			Vector2 direction = (Vector2) Input.mousePosition - _mouseStart;
			Vector2 normalDir = direction.normalized;
			Vector2 absDir = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

			_newIndex = Point.Clone(_moving.Index);
			Point add = Point.Zero;
			if (direction.magnitude > 32) //Если мышка сместилась на 32 пикселя с начальной точки
			{
				if(absDir.x > absDir.y)
					add = new Point(normalDir.x > 0 ? 1 : -1, 0);
				else if(absDir.x < absDir.y)
					add = new Point(0, normalDir.y > 0 ? -1 : 1);
			}
			_newIndex.Add(add);

			Vector2 pos = _game.GetPositionFromPoint(_moving.Index);
			if (!_newIndex.Equals(_moving.Index))
				pos += Point.Multiply(new Point(add.x, -add.y), 16).ToVector();

			_moving.MovePositionTo(pos);
		}
	}


	public void MovePiece(NodePiece piece)
	{
		if(_moving != null) return;

		_moving = piece;
		_mouseStart = Input.mousePosition;
	}

	public void DropPiece()
	{
		if(_moving == null) return;
		
		Debug.Log("This is <b>bold</b> and <i>italic</i> <color=red>BIG</color> <color=yellow>message</color> <i><b><size=50><color=green>PIECE</color></size></b></i>");
	
		if (!_newIndex.Equals(_moving.Index))
			_game.FlipPieces(_moving.Index, _newIndex, true);
		else
			_game.ResetPiece(_moving);
		
		_moving = null;
	}
}
