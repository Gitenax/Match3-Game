using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
	public int Value; // 0 - blank, 1 - cube, 2 - sphere, 3 - cylinder, 4 - pyramid, 5 - diamond, -1 - hole
	public Point Index;

	private NodePiece _piece;

	public Node(int value, Point index)
	{
		Value = value;
		Index = index;
	}

	public void SetPiece(NodePiece piece)
	{
		_piece = piece;
		Value = (piece == null) ? 0 : piece.Value;
		if (piece == null) return;
		piece.SetIndex(Index);
	}

	public NodePiece GetPiece()
	{
		return _piece;
	}
}
