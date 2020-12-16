namespace Pieces
{
	[System.Serializable]
	public class FlippedPieces
	{
		public GamePiece one;
		public GamePiece two;

		public FlippedPieces(GamePiece one, GamePiece two)
		{
			this.one = one;
			this.two = two;
		}

		public GamePiece GetOtherGamePiece(GamePiece p)
		{
			return p == one 
				? two 
				: p == two 
					? one 
					: null;
		}
	}
}
