using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FlippedPieces
{
	public NodePiece one;
	public NodePiece two;

	public FlippedPieces(NodePiece one, NodePiece two)
	{
		this.one = one;
		this.two = two;
	}

	public NodePiece GetOtherNodePiece(NodePiece p)
	{
		return p == one 
			? two 
			: p == two 
				? one 
				: null;
	}
}
