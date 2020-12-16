using Grid;

namespace Pieces
{
	[System.Serializable]
	public class Node
	{
		public PieceType Type;
		public Point Index;

		private GamePiece _piece;

		public Node(PieceType type, Point index)
		{
			Type = type;
			Index = index;
		}

		public void SetPiece(GamePiece piece)
		{
			_piece = piece;
			Type = (piece == null) ? 0 : piece.Type;
			if (piece == null) return;
			piece.Index = Index;
		}

		public GamePiece GetPiece()
		{
			return _piece;
		}
	}
}
